using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Tests.Categories;

namespace AspNetCore.Boilerplate.Tests.Books;

public class Book : Entity<int>, IAuditable
{
    public required string Name { get; set; }

    public required ICollection<BookAuthor> Authors { get; init; }

    public Category? Category { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }
}
