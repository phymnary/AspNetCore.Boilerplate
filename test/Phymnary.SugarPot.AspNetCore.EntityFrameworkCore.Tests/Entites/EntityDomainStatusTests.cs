using Microsoft.EntityFrameworkCore;
using Phymnary.SugarPot.AspNetCore.Entities;

namespace Phymnary.SugarPot.AspNetCore.EntityFrameworkCore.Tests.Entites;

public class EntityDomainStatusTests
{
    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Author> Authors => Set<Author>();

        public DbSet<Book> Books => Set<Book>();
    }

    private class Author : Entity<Guid>
    {
        protected Author() { }

        public Author(Guid id)
            : base(id) { }

        public required string Name { get; set; }

        public ICollection<Book> Books { get; } = [];
    }

    private class Book : Entity<Guid>
    {
        protected Book() { }

        public Book(Guid id)
            : base(id) { }

        public required string Title { get; set; }
    }

    [Fact]
    public async Task on_attached_entities_get_added_to_dbcontext()
    {
        var dbName = $"bookstore-{Guid.NewGuid()}";

        var ct = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var authorId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        await using (var dbContext = new TestDbContext(options))
        {
            var author = new Author(authorId) { Name = "Robert C. Martin" };

            dbContext.Authors.Add(author);
            await dbContext.SaveChangesAsync(ct);
        }

        await using (var dbContext = new TestDbContext(options))
        {
            var savedAuthor = await dbContext
                .Authors.Include(x => x.Books)
                .SingleAsync(x => x.Id == authorId, ct);

            var book = new Book(bookId) { Title = "Clean Architecture" };
            savedAuthor.Books.Add(book);
            book.DomainStatus.OnAttached();

            foreach (
                var entry in dbContext
                    .ChangeTracker.Entries<IEntity>()
                    .Where(e => e.Entity.DomainStatus.IsAdded)
            )
            {
                entry.State = EntityState.Added;
            }

            await dbContext.SaveChangesAsync(ct);
            Assert.Equal("Robert C. Martin", savedAuthor.Name);
        }

        await using (var dbContext = new TestDbContext(options))
        {
            var author = await dbContext
                .Authors.Include(x => x.Books)
                .SingleAsync(x => x.Id == authorId, ct);

            Assert.Single(author.Books);
            Assert.Contains(author.Books, x => x.Id == bookId && x.Title == "Clean Architecture");
        }
    }
}
