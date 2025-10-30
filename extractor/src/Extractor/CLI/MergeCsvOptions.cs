using System.CommandLine;

namespace Extractor.CLI;

internal class MergeCsvOptions
{
    internal static Option<string> OutOption = new("--out")
    {
        Description = "Out .csv filename.",
        Required = true,
    };

    internal static Option<string[]> InputOption = new("--input")
    {
        Description = "List of .csv files to merge.",
        Required = true,
        AllowMultipleArgumentsPerToken = true,
    };
}
