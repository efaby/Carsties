using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", true));
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMqUsername", "guest"));
            h.Password(builder.Configuration.GetValue("RabbitMqPassword", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSignalR();


var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
