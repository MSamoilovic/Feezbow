namespace Feezbow.Application.Features.Digest;

public record GenerateDigestQueryResponse(string Markdown, bool FromCache, DateTime GeneratedAt);
