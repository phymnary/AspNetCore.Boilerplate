using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AspNetCore.Boilerplate.Domain;
using static System.Linq.Expressions.Expression;

namespace AspNetCore.Boilerplate.EntityFrameworkCore;

/// <summary>
/// Create function to map all required properties with setter, can be overridden with object initialization
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EntityUpdateOptions<TEntity>
    where TEntity : class, IEntity
{
    public EntityUpdateOptions()
    {
        var from = Parameter(typeof(TEntity), "from");
        var to = Parameter(typeof(TEntity), "to");

        List<BinaryExpression> assigns = [];

        foreach (var propertyInfo in typeof(TEntity).GetProperties())
        {
            if (SpecialEntityProperties.DefaultUpdateIgnores.Contains(propertyInfo.Name))
                continue;

            if (
                propertyInfo is { SetMethod.IsPublic: true, GetMethod.IsPublic: true }
                && propertyInfo.GetCustomAttribute<RequiredMemberAttribute>() is not null
                && IsNotInit(propertyInfo.SetMethod)
            )
                assigns.Add(
                    Assign(Property(to, propertyInfo.Name), Property(from, propertyInfo.Name))
                );
        }

        var body = Block(assigns);
        Run = Lambda<Action<TEntity, TEntity>>(body, from, to).Compile();
    }

    public Action<TEntity, TEntity> Run { get; init; }

    private static bool IsNotInit(MethodInfo setMethod)
    {
        return !setMethod
            .ReturnParameter.GetRequiredCustomModifiers()
            .Contains(typeof(IsExternalInit));
    }
}
