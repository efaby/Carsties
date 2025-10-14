using System;
using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionBusTest : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private ITestHarness _harness;

    public AuctionBusTest(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _harness = _factory.Services.GetTestHarness();
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
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        // Arrange
        var auction = getAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.getBearerForUser("bob"));
        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", auction);
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _harness.Published.Any<AuctionCreated>());
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
