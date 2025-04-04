// DocumentTypesController.cs
namespace DocumentManagementML.API.Controllers
{
    [ApiController]
    [Route("api/v1/document-types")]
    public class DocumentTypesController : ControllerBase
    {
        private readonly IDocumentTypeService _documentTypeService;
        private readonly ILogger<DocumentTypesController> _logger;

        public DocumentTypesController(
            IDocumentTypeService documentTypeService,
            ILogger<DocumentTypesController> logger)
        {
            _documentTypeService = documentTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentTypeDto>>> GetDocumentTypes(
            [FromQuery] int skip = 0,
            [FromQuery] int limit = 100)
        {
            try
            {
                var documentTypes = await _documentTypeService.GetDocumentTypesAsync(skip, limit);
                return Ok(documentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving document types: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving document types
                return StatusCode(500, "An error occurred while retrieving document types");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentTypeDto>> GetDocumentType(int id)
        {
            try
            {
                var documentType = await _documentTypeService.GetDocumentTypeByIdAsync(id);
                return Ok(documentType);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving document type {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the document type");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DocumentTypeDto>> CreateDocumentType(DocumentTypeCreateDto documentTypeDto)
        {
            try
            {
                var documentType = await _documentTypeService.CreateDocumentTypeAsync(documentTypeDto);
                
                return CreatedAtAction(
                    nameof(GetDocumentType),
                    new { id = documentType.DocumentTypeId },
                    documentType
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating document type: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the document type");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentTypeDto>> UpdateDocumentType(int id, DocumentTypeUpdateDto documentTypeDto)
        {
            try
            {
                var documentType = await _documentTypeService.UpdateDocumentTypeAsync(id, documentTypeDto);
                return Ok(documentType);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document type {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the document type");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeactivateDocumentType(int id)
        {
            try
            {
                await _documentTypeService.DeactivateDocumentTypeAsync(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating document type {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deactivating the document type");
            }
        }
    }
}