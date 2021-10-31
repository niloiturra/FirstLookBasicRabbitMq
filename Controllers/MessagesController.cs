using System.Text;
using System.Text.Json;
using FirstIntegrationRabbitMQ.Models;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace FirstIntegrationRabbitMQ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ConnectionFactory _factory;
        private const string QUEUE_NAME = "messages";
        
        public MessagesController()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
        }

        [HttpPost]
        public IActionResult PostMessage([FromBody] MessageInputModel message)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                        QUEUE_NAME,
                        false,
                        false,
                        false,
                        null
                    );

                    var stringifiesMessage = JsonSerializer.Serialize(message);
                    var bytesMessage = Encoding.UTF8.GetBytes(stringifiesMessage);

                    channel.BasicPublish("", QUEUE_NAME, null, bytesMessage);
                }
            }
            
            return Accepted();
        }
    }
}