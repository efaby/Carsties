using System;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming BidPlaced event on search service");
        // Here you would add logic to update the search index with the new bid information.
        if (context.Message.AuctionId == null)
        {
            Console.WriteLine("AuctionId is null in BidPlaced event.");
            return;
        }

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (auction == null)
        {
            Console.WriteLine($"Auction with ID {context.Message.AuctionId} not found.");
            return;
        }

        if (!string.IsNullOrEmpty(context.Message.BidStatus) &&
            context.Message.BidStatus.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}
