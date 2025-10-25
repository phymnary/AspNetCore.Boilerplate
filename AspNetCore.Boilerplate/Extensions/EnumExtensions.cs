using System.Linq.Expressions;
using AspNetCore.Boilerplate.Exceptions;

namespace AspNetCore.Boilerplate.Extensions;

public static class EnumExtensions
{
    public static int Parse<T>(this T? value, T defaultValue = default)
        where T : struct, Enum
    {
        var parsing = value ?? defaultValue;

        if (!Enum.IsDefined(parsing))
            throw new AspBadRequestException(
                $"Enum {typeof(T).Name} with value {value} is not defined"
            );

        return CompiledLambdaFunc(parsing);
    }

    private static int CompiledLambdaFunc<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return CachedCast<TEnum>.Cast(value);
    }

    private static class CachedCast<T>
        where T : struct, Enum
    {
        public static readonly Func<T, int> Cast = GenerateFunc<T>();
    }

    private static Func<TEnum, int> GenerateFunc<TEnum>()
        where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(TEnum));

        var body = Expression.Convert(inputParameter, typeof(int));
        var lambda = Expression.Lambda<Func<TEnum, int>>(body, inputParameter);
        var func = lambda.Compile();

        return func;
    }
}
