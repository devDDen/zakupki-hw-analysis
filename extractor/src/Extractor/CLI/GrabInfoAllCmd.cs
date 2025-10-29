using System.Diagnostics;
using CsvHelper;
using Extractor.FileStorageProvider;

namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    public static int RunGrabAllCmd(string inputCsv, OutFormat outformat, string outdir, int retries)
    {
        if (!File.Exists(inputCsv))
        {
            Console.Error.WriteLine($"File {inputCsv} is not exists");
            return 1;
        }

        switch (outformat)
        {
            case OutFormat.files:
                {
                    string? fullpath = GrabInfoCommon.PrepareDirectory(outdir);
                    if (fullpath == null) { return 1; }
                    outdir = fullpath;
                    break;
                }
            case OutFormat.zip:
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

        try
        {
            GrabInfoAllCmd(inputCsv, outformat, outdir, retries);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            return 2;
        }

        return 0;
    }

    internal static void GrabInfoAllCmd(string inputCsv, OutFormat outformat, string outdir, int retries)
    {
        using var grabber = new Grabber();

        using IFileStorageProvider fileStorageProvider = outformat switch
        {
            OutFormat.files => new DirectoryFileStorageProvider(outdir),
            OutFormat.zip => new ZipFileStorageProvider(outdir),
            _ => throw new ArgumentException("Unsupported format option"),
        };
        using var inputReader = new StreamReader(inputCsv);
        using var csvReader = new CsvReader(inputReader, Searcher.CsvCfg);
        var records = csvReader.GetRecords<Searcher.CsvData>();
        foreach (var record in records)
        {
            var id = record.Id.Substring(1); // skip first № symbol

            Debug.WriteLine($"Process id: {id}");

            var json = GrabInfoCommon.GrabInfo(grabber, id, retries);
            if (json == null) { continue; }

            string output = GrabInfoCommon.SerializeJson(json);

            try
            {
                fileStorageProvider.Store(GrabInfoCommon.MakeFilename(id), output);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}");
                break;
            }
        }
    }
}
