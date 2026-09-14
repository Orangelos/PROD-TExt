namespace Diplom.Models
{
    public class SemanticComparisonResult
    {
        public string FileName1 { get; set; } = string.Empty;
        public string FileName2 { get; set; } = string.Empty;
        public DateTime UploadedAt1 { get; set; }
        public DateTime UploadedAt2 { get; set; }
        public string Method { get; set; } = string.Empty;   // TF, TFIDF, Nomic, Gemma, BGEM3
        public int VectorSize { get; set; }

        public double CosineSimilarity { get; set; }
        public double ManhattanDistance { get; set; }
        public double EuclideanDistance { get; set; }
        public double JaccardCoefficient { get; set; }
        public double MinkowskiDistance { get; set; }
        public double ChebyshevDistance { get; set; }
        public double JeffreyDivergence { get; set; }
        public double HellingerDistance { get; set; }
        public double BhattacharyyaDistance { get; set; }
        public double PearsonCorrelation { get; set; }
        public double SpearmanCorrelation { get; set; }
    }
}