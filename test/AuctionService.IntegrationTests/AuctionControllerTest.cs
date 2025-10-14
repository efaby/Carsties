using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTest : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    private const string GT_ID = "bbab4d5a-8565-48b1-9450-5ac2a5c4a654";

    public AuctionControllerTest(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<List<Auction>>("/api/auctions");

        // Assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidIdShouldReturnOneAuction()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<Auction>($"/api/auctions/{GT_ID}");
        // Assert
        Assert.NotNull(response);
        Assert.Equal(GT_ID, response.Id.ToString());
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidIdShouldReturnNotFound()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync($"/api/auctions/{Guid.NewGuid()}");
        // Assert

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturn404()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync($"/api/auctions/not-a-guid");
        // Assert

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTest(db);
        return Task.CompletedTask;

    }

    [Fact]
    public async Task CreateAuction_WithNotAuth_ShouldReturn401()
    {
        // Arrange
        var auction = new CreateAuctionDto { Make = "test" };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturn201()
    {
        // Arrange
        var auction = getAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.getBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturn400()
    {
        // arrange
        var auction = getAuctionForCreate();
        auction.Make = null; // Invalid Make
        _httpClient.SetFakeJwtBearerToken(AuthHelper.getBearerForUser("bob"));
        // act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturn200()
    {
        // arrange
        var updateAuction = new UpdateAuctionDto { Make = "Updated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.getBearerForUser("bob"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{GT_ID}", updateAuction);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturn403()
    {
        // arrange 
        var updateAuction = new UpdateAuctionDto { Make = "Updated" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.getBearerForUser("notbob"));
        // act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{GT_ID}", updateAuction);

        // assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private CreateAuctionDto getAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "testImageUrl",
            Year = 10,
            Color = "test",
            Mileage = 10,
            ReservePrice = 10
        };
    }


}
