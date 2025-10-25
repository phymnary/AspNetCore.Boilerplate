using AspNetCore.Boilerplate.Domain;

namespace AspNetCore.Boilerplate.Tests.Categories;

public class Category : Entity<int>
{
    public required string Name { get; set; }
}
