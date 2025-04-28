// SimpleTextExtractor.cs
using DocumentManagementML.Domain.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagementML.Infrastructure.ML
{
    /// <summary>
    /// A simple implementation of ITextExtractor for phase 1
    /// </summary>
    public class SimpleTextExtractor : ITextExtractor
    {
        private readonly ILogger<SimpleTextExtractor> _logger;

        /// <summary>
        /// Initializes a new instance of the SimpleTextExtractor class
        /// </summary>
        /// <param name="logger">Logger</param>
        public SimpleTextExtractor(ILogger<SimpleTextExtractor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Extracts text from a document stream
        /// </summary>
        /// <param name="documentStream">Document content stream</param>
        /// <param name="fileExtension">File extension</param>
        /// <returns>Extracted text</returns>
        public async Task<string> ExtractTextAsync(Stream documentStream, string fileExtension)
        {
            try
            {
                _logger.LogInformation("Extracting text from document with extension: {FileExtension}", fileExtension);
                
                // For phase 1, we'll only handle simple text files
                // More complex formats like PDF, DOCX, etc. would require specific libraries
                
                // Normalize the file extension
                fileExtension = fileExtension.ToLowerInvariant().TrimStart('.');
                
                if (fileExtension == "txt" || fileExtension == "text")
                {
                    // For text files, we can just read the stream
                    using var reader = new StreamReader(documentStream, Encoding.UTF8, leaveOpen: true);
                    var text = await reader.ReadToEndAsync();
                    
                    // Reset the stream position for potential reuse
                    if (documentStream.CanSeek)
                    {
                        documentStream.Position = 0;
                    }
                    
                    return text;
                }
                
                // For all other file types in phase 1, return a placeholder
                _logger.LogWarning("Unsupported file format for text extraction in phase 1: {FileExtension}", fileExtension);
                return $"[Phase 1 Text Extraction Placeholder - {fileExtension} format not supported yet]";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from document with extension: {FileExtension}", fileExtension);
                return string.Empty;
            }
        }
    }
}