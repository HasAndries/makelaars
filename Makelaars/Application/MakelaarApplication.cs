using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application.Queries;
using Makelaars.Infrastructure.Funda;
using Makelaars.Infrastructure.Funda.Results;
using MediatR;

namespace Makelaars.Application
{
    public class MakelaarApplication : IDisposable
    {
        private readonly IFundaApiClient _fundaApiClient;
        private readonly IMediator _mediator;

        public MakelaarApplication(IMediator mediator, IFundaApiClient fundaApiClient)
        {
            _mediator = mediator;
            _fundaApiClient = fundaApiClient;
            _fundaApiClient.RateLimitingUpdated += RateLimitingUpdated;
        }

        public void Dispose()
        {
            _fundaApiClient.RateLimitingUpdated -= RateLimitingUpdated;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var query = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = false,
                StatusUpdate = StatusUpdated
            };
            var result = await _mediator.Send<TopMakelaarsResult>(query, cancellationToken);
            PrintTopMakelaars(query, result);

            query = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true
            };
            result = await _mediator.Send<TopMakelaarsResult>(query, cancellationToken);
            PrintTopMakelaars(query, result);
        }

        private void StatusUpdated(GetAllOffersStatus status)
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

        private void PrintTopMakelaars(TopMakalaarsQuery query, TopMakelaarsResult result)
        {
            var caption = new StringBuilder($"Top {result.Makelaars.Count()} Makelaars - {query.Location}");
            if (query.WithGarden)
            {
                caption.Append(" with Garden");
            }

            if (result.Status == TopMakelaarsResultStatus.Error)
            {
                Console.WriteLine($"Error occurred: {result.Error}");
                return;
            }

            var header = $"| {"Nr",2} | {"Name",-50} | {"Count",5} |";
            Console.WriteLine(new string('-', header.Length));
            Console.WriteLine(caption);
            Console.WriteLine(new string('-', header.Length));
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var makelaar in result.Makelaars)
            {
                Console.WriteLine($"| {makelaar.Rank, 2} | {makelaar.Name, -50} | {makelaar.ListingCount, 5} |");
            }
            Console.WriteLine(new string('-', header.Length));
        }
    }
}
