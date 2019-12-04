using System.Collections.Generic;
using System.Net;
using Makelaars.Infrastructure.Funda.Models;

namespace Makelaars.Infrastructure.Funda.Operations
{
    public class GetOffersOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SearchCommand { get; set; }
        public OfferTypes? Type { get; set; }

        public GetOffersOptions Copy()
        {
            return new GetOffersOptions()
            {
                Page = Page,
                PageSize = PageSize,
                SearchCommand = SearchCommand,
                Type = Type,
            };
        }

        public GetOffersOptions IncreasePage()
        {
            Page++;
            return this;
        }
    }

    public enum OfferTypes
    {
        Koop
    }

    public class GetOffersResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public PagingInfo Paging { get; set; }
        public int TotalObjects { get; set; }
    }
}
