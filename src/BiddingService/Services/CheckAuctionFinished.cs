using System;
using System.Globalization;
using BiddingService.Models;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private ILogger<CheckAuctionFinished> _logger;
    private IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _services = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CheckAuctionFinished Service is starting.");

        stoppingToken.Register(() => _logger.LogInformation("===> Auction check is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);
            await Task.Delay(5000, stoppingToken);
        }

    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        var finishedAuctions = await DB.Find<Auction>()
            .Match(a => a.AuctionEnd < DateTime.UtcNow)
            .Match(a => !a.Finished)
            .ExecuteAsync();

        if (finishedAuctions.Count == 0) return;
        _logger.LogInformation("===> Found {Count} auctions that have completed.", finishedAuctions.Count);

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            var winningBid = await DB.Find<Bid>()
                .Match(b => b.AuctionId == auction.ID)
                .Match(b => b.BidStatus == BidStatus.Accepted)
                .Sort(b => b.Descending(s => s.Amount))
                .ExecuteFirstAsync(stoppingToken);
            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = auction.Seller,
            }, stoppingToken);
        }

    }
}
