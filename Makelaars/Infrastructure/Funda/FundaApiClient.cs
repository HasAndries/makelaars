using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Operations;
using Newtonsoft.Json;

namespace Makelaars.Infrastructure.Funda
{
    public class FundaApiClient : IFundaApiClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly FundaApiClientOptions _options;
        
        public FundaApiClient(FundaApiClientOptions options)
        {
            if (string.IsNullOrEmpty(options.ApiUrl))
            {
                throw new ArgumentNullException(nameof(options.ApiUrl));
            }
            if (string.IsNullOrEmpty(options.ApiKey))
            {
                throw new ArgumentNullException(nameof(options.ApiKey));
            }
            _options = options;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task<GetOffersResult> GetOffers(GetOffersOptions options, CancellationToken cancellationToken)
        {
            // Make request
            var parameters = new List<string>()
            {
                $"page={options.Page}",
                $"PageSize={options.PageSize}"
            };
            if (options.Type != null)
            {
                var type = FirstCharLower($"{options.Type}");
                parameters.Add($"type={type}");
            }
            if (options.SearchCommand != null)
            {
                parameters.Add($"zo={options.SearchCommand}");
            }
            var requestUrl = BuildRequestUrl("/feeds/Aanbod.svc", parameters);
            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            // Build result
            var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var content = new
            {
                Objects = new []
                {
                    new
                    {
                        Id = default(Guid),
                        MakelaarId = default(int),
                        MakelaarNaam = default(string),
                        IsVerkocht = default(bool),
                        VerkoopStatus = default(string),
                    }
                },
                Paging = new
                {
                    AantalPaginas = default(int),
                    HuidigePagina = default(int),
                    VolgendeUrl = default(string),
                    VorigeUrl = default(string)
                },
                TotaalAantalObjecten = default(int)
            };
            content = JsonConvert.DeserializeAnonymousType(contentString, content);
            var result = new GetOffersResult
            {
                StatusCode = response.StatusCode
            };
            if (content != null)
            {
                result.Paging = new PagingInfo
                {
                    TotalPages = content.Paging.AantalPaginas,
                    CurrentPage = content.Paging.HuidigePagina,
                    NextUrl = content.Paging.VolgendeUrl,
                    PreviousUrl = content.Paging.VorigeUrl,
                };
                result.TotalObjects = content.TotaalAantalObjecten;
                if (content.Objects != null)
                {
                    result.Offers = content.Objects.Select(o => new Offer
                    {
                        Id = o.Id,
                        MakelaarId = o.MakelaarId,
                        MakelaarNaam = o.MakelaarNaam,
                        IsVerkocht = o.IsVerkocht,
                        VerkoopStatus = o.VerkoopStatus,
                    });
                }
            }

            return result;
        }

        private string BuildRequestUrl(string servicePath, List<string> parameters)
        {
            var uriBuilder = new UriBuilder(_options.ApiUrl)
            {
                Path = string.Join("/", servicePath, _options.ApiKey),
                Query = $"?{string.Join("&", parameters)}"
            };
            return uriBuilder.Uri.AbsoluteUri;
        }

        private string FirstCharLower(string s)
        {
            return string.IsNullOrEmpty(s) ? s : $"{s.Substring(0, 1).ToLower()}{s.Substring(1)}";
        }
    }
}