using System;
using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Models;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using BiddingService.Services;

namespace BiddingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BidsController : Controller
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publicEndpoint;
    private readonly GrpcAuctionClient _grpcClient;

    public BidsController(IMapper mapper, IPublishEndpoint publicEndpoint,
        GrpcAuctionClient grpcClient)
    {
        _mapper = mapper;
        _publicEndpoint = publicEndpoint;
        _grpcClient = grpcClient;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            auction = _grpcClient.GetAuction(auctionId);

            if (auction == null) return BadRequest("Cannot accept bids on this auction at this time");
        }
        if (auction.Seller == User?.Identity?.Name)
        {
            return BadRequest("You cannot bid on your own auction");
        }
        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = User?.Identity?.Name,
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highBid = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(b => b.Amount))
            .ExecuteFirstAsync();

            if (highBid != null && highBid.Amount < amount || highBid == null)
            {
                bid.BidStatus = amount > auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }

            if (highBid != null && bid.Amount <= highBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }
        await DB.SaveAsync(bid);

        await _publicEndpoint.Publish(_mapper.Map<BidPlaced>(bid));
        return Ok(_mapper.Map<BidDto>(bid));
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(b => b.Amount))
            .ExecuteAsync();

        return bids.Select(_mapper.Map<BidDto>).ToList();
    }
}
