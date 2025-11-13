namespace PortSafe.Services.AI.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public double ConfidenceScore { get; set; }
        public string? MatchedName { get; set; }
        public string? MatchedCEP { get; set; }
        public int? UnidadeId { get; set; }
        public string? Reason { get; set; }
        public List<SuggestedMatch> Suggestions { get; set; } = new();
    }

    public class SuggestedMatch
    {
        public string? NomeMorador { get; set; }
        public string? CEP { get; set; }
        public string? Endereco { get; set; }
        public int UnidadeId { get; set; }
        public double SimilarityScore { get; set; }
        public string? Reason { get; set; }
    }
}
