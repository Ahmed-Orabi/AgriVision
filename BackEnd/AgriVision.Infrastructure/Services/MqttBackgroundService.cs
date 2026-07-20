using AgriVision.Application.Interfaces;
using Azure.Identity;
using HiveMQtt.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Infrastructure.Services
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _config;
        public MqttBackgroundService(IServiceScopeFactory serviceScopeFactory, IConfiguration config)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var broker = _config["MQTT:Broker"] ?? throw new InvalidOperationException("MQTT broker is not configured.");
            var username = _config["MQTT:Username"] ?? throw new InvalidOperationException("MQTT username is not configured.");
            var password = _config["MQTT:Password"] ?? throw new InvalidOperationException("MQTT password is not configured.");
            var topic = _config["MQTT:Topics:SoilReadings"] ?? throw new InvalidOperationException("MQTT topic for soil readings is not configured.");

            SecureString ToSecureString(string plain)
            {
                var ss = new SecureString();
                foreach (var c in plain)
                {
                    ss.AppendChar(c);
                }
                ss.MakeReadOnly();
                return ss;
            }

            var passwordSecure = ToSecureString(password);

            var options = new HiveMQClientOptionsBuilder()
                                .WithBroker(broker)
                                .WithPort(8883)
                                .WithUseTls(true)
                                .WithUserName(username)
                                .WithPassword(passwordSecure)
                                .Build();
            var client = new HiveMQClient(options);
            client.AfterConnect += async (sender, eventArgs) =>
            {
                Console.WriteLine($"Connected with result code: {eventArgs.ConnectResult.ReasonCode.ToString()}");
                // Subscribe to topics after successful connection
                var subscribeResult = await client.SubscribeAsync(topic).ConfigureAwait(false);

                // Print subscriptions: if available, join their string representations
                if (subscribeResult?.Subscriptions != null)
                {
                    Console.WriteLine($"Subscribed to {subscribeResult.Subscriptions.Count()} topics");
                }
                else
                {
                    Console.WriteLine("subscribeResult.Subscriptions = null");
                }
            };
            client.OnMessageReceived += async (sender, eventArgs) =>
            {
                // Take action on message received with:
                // sender as HiveMQClient and eventArgs.ApplicationMessage
                var topic = eventArgs.PublishMessage.Topic;
                var payload = eventArgs.PublishMessage.Payload;
                Console.WriteLine($"Received message on topic: {topic}");                
                using var scope = _serviceScopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMqttMessageHandler>();
                if (topic != null && payload != null)
                    await handler.HandleMessageAsync(topic, payload);
                else
                    Console.WriteLine("topic or payload is null, cannot handle message.");
            };
            var connectResult = await client.ConnectAsync().ConfigureAwait(false);
            Console.WriteLine($"connectresult reasoncode is {connectResult.ReasonCode}");
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("MQTT service stopping...");
            }
            finally
            {
                await client.DisconnectAsync();

                Console.WriteLine("Disconnected cleanly.");
            }
        }
    }
}
