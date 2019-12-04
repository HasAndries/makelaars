namespace Makelaars.Infrastructure.Funda
{
    public class FundaApiClientOptions
    {
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; } = "http://partnerapi.funda.nl";
    }
}