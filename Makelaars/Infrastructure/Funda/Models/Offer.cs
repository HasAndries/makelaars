using System;

namespace Makelaars.Infrastructure.Funda.Models
{
    public class Offer
    {
        public Guid Id { get; set; }
        public int MakelaarId { get; set; }
        public string MakelaarNaam { get; set; }
    }
}
