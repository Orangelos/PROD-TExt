using System.Collections.Generic;

namespace Diplom.Models
{
    // Класс для одной строки сравнения метрик (оставляем)
    public class MetricComparison
    {
        public string Name { get; set; } = string.Empty;
        public double Value1 { get; set; }
        public double Value2 { get; set; }
        public double DiffPercent { get; set; }
        public string RowClass { get; set; } = string.Empty; // table-danger / table-warning
    }

    // Результат структурного сравнения двух текстов
    public class StructuralComparisonResult
    {
        public string FileName1 { get; set; } = string.Empty;
        public string FileName2 { get; set; } = string.Empty;
        public DateTime UploadedAt1 { get; set; }
        public DateTime UploadedAt2 { get; set; }

        public List<MetricComparison> Metrics { get; set; } = new();

        // Базовые метрики
        public double CosineSimilarity { get; set; }
        public double ManhattanDistance { get; set; }

        // Новые метрики
        public double EuclideanDistance { get; set; }
        public double JaccardCoefficient { get; set; }         // обобщённый для неотрицательных векторов
        public double MinkowskiDistance { get; set; }           // p=3 (можно изменить)
        public double ChebyshevDistance { get; set; }
        // Махаланобис не реализован – недостаточно данных
        public double JeffreyDivergence { get; set; }          // симметричная KL
        public double HellingerDistance { get; set; }
        public double BhattacharyyaDistance { get; set; }
        public double PearsonCorrelation { get; set; }
        public double SpearmanCorrelation { get; set; }
    }
}