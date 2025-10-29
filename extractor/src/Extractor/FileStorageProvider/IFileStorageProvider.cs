namespace Extractor.FileStorageProvider;

public interface IFileStorageProvider : IDisposable
{
    void Store(string filename, string data);
    void IDisposable.Dispose() { }
}
