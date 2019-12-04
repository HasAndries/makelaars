using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Makelaars.Application.Queries;
using Makelaars.Application.QueryHandlers;
using Makelaars.Infrastructure.Funda;
using Makelaars.Infrastructure.Funda.Models;
using Makelaars.Infrastructure.Funda.Results;
using Moq;
using NUnit.Framework;

namespace Makelaars.Tests.Application.QueryHandlers
{
    public class TopMekalaarsQueryHandlerTests
    {
        private TopMekalaarsQueryHandler _handler;
        private Mock<IFundaApiClient> _fundaApiClient;

        [SetUp]
        public void Setup()
        {
            _fundaApiClient = new Mock<IFundaApiClient>();
            _handler = new TopMekalaarsQueryHandler(_fundaApiClient.Object);
        }

        [Test]
        public async Task ShouldCorrectlyBuildRequest()
        {
            var getAllOffersResult = new GetAllOffersResult()
            {
                Offers = new List<Offer>()
            };
            var lastType = default(OfferTypes?);
            var lastSearchCommand = default(string);
            _fundaApiClient
                .Setup(c => c.GetAllOffers(It.IsAny<OfferTypes>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<Action<GetAllOffersStatus>>()))
                .Callback<OfferTypes?, string, CancellationToken, Action<GetAllOffersStatus>>((type, searchCommand, cancellationToken, statusUpdate) =>
                {
                    lastType = type;
                    lastSearchCommand = searchCommand;
                })
                .Returns(Task.FromResult(getAllOffersResult));

            var request = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true
            };

            await _handler.Handle(request, CancellationToken.None);
            Assert.AreEqual(OfferTypes.Koop, lastType);
            Assert.AreEqual("/Amsterdam/tuin/", lastSearchCommand);

            request = new TopMakalaarsQuery()
            {
                Location = "Leiden",
                WithGarden = false
            };
            await _handler.Handle(request, CancellationToken.None);
            Assert.AreEqual(OfferTypes.Koop, lastType);
            Assert.AreEqual("/Leiden/", lastSearchCommand);
        }

        [Test]
        public async Task ShouldRankMakelaars()
        {
            var getAllOffersResult = new GetAllOffersResult()
            {
                Offers = new List<Offer>()
            };
            _fundaApiClient
                .Setup(c => c.GetAllOffers(It.IsAny<OfferTypes>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<Action<GetAllOffersStatus>>()))
                .Returns(Task.FromResult(getAllOffersResult));

            var request = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true
            };
            var offers = new List<Offer>();
            offers.AddRange(CreateOffers(1, "Test 1", 2));
            offers.AddRange(CreateOffers(2, "Test 2", 20));
            offers.AddRange(CreateOffers(3, "Test 3", 13));
            offers.AddRange(CreateOffers(4, "Test 4", 40));
            getAllOffersResult.Offers = offers;

            var result = await _handler.Handle(request, CancellationToken.None);
            var makelaars = result.Makelaars.ToList();
            Assert.AreEqual(4, makelaars[0].Id);
            Assert.AreEqual(2, makelaars[1].Id);
            Assert.AreEqual(3, makelaars[2].Id);
            Assert.AreEqual(1, makelaars[3].Id);
        }

        [Test]
        public async Task ShouldLimitMakelaarsTo10()
        {
            var getAllOffersResult = new GetAllOffersResult()
            {
                Offers = new List<Offer>()
            };
            _fundaApiClient
                .Setup(c => c.GetAllOffers(It.IsAny<OfferTypes>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<Action<GetAllOffersStatus>>()))
                .Returns(Task.FromResult(getAllOffersResult));

            var request = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true
            };
            var offers = new List<Offer>();
            offers.AddRange(CreateOffers(1, "Test 1", 2));
            offers.AddRange(CreateOffers(2, "Test 2", 20));
            offers.AddRange(CreateOffers(3, "Test 3", 13));
            offers.AddRange(CreateOffers(4, "Test 4", 14));
            offers.AddRange(CreateOffers(5, "Test 5", 15));
            offers.AddRange(CreateOffers(6, "Test 6", 16));
            offers.AddRange(CreateOffers(7, "Test 7", 17));
            offers.AddRange(CreateOffers(8, "Test 8", 18));
            offers.AddRange(CreateOffers(9, "Test 9", 19));
            offers.AddRange(CreateOffers(10, "Test 10", 10));
            offers.AddRange(CreateOffers(11, "Test 11", 11));
            offers.AddRange(CreateOffers(12, "Test 12", 12));
            getAllOffersResult.Offers = offers;

            var result = await _handler.Handle(request, CancellationToken.None);
            var makelaars = result.Makelaars.ToList();
            Assert.AreEqual(10, makelaars.Count);
        }

        [Test]
        public async Task ShouldHandleApiError()
        {
            var getAllOffersResult = new GetAllOffersResult()
            {
                Offers = new List<Offer>()
            };
            var error = new Exception();
            _fundaApiClient
                .Setup(c => c.GetAllOffers(It.IsAny<OfferTypes>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<Action<GetAllOffersStatus>>()))
                .Throws(error);

            var request = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true
            };

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.AreEqual(TopMakelaarsResultStatus.Error, result.Status);
            Assert.AreEqual(error, result.Error);
        }

        [Test]
        public async Task ShouldTriggerStatusUpdate()
        {
            var getAllOffersResult = new GetAllOffersResult()
            {
                Offers = new List<Offer>()
            };
            _fundaApiClient
                .Setup(c => c.GetAllOffers(It.IsAny<OfferTypes>(), It.IsAny<string>(), CancellationToken.None, It.IsAny<Action<GetAllOffersStatus>>()))
                .Callback<OfferTypes?, string, CancellationToken, Action<GetAllOffersStatus>>((type, searchCommand, cancellationToken, statusUpdate) =>
                {
                    statusUpdate(new GetAllOffersStatus()
                    {
                        CurrentOffers = 10,
                        TotalOffers = 999
                    });
                })
                .Returns(Task.FromResult(getAllOffersResult));

            var lastStatus = default(GetAllOffersStatus);
            var request = new TopMakalaarsQuery()
            {
                Location = "Amsterdam",
                WithGarden = true,
                StatusUpdate = status => { lastStatus = status; }
            };

            await _handler.Handle(request, CancellationToken.None);
            Assert.AreEqual(10, lastStatus.CurrentOffers);
            Assert.AreEqual(999, lastStatus.TotalOffers);
        }

        private IEnumerable<Offer> CreateOffers(int makelaarId, string makelaarName, int numberOfOffers)
        {
            for (int ct = 0; ct < numberOfOffers; ct++)
            {
                yield return new Offer()
                {
                    Id = Guid.NewGuid(),
                    MakelaarId = makelaarId,
                    MakelaarNaam = makelaarName
                };
            }
        }
    }
}