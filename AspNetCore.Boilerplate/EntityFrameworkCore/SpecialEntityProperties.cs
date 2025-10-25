using AspNetCore.Boilerplate.Domain;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public static class SpecialEntityProperties
{
    public static readonly string[] DefaultUpdateIgnores =
    [
        .. typeof(IAuditable).GetProperties().Select(property => property.Name),
        .. typeof(ISoftDelete).GetProperties().Select(property => property.Name),
        .. typeof(IMultiTenant).GetProperties().Select(property => property.Name),
        "CreatedBy",
        "UpdatedBy",
        "DeletedBy",
        "Tenant",
    ];
}
