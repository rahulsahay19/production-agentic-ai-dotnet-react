using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.AI;
using NexaStore.Infrastructure.AI;
using System.Text.Json;

namespace NexaStore.Api.Controllers
{
    [ApiController]
    [Route("api/assistant")]
    [Tags("Nexa AI")]

    public sealed class AssistantController(INexaAssistantService assistant,
        AiOptions options,
        ILogger<AssistantController> logger) : ControllerBase
    {
        [HttpGet("status")]
        [ProducesResponseType<AssistantStatusDto>(StatusCodes.Status200OK)]
        public ActionResult<AssistantStatusDto> GetStatus() => Ok(new AssistantStatusDto(
            options.Enabled,
            options.Provider,
            options.Model,
            "Local LLM Conversation"));

        [HttpPost("chat/stream")]
        [Produces("text/event-stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task Stream([FromBody] AssistantChatRequest request, CancellationToken cancellationToken)
        {
            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache, no-transform";
            Response.Headers.Append("X-Accel-Buffering", "no");
            try
            {
                await foreach(var chunk in assistant.StreamResponseAsync(request.Messages, cancellationToken))
                {
                    await WriteEventAsync("delta", new { content = chunk }, cancellationToken);
                }
                await WriteEventAsync("done", new { }, cancellationToken);
            }
            catch(OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Nexa AI streaming request was cancelled by the client.");
            }
            catch(Exception exception)
            {
                logger.LogError(exception, "Local AI provider failed while streaming a response");
                await WriteEventAsync("error", new { message = "Nexa AI couldn't reach local model. Confirm Ollama and configured model are running;" },
                    cancellationToken);
            }
        }

        //Server Sent Event aka SSE
        //Events -> delta, done, error
        //event: delta
        //data: {"content":"hello"}
        //event: done
        //data: {}
        private async Task WriteEventAsync(string eventName, object payload, CancellationToken cancellationToken)
        {
            await Response.WriteAsync($"event: {eventName}\n", cancellationToken);
            await Response.WriteAsync($"data: {JsonSerializer.Serialize(payload)}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}