// DocumentService.cs
namespace DocumentManagementML.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentRepository documentRepository,
            IDocumentTypeRepository documentTypeRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _documentTypeRepository = documentTypeRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DocumentDto> GetDocumentByIdAsync(int id)
        {
            var document = await _documentRepository.GetWithMetadataAsync(id);
            if (document == null)
            {
                throw new NotFoundException($"Document with ID {id} not found");
            }

            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsAsync(int skip = 0, int limit = 100)
        {
            var documents = await _documentRepository.GetActiveDocumentsAsync(skip, limit);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }

        public async Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, Stream fileStream, string fileName)
        {
            // Check if document type exists
            if (documentDto.DocumentTypeId.HasValue)
            {
                var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                if (documentType == null)
                {
                    throw new NotFoundException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                }
            }

            // Store file and generate hash
            var filePath = await _fileStorageService.StoreFileAsync(fileStream, fileName, documentDto.FileType);
            var fileSize = await _fileStorageService.GetFileSizeAsync(filePath);
            var contentHash = await GenerateFileHashAsync(fileStream);

            // Create document entity
            var document = new Document
            {
                DocumentName = documentDto.DocumentName,
                FileLocation = filePath,
                FileType = documentDto.FileType,
                FileSizeBytes = fileSize,
                ContentHash = contentHash,
                DocumentTypeId = documentDto.DocumentTypeId,
                CreatedDate = DateTime.UtcNow,
                CreatedById = 1, // TODO: Replace with authenticated user ID
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedById = 1, // TODO: Replace with authenticated user ID
                IsDeleted = false
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation($"Document created with ID: {document.DocumentId}");
            return _mapper.Map<DocumentDto>(document);
        }

        public async Task<DocumentDto> UpdateDocumentAsync(int id, DocumentUpdateDto documentDto)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                throw new NotFoundException($"Document with ID {id} not found");
            }

            // Update document properties
            if (!string.IsNullOrEmpty(documentDto.DocumentName))
            {
                document.DocumentName = documentDto.DocumentName;
            }

            if (documentDto.DocumentTypeId.HasValue)
            {
                var documentType = await _documentTypeRepository.GetByIdAsync(documentDto.DocumentTypeId.Value);
                if (documentType == null)
                {
                    throw new NotFoundException($"Document type with ID {documentDto.DocumentTypeId.Value} not found");
                }
                document.DocumentTypeId = documentDto.DocumentTypeId.Value;
            }

            document.LastModifiedDate = DateTime.UtcNow;
            document.LastModifiedById = 1; // TODO: Replace with authenticated user ID

            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation($"Document updated: {document.DocumentId}");
            return _mapper.Map<DocumentDto>(document);
        }

        public async Task DeleteDocumentAsync(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                throw new NotFoundException($"Document with ID {id} not found");
            }

            await _documentRepository.SoftDeleteAsync(id);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation($"Document soft deleted: {id}");
        }

        public async Task<IEnumerable<DocumentDto>> GetDocumentsByTypeAsync(int typeId, int skip = 0, int limit = 100)
        {
            var documents = await _documentRepository.GetByTypeIdAsync(typeId);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents)
                .Skip(skip)
                .Take(limit);
        }

        private async Task<string> GenerateFileHashAsync(Stream fileStream)
        {
            using (var sha256 = SHA256.Create())
            {
                fileStream.Position = 0;
                var hashBytes = await sha256.ComputeHashAsync(fileStream);
                fileStream.Position = 0; // Reset position for further processing
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}