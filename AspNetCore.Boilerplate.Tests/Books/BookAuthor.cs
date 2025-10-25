using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Tests.Authors;

namespace AspNetCore.Boilerplate.Tests.Books;

public class BookAuthor : Entity<int>
{
    public Book Book { get; init; } = null!;

    public Author Author { get; init; } = null!;
}
