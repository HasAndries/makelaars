using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application.Queries;
using MediatR;

namespace Makelaars.Application
{
    public class MakelaarApplication
    {
        private readonly IMediator _mediator;

        public MakelaarApplication(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var query = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = false
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
