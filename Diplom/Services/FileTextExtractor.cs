using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Diplom.Services
{
    public static class FileTextExtractor
    {
        public static string ExtractText(Stream fileStream, string extension)
        {
            return extension.ToLower() switch
            {
                ".txt" => ExtractFromTxt(fileStream),
                ".docx" => ExtractFromDocx(fileStream),
                ".pdf" => ExtractFromPdf(fileStream),
                _ => throw new NotSupportedException($"Формат {extension} не поддерживается")
            };
        }

        private static string ExtractFromTxt(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return reader.ReadToEnd();
        }

        private static string ExtractFromDocx(Stream stream)
        {
            var sb = new StringBuilder();
            using var doc = WordprocessingDocument.Open(stream, false);
            var paragraphs = doc.MainDocumentPart?.Document?.Body?.Elements<Paragraph>();
            if (paragraphs == null) return string.Empty;

            foreach (var paragraph in paragraphs)
            {
                // Пропускаем пустые параграфы, но оставляем разделитель
                string text = paragraph.InnerText;
                if (string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(); // две пустые строки разделят абзацы
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(text);
                }
            }

            return sb.ToString().Trim();
        }

        private static string ExtractFromPdf(Stream stream)
        {
            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(stream);
            string previousLine = null;

            foreach (var page in pdf.GetPages())
            {
                var lines = page.GetWords()
                    .GroupBy(w => w.BoundingBox.Bottom)
                    .OrderBy(g => g.Key)
                    .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (!string.IsNullOrEmpty(previousLine))
                        {
                            sb.AppendLine(); // конец абзаца
                            sb.AppendLine();
                            previousLine = null;
                        }
                    }
                    else
                    {
                        sb.AppendLine(line);
                        previousLine = line;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}