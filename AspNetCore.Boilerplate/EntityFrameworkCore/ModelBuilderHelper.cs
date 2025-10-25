using System.Linq.Expressions;
using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

public class ModelBuilderHelper(
    ModelBuilder builder,
    Expression<Func<Guid>>? tenantIdProperty = null
)
{
    private ModelBuilder _builder = builder;

    public ModelBuilderHelper BuildEntity<TEntity>(
        Action<EntityTypeBuilder<TEntity>>? additionalConfigure = null,
        string? schema = null
    )
        where TEntity : class, IEntity
    {
        _builder = _builder.BuildEntity(additionalConfigure, schema, tenantIdProperty);

        return this;
    }
}
