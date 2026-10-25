using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Knowledge;

namespace NexaStore.Api.Controllers
{
    [ApiController]
    [Route("api/knowledge")]
    [Tags("Knowledge Base")]
    public sealed class KnowledgeController(IKnowledgeService knowledge) : ControllerBase
    {
        [HttpGet("status")]
        public async Task<ActionResult<KnowledgeStatusDto>> GetSatus(CancellationToken cancellationToken) =>
            Ok(await knowledge.GetStatusAsync(cancellationToken));
        
        [HttpPost("ingest")]
        public async Task<ActionResult<KnowledgeIngestionResult>> Ingest(CancellationToken cancellationToken) =>
            Ok(await knowledge.IngestAsync(cancellationToken));

        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyCollection<KnowledgeSearchResult>>> Search(
            [FromQuery] string query,
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                ModelState.AddModelError(nameof(query), "A search query is required.");
                return ValidationProblem(ModelState);
            }
            return Ok(await knowledge.SearchAsync(query, limit, cancellationToken)); ;
        }
    }
}
