
using System.IO.Compression;
using Extractor.FileStorageProvider;

class ZipFileStorageProvider : IFileStorageProvider, IDisposable
{
    private ZipArchive Archive { get; init; }
    private CompressionLevel CompressionLevel { get; init; }

    public ZipFileStorageProvider(string archivePath, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var writer = File.Create(archivePath);
        Archive = new ZipArchive(writer, ZipArchiveMode.Create);
        CompressionLevel = compressionLevel;
    }

    public void Dispose()
    {
        Archive.Dispose();
    }

    public void Store(string filename, string data)
    {
        var entry = Archive.CreateEntry(filename, CompressionLevel);
        using var entryStream = entry.Open();
        using var entryWriter = new StreamWriter(entryStream);
        entryWriter.Write(data);
    }
}
