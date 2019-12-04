namespace Makelaars.Infrastructure.Funda.Models
{
    public class PagingInfo
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string NextUrl { get; set; }
        public string PreviousUrl { get; set; }
    }
}
