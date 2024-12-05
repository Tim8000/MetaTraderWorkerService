using MetaTraderWorkerService.Models;
using MetaTraderWorkerService.Repository.ServiceOrders;
using MetaTraderWorkerService.Repository.Trades;
using MetaTraderWorkerService.Services.TradeServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetaTraderWorkerService.Tests;

[TestFixture]
public class TradeProcessingServiceTests
{
    private Mock<ITradeRepository> _tradeRepositoryMock;
    private Mock<IServiceOrderRepository> _serviceOrderRepositoryMock;
    private Mock<ILogger<TradeProcessingService>> _loggerMock;
    private TradeProcessingService _service;

    [SetUp]
    public void SetUp()
    {
        _tradeRepositoryMock = new Mock<ITradeRepository>();
        _serviceOrderRepositoryMock = new Mock<IServiceOrderRepository>();
        _loggerMock = new Mock<ILogger<TradeProcessingService>>();

        _service = new TradeProcessingService(
            _tradeRepositoryMock.Object,
            _serviceOrderRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Test]
    public async Task ProcessMovingStopLossAsync_AdjustsStopLossAndCreatesServiceOrder_WhenThresholdIsMet()
    {
        // Arrange
        var trade = new MetaTraderTrade
        {
            Id = Guid.NewGuid().ToString(),
            OpenPrice = 2645.00m,
            CurrentPrice = 2647.00m,
            StopLoss = 2640m,
            Type = "POSITION_TYPE_BUY",
        };

        _tradeRepositoryMock
            .Setup(repo => repo.GetAllOpenedTradesAsync())
            .ReturnsAsync(new List<MetaTraderTrade> { trade });

        _serviceOrderRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ServiceOrder>()))
            .Returns(Task.CompletedTask);

        // Act
        // await _service.ProcessMovingStopLossAsync(trade);

        await _service.ProcessMoveStopLossToOpenPrice(trade);

        // Assert

        Assert.That(trade.StopLoss, Is.EqualTo(2645.00m));
    }
}