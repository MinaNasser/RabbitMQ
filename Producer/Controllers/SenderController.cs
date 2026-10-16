using Microsoft.AspNetCore.Mvc;
using Producer.Services;
using Shared.Messages;

namespace Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SenderController(IRabbitMQService rabbitMQService) : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService = rabbitMQService;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] FirstDemoMessage fistDemo, [FromQuery] string routingKey)
        {
            try
            {

                await _rabbitMQService.PublishMessageAsync(fistDemo, routingKey);
                return Ok("Message sent to RabbitMQ");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending message: {ex.Message}");
            }
        }
    }
}
