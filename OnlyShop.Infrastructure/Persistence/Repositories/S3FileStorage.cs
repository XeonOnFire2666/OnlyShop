using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OnlyShop.Application.Abstractions.Infrastructure;
using OnlyShop.Application.Common.Models;

namespace OnlyShop.Infrastructure.FileStorage;

public class S3FileStorage(IAmazonS3 s3Client, IOptions<FileStorageOptions> options) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(FileUpload file, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var key = $"{folder.Trim('/')}/{fileName}";

        if (file.Content.CanSeek && file.Content.Position > 0)
        {
            file.Content.Position = 0;
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.S3BucketName,
            Key = key,
            InputStream = file.Content,
            ContentType = file.ContentType,
            DisablePayloadSigning = true
        };

        await s3Client.PutObjectAsync(putRequest, ct);

        return key;
    }

    public async Task DeleteAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _options.S3BucketName,
            Key = path.TrimStart('/')
        };

        await s3Client.DeleteObjectAsync(deleteRequest, ct);
    }

    public string GetPublicUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}