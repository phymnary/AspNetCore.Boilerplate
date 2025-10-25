using System.Text;

namespace AspNetCore.Boilerplate.Extensions;

public static class UriBuilderExtensions
{
    public static UriBuilder WithQueries(
        this UriBuilder builder,
        Dictionary<string, string> queries
    )
    {
        var sb = new StringBuilder();

        foreach (var pair in queries)
        {
            sb.Append($"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}");
        }

        var queryToAppend = sb.ToString();

        if (builder.Query is { Length: > 1 })
            builder.Query = builder.Query + "&" + queryToAppend;
        else
            builder.Query = queryToAppend;

        return builder;
    }
}
