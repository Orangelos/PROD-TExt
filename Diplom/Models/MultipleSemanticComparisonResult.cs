namespace Diplom.Models
{
    public class MultipleSemanticComparisonResult
    {
        public string FileName1 { get; set; } = string.Empty;
        public string FileName2 { get; set; } = string.Empty;
        public DateTime UploadedAt1 { get; set; }
        public DateTime UploadedAt2 { get; set; }
        public List<SemanticComparisonResult> MethodResults { get; set; } = new();
    }
}