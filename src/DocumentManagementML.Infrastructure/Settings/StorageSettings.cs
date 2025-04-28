namespace DocumentManagementML.Infrastructure.Settings
{
    public class StorageSettings
    {
        public string BasePath { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; } = 10485760; // 10MB default
        public string[] AllowedFileTypes { get; set; } = { ".pdf", ".docx", ".txt" };
    }
} 