using System;

namespace DocumentManagementML.Application.DTOs
{
    /// <summary>
    /// Data transfer object for ML model metrics.
    /// </summary>
    public class ModelMetricsDto
    {
        /// <summary>
        /// Gets or sets the model ID.
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the accuracy of the model.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the precision of the model.
        /// </summary>
        public double Precision { get; set; }

        /// <summary>
        /// Gets or sets the recall of the model.
        /// </summary>
        public double Recall { get; set; }

        /// <summary>
        /// Gets or sets the F1 score of the model.
        /// </summary>
        public double F1Score { get; set; }

        /// <summary>
        /// Gets or sets the training time in seconds.
        /// </summary>
        public double TrainingTime { get; set; }

        /// <summary>
        /// Gets or sets the date when the model was last trained.
        /// </summary>
        public DateTime LastTrainingDate { get; set; }

        /// <summary>
        /// Gets or sets the number of documents used for training.
        /// </summary>
        public int TrainingDocumentCount { get; set; }

        /// <summary>
        /// Gets or sets the number of document types in the model.
        /// </summary>
        public int DocumentTypeCount { get; set; }

        /// <summary>
        /// Gets or sets the confusion matrix.
        /// </summary>
        public double[,] ConfusionMatrix { get; set; } = new double[0, 0];
    }
}