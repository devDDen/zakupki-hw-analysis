namespace Extractor.FileStorageProvider;

class DirectoryFileStorageProvider(string basepath) : IFileStorageProvider
{
    public string BasePath { get; init; } = basepath;

    public void Store(string filename, string data)
    {
        File.WriteAllText(Path.Combine(BasePath, filename), data);
    }
}
