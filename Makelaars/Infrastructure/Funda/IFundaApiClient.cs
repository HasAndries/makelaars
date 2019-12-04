using System.Threading;
using System.Threading.Tasks;
using Makelaars.Infrastructure.Funda.Operations;

namespace Makelaars.Infrastructure.Funda
{
    public interface IFundaApiClient
    {
        Task<GetOffersResult> GetOffers(GetOffersOptions options, CancellationToken cancellationToken);
    }
}
