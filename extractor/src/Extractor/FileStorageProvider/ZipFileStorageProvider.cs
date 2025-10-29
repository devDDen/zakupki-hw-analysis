
using System.IO.Compression;

namespace Extractor.FileStorageProvider;

class ZipFileStorageProvider : IFileStorageProvider, IDisposable
{
    private ZipArchive Archive { get; init; }
    private CompressionLevel CompressionLevel { get; init; }
    public bool Overwrite { get; set; }

    public ZipFileStorageProvider(string archivePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, bool overwrite = false)
    {
        CompressionLevel = compressionLevel;
        Overwrite = overwrite;

        if (!overwrite && File.Exists(archivePath))
        {
            Archive = new ZipArchive(File.Open(archivePath, FileMode.Open, FileAccess.ReadWrite), ZipArchiveMode.Update);
        }
        else
        {
            Archive = new ZipArchive(File.Create(archivePath), ZipArchiveMode.Create);
        }
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
