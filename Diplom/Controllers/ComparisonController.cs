// Controllers/ComparisonController.cs
using Diplom.Models;
using Diplom.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Controllers
{
    public class ComparisonController : Controller
    {
        private readonly DiplomDbContext _context;
        public ComparisonController(DiplomDbContext context) => _context = context;

        public async Task<IActionResult> StructuralComparison(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("Не указаны идентификаторы файлов.");

            var parts = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return BadRequest("Необходимо выбрать ровно 2 файла.");

            if (!int.TryParse(parts[0], out int id1) || !int.TryParse(parts[1], out int id2))
                return BadRequest("Некорректные идентификаторы.");

            var file1 = await _context.TextFiles.FindAsync(id1);
            var file2 = await _context.TextFiles.FindAsync(id2);
            if (file1 == null || file2 == null)
                return NotFound("Один из файлов не найден.");

            var analysis1 = TextAnalysisService.Analyze(file1.Content, file1.FileName, file1.UploadedAt);
            var analysis2 = TextAnalysisService.Analyze(file2.Content, file2.FileName, file2.UploadedAt);

            var result = new StructuralComparisonResult
            {
                FileName1 = file1.FileName,
                FileName2 = file2.FileName,
                UploadedAt1 = file1.UploadedAt,
                UploadedAt2 = file2.UploadedAt
            };

            // Сырые векторы и таблица сравнения
            var allKeys = analysis1.AllMetrics.Keys.ToList();
            var raw1 = new List<double>();
            var raw2 = new List<double>();

            foreach (var key in allKeys)
            {
                double val1 = analysis1.AllMetrics.ContainsKey(key) ? analysis1.AllMetrics[key] : 0;
                double val2 = analysis2.AllMetrics.ContainsKey(key) ? analysis2.AllMetrics[key] : 0;

                double diffPercent = 0;
                double maxAbs = Math.Max(Math.Abs(val1), Math.Abs(val2));
                if (maxAbs > 0)
                    diffPercent = Math.Abs(val1 - val2) / maxAbs * 100.0;

                string rowClass = "";
                if (diffPercent < 3.0) rowClass = "table-danger";
                else if (diffPercent < 6.0) rowClass = "table-warning";

                result.Metrics.Add(new MetricComparison
                {
                    Name = key,
                    Value1 = val1,
                    Value2 = val2,
                    DiffPercent = diffPercent,
                    RowClass = rowClass
                });

                raw1.Add(val1);
                raw2.Add(val2);
            }

            if (raw1.Count == 0) return View(result);

            // === НАЗНАЧЕНИЕ ВЕСОВ (ваши актуальные) ===
            var weights = new Dictionary<string, double>
            {
                ["Абзацев"] = 0.1,
                ["Слов"] = 0.5,
                ["Букв"] = 0.5,
                ["Предложений"] = 0.6,
                ["Средняя длина абзаца (слов)"] = 0.1,
                ["Средняя длина абзаца (букв)"] = 0.1,
                ["Средняя длина абзаца (предложений)"] = 0.1,
                ["Средняя длина предложения (слов)"] = 1.0,
                ["Средняя длина предложения (букв)"] = 1.0,
                ["Средняя длина предложения (слогов)"] = 1.0,
                ["Средняя длина слова (букв)"] = 1.0,
                ["Средняя длина слова (слогов)"] = 1.0,
                ["Уникальные слова (%)"] = 1.0,
                ["Индекс Guiraud"] = 1.0,
                ["Средняя частота повторения"] = 1.0,
                ["Односложные слова (%)"] = 1.0,
                ["Сложные слова (>3 слогов) (%)"] = 1.0,
                ["Процент слов длиннее 6 букв"] = 0.8,
                ["LIX"] = 0.8,
                ["RIX"] = 0.8,
                ["Энтропия частей речи"] = 1.0,
                ["Индекс Honoré"] = 1.0,
                ["Существительные (%)"] = 1.0,
                ["Глаголы (%)"] = 1.0,
                ["Прилагательные (%)"] = 1.0,
                ["Наречия (%)"] = 0.9,
                ["Местоимения (%)"] = 0.9,
                ["Числительные (%)"] = 0.9,
                ["Предлоги (%)"] = 0.9,
                ["Уникальных аббревиатур"] = 0.6,
                ["Союзы (%)"] = 0.9,
                ["Частицы (%)"] = 0.9,
                ["Междометия (%)"] = 0.7,
                ["Причастия (%)"] = 0.1,
                ["Деепричастия (%)"] = 0.1,
                ["Предикативы (%)"] = 0.8,
                ["Вводные слова (%)"] = 0.8,
                ["Нераспознанные (%)"] = 0.5,
                ["Служебные слова (%)"] = 0.9,
                ["Уникальных союзов"] = 0.7,
                ["Всего союзов"] = 0.5,
                ["Уникальных местоимений"] = 0.7,
                ["Всего местоимений"] = 0.5,
                ["Мета-текстовых маркеров"] = 0.9,
                ["Знаков препинания всего"] = 0.5,
                ["Уникальных знаков препинания"] = 0.1,
                ["Вопросительные предложения (%)"] = 0.3,
                ["Восклицательные предложения (%)"] = 0.1,
                ["Сложные предложения (%)"] = 1.0,
                ["Аббревиатур"] = 0.5,
                ["Предложения с прямой речью (%)"] = 0.9,
                ["Стандартное отклонение длин абзацев (слов)"] = 0.8,
                ["Оборневой"] = 1.0,
                ["Тулдавы"] = 1.0,
                ["Колемана"] = 1.0,
                ["ARI"] = 1.0,
                ["Флеш-Кинкейд (учеб.)"] = 1.0,
                ["Флеш-Кинкейд (худ.)"] = 1.0,
                ["Мацковского"] = 1.0,
                ["Дейл-Челл"] = 1.0,
                ["Туманность Ганнинга"] = 1.0,
                ["Пауэрс-Самнер-Кернл"] = 1.0,
                ["SMOG"] = 1.0,
                ["Уникальных 2-грамм символов"] = 1.0,
                ["Всего 2-грамм символов"] = 1.0,
                ["Уникальных 3-грамм символов"] = 1.0,
                ["Всего 3-грамм символов"] = 1.0
            };

            // Применяем веса и нормируем каждую метрику на максимум из двух файлов
            int n = raw1.Count;
            double[] a = new double[n];
            double[] b = new double[n];
            for (int i = 0; i < n; i++)
            {
                double w = weights.ContainsKey(allKeys[i]) ? weights[allKeys[i]] : 1.0;
                double v1 = raw1[i] * w;
                double v2 = raw2[i] * w;
                double maxVal = Math.Max(Math.Abs(v1), Math.Abs(v2)) + 1e-10;
                a[i] = v1 / maxVal;
                b[i] = v2 / maxVal;
            }

            // Теперь вычисляем все метрики на нормированных векторах
            result.CosineSimilarity = a.Zip(b, (x, y) => x * y).Sum() /
                (Math.Sqrt(a.Sum(x => x * x)) * Math.Sqrt(b.Sum(x => x * x)));

            result.ManhattanDistance = a.Zip(b, (x, y) => Math.Abs(x - y)).Sum();
            result.EuclideanDistance = Math.Sqrt(a.Zip(b, (x, y) => (x - y) * (x - y)).Sum());
            result.MinkowskiDistance = Math.Pow(a.Zip(b, (x, y) => Math.Pow(Math.Abs(x - y), 3)).Sum(), 1.0 / 3.0);
            result.ChebyshevDistance = a.Zip(b, (x, y) => Math.Abs(x - y)).Max();
            result.JaccardCoefficient = GetGeneralizedJaccard(a, b);  // векторы неотрицательны

            // Вероятностные метрики на сглаженных нормированных векторах
            double[] p1 = a.Select(x => Math.Max(x, 1e-10) + 1e-6).ToArray();
            double[] p2 = b.Select(x => Math.Max(x, 1e-10) + 1e-6).ToArray();
            double sum1 = p1.Sum();
            double sum2 = p2.Sum();
            p1 = p1.Select(x => x / sum1).ToArray();
            p2 = p2.Select(x => x / sum2).ToArray();
            result.JeffreyDivergence = KLDivergence(p1, p2) + KLDivergence(p2, p1);
            result.HellingerDistance = GetHellingerDistance(p1, p2);
            result.BhattacharyyaDistance = GetBhattacharyyaDistance(p1, p2);

            result.PearsonCorrelation = GetPearsonCorrelation(a, b);
            result.SpearmanCorrelation = GetSpearmanCorrelation(a, b);

            return View(result);
        }

        // ----- СЕМАНТИЧЕСКОЕ СРАВНЕНИЕ (ВСЕ МЕТОДЫ) -----
        // ----- СЕМАНТИЧЕСКОЕ СРАВНЕНИЕ (ВСЕ МЕТОДЫ С ЗАЩИТОЙ ОТ ОШИБОК) -----
        public async Task<IActionResult> FullSemanticComparison(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("Не указаны идентификаторы файлов.");

            var parts = ids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return BadRequest("Необходимо выбрать ровно 2 файла.");

            if (!int.TryParse(parts[0], out int id1) || !int.TryParse(parts[1], out int id2))
                return BadRequest("Некорректные идентификаторы.");

            var file1 = await _context.TextFiles.FindAsync(id1);
            var file2 = await _context.TextFiles.FindAsync(id2);
            if (file1 == null || file2 == null)
                return NotFound("Один из файлов не найден.");

            var result = new MultipleSemanticComparisonResult
            {
                FileName1 = file1.FileName,
                FileName2 = file2.FileName,
                UploadedAt1 = file1.UploadedAt,
                UploadedAt2 = file2.UploadedAt
            };

            string[] methods = { "TF", "TFIDF", "NOMIC", "BGEM3", "QWEN" };

            var tasks = methods.Select(async method =>
            {
                var single = new SemanticComparisonResult
                {
                    FileName1 = file1.FileName,
                    FileName2 = file2.FileName,
                    UploadedAt1 = file1.UploadedAt,
                    UploadedAt2 = file2.UploadedAt,
                    Method = method,
                    VectorSize = 0
                };

                try
                {
                    Dictionary<string, double> vec1 = null, vec2 = null;
                    double[] arr1 = null, arr2 = null;

                    switch (method)
                    {
                        case "TF":
                            vec1 = TextVectorizationService.GetTfVector(file1.Content);
                            vec2 = TextVectorizationService.GetTfVector(file2.Content);
                            break;
                        case "TFIDF":
                            vec1 = TextVectorizationService.GetTfIdfVector(file1.Content, file2.Content);
                            vec2 = TextVectorizationService.GetTfIdfVector(file2.Content, file1.Content);
                            break;
                        case "NOMIC":
                        case "BGEM3":
                        case "QWEN":
                            var model = method switch
                            {
                                "NOMIC" => "nomic-embed-text-v2-moe",
                                "BGEM3" => "bge-m3",
                                "QWEN" => "qwen3-embedding:0.6b",
                                _ => "nomic-embed-text"
                            };
                            arr1 = await TextVectorizationService.GetOllamaEmbedding(model, file1.Content);
                            arr2 = await TextVectorizationService.GetOllamaEmbedding(model, file2.Content);
                            break;
                        default:
                            single.VectorSize = -1;
                            return single;
                    }

                    if (vec1 != null && vec2 != null)
                    {
                        var allKeys = vec1.Keys.Union(vec2.Keys).ToList();
                        arr1 = allKeys.Select(k => vec1.TryGetValue(k, out var v) ? v : 0).ToArray();
                        arr2 = allKeys.Select(k => vec2.TryGetValue(k, out var v) ? v : 0).ToArray();
                    }

                    if (arr1 == null || arr2 == null)
                    {
                        single.VectorSize = -1;
                        return single;
                    }

                    single.VectorSize = arr1.Length;
                    int n = arr1.Length;
                    double[] a = arr1, b = arr2;

                    // L2‑нормализация
                    double normA = Math.Sqrt(a.Sum(x => x * x));
                    double normB = Math.Sqrt(b.Sum(x => x * x));
                    double[] u1 = new double[n];
                    double[] u2 = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        u1[i] = normA > 1e-10 ? a[i] / normA : 0;
                        u2[i] = normB > 1e-10 ? b[i] / normB : 0;
                    }

                    // Метрики сходства на нормализованных векторах
                    single.CosineSimilarity = u1.Zip(u2, (x, y) => x * y).Sum();
                    single.ManhattanDistance = u1.Zip(u2, (x, y) => Math.Abs(x - y)).Sum();
                    single.EuclideanDistance = Math.Sqrt(u1.Zip(u2, (x, y) => (x - y) * (x - y)).Sum());
                    single.MinkowskiDistance = Math.Pow(u1.Zip(u2, (x, y) => Math.Pow(Math.Abs(x - y), 3)).Sum(), 1.0 / 3.0);
                    single.ChebyshevDistance = u1.Zip(u2, (x, y) => Math.Abs(x - y)).Max();

                    // Обобщённый Жаккард на модулях компонент
                    double sumMinAbs = 0, sumMaxAbs = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double absAi = Math.Abs(u1[i]);
                        double absBi = Math.Abs(u2[i]);
                        sumMinAbs += Math.Min(absAi, absBi);
                        sumMaxAbs += Math.Max(absAi, absBi);
                    }
                    single.JaccardCoefficient = sumMaxAbs > 0 ? sumMinAbs / sumMaxAbs : 1.0;

                    single.PearsonCorrelation = GetPearsonCorrelation(u1, u2);
                    single.SpearmanCorrelation = GetSpearmanCorrelation(u1, u2);

                    // Вероятностные метрики скрываем (всегда 0)
                    single.JeffreyDivergence = 0;
                    single.HellingerDistance = 0;
                    single.BhattacharyyaDistance = 0;
                }
                catch
                {
                    single.VectorSize = -1;
                }

                return single;
            });

            result.MethodResults = (await Task.WhenAll(tasks)).Where(r => r != null).ToList();
            return View("MultipleSemanticComparison", result);
        }

        // ----- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ -----
        private (double[] a, double[] b) RankTransform(List<double> v1, List<double> v2)
        {
            int n = v1.Count;
            double[] a = new double[n];
            double[] b = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (v1[i] < v2[i])
                {
                    a[i] = 1.0;
                    b[i] = 2.0;
                }
                else if (v1[i] > v2[i])
                {
                    a[i] = 2.0;
                    b[i] = 1.0;
                }
                else
                {
                    a[i] = 1.5;
                    b[i] = 1.5;
                }
            }
            return (a, b);
        }

        private double GetCosineSimilarity(double[] a, double[] b)
        {
            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            return (normA > 0 && normB > 0) ? dot / (Math.Sqrt(normA) * Math.Sqrt(normB)) : 0;
        }

        private double GetGeneralizedJaccard(double[] a, double[] b)
        {
            double sumMin = 0, sumMax = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sumMin += Math.Min(a[i], b[i]);
                sumMax += Math.Max(a[i], b[i]);
            }
            return sumMax > 0 ? sumMin / sumMax : 1.0;
        }

        private double GetMinkowskiDistance(double[] a, double[] b, double p)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(Math.Abs(a[i] - b[i]), p);
            return Math.Pow(sum, 1.0 / p);
        }

        private double GetHellingerDistance(double[] p, double[] q)
        {
            double sum = 0;
            for (int i = 0; i < p.Length; i++)
                sum += Math.Pow(Math.Sqrt(p[i]) - Math.Sqrt(q[i]), 2);
            return Math.Sqrt(sum) / Math.Sqrt(2.0);
        }

        private double GetBhattacharyyaDistance(double[] p, double[] q)
        {
            double bc = 0;
            for (int i = 0; i < p.Length; i++)
                bc += Math.Sqrt(p[i] * q[i]);
            return -Math.Log(bc + 1e-15);
        }

        private double GetPearsonCorrelation(double[] a, double[] b)
        {
            double meanA = a.Average(), meanB = b.Average();
            double cov = 0, varA = 0, varB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                var diffA = a[i] - meanA;
                var diffB = b[i] - meanB;
                cov += diffA * diffB;
                varA += diffA * diffA;
                varB += diffB * diffB;
            }
            return (varA > 0 && varB > 0) ? cov / Math.Sqrt(varA * varB) : 0;
        }

        private double GetSpearmanCorrelation(double[] a, double[] b)
        {
            var ranksA = GetRanks(a);
            var ranksB = GetRanks(b);
            double d2 = 0;
            int n = a.Length;
            for (int i = 0; i < n; i++)
                d2 += Math.Pow(ranksA[i] - ranksB[i], 2);
            return 1.0 - (6.0 * d2) / (n * (n * n - 1));
        }

        private double[] Normalize(double[] vec)
        {
            double sum = vec.Sum() + 1e-10 * vec.Length;
            return vec.Select(v => (v + 1e-10) / sum).ToArray();
        }

        private double KLDivergence(double[] p, double[] q)
        {
            double kl = 0;
            for (int i = 0; i < p.Length; i++)
                if (p[i] > 0 && q[i] > 0)
                    kl += p[i] * Math.Log(p[i] / q[i]);
            return kl;
        }

        private double[] GetRanks(double[] arr)
        {
            int n = arr.Length;
            var indexed = arr.Select((val, idx) => new { Val = val, Idx = idx })
                            .OrderBy(x => x.Val)
                            .ToList();
            double[] ranks = new double[n];
            int i = 0;
            while (i < n)
            {
                int j = i;
                while (j < n && indexed[j].Val == indexed[i].Val) j++;
                double avgRank = (i + j + 1) / 2.0;   // ранги от 1 до n
                for (int k = i; k < j; k++)
                    ranks[indexed[k].Idx] = avgRank;
                i = j;
            }
            return ranks;
        }
    }
}