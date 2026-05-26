using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Models.Recognition;

public class ServiceRecognitionUploadRequest
{
    [FromForm(Name = "image")]
    public IFormFile Image { get; set; } = default!;

    [FromForm(Name = "appointmentId")]
    public Guid? AppointmentId { get; set; }

    [FromForm(Name = "clientId")]
    public Guid? ClientId { get; set; }

    [FromForm(Name = "professionalId")]
    public Guid? ProfessionalId { get; set; }

    [FromForm(Name = "tenantId")]
    public Guid? TenantId { get; set; }

    [FromForm(Name = "branchId")]
    public Guid? BranchId { get; set; }
}
