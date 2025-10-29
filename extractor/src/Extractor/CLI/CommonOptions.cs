using System.CommandLine;

namespace Extractor.CLI;

internal class CommonOptions
{
    internal static Option<int> RetriesOption = new("--retries")
    {
        Description = "Number or retries with exponential delay.",
        DefaultValueFactory = parseResult => 3,
    };
}
