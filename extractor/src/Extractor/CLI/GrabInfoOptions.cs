using System.CommandLine;
using System.ComponentModel.DataAnnotations;

namespace Extractor.CLI;

internal class GrabInfoOptions
{
    internal static Option<OutFormatStdout> OutformatStdoutOption = new("--outformat")
    {
        Description = "Output format.",
        DefaultValueFactory = parsedResult => OutFormatStdout.stdout,
    };

    internal static Option<string?> OutdirOrStdoutOption = new("--outdir")
    {
        Description = "Out directory or zip or 'stdout'.",
        DefaultValueFactory = parseResult => null,
    };

    internal static Option<OutFormat> OutformatOption = new("--outformat")
    {
        Description = "Output format.",
        Required = true,
    };

    internal static Option<string> OutdirOption = new("--outdir")
    {
        Description = "Out directory or zip.",
        Required = true,
    };

    internal static Option<string> IdOption = new("--id")
    {
        Description = "Reg number.",
        Required = true,
    };

    internal static Option<string> InputCSVOption = new("--input")
    {
        Description = "Path to merged.csv.",
        Required = true,
    };
}
