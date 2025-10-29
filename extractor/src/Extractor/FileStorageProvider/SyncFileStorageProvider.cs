namespace Extractor.FileStorageProvider;

public class SyncFileStorageProvider(IFileStorageProvider fileStorageProvider) : IFileStorageProvider
{
    private static readonly object _lock = new();
    private IFileStorageProvider FileStorageProvider { get; init; } = fileStorageProvider;

    public void Store(string filename, string data)
    {
        lock (_lock)
        {
            FileStorageProvider.Store(filename, data);
        }
    }

    public void Dispose()
    {
        FileStorageProvider.Dispose();
    }
}
