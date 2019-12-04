using System;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Results;

namespace Makelaars.Infrastructure.Funda
{
    public interface IFundaApiClient
    {
        Task<GetOffersResult> GetOffers(OfferTypes? type, string searchCommand, int? page, int? pageSize, CancellationToken cancellationToken);
        Task<GetAllOffersResult> GetAllOffers(OfferTypes? type, string searchCommand, CancellationToken cancellationToken, Action<GetAllOffersStatus> statusUpdate);
        event Action<RateLimitingStatus> RateLimitingUpdated;
    }
}
