using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.EntityFrameworkCore;
using AspNetCore.Boilerplate.Tests.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Tests.Books;

public interface IBookRepository : IRepository<Book>;

[Service(Lifetime.Scoped)]
internal class BookRepository(
    BookStoreDbContext context,
    EfRepositoryAddons addons,
    IRepositoryOptions<Book> options
) : EfRepository<BookStoreDbContext, Book>(context, addons, options), IBookRepository;
