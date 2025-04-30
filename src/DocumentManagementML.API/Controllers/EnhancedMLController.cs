// -----------------------------------------------------------------------------
// <copyright file="EnhancedMLController.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Enhanced API controller for machine learning operations with
//                     standardized responses and improved error handling
// -----------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;
using DocumentManagementML.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManagementML.API.Controllers
{
    /// <summary>
    /// Enhanced API controller for machine learning operations
    /// </summary>
    [ApiController]
    [Route("api/v1/ml")]
    [Produces("application/json")]
    public class EnhancedMLController : BaseApiController
    {
        private readonly IDocumentClassificationService _classificationService;

        /// <summary>
        /// Initializes a new instance of the EnhancedMLController class
        /// </summary>
        /// <param name="classificationService">Document classification service</param>
        /// <param name="logger">Logger</param>
        public EnhancedMLController(
            IDocumentClassificationService classificationService,
            ILogger<EnhancedMLController> logger)
            : base(logger)
        {
            _classificationService = classificationService ?? throw new ArgumentNullException(nameof(classificationService));
        }

        /// <summary>
        /// Starts the model training process
        /// </summary>
        /// <returns>Accepted response with a link to check status</returns>
        [HttpPost("train")]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TrainModel()
        {
            try
            {
                Logger.LogInformation("Starting model training process");
                
                // Start training (this could be a long-running task)
                _ = _classificationService.TrainModelAsync();
                
                // Return 202 Accepted with a link to check status
                return AcceptedAtAction(nameof(GetModelStatus), null, null, "Model training started successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error starting model training");
                return StatusCode(500, ResponseDto.Fail("An error occurred while starting model training"));
            }
        }

        /// <summary>
        /// Gets the current status of model training
        /// </summary>
        /// <returns>Model status information</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ResponseDto<ModelStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status500InternalServerError)]
        public IActionResult GetModelStatus()
        {
            try
            {
                // In a real implementation, you would track the training status
                // For now, we'll return a simple status
                var status = new ModelStatusDto
                {
                    Status = "Training in progress",
                    Progress = 50,
                    StartedAt = DateTime.UtcNow.AddMinutes(-5),
                    EstimatedCompletionTime = DateTime.UtcNow.AddMinutes(5)
                };
                
                return Ok(ResponseDto<ModelStatusDto>.Ok(status));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting model status");
                return StatusCode(500, ResponseDto.Fail("An error occurred while getting model status"));
            }
        }

        /// <summary>
        /// Gets evaluation metrics for the trained model
        /// </summary>
        /// <returns>Model metrics DTO</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(ResponseDto<ModelMetricsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetModelMetrics()
        {
            return await ExecuteAsync(
                () => _classificationService.EvaluateModelAsync(),
                "Error getting model metrics");
        }
    }
    
    /// <summary>
    /// Model status information DTO
    /// </summary>
    public class ModelStatusDto
    {
        /// <summary>
        /// Current status
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; set; }
        
        /// <summary>
        /// When the training started
        /// </summary>
        public DateTime StartedAt { get; set; }
        
        /// <summary>
        /// Estimated completion time
        /// </summary>
        public DateTime EstimatedCompletionTime { get; set; }
    }
}