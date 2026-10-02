namespace Personalhandboksassistent.Services;

// Ser till att sökindexet uppdateras efter att dokument har ändrats.
public interface IIndexRefresher
{
    Task RefreshAsync(CancellationToken cancellationToken);
}

// Används när ingen indexer är konfigurerad; då finns inget att uppdatera.
public sealed class NoOpIndexRefresher : IIndexRefresher
{
    public Task RefreshAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
