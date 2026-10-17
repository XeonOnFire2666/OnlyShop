namespace OnlyShop.Infrastructure.FileStorage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "wwwroot/uploads";
    public string PublicBaseUrl { get; set; } = "/uploads";

    public string S3Endpoint { get; set; } = string.Empty;
    public string S3AccessKey { get; set; } = string.Empty;
    public string S3SecretKey { get; set; } = string.Empty;
    public string S3BucketName { get; set; } = string.Empty;
    public string S3Region { get; set; } = "eu-central-1";        
}