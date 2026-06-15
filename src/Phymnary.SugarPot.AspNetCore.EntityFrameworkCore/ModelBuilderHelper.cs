using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phymnary.SugarPot.AspNetCore.Entities;
using Phymnary.SugarPot.AspNetCore.Extensions;

namespace Phymnary.SugarPot.AspNetCore;

public class ModelBuilderHelper(ModelBuilder builder)
{
#if NET10_0_OR_GREATER
    public const string SoftDelelteQueryFilterName = "EFCore.Boilerplate.SoftDelete";

    public const string MultiTenancyQueryFilterName = "EFCore.Boilerplate.MultiTenancy";
#endif

    private ModelBuilder _builder = builder;

    public Expression<Func<Guid>>? TenantIdAccessor { private get; init; }

    public ModelBuilderHelper BuildEntity<TEntity>(
        Action<EntityTypeBuilder<TEntity>>? additionalConfigure = null,
        string? schema = null
    )
        where TEntity : class, IEntity
    {
        _builder = _builder.BuildEntity(additionalConfigure, schema, TenantIdAccessor);

        return this;
    }
}
