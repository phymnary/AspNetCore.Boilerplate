namespace AspNetCore.Boilerplate.EntityFrameworkCore.Extensions;

internal static class GuidExtensions
{
    public static Guid? NullIfEmpty(this Guid? value)
    {
        return value == Guid.Empty ? null : value;
    }
}
