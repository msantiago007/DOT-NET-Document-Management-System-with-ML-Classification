using System;
using System.Collections.Generic;

namespace DocumentManagementML.Domain.Entities
{
    public class ModelMetrics
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public double MicroAccuracy { get; set; }
        public double MacroAccuracy { get; set; }
        public double LogLoss { get; set; }
        public double LogLossReduction { get; set; }
        public Dictionary<string, double> ClassMetrics { get; set; } = new Dictionary<string, double>();
        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
    }
} 