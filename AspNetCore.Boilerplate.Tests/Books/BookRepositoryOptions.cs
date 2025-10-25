using AspNetCore.Boilerplate.EntityFrameworkCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Tests.Books;

[Service(Lifetime.Singleton)]
public class BookRepositoryOptions : EfRepositoryOptions<Book>
{
    public BookRepositoryOptions(IValidator<Book> validator)
    {
        Validator = validator;

        QueryOptions = new EntityQueryOptions<Book>
        {
            DefaultIncludeQuery = queryable => queryable,
            IncludeDetailsQuery = queryable =>
                queryable.Include(book => book.Authors).ThenInclude(authors => authors.Author),
        };

        UpdateOptions = new EntityUpdateOptions<Book>();
    }
}
