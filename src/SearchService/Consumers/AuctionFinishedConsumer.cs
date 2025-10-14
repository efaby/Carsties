using System;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("--> Consuming AuctionFinished event on search service");
        // Here you would add logic to update the search index with the auction finished information.
        if (context.Message.AuctionId == null)
        {
            Console.WriteLine("AuctionId is null in AuctionFinished event.");
            return;
        }

        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        if (auction == null)
        {
            Console.WriteLine($"Auction with ID {context.Message.AuctionId} not found.");
            return;
        }

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount.HasValue ? (int)context.Message.Amount.Value : 0;
        }
        auction.Status = "Finished";
        await auction.SaveAsync();
    }
}
