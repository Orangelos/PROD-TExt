namespace Diplom.Models
{
    public class StructuralAnalysisResult
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

        // Базовые статистики
        public int ParagraphCount { get; set; }
        public int WordCount { get; set; }
        public int LetterCount { get; set; }
        public int SentenceCount { get; set; }

        // Средние длины
        public double AvgParagraphLengthWords { get; set; }
        public double AvgParagraphLengthLetters { get; set; }
        public double AvgParagraphLengthSentences { get; set; }      // NEW
        public double AvgSentenceLengthWords { get; set; }
        public double AvgSentenceLengthLetters { get; set; }
        public double AvgWordLengthLetters { get; set; }

        // Слоговые характеристики
        public int TotalSyllables { get; set; }
        public double AvgWordLengthSyllables { get; set; }

        // Лексическое разнообразие
        public double PercentUniqueWords { get; set; }               // TTR
        public double GuiraudIndex { get; set; }                     // NEW
        public double AvgWordFrequency { get; set; }

        // Сложность слов
        public double PercentSingleSyllableWords { get; set; }
        public double PercentComplexWords { get; set; }              // >3 слогов

        // Части речи (все, что даёт Nestor)
        public double PercentNouns { get; set; }
        public double PercentVerbs { get; set; }
        public double PercentAdjectives { get; set; }
        public double PercentAdverbs { get; set; }                   // NEW
        public double PercentPronouns { get; set; }                  // NEW
        public double PercentNumerals { get; set; }                  // NEW
        public double PercentPrepositions { get; set; }              // NEW
        public double PercentConjunctions { get; set; }              // NEW
        public double PercentParticles { get; set; }                 // NEW
        public double PercentInterjections { get; set; }             // NEW
        public double PercentParticiples { get; set; }               // NEW
        public double PercentAdverbialParticiples { get; set; }      // NEW
      //  public double PercentOtherPartsOfSpeech { get; set; }  // NEW
        //public double PercentBriefAdjectives { get; set; }
        //public double PercentBriefParticiples { get; set; }
        public double PercentPredicatives { get; set; }
        public double PercentParentheses { get; set; }
        public double PercentUnknown { get; set; }

        // Служебные слова (агрегация)
        public double PercentFunctionWords { get; set; }             // NEW

        // Разнообразие союзов и местоимений
        public int UniqueConjunctions { get; set; }                  // NEW
        public int TotalConjunctions { get; set; }                   // NEW
        public int UniquePronouns { get; set; }                      // NEW
        public int TotalPronouns { get; set; }                       // NEW

        // Мета-текстовые маркеры (приблизительно)
        public int MetaTextMarkersCount { get; set; }                // NEW

        // Знаки препинания
        public int TotalPunctuationMarks { get; set; }               // NEW
        public int UniquePunctuationMarks { get; set; }              // NEW
        public double PercentQuestionSentences { get; set; }         // NEW
        public double PercentExclamatorySentences { get; set; }      // NEW

        // Сложность предложений
        public double PercentComplexSentences { get; set; }          // NEW
                                                                     // Средняя длина предложения в слогах
        public double AvgSentenceLengthSyllables { get; set; }

        // Длинные слова (>6 букв)
        public double PercentLongWords { get; set; }
        public double LIX { get; set; }
        public double RIX { get; set; }

        // Энтропия распределения частей речи
        public double PoSEntropy { get; set; }

        // Аббревиатуры
        public int AbbreviationCount { get; set; }

        // Предложения с прямой речью
        public double PercentDirectSpeechSentences { get; set; }

        // Индекс Honoré
        public double HonoreIndex { get; set; }

        // Стандартное отклонение длин абзацев (в словах)
        public double StdDevParagraphLengthWords { get; set; }

        // Символьные N-граммы
        public int UniqueCharBigrams { get; set; }
        public int TotalCharBigrams { get; set; }
        public int UniqueCharTrigrams { get; set; }
        public int TotalCharTrigrams { get; set; }
        // Индексы удобочитаемости
        public double ReadabilityOborneva { get; set; }
        public double ReadabilityTuldava { get; set; }
        public double ReadabilityColeman { get; set; }

        public double ReadabilityARI { get; set; }
        public double ReadabilityFleschKincaidEducational { get; set; }
        public double ReadabilityFleschKincaidFiction { get; set; }
        public double ReadabilityMatskovsky { get; set; }
        public double ReadabilityDaleChall { get; set; }
        public double ReadabilityGunningFog { get; set; }
        public double ReadabilityPowersSumnerKearl { get; set; }
        public double ReadabilitySMOG { get; set; }
        public List<string> AbbreviationList { get; set; } = new();
        // Словарь всех метрик для сравнения
        // Лексическое разнообразие
        public int UniqueWordCount { get; set; }         // NEW: абсолютное число уникальных слов

        // Части речи
        public double PercentNotionalWords { get; set; } // NEW: доля знаменательных слов

        // Структура предложений
        public double PunctuationPer100Words { get; set; } // NEW: знаков препинания на 100 слов
        public Dictionary<string, double> AllMetrics => new()
        {
            ["Абзацев"] = ParagraphCount,
            ["Слов"] = WordCount,
            ["Букв"] = LetterCount,
            ["Предложений"] = SentenceCount,
            ["Средняя длина абзаца (слов)"] = AvgParagraphLengthWords,
            ["Средняя длина абзаца (букв)"] = AvgParagraphLengthLetters,
            ["Средняя длина абзаца (предложений)"] = AvgParagraphLengthSentences,
            ["Средняя длина предложения (слов)"] = AvgSentenceLengthWords,
            ["Средняя длина предложения (букв)"] = AvgSentenceLengthLetters,
            ["Средняя длина предложения (слогов)"] = AvgSentenceLengthSyllables,
            ["Средняя длина слова (букв)"] = AvgWordLengthLetters,
            ["Средняя длина слова (слогов)"] = AvgWordLengthSyllables,
            ["Уникальные слова (%)"] = PercentUniqueWords,
            ["Индекс Guiraud"] = GuiraudIndex,
            ["Средняя частота повторения"] = AvgWordFrequency,
            ["Односложные слова (%)"] = PercentSingleSyllableWords,
            ["Сложные слова (>3 слогов) (%)"] = PercentComplexWords,
            ["Процент слов длиннее 6 букв"] = PercentLongWords,
            ["LIX"] = LIX,
            ["RIX"] = RIX,
            ["Энтропия частей речи"] = PoSEntropy,
            ["Индекс Honoré"] = HonoreIndex,
            ["Существительные (%)"] = PercentNouns,
            ["Глаголы (%)"] = PercentVerbs,
            ["Прилагательные (%)"] = PercentAdjectives,
            ["Наречия (%)"] = PercentAdverbs,
            ["Местоимения (%)"] = PercentPronouns,
            ["Числительные (%)"] = PercentNumerals,
            ["Предлоги (%)"] = PercentPrepositions,
            ["Уникальных аббревиатур"] = AbbreviationList.Count,
            ["Союзы (%)"] = PercentConjunctions,
            ["Частицы (%)"] = PercentParticles,
            ["Междометия (%)"] = PercentInterjections,
            ["Причастия (%)"] = PercentParticiples,
            ["Деепричастия (%)"] = PercentAdverbialParticiples,
            ["Предикативы (%)"] = PercentPredicatives,
            ["Вводные слова (%)"] = PercentParentheses,
            ["Нераспознанные (%)"] = PercentUnknown,
            ["Служебные слова (%)"] = PercentFunctionWords,
            ["Уникальных союзов"] = UniqueConjunctions,
            ["Всего союзов"] = TotalConjunctions,
            ["Уникальных местоимений"] = UniquePronouns,
            ["Всего местоимений"] = TotalPronouns,
            ["Мета-текстовых маркеров"] = MetaTextMarkersCount,
            ["Знаков препинания всего"] = TotalPunctuationMarks,
            ["Уникальных знаков препинания"] = UniquePunctuationMarks,
            ["Вопросительные предложения (%)"] = PercentQuestionSentences,
            ["Восклицательные предложения (%)"] = PercentExclamatorySentences,
            ["Сложные предложения (%)"] = PercentComplexSentences,
            ["Аббревиатур"] = AbbreviationCount,
            ["Предложения с прямой речью (%)"] = PercentDirectSpeechSentences,
            ["Стандартное отклонение длин абзацев (слов)"] = StdDevParagraphLengthWords,
            ["Оборневой"] = ReadabilityOborneva,
            ["Тулдавы"] = ReadabilityTuldava,
            ["Колемана"] = ReadabilityColeman,
            ["ARI"] = ReadabilityARI,
            ["Флеш-Кинкейд (учеб.)"] = ReadabilityFleschKincaidEducational,
            ["Флеш-Кинкейд (худ.)"] = ReadabilityFleschKincaidFiction,
            ["Мацковского"] = ReadabilityMatskovsky,
            ["Дейл-Челл"] = ReadabilityDaleChall,
            ["Туманность Ганнинга"] = ReadabilityGunningFog,
            ["Пауэрс-Самнер-Кернл"] = ReadabilityPowersSumnerKearl,
            ["SMOG"] = ReadabilitySMOG,
            ["Уникальных 2-грамм символов"] = UniqueCharBigrams,
            ["Всего 2-грамм символов"] = TotalCharBigrams,
            ["Уникальных 3-грамм символов"] = UniqueCharTrigrams,
            ["Всего 3-грамм символов"] = TotalCharTrigrams
        };
    }
}