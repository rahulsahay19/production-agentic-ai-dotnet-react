using NexaStore.Application.AI;
using System.Collections.Concurrent;

namespace NexaStore.Infrastructure.AI
{
    //Tracks One JEV Execution
    public sealed class JevRunContext(JevOptions options)
    {
        private readonly ConcurrentDictionary<string, byte> _observedTools = new(StringComparer.Ordinal);
        private JevDecisionDto? _decision;

        //Starting a new JEV Run
        public void Begin(JevDecisionDto decision)
        {
            _observedTools.Clear();
            _decision = decision;
        }

        //Track Actual Execution
        public void RecordTool(string toolName) => _observedTools.TryAdd(toolName, 0);

        //Compare the Expected vs Actual
        public JevVerificationDto Verify(string response)
        {
            var decision = _decision ?? throw new InvalidOperationException("A JEV run was not initialized.");
            var evidenceUsed = _observedTools.Keys.Order(StringComparer.Ordinal).ToArray();

            if (!options.Enabled)
            {
                return new JevVerificationDto(true, "not-required", evidenceUsed, []);
            }

            if (!decision.Allowed)
            {
                return new JevVerificationDto(true, "blocked-by-policy", evidenceUsed, []);
            }

            //Check the required tools
            var missing = decision.RequiredTools
                .Where(required => !_observedTools.ContainsKey(required))
                .Select(required => $"tool:{required}")
                .ToList();

            if (string.IsNullOrWhiteSpace(response))
            {
                missing.Add("non-empty-response");
            }

            //Citation is required
            var requiresKnowledge = decision.RequiredTools.Contains("search_knowledge_base", StringComparer.Ordinal);

            var insufficientEvidenceDisclosed =
                response.Contains("insufficient evidence", StringComparison.OrdinalIgnoreCase)
                || response.Contains("indexed documents do not", StringComparison.OrdinalIgnoreCase)
                || response.Contains("documents do not establish", StringComparison.OrdinalIgnoreCase);
            if (requiresKnowledge
                && options.RequireDocumentCitations
                && !insufficientEvidenceDisclosed
                && !ContainsDocumentCitation(response))
            {
                missing.Add("document-citation");
            }

            return missing.Count == 0
                ? new JevVerificationDto(true, "verified", evidenceUsed, [])
                : new JevVerificationDto(false, "evidence-incomplete", evidenceUsed, missing);
        }

        private static bool ContainsDocumentCitation(string response)
        {
            var openBracket = response.IndexOf('[', StringComparison.Ordinal);
            if (openBracket < 0) return false;
            var pageMarker = response.IndexOf(", p.", openBracket, StringComparison.OrdinalIgnoreCase);
            return pageMarker > openBracket && response.IndexOf(']', pageMarker) > pageMarker;
        }
    }    
}
