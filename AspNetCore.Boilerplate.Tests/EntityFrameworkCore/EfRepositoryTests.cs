using AspNetCore.Boilerplate.Api;
using AspNetCore.Boilerplate.EntityFrameworkCore;
using AspNetCore.Boilerplate.Tests.Books;
using AspNetCore.Boilerplate.Tests.Categories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Boilerplate.Tests.EntityFrameworkCore;

public class EfRepositoryTests
{
    [Fact]
    public async Task track_changes_test()
    {
        var options = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseInMemoryDatabase(nameof(track_changes_test))
            .Options;
        var dbContext = new BookStoreDbContext(options);
        var repository = new BookRepository(
            dbContext,
            new EfRepositoryAddons(new HttpContextCancellationTokenProvider()),
            new BookRepositoryOptions(new BookValidator())
        );

        var book = await repository.InsertAsync(new Book { Name = "Harry Potter", Authors = [] });
        book.Name = "Harry Potter chapter 1";
        book.Category = new Category { Name = "Children Books" };
        await repository.UpdateAsync(book);

        book.Name = "Harry Potter 1: Harry Potter and the Philosopher's Stone ";
        await repository.UpdateAsync(book);
    }
}
