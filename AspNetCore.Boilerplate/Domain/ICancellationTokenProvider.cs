namespace AspNetCore.Boilerplate.Domain;

public interface ICancellationTokenProvider
{
    void Set(CancellationToken cancellationToken);

    CancellationToken GetRequestAborted(CancellationToken cancellationToken);
}
