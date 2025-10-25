namespace AspNetCore.Boilerplate.Domain.Auditing;

public class AuditingEntityMapper<TConcrete, TImplement>
    where TImplement : class
{
    public required Func<TConcrete, TImplement> Map { get; init; }
}
