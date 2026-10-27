using Microsoft.AspNetCore.Mvc;
using NexaStore.Infrastructure.Mcp;

namespace NexaStore.Api.Controllers
{
    [ApiController]
    [Route("api/mcp")]
    [Tags("Model Context Protocol")]
    public class McpController(McpOptions options): ControllerBase
    {
        [HttpGet("status")]
        public ActionResult<object> GetStatus() => Ok(
            new
            {
                options.Enabled,
                options.UseForAssistant,
                EndPoint = "/mcp",
                Transport = "Streamable HTTP",
                Protocol = "MCP",
                Tools = options.AllowedTools
            });
    }
}
