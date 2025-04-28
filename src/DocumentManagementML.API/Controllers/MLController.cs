// MLController.cs
using System;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Controllers
{
    [ApiController]
    [Route("api/v1/ml")]
    public class MLController : ControllerBase
    {
        private readonly IDocumentClassificationService _classificationService;
        private readonly ILogger<MLController> _logger;

        public MLController(
            IDocumentClassificationService classificationService,
            ILogger<MLController> logger)
        {
            _classificationService = classificationService;
            _logger = logger;
        }

        [HttpPost("train")]
        public async Task<ActionResult> TrainModel()
        {
            try
            {
                _logger.LogInformation("Starting model training process");
                
                // Start training (this could be a long-running task)
                var trainingTask = _classificationService.TrainModelAsync();
                
                // Return 202 Accepted with a link to check status
                return AcceptedAtAction(nameof(GetModelStatus), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting model training: {ex.Message}");
                return StatusCode(500, "An error occurred while starting model training");
            }
        }

        [HttpGet("status")]
        public ActionResult GetModelStatus()
        {
            try
            {
                // In a real implementation, you would track the training status
                // For now, we'll return a simple status
                return Ok(new { Status = "Training in progress" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting model status: {ex.Message}");
                return StatusCode(500, "An error occurred while getting model status");
            }
        }

        [HttpGet("metrics")]
        public async Task<ActionResult> GetModelMetrics()
        {
            try
            {
                var metrics = await _classificationService.EvaluateModelAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting model metrics: {ex.Message}");
                return StatusCode(500, "An error occurred while getting model metrics");
            }
        }
    }
}