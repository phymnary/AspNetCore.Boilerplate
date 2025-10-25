namespace AspNetCore.Boilerplate.Api;

public class AspErrorDto
{
    public required AspErrorMessage Error { get; init; }
}

public class AspErrorMessage
{
    public required string Message { get; init; }

    public string? Code { get; init; }

    public string? Detail { get; init; }

    public IEnumerable<AspValidationErrorDetail>? InvalidParameters { get; init; }
}

public class AspValidationErrorDetail
{
    public required string Property { get; init; }

    public required string Message { get; init; }
}
