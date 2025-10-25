using FluentValidation;

namespace AspNetCore.Boilerplate.Tests.Books;

[Service(Lifetime.Singleton)]
public class BookValidator : AbstractValidator<Book>
{
    public BookValidator()
    {
        RuleFor(book => book.Name).NotEmpty();
    }
}
