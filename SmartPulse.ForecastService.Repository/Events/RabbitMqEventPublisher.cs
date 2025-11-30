using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartPulse.ForecastService.Repository.Events
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly RabbitMqOptions _options;
        private readonly ConnectionFactory _factory;

        public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
            _factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password
            };
        }

        public async Task PublishPositionChangedAsync(PositionChangedEvent evt)
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            var json = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
    }
}
