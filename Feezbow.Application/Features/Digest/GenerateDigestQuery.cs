using MediatR;

namespace Feezbow.Application.Features.Digest;

public record GenerateDigestQuery(long ProjectId) : IRequest<GenerateDigestQueryResponse>;
