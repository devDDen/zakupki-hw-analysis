using Extractor.FileStorageProvider;

namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    public static int RunGrabInfoCmd(string id, OutFormatStdout outformat, string? outdir, int retries)
    {
        if (outformat != OutFormatStdout.stdout)
        {
            if (outdir == null)
            {
                Console.Error.WriteLine($"--outformat is not stdout, but outdir is not specified");
                return 1;
            }

            switch (outformat)
            {
                case OutFormatStdout.file:
                    {
                        string? fullpath = GrabInfoCommon.PrepareDirectory(outdir);
                        if (fullpath == null) { return 1; }
                        outdir = fullpath;
                        break;
                    }
                case OutFormatStdout.zip:
                    {
                        string? fullpath = GrabInfoCommon.PrepareDirectoryAndCheckZip(outdir);
                        if (fullpath == null) { return 1; }
                        outdir = fullpath;
                        break;
                    }
                default:
                    Console.Error.WriteLine("Unsupported format option");
                    return 1;
            }
        }

        try
        {
            GrabInfoCmd(id, outformat, outdir, retries);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            return 2;
        }

        return 0;
    }

    internal static void GrabInfoCmd(string id, OutFormatStdout outformat, string? outdir, int retries)
    {
        using var grabber = new Grabber();

        var json = GrabInfoCommon.GrabInfo(grabber, id, retries);
        if (json == null) { return; }

        string output = GrabInfoCommon.SerializeJson(json);

        if (outformat == OutFormatStdout.stdout)
        {
            Console.WriteLine(output);
            return;
        }

        try
        {
            if (outdir == null)
            {
                ArgumentNullException.ThrowIfNull(outdir, nameof(outdir));
            }

            using IFileStorageProvider fileStorageProvider = outformat switch
            {
                OutFormatStdout.file => new DirectoryFileStorageProvider(outdir),
                OutFormatStdout.zip => new ZipFileStorageProvider(outdir),
                _ => throw new ArgumentException("Invalid format option"),
            };

            fileStorageProvider.Store(GrabInfoCommon.MakeFilename(id), output);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}");
            Console.WriteLine(output);
        }
    }
}
