using AspNetCore.Boilerplate.Api;
using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Auditing;
using AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors;
using AspNetCore.Boilerplate.EntityFrameworkCore.Interceptors.Trackers;
using AspNetCore.Boilerplate.Tests.Auditing.OwnedTests;
using AspNetCore.Boilerplate.Tests.Authors;
using AspNetCore.Boilerplate.Tests.Books;
using AspNetCore.Boilerplate.Tests.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AspNetCore.Boilerplate.Tests.Auditing;

public class AuditingMetadataTests
{
    [Fact]
    public async Task audit_tree()
    {
        var options = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseInMemoryDatabase(nameof(audit_tree))
            .Options;

        var dbContext = new BookStoreDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        var metadata = new AuditingMetadata();
        var changeTracker = new EntityPropertyChangeTracker<
            BookStoreDbContext,
            AppPropertyChangeAudit
        >(
            dbContext,
            new HttpContextCurrentUser(),
            metadata,
            new AuditingEntityMapper<PropertyChangeAudit, AppPropertyChangeAudit>
            {
                Map = data => new AppPropertyChangeAudit
                {
                    EntityName = data.EntityName,
                    PropertyName = data.PropertyName,
                    TypeName = data.TypeName,
                    EntityId = data.EntityId,
                    OldValue = data.OldValue,
                    NewValue = data.NewValue,
                    ModifiedById = data.ModifiedById,
                    ModifiedAt = data.ModifiedAt,
                    IsDeleted = data.IsDeleted,
                },
            }
        );
        var interceptor = new AuditOnSavingInterceptor(new HttpContextCurrentUser(), changeTracker);

        {
            dbContext.Trees.Add(new Tree { Name = "tree", Location = "earth" });
            await dbContext.SaveChangesAsync();
        }
        {
            var tree = await dbContext.Trees.Include(it => it.Branch!.Book).FirstAsync();
            tree.Branch = new Branch
            {
                Name = "branch",
                Leaf = new Leaf { Value = 100 },
                Book = dbContext
                    .Books.Add(
                        new Book
                        {
                            Name = "Dune",
                            Authors =
                            [
                                new BookAuthor { Author = new Author { Name = "Frank Herbert" } },
                            ],
                        }
                    )
                    .Entity,
            };
            await interceptor.SavingChangesAsync(
                new DbContextEventData(null!, null!, dbContext),
                default
            );
            var changes = dbContext.ChangeTracker.Entries<AppPropertyChangeAudit>().ToArray();

            Assert.Equal(4, changes.Length);
            await dbContext.SaveChangesAsync();
            dbContext.ChangeTracker.Clear();
        }
    }
}
