using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Extractor;

class Program
{
    static void GrabInfo(string id, string outdir, int retries)
    {
        using var grabber = new Grabber(outdir);
        var json = grabber.GrabInfo(id, retries);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        string output = JsonSerializer.Serialize(json, options);

        if (outdir == "stdout")
        {
            Console.WriteLine(output);
        }
        else
        {
            try
            {
                File.WriteAllText($"{outdir}/{id}.json", output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(output);
            }
        }
    }

    static void Search(string query, string workdir, string? publishDate, int retries)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // For Windows-1251 encoding support

        using var searcher = new Searcher(workdir);
        searcher.Search(query, publishDate, retries);
    }

    static int Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        Option<string> idOption = new("--id")
        {
            Description = "Reg number.",
            Required = true,
        };

        Option<string> outdirOption = new("--outdir")
        {
            Description = "Out directory or 'stdout'.",
            DefaultValueFactory = parseResult => "stdout",
        };

        Option<int> retriesOption = new("--retries")
        {
            Description = "Number or retries with exponential delay.",
            DefaultValueFactory = parseResult => 3,
        };

        RootCommand rootCommand = new("zakupki.gov.ru parser");

        Command grabCommand = new("grab", "Get info from zakupki.gov.ru by id")
        {
            idOption,
            outdirOption,
            retriesOption,
        };
        rootCommand.Add(grabCommand);

        grabCommand.SetAction(parseResult =>
        {
            var id = parseResult.GetRequiredValue(idOption);
            var outdir = parseResult.GetRequiredValue(outdirOption);
            var retries = parseResult.GetValue(retriesOption);

            if (outdir != "stdout")
            {
                outdir = Path.GetFullPath(outdir);
                try
                {
                    Directory.CreateDirectory(outdir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }

            GrabInfo(id, outdir, retries);
        });

        Option<string> queryOption = new("--query")
        {
            Description = "Query to search.",
            Required = true,
        };

        Option<string> workdirOption = new("--workdir")
        {
            Description = "Directory to store downloaded files.",
            Required = true,
        };

        Option<string?> fromPublishDateOption = new("--from-publish-date")
        {
            Description = "Publish date to start search in dd.mm.yyyy format.",
            DefaultValueFactory = parseResult => null,
        };

        Command searchCommand = new("search", "Search ids by query")
        {
            queryOption,
            workdirOption,
            fromPublishDateOption,
            retriesOption,
        };
        rootCommand.Add(searchCommand);

        searchCommand.SetAction(parseResult =>
        {
            var query = parseResult.GetRequiredValue(queryOption);
            var workdir = parseResult.GetRequiredValue(workdirOption);
            var publishDate = parseResult.GetValue(fromPublishDateOption);
            var retries = parseResult.GetValue(retriesOption);

            try
            {
                workdir = Path.GetFullPath(workdir);
                Directory.CreateDirectory(workdir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            Search(query, workdir, publishDate, retries);
        });

        return rootCommand.Parse(args).Invoke();
    }
}
