using System;
using System.Collections.Generic;
using Makelaars.Application.Models;
using Makelaars.Infrastructure.Funda.Results;
using MediatR;

namespace Makelaars.Application.Queries
{
    public class TopMakalaarsQuery : IRequest<TopMakelaarsResult>, IRequest<Unit>
    {
        public string Location { get; set; }
        public bool WithGarden { get; set; }
        public Action<GetAllOffersStatus> StatusUpdate { get; set; }
    }

    public class TopMakelaarsResult
    {
        public IEnumerable<TopMakelaar> Makelaars { get; set; }
        public TopMakelaarsResultStatus Status { get; set; }
        public Exception Error { get; set; }
    }

    public enum TopMakelaarsResultStatus
    {
        Ok,
        Error
    }
}
