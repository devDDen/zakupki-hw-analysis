using System.Diagnostics;
using Extractor.FileStorageProvider;

namespace Extractor.CLI;

internal class GrubInfoThreadedContext(SyncFileStorageProvider fileStorageProvider, int retries, AutoResetEvent doneEvent) : IDisposable
{
    private Grabber Grabber { get; init; } = new();
    private SyncFileStorageProvider FileStorageProvider { get; init; } = fileStorageProvider;
    private int Retries { get; init; } = retries;
    private AutoResetEvent DoneEvent { get; set; } = doneEvent;

    public void GrabId(object? threadContext)
    {
        string id = (string)threadContext!;
        Debug.WriteLine($"Process id: {id}");
        try
        {
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
