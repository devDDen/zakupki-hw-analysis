using System.Diagnostics;
using Extractor.FileStorageProvider;
using OpenQA.Selenium;

namespace Extractor.CLI;

internal class GrubInfoThreadedContext(SyncFileStorageProvider fileStorageProvider, int retries, AutoResetEvent doneEvent) : IDisposable
{
    private Grabber Grabber { get; set; } = new();
    private SyncFileStorageProvider FileStorageProvider { get; init; } = fileStorageProvider;
    private int Retries { get; init; } = retries;
    private AutoResetEvent DoneEvent { get; set; } = doneEvent;
    private int Counter { get; set; } = 0;

    private void ProcessCounter()
    {
        if (Counter >= 5000)
        {
            Grabber.Dispose();
            Grabber = new();
            Counter = 0;
        }
        else
        {
            Counter++;
        }
    }

    public void GrabId(object? threadContext)
    {
        string id = (string)threadContext!;
        Debug.WriteLine($"Process id: {id}");
        try
        {
            ProcessCounter();

            var json = GrabInfoCommon.GrabInfo(Grabber, id, Retries);

            if (json != null)
            {
                Debug.WriteLine($"Serialize id: {id}");
                FileStorageProvider.Store(GrabInfoCommon.MakeFilename(id), GrabInfoCommon.SerializeJson(json));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception occured while processing id '{id}': {ex.Message}");
        }
        finally
        {
            DoneEvent.Set();
        }
    }

    public void Dispose()
    {
        Grabber.Dispose();
    }
}
