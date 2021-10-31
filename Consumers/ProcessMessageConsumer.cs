using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FirstIntegrationRabbitMQ.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FirstIntegrationRabbitMQ.Consumers
{
    public class ProcessMessageConsumer : BackgroundService
    {
        private readonly RabbitMqConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageInputModel> _logger;

        public ProcessMessageConsumer(IOptions<RabbitMqConfiguration> option, IServiceProvider serviceProvider, ILogger<MessageInputModel> logger)
        {
            _configuration = option.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                _configuration.Queue,
                false,
                false,
                false,
                null
            );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                
                _logger.LogInformation(contentString);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            
            _channel.BasicConsume(_configuration.Queue, false, consumer);

            return Task.CompletedTask;
        }
    }
}