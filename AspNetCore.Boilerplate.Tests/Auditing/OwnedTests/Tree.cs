using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Tests.Books;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Boilerplate.Tests.Auditing.OwnedTests;

internal class Tree : Entity<int>, IAuditable
{
    public required string Name { get; set; }

    [DisableAuditing]
    public required string Location { get; set; }

    public Branch? Branch { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }
}

[Owned]
internal class Branch
{
    [DisableAuditing]
    public required string Name { get; set; }

    public Leaf? Leaf { get; init; }

    public Book? Book { get; set; }
}

[Owned]
internal class Leaf
{
    public required int Value { get; set; }
}
