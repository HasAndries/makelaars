using System.Collections.Generic;
using Makelaars.Infrastructure.Funda.Models;

namespace Makelaars.Infrastructure.Funda.Results
{
    public class GetOffersResult
    {
        public IEnumerable<Offer> Offers { get; set; }
        public PagingInfo Paging { get; set; }
        public int TotalObjects { get; set; }
    }
}
