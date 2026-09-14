using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Diplom.Services
{
    public static class TextVectorizationService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434"),
            Timeout = TimeSpan.FromMinutes(3)
        };

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "и", "в", "во", "не", "что", "он", "на", "я", "с", "со", "как", "а", "то", "все", "она", "так", "но",
            "да", "ты", "к", "у", "же", "вы", "за", "бы", "по", "только", "ее", "мне", "было", "вот", "от", "меня",
            "еще", "нет", "о", "из", "ему", "теперь", "когда", "даже", "ну", "вдруг", "ли", "если", "уже", "или",
            "ни", "быть", "был", "него", "до", "вас", "нибудь", "опять", "уж", "вам", "ведь", "там", "потом",
            "себя", "ничего", "ей", "может", "они", "тут", "где", "есть", "надо", "ней", "для", "мы", "тебя",
            "их", "чем", "была", "сам", "чтоб", "без", "будто", "чего", "раз", "тоже", "себе", "под", "будет",
            "ж", "тогда", "кто", "этот", "того", "потому", "этого", "какой", "совсем", "ним", "здесь", "этом",
            "один", "почти", "мой", "тем", "чтобы", "нее", "сейчас", "были", "куда", "зачем", "всех", "никогда",
            "можно", "при", "наконец", "два", "об", "другой", "хоть", "после", "над", "больше", "тот", "через",
            "эти", "нас", "про", "всего", "них", "какая", "много", "разве", "три", "эту", "моя", "впрочем",
            "хорошо", "свою", "этой", "перед", "иногда", "лучше", "чуть", "том", "нельзя", "такой", "им",
            "более", "всегда", "конечно", "всю", "между"
        };

        private static readonly ConcurrentDictionary<string, double[]> EmbeddingCache = new();

        public static Dictionary<string, double> GetTfVector(string text)
        {
            var words = Tokenize(text);
            var freq = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            int total = words.Count;
            foreach (var word in words)
            {
                if (!freq.ContainsKey(word))
                    freq[word] = 0;
                freq[word] += 1.0 / total;
            }
            return freq;
        }

        public static Dictionary<string, double> GetTfIdfVector(string text, string otherText)
        {
            var words1 = Tokenize(text);
            var words2 = Tokenize(otherText);
            var allTerms = new HashSet<string>(words1.Concat(words2), StringComparer.OrdinalIgnoreCase);
            double totalWords1 = words1.Count;

            var tf1 = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var w in words1)
            {
                tf1[w] = tf1.TryGetValue(w, out var val) ? val + 1.0 / totalWords1 : 1.0 / totalWords1;
            }

            var idf = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            int corpusSize = 2;
            foreach (var term in allTerms)
            {
                int docCount = (words1.Contains(term) ? 1 : 0) + (words2.Contains(term) ? 1 : 0);
                idf[term] = Math.Log((double)corpusSize / (1 + docCount)) + 1;
            }

            var tfidf = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var term in allTerms)
            {
                double tf = tf1.TryGetValue(term, out var t) ? t : 0;
                tfidf[term] = tf * idf[term];
            }
            return tfidf;
        }

        public static async Task<double[]> GetOllamaEmbedding(string model, string text)
        {
            string key = $"{model}|{text.GetHashCode()}";
            if (EmbeddingCache.TryGetValue(key, out var cached))
                return cached;

            var payload = new { model, input = text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/api/embed", content);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var embedding = doc.RootElement.GetProperty("embeddings")[0]
                                   .EnumerateArray()
                                   .Select(v => v.GetDouble())
                                   .ToArray();

                EmbeddingCache[key] = embedding;
                return embedding;
            }
            catch (TaskCanceledException)
            {
                throw new Exception($"Не удалось получить эмбеддинг от Ollama для модели '{model}'. " +
                                    "Проверьте, запущен ли сервер Ollama и скачана ли модель.");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка подключения к Ollama: {ex.Message}. " +
                                    "Убедитесь, что Ollama запущена на http://localhost:11434.");
            }
        }

        private static List<string> Tokenize(string text)
        {
            return Regex.Matches(text, @"\w+")
                        .Cast<Match>()
                        .Select(m => m.Value.ToLower())
                        .Where(w => !StopWords.Contains(w))
                        .ToList();
        }
    }
}