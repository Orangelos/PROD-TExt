using Diplom.Models;
using System.Text.RegularExpressions;
using Nestor;

namespace Diplom.Services
{
    public static class TextAnalysisService
    {
        private static readonly NestorMorph _morph = new NestorMorph();
        private static readonly HashSet<string> MetaMarkers = new()
        {
            "таким образом", "следовательно", "итак", "во-первых", "во-вторых",
            "в-третьих", "наконец", "кроме того", "более того", "вместе с тем",
            "в то же время", "например", "в частности", "однако", "тем не менее",
            "в связи с", "в результате", "в заключение", "поэтому", "значит",
            "в общем", "в целом", "с одной стороны", "с другой стороны"
        };

        public static StructuralAnalysisResult Analyze(string content, string fileName, DateTime uploadedAt)
        {
            var result = new StructuralAnalysisResult
            {
                FileName = fileName,
                UploadedAt = uploadedAt
            };

            if (string.IsNullOrWhiteSpace(content))
                return result;

            // --- Абзацы, слова, буквы, предложения ---
            var paragraphs = Regex.Split(content, @"\r?\n\s*\r?\n")
                                 .Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            result.ParagraphCount = paragraphs.Length;

            var cleanText = Regex.Replace(content, @"[^\w\s]", " ");
            var words = Regex.Matches(cleanText, @"\w+")
                            .Cast<Match>().Select(m => m.Value.ToLower()).ToArray();
            result.WordCount = words.Length;

            var letters = Regex.Matches(content, @"[a-zA-Zа-яА-ЯёЁ]");
            result.LetterCount = letters.Count;

            var sentences = Regex.Split(content, @"(?<=[.!?;…])\s+")
                                .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            result.SentenceCount = sentences.Length > 0 ? sentences.Length : 1;

            // --- Средние длины ---
            result.AvgParagraphLengthWords = result.ParagraphCount > 0 ? (double)result.WordCount / result.ParagraphCount : 0;
            result.AvgParagraphLengthLetters = result.ParagraphCount > 0 ? (double)result.LetterCount / result.ParagraphCount : 0;
            result.AvgParagraphLengthSentences = result.ParagraphCount > 0 ? (double)result.SentenceCount / result.ParagraphCount : 0;
            result.AvgSentenceLengthWords = result.SentenceCount > 0 ? (double)result.WordCount / result.SentenceCount : 0;
            result.AvgSentenceLengthLetters = result.SentenceCount > 0 ? (double)result.LetterCount / result.SentenceCount : 0;
            result.AvgWordLengthLetters = result.WordCount > 0 ? (double)result.LetterCount / result.WordCount : 0;

            // --- Слоги и сложность слов ---
            int totalSyllables = 0, singleSyllableCount = 0, complexWordCount = 0;
            foreach (var word in words)
            {
                int syllables = CountSyllables(word);
                totalSyllables += syllables;
                if (syllables == 1) singleSyllableCount++;
                if (syllables > 3) complexWordCount++;
            }
            result.TotalSyllables = totalSyllables;
            result.AvgWordLengthSyllables = result.WordCount > 0 ? (double)totalSyllables / result.WordCount : 0;
            result.PercentSingleSyllableWords = result.WordCount > 0 ? 100.0 * singleSyllableCount / result.WordCount : 0;
            result.PercentComplexWords = result.WordCount > 0 ? 100.0 * complexWordCount / result.WordCount : 0;

            // --- Лексическое разнообразие ---
            var uniqueWordsCount = words.Distinct().Count();
            result.PercentUniqueWords = result.WordCount > 0 ? 100.0 * uniqueWordsCount / result.WordCount : 0;
            result.UniqueWordCount = uniqueWordsCount;
            result.GuiraudIndex = result.WordCount > 0 ? uniqueWordsCount / Math.Sqrt(result.WordCount) : 0;
            result.AvgWordFrequency = uniqueWordsCount > 0 ? (double)result.WordCount / uniqueWordsCount : 0;

            // --- Части речи и служебные слова ---
            AnalyzePartsOfSpeech(content, result);
            result.PercentNotionalWords = result.PercentNouns + result.PercentVerbs
    + result.PercentAdjectives + result.PercentAdverbs + result.PercentNumerals;
            // --- Разнообразие союзов и местоимений ---
            AnalyzeFunctionWordsDiversity(content, result);

            // --- Мета-текстовые маркеры ---
            result.MetaTextMarkersCount = CountMetaMarkers(content);

            // --- Знаки препинания ---
            CountPunctuation(content, result);
            result.PunctuationPer100Words = result.WordCount > 0
    ? result.TotalPunctuationMarks * 100.0 / result.WordCount
    : 0;
            // --- Сложность предложений ---
            result.PercentComplexSentences = CalculateComplexSentences(content);
            // Средняя длина предложения в слогах
            result.AvgSentenceLengthSyllables = result.SentenceCount > 0 ? (double)totalSyllables / result.SentenceCount : 0;

            // Процент длинных слов (>6 букв)
            int longWordCount = words.Count(w => w.Length > 6);
            result.PercentLongWords = result.WordCount > 0 ? 100.0 * longWordCount / result.WordCount : 0;

            // LIX и RIX
            result.LIX = result.AvgSentenceLengthWords + result.PercentLongWords;
            result.RIX = result.PercentLongWords;

            // Энтропия частей речи
            double ent = 0.0;
            var parts = new[] {
    result.PercentNouns, result.PercentVerbs, result.PercentAdjectives,
    result.PercentAdverbs, result.PercentPronouns, result.PercentNumerals,
    result.PercentPrepositions, result.PercentConjunctions, result.PercentParticles,
    result.PercentInterjections, result.PercentParticiples, result.PercentAdverbialParticiples,
    result.PercentPredicatives, result.PercentParentheses, result.PercentUnknown
};
            foreach (var p in parts)
            {
                double prob = p / 100.0;
                if (prob > 0)
                    ent -= prob * Math.Log(prob, 2);
            }
            result.PoSEntropy = ent;

            var abbreviationMatches = Regex.Matches(content, @"\b[А-ЯЁA-Z]{2,}\b");
            result.AbbreviationCount = abbreviationMatches.Count;
            result.AbbreviationList = abbreviationMatches.Cast<Match>()
                                                .Select(m => m.Value)
                                                .Distinct()
                                                .ToList();

            // Предложения с прямой речью (содержат « или »)
            int directSpeechCount = sentences.Count(s => s.Contains('«') || s.Contains('»'));
            result.PercentDirectSpeechSentences = result.SentenceCount > 0 ? 100.0 * directSpeechCount / result.SentenceCount : 0;

            // Индекс Honoré
            var wordFreq = words.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
            int hapaxLegomena = wordFreq.Count(kvp => kvp.Value == 1);
            int V = wordFreq.Count;
            int N = words.Length;
            if (V > 0 && hapaxLegomena < V) // формула требует V1 < V
                result.HonoreIndex = 100.0 * Math.Log(N) / (1.0 - (double)hapaxLegomena / V);
            else
                result.HonoreIndex = 0;

            // Стандартное отклонение длин абзацев в словах
            var paragraphWordCounts = paragraphs.Select(p => Regex.Matches(p, @"\w+").Count).ToArray();
            if (paragraphWordCounts.Length > 1)
            {
                double avg = paragraphWordCounts.Average();
                double sumSq = paragraphWordCounts.Sum(c => (c - avg) * (c - avg));
                result.StdDevParagraphLengthWords = Math.Sqrt(sumSq / (paragraphWordCounts.Length - 1)); // выборочное ст.откл.
            }
            else
                result.StdDevParagraphLengthWords = 0;

            // Символьные N-граммы
            var chars = content.ToCharArray(); // можно без пробелов: content.Where(c => !char.IsWhiteSpace(c))
            var bigrams = Enumerable.Range(0, chars.Length - 1).Select(i => new string(new[] { chars[i], chars[i + 1] }));
            var trigrams = Enumerable.Range(0, chars.Length - 2).Select(i => new string(new[] { chars[i], chars[i + 1], chars[i + 2] }));

            result.TotalCharBigrams = bigrams.Count();
            result.UniqueCharBigrams = bigrams.Distinct().Count();
            result.TotalCharTrigrams = trigrams.Count();
            result.UniqueCharTrigrams = trigrams.Distinct().Count();
            // --- Индексы удобочитаемости ---
            double asl = result.AvgSentenceLengthWords;
            double asw = result.AvgWordLengthSyllables;
            double percentComplex = result.PercentComplexWords;
            double letterPer100 = result.AvgWordLengthLetters * 100;
            double sentPer100 = result.SentenceCount > 0 ? 100.0 * result.SentenceCount / result.WordCount : 0;

            result.ReadabilityOborneva = 206.835 - 1.3 * asl - 60.1 * asw;
            result.ReadabilityTuldava = asw * Math.Log10(asl > 0 ? asl : 1);
            result.ReadabilityColeman = 0.0588 * letterPer100 - 0.296 * sentPer100 - 15.8;
            result.ReadabilityARI = 4.71 * (result.LetterCount / (double)result.WordCount) + 0.5 * asl - 21.43;
            result.ReadabilityFleschKincaidEducational = 208.7 - 2.6 * asl - 39.2 * asw;
            result.ReadabilityFleschKincaidFiction = 206.835 - 1.3 * asl - 60.1 * asw;
            result.ReadabilityMatskovsky = 0.62 * asl + 0.123 * percentComplex + 0.051;
            result.ReadabilityDaleChall = 0.1579 * percentComplex + 0.059 * asl;
            result.ReadabilityGunningFog = 0.4 * (0.78 * asl + 100 * percentComplex / 100.0);
            double syllablesPer100 = result.AvgWordLengthSyllables * 100;
            result.ReadabilityPowersSumnerKearl = 0.0778 * asl + 0.0455 * syllablesPer100 - 2.2029;
            double complexWordForSmog = complexWordCount;
            result.ReadabilitySMOG = 1.043 * complexWordForSmog * (30.0 / Math.Max(1, result.SentenceCount)) + 3.1291;

            return result;
        }

        private static void AnalyzePartsOfSpeech(string content, StructuralAnalysisResult result)
        {
            var words = Regex.Matches(content, @"\w+").Cast<Match>().Select(m => m.Value.ToLower());
            int total = 0;
            int n = 0, v = 0, a = 0, adv = 0, pro = 0, num = 0, prep = 0, conj = 0, part = 0, interj = 0, partic = 0, adverbPart = 0;
            int predic = 0, parenthes = 0, unknown = 0;

            // Эвристики для причастий и деепричастий 
            var participlePattern = new Regex(
                @"\w+(?:ущ|ющ|ащ|ящ)(?:ий|ый|ая|яя|ое|ее|ые|ие|ого|его|ому|ему|ую|юю|им|ом|ем|их|ых|ими|ыми)(?:ся)?$|" +  // действительные наст. вр.
                @"\w+(?:вш|ш)(?:ий|ый|ая|яя|ое|ее|ые|ие|ого|его|ому|ему|ую|юю|им|ом|ем|их|ых|ими|ыми)(?:ся)?$|" +       // действительные прош. вр.
                @"\w+(?:ем|им|ом)(?:ый|ая|ое|ые|ого|ому|ым|ом|ых|ыми)(?:ся)?$",                                          // страдательные наст. вр.
                RegexOptions.IgnoreCase);

            var transgressivePattern = new Regex(
                @"\w+(?:вши|вшись|ши|учи|ючи)$",   // деепричастия 
                RegexOptions.IgnoreCase);

            foreach (var word in words)
            {
                var info = _morph.WordInfo(word);
                if (info == null || info.Length == 0)
                {
                    unknown++;
                    total++;
                    continue;
                }

                var pos = info[0].Tag.Pos;
                total++;

                // Сначала проверяем эвристики на причастие/деепричастие
                if (participlePattern.IsMatch(word))
                {
                    partic++;
                    continue;
                }
                if (transgressivePattern.IsMatch(word))
                {
                    adverbPart++;
                    continue;
                }

                switch (pos)
                {
                    case Pos.Noun: n++; break;
                    case Pos.Verb: v++; break;
                    case Pos.Adjective: a++; break;
                    case Pos.Adverb: adv++; break;
                    case Pos.Pronoun: pro++; break;
                    case Pos.Numeral: num++; break;
                    case Pos.Preposition: prep++; break;
                    case Pos.Conjunction: conj++; break;
                    case Pos.Particle: part++; break;
                    case Pos.Interjection: interj++; break;
                    case Pos.Predicative: predic++; break;
                    case Pos.Parenthesis: parenthes++; break;
                    default: unknown++; break;
                }
            }

            if (total > 0)
            {
                result.PercentNouns = 100.0 * n / total;
                result.PercentVerbs = 100.0 * v / total;
                result.PercentAdjectives = 100.0 * a / total;
                result.PercentAdverbs = 100.0 * adv / total;
                result.PercentPronouns = 100.0 * pro / total;
                result.PercentNumerals = 100.0 * num / total;
                result.PercentPrepositions = 100.0 * prep / total;
                result.PercentConjunctions = 100.0 * conj / total;
                result.PercentParticles = 100.0 * part / total;
                result.PercentInterjections = 100.0 * interj / total;
                result.PercentParticiples = 100.0 * partic / total;
                result.PercentAdverbialParticiples = 100.0 * adverbPart / total;
                result.PercentPredicatives = 100.0 * predic / total;
                result.PercentParentheses = 100.0 * parenthes / total;
                result.PercentUnknown = 100.0 * unknown / total;

                result.PercentFunctionWords = 100.0 * (prep + conj + part + interj) / total;
            }
        }

        // ────────────────────────────────────────
        //  Разнообразие союзов и местоимений (Lemma преобразуем в string)
        // ────────────────────────────────────────
        private static void AnalyzeFunctionWordsDiversity(string content, StructuralAnalysisResult result)
        {
            var words = Regex.Matches(content, @"\w+").Cast<Match>().Select(m => m.Value.ToLower());
            var conjSet = new HashSet<string>();
            var pronSet = new HashSet<string>();
            int totalConj = 0, totalPron = 0;

            foreach (var word in words)
            {
                var info = _morph.WordInfo(word);
                if (info == null || info.Length == 0) continue;
                var pos = info[0].Tag.Pos;

                if (pos == Pos.Conjunction)
                {
                    totalConj++;
                    // Используем само слово – это гарантирует реальное разнообразие
                    conjSet.Add(word);
                }
                else if (pos == Pos.Pronoun)
                {
                    totalPron++;
                    pronSet.Add(word);
                }
            }

            result.TotalConjunctions = totalConj;
            result.UniqueConjunctions = conjSet.Count;
            result.TotalPronouns = totalPron;
            result.UniquePronouns = pronSet.Count;
        }

        // ────────────────────────────────────────
        //  Мета-маркеры
        // ────────────────────────────────────────
        private static int CountMetaMarkers(string content)
        {
            int count = 0;
            var lower = content.ToLower();
            foreach (var marker in MetaMarkers)
                if (lower.Contains(marker)) count++;
            return count;
        }

        // ────────────────────────────────────────
        //  Подсчёт знаков препинания
        // ────────────────────────────────────────
        private static void CountPunctuation(string content, StructuralAnalysisResult result)
        {
            var punctuationMarks = Regex.Matches(content, @"[.,!?;:…""«»()\[\]{}\-–—]");
            result.TotalPunctuationMarks = punctuationMarks.Count;
            result.UniquePunctuationMarks = punctuationMarks.Cast<Match>().Select(m => m.Value).Distinct().Count();

            int questionCount = Regex.Matches(content, @"\?").Count;
            int exclamCount = Regex.Matches(content, @"!").Count;
            result.PercentQuestionSentences = result.SentenceCount > 0 ? 100.0 * questionCount / result.SentenceCount : 0;
            result.PercentExclamatorySentences = result.SentenceCount > 0 ? 100.0 * exclamCount / result.SentenceCount : 0;
        }

        // ────────────────────────────────────────
        //  Доля сложных предложений (более 1 глагола)
        // ────────────────────────────────────────
        private static double CalculateComplexSentences(string content)
        {
            var sentences = Regex.Split(content, @"(?<=[.!?;…])\s+").Where(s => !string.IsNullOrWhiteSpace(s));
            int complexCount = 0, total = 0;
            foreach (var sentence in sentences)
            {
                total++;
                int verbCount = 0;
                var words = Regex.Matches(sentence, @"\w+").Cast<Match>().Select(m => m.Value.ToLower());
                foreach (var word in words)
                {
                    var info = _morph.WordInfo(word);
                    if (info != null && info.Length > 0 && info[0].Tag.Pos == Pos.Verb)
                        verbCount++;
                }
                if (verbCount > 1) complexCount++;
            }
            return total > 0 ? 100.0 * complexCount / total : 0;
        }

        // ────────────────────────────────────────
        //  Подсчёт слогов
        // ────────────────────────────────────────
        private static int CountSyllables(string word)
        {
            int count = 0;
            bool lastWasVowel = false;
            foreach (char c in word.ToLower())
            {
                bool isVowel = "аеёиоуыэюяaeiouy".Contains(c);
                if (isVowel && !lastWasVowel)
                {
                    count++;
                    lastWasVowel = true;
                }
                else if (!isVowel) lastWasVowel = false;
            }
            return count > 0 ? count : 1;
        }
    }
}