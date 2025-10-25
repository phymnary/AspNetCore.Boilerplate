namespace AspNetCore.Boilerplate.Domain;

public interface ISoftDelete
{
    Guid? DeletedById { get; set; }

    DateTimeOffset? DeletedAt { get; set; }

    public void Undo()
    {
        DeletedAt = null;
        DeletedById = null;
    }
}
