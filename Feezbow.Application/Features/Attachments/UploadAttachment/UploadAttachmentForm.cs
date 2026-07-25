using Feezbow.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Feezbow.Application.Features.Attachments.UploadAttachment;

/// <summary>Multipart/form-data payload za upload atačmenta (web-boundary model).</summary>
public class UploadAttachmentForm
{
    public AttachmentOwnerType OwnerType { get; set; }
    public long OwnerId { get; set; }
    public IFormFile File { get; set; } = null!;
    public string? Description { get; set; }
}
