namespace OnlyShop.Application.Common.Models;

public sealed record FileUpload(Stream Content, string FileName, string ContentType);