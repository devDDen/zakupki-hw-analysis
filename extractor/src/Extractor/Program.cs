using System.CommandLine;
using System.Diagnostics;
using Extractor.CLI;

namespace Extractor;

class Program
{
    static int Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        RootCommand rootCommand = new("zakupki.gov.ru parser");

        Command grabCommand = new("grab", "Get info from zakupki.gov.ru by id")
        {
            GrabInfoOptions.IdOption,
            GrabInfoOptions.OutformatStdoutOption,
            GrabInfoOptions.OutdirOrStdoutOption,
            CommonOptions.RetriesOption,
        };
        rootCommand.Add(grabCommand);

        grabCommand.SetAction(parseResult =>
        {
            var id = parseResult.GetRequiredValue(GrabInfoOptions.IdOption);
            var format = parseResult.GetRequiredValue(GrabInfoOptions.OutformatStdoutOption);
            var outdir = parseResult.GetValue(GrabInfoOptions.OutdirOrStdoutOption);
            var retries = parseResult.GetRequiredValue(CommonOptions.RetriesOption);

            ExtractorCLI.RunGrabInfoCmd(id, format, outdir, retries);
        });

        Command searchCommand = new("search", "Search ids by query")
        {
            SearchOptions.QueryOption,
            SearchOptions.WorkdirOption,
            SearchOptions.FromPublishDateOption,
            CommonOptions.RetriesOption,
        };
        rootCommand.Add(searchCommand);

        searchCommand.SetAction(parseResult =>
        {
            var query = parseResult.GetRequiredValue(SearchOptions.QueryOption);
            var workdir = parseResult.GetRequiredValue(SearchOptions.WorkdirOption);
            var publishDate = parseResult.GetValue(SearchOptions.FromPublishDateOption);
            var retries = parseResult.GetRequiredValue(CommonOptions.RetriesOption);

            return ExtractorCLI.RunSearchCmd(query, workdir, publishDate, retries);
        });

        Command grabAllCommand = new("grab-all", "Get info by ids from csv (after search)")
        {
            GrabInfoOptions.InputCSVOption,
            GrabInfoOptions.OutformatOption,
            GrabInfoOptions.OutdirOption,
            CommonOptions.RetriesOption,
            GrabInfoOptions.ParallelJobsOption,
        };
        rootCommand.Add(grabAllCommand);

        grabAllCommand.SetAction(parseResult =>
        {
            var input = parseResult.GetRequiredValue(GrabInfoOptions.InputCSVOption);
            var outformat = parseResult.GetRequiredValue(GrabInfoOptions.OutformatOption);
            var outdir = parseResult.GetRequiredValue(GrabInfoOptions.OutdirOption);
            var retries = parseResult.GetRequiredValue(CommonOptions.RetriesOption);
            var jobs = parseResult.GetRequiredValue(GrabInfoOptions.ParallelJobsOption);

            return ExtractorCLI.RunGrabAllCmd(input, outformat, outdir, retries, jobs);
        });

        return rootCommand.Parse(args).Invoke();
    }
}
