using AspNetCore.Boilerplate.EntityFrameworkCore;
using AspNetCore.Boilerplate.Tests.Auditing;
using AspNetCore.Boilerplate.Tests.Auditing.OwnedTests;
using AspNetCore.Boilerplate.Tests.Authors;
using AspNetCore.Boilerplate.Tests.Books;
using AspNetCore.Boilerplate.Tests.Categories;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Tests.EntityFrameworkCore;

internal class BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; init; }

    public DbSet<Author> Authors { get; init; }

    public DbSet<BookAuthor> BookAuthors { get; init; }

    public DbSet<Category> Categories { get; init; }

    public DbSet<Tree> Trees { get; init; }

    public DbSet<AppPropertyChangeAudit> AppPropertyChangeAudits { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        new ModelBuilderHelper(modelBuilder)
            .BuildEntity<Book>()
            .BuildEntity<Author>()
            .BuildEntity<BookAuthor>()
            .BuildEntity<Category>()
            .BuildEntity<Tree>()
            .BuildEntity<AppPropertyChangeAudit>();
    }
}
