namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    static private bool ShouldStop { get; set; } = false;

    internal static void Stop()
    {
        ShouldStop = true;
    }
}
