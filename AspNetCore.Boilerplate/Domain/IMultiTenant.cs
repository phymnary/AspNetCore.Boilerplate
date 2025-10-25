namespace AspNetCore.Boilerplate.Domain;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}
