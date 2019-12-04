using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application.Models;
using Makelaars.Application.Queries;
using Makelaars.Infrastructure.Funda;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Operations;
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
            // Initial call to get paging info
            var options = BuildGetOffersOptions(request);
            var initialGetOffersResult = await _fundaApiClient.GetOffers(options, cancellationToken);
            var totalPages = initialGetOffersResult.Paging.TotalPages;
            var offers = new List<Offer>();
            offers.AddRange(initialGetOffersResult.Offers);

            // Call pages in parallel
            var parallelOptions = new ParallelOptions()
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 4
            };
            Parallel.For(2, totalPages + 1, parallelOptions, async i =>
            {
                var nextOptions = options.Copy().IncreasePage();
                var nextGetOffersResult = await _fundaApiClient.GetOffers(nextOptions, cancellationToken);
                if (nextGetOffersResult?.Offers != null)
                {
                    lock (offers)
                    {
                        offers.AddRange(nextGetOffersResult.Offers);
                    }
                }
            });

            // Build makelaar list
            var rank = 1;
            var makelaars = offers
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
                });

            var result = new TopMakelaarsResult
            {
                Makelaars = makelaars,
                Status = TopMakelaarsResultStatus.Ok
            };
            return await Task.FromResult(result);
        }

        private GetOffersOptions BuildGetOffersOptions(TopMakalaarsQuery request)
        {
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

            return new GetOffersOptions
            {
                Type = OfferTypes.Koop,
                SearchCommand = searchCommand.ToString()
            };
        }
    }
}
