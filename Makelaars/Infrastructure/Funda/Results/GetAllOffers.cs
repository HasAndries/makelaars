using System.Collections.Generic;
using Makelaars.Infrastructure.Funda.Models;

namespace Makelaars.Infrastructure.Funda.Results
{
    public class GetAllOffersResult
    {
        public IEnumerable<Offer> Offers { get; set; }
    }

    public class GetAllOffersStatus
    {
        public int TotalOffers { get; set; }
        public int CurrentOffers { get; set; }
    }
}
