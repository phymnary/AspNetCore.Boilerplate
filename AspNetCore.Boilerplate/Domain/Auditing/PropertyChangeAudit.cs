namespace AspNetCore.Boilerplate.Domain.Auditing;

public abstract class PropertyChangeAudit
{
    public required string EntityName { get; set; }

    public required string PropertyName { get; set; }

    public required string TypeName { get; set; }

    public required string EntityId { get; set; }

    public required string OldValue { get; set; }

    public required string NewValue { get; set; }

    public required Guid? ModifiedById { get; set; }

    public required DateTimeOffset ModifiedAt { get; set; }

    public bool IsDeleted { get; set; }
}
