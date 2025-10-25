namespace AspNetCore.Boilerplate.Domain.Auditing;

/// <summary>
/// Specific what properties need to be audited of target class
/// </summary>
/// <param name="properties"></param>
[AttributeUsage(AttributeTargets.Class)]
public class AuditedAttribute(params string[] properties) : Attribute
{
    public string[] Properties { get; } = properties;
}
