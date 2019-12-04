using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application.Models;
using Makelaars.Application.Queries;
using Makelaars.Infrastructure.Funda;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Results;
using MediatR;

namespace Makelaars.Application.QueryHandlers
{
    public class TopMekalaarsQueryHandler : IRequestHandler<TopMakalaarsQuery, TopMakelaarsResult>, IDisposable
    {
        private readonly IFundaApiClient _fundaApiClient;

        public TopMekalaarsQueryHandler(IFundaApiClient fundaApiClient)
        {
            _fundaApiClient = fundaApiClient;
            _fundaApiClient.RateLimitingUpdated += RateLimitingUpdated;
        }

        public void Dispose()
        {
            _fundaApiClient.RateLimitingUpdated -= RateLimitingUpdated;
        }

        public async Task<TopMakelaarsResult> Handle(TopMakalaarsQuery request, CancellationToken cancellationToken)
        {
            // Build options with search command
            var searchCommand = new StringBuilder();
            if (!string.IsNullOrEmpty(request.Location))
            {
                searchCommand.Append($"/{request.Location}");
            }
            if (request.WithGarden)
            {
                searchCommand.Append("/tuin");
            }
            searchCommand.Append("/");

            // Get all offers
            GetAllOffersResult getAllOffersResult;
            try
            {
                getAllOffersResult = await _fundaApiClient.GetAllOffers(OfferTypes.Koop, searchCommand.ToString(), cancellationToken, StatusUpdate);
            }
            catch (Exception ex)
            {
                return new TopMakelaarsResult()
                {
                    Status = TopMakelaarsResultStatus.Error,
                    Error = ex
                };
            }

            // Build makelaar list
            var rank = 1;
            var makelaars = getAllOffersResult.Offers
                .GroupBy(o => o.MakelaarId)
                .Select(g => new TopMakelaar
                {
                    Id = g.Key,
                    Name = g.First().MakelaarNaam,
                    ListingCount = g.Count(),
                })
                .OrderByDescending(m => m.ListingCount)
                .Take(10)
                .Select(m =>
                {
                    m.Rank = rank;
                    rank++;
                    return m;
                })
                .ToArray();

            var result = new TopMakelaarsResult
            {
                Makelaars = makelaars,
                Status = TopMakelaarsResultStatus.Ok
            };
            return await Task.FromResult(result);
        }

        private async void StatusUpdate(GetAllOffersStatus status)
        {
            Console.Write($"\r{status.CurrentOffers} of {status.TotalOffers} offers fetched");
            if (status.CurrentOffers == status.TotalOffers)
            {
                Console.WriteLine();
            }
        }

        private async void RateLimitingUpdated(RateLimitingStatus status)
        {
            var rateLimitationText = status.RateLimiting ? "ACTIVE" : "INACTIVE";
            Console.WriteLine($"\r\nRate limitation: {rateLimitationText}");
        }
    }
}
