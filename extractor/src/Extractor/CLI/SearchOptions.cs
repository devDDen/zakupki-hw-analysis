using System.CommandLine;

namespace Extractor.CLI;

internal class SearchOptions
{
    internal static Option<string> QueryOption = new("--query")
    {
        Description = "Query to search.",
        Required = true,
    };

    internal static Option<string> WorkdirOption = new("--workdir")
    {
        Description = "Directory to store downloaded files.",
        Required = true,
    };

    internal static Option<string?> FromPublishDateOption = new("--from-publish-date")
    {
        Description = "Publish date to start search in dd.mm.yyyy format.",
        DefaultValueFactory = parseResult => null,
    };

    internal static Option<string> QueryFileOption = new("--query-file")
    {
        Description = "File with queries separated by new line.",
        Required = true,
    };
}
