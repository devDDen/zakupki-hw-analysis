using System.Diagnostics;
using CsvHelper;
using Extractor.FileStorageProvider;

namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    internal static void GrabInfoThreaded(int j, string inputCsv, OutFormat outformat, string outdir, int retries)
    {
        ThreadPool.SetMaxThreads(j, j);

        using SyncFileStorageProvider syncFileStorageProvider = new(outformat switch
        {
            OutFormat.files => new DirectoryFileStorageProvider(outdir),
            OutFormat.zip => new ZipFileStorageProvider(outdir),
            _ => throw new ArgumentException("Invalid format option"),
        });

        var doneEvents = new AutoResetEvent[j];
        var contexts = new GrubInfoThreadedContext[j];

        for (int i = 0; i < j; i++)
        {
            doneEvents[i] = new AutoResetEvent(true);
            contexts[i] = new GrubInfoThreadedContext(syncFileStorageProvider, retries, doneEvents[i]);
        }

        Debug.WriteLine("Initialization complete");

        using var inputReader = new StreamReader(inputCsv);
        using var csvReader = new CsvReader(inputReader, Searcher.CsvCfg);
        var records = csvReader.GetRecords<Searcher.CsvData>();
        foreach (var record in records)
        {
            string id = record.Id.Substring(1); // skip first № symbol

            Debug.WriteLine($"Prepare id: {id}");

            int freeThread = WaitHandle.WaitAny(doneEvents);
            ThreadPool.QueueUserWorkItem(contexts[freeThread].GrabId, id);
        }

        WaitHandle.WaitAll(doneEvents);

        for (int i = 0; i < j; i++)
        {
            contexts[i].Dispose();
        }
    }
}
