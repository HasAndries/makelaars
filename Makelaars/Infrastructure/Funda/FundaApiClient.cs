using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Results;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Makelaars.Infrastructure.Funda
{
    public class FundaApiClient : IFundaApiClient, IDisposable
    {
        private const int DefaultPage = 1;
        private const int MaxPageSize = 25;

        private readonly HttpClient _httpClient;
        private readonly FundaApiClientOptions _options;
        private readonly AsyncPolicyWrap<HttpResponseMessage> _retryPolicy;
        private readonly object _lock = new object();
        private bool _isRateLimiting;

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
            if (options.DefaultPageSize > MaxPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(options.DefaultPageSize), options.DefaultPageSize, $"Page size cannot be larger than the maximum page size({MaxPageSize})");
            }
            _options = options;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _retryPolicy = CreateRetryPolicy();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public async Task<GetOffersResult> GetOffers(OfferTypes? type, string searchCommand, int? page, int? pageSize, CancellationToken cancellationToken)
        {
            // Build request URL
            pageSize ??= _options.DefaultPageSize;
            pageSize = Math.Min(pageSize.Value, MaxPageSize);
            var parameters = new List<string>()
            {
                $"page={page ?? DefaultPage}",
                $"PageSize={pageSize}"
            };
            if (type != null)
            {
                var typeString = FirstCharLower($"{type}");
                parameters.Add($"type={typeString}");
            }
            if (searchCommand != null)
            {
                parameters.Add($"zo={searchCommand}");
            }
            var requestUrl = BuildRequestUrl("/feeds/Aanbod.svc", parameters);

            // Make request
            using var response = await _retryPolicy.ExecuteAsync(async () => await _httpClient.GetAsync(requestUrl, cancellationToken));

            // Get content from response body
            var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var content = new
            {
                Objects = new[]
                {
                    new
                    {
                        Id = default(Guid),
                        MakelaarId = default(int),
                        MakelaarNaam = default(string),
                    }
                },
                Paging = new
                {
                    AantalPaginas = default(int),
                    HuidigePagina = default(int),
                },
                TotaalAantalObjecten = default(int)
            };
            content = JsonConvert.DeserializeAnonymousType(contentString, content);
            var result = new GetOffersResult();
            // Build result from content
            if (content != null)
            {
                result.Paging = new PagingInfo
                {
                    TotalPages = content.Paging.AantalPaginas,
                    CurrentPage = content.Paging.HuidigePagina,
                };
                result.TotalObjects = content.TotaalAantalObjecten;
                if (content.Objects != null)
                {
                    result.Offers = content.Objects.Select(o => new Offer
                    {
                        Id = o.Id,
                        MakelaarId = o.MakelaarId,
                        MakelaarNaam = o.MakelaarNaam,
                    });
                }
            }

            return result;
        }

        public async Task<GetAllOffersResult> GetAllOffers(OfferTypes? type, string searchCommand, CancellationToken cancellationToken, Action<GetAllOffersStatus> statusUpdate)
        {
            // Initial call to get first page results and paging info
            var initialGetOffersResult = await GetOffers(type, searchCommand, 1, null, cancellationToken);
            var offers = initialGetOffersResult.Offers.ToList();

            // Call rest of pages in parallel
            var tasks = Enumerable.Range(2, initialGetOffersResult.Paging.TotalPages).Select(async pageNumber =>
            {
                var pageResult = await GetOffers(type, searchCommand, pageNumber, null, cancellationToken);
                if (pageResult?.Offers != null)
                {
                    lock (offers)
                    {
                        offers.AddRange(pageResult.Offers);
                        statusUpdate?.Invoke(new GetAllOffersStatus()
                        {
                            TotalOffers = initialGetOffersResult.TotalObjects,
                            CurrentOffers = offers.Count
                        });
                    }
                }
            });
            await Task.WhenAll(tasks);
            return new GetAllOffersResult()
            {
                Offers = offers
            };
        }

        public event Action<RateLimitingStatus> RateLimitingUpdated;

        private AsyncPolicyWrap<HttpResponseMessage> CreateRetryPolicy()
        {
            var rateLimitPolicy = Policy
                .HandleResult<HttpResponseMessage>(responseMessage =>
                {
                    SetRateLimiting(IsRateLimiting(responseMessage));
                    return _isRateLimiting;
                })
                .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(5), onRetryAsync: async (result, timeSpan) =>
                {
                    SetRateLimiting(IsRateLimiting(result.Result));
                });
            var transientErrorPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(i * 2));

            return Policy.WrapAsync(rateLimitPolicy, transientErrorPolicy);
        }

        private string BuildRequestUrl(string servicePath, IEnumerable<string> parameters)
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

        public static bool IsRateLimiting(HttpResponseMessage responseMessage)
        {
            return responseMessage.StatusCode == HttpStatusCode.Unauthorized && responseMessage.ReasonPhrase == "Request limit exceeded";
        }

        private void SetRateLimiting(bool isRateLimiting)
        {
            lock (_lock)
            {
                if (_isRateLimiting == isRateLimiting) return;
                _isRateLimiting = isRateLimiting;
            }

            RateLimitingUpdated?.Invoke(new RateLimitingStatus()
            {
                RateLimiting = _isRateLimiting
            });
        }
    }
}