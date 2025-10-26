namespace Extractor.FileStorageProvider;

interface IFileStorageProvider : IDisposable
{
    void Store(string filename, string data);
    void IDisposable.Dispose() { }
}
