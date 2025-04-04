// TextExtractor.cs
namespace DocumentManagementML.Infrastructure.ML
{
    public class TextExtractor : ITextExtractor
    {
        private readonly ILogger<TextExtractor> _logger;

        public TextExtractor(ILogger<TextExtractor> logger)
        {
            _logger = logger;
        }

        public async Task<string> ExtractTextAsync(Stream documentStream, string fileExtension)
        {
            _logger.LogInformation($"Extracting text from document with extension: {fileExtension}");
            
            // Reset stream position
            documentStream.Position = 0;
            
            try
            {
                switch (fileExtension.ToLowerInvariant())
                {
                    case ".txt":
                        return await ExtractFromTextFileAsync(documentStream);
                    case ".pdf":
                        return await ExtractFromPdfAsync(documentStream);
                    case ".docx":
                        return await ExtractFromDocxAsync(documentStream);
                    default:
                        _logger.LogWarning($"Unsupported file extension: {fileExtension}");
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting text from document: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                // Reset stream position again
                documentStream.Position = 0;
            }
        }

        private async Task<string> ExtractFromTextFileAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private async Task<string> ExtractFromPdfAsync(Stream stream)
        {
            // Using PdfPig for PDF text extraction
            using var pdf = PdfDocument.Open(stream, new ParsingOptions { UseLenientParsing = true });
            
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                var text = page.Text;
                sb.AppendLine(text);
            }
            
            return sb.ToString();
        }

        private async Task<string> ExtractFromDocxAsync(Stream stream)
        {
            // Using DocumentFormat.OpenXml for DOCX text extraction
            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document.Body;
            
            if (body != null)
            {
                return body.InnerText;
            }
            
            return string.Empty;
        }
    }
}