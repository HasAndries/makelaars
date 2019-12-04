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
    public class TopMekalaarsQueryHandler : IRequestHandler<TopMakalaarsQuery, TopMakelaarsResult>
    {
        private readonly IFundaApiClient _fundaApiClient;

        public TopMekalaarsQueryHandler(IFundaApiClient fundaApiClient)
        {
            _fundaApiClient = fundaApiClient;
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
                getAllOffersResult = await _fundaApiClient.GetAllOffers(OfferTypes.Koop, searchCommand.ToString(), cancellationToken, request.StatusUpdate);
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
    }
}
