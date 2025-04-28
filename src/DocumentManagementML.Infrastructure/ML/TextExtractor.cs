// TextExtractor.cs
using System;
using System.IO;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Services;
using Microsoft.Extensions.Logging;

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
            try
            {
                _logger.LogInformation($"Extracting text from document with extension {fileExtension}");
                
                // Simple implementation for now
                using var reader = new StreamReader(documentStream);
                var text = await reader.ReadToEndAsync();
                
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting text from document: {ex.Message}");
                return string.Empty;
            }
        }
    }
}