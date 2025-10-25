using System.Linq.Expressions;
using AspNetCore.Boilerplate.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static System.Linq.Expressions.Expression;

namespace AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder BuildEntity<TEntity>(
        this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<TEntity>>? additionalConfigure = null,
        string? schema = null,
        Expression<Func<Guid>>? tenantIdProperty = null
    )
        where TEntity : class, IEntity
    {
        modelBuilder.Entity<TEntity>(b =>
        {
            b.ToTable(typeof(TEntity).Name, schema);

            if (CreateQueryFilter<TEntity>(tenantIdProperty) is { } queryFilter)
                b.HasQueryFilter(queryFilter);

            additionalConfigure?.Invoke(b);
        });

        return modelBuilder;
    }

    private static Expression<Func<TEntity, bool>>? CreateQueryFilter<TEntity>(
        Expression<Func<Guid>>? tenantIdProperty
    )
        where TEntity : IEntity
    {
        var type = typeof(TEntity);
        var entity = Parameter(type, "entity");
        List<BinaryExpression> conditions = [];

        if (type.IsAssignableTo(typeof(ISoftDelete)))
            conditions.Add(Equal(Property(entity, nameof(ISoftDelete.DeletedAt)), Constant(null)));

        if (type.IsAssignableTo(typeof(IMultiTenant)) && tenantIdProperty is not null)
            conditions.Add(
                Equal(Property(entity, nameof(IMultiTenant.TenantId)), tenantIdProperty.Body)
            );

        if (conditions.Count == 0)
            return null;

        var predicate = conditions.Aggregate<BinaryExpression, BinaryExpression?>(
            null,
            (current, condition) => current is null ? condition : And(current, condition)
        );

        return predicate is null ? null : Lambda<Func<TEntity, bool>>(predicate, entity);
    }
}
