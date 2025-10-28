using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using Extractor.FileStorageProvider;

namespace Extractor;

class Program
{
    static void GrabInfo(string id, string outdir, int retries)
    {
        using var grabber = new Grabber();
        var json = grabber.GrabInfo(id, retries);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
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

    static void GrabAll(string input, OutFormat outformat, string outdir, int retries)
    {
        using var grabber = new Grabber();

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        using IFileStorageProvider fileStorageProvider = outformat switch
        {
            OutFormat.files => new DirectoryFileStorageProvider(outdir),
            OutFormat.zip => new ZipFileStorageProvider(Path.Combine(outdir, "archive.zip")),
            _ => throw new ArgumentException("Ivalid format option"),
        };
        using var inputReader = new StreamReader(input);
        using var csvReader = new CsvReader(inputReader, Searcher.CsvCfg);
        var records = csvReader.GetRecords<Searcher.CsvData>();
        foreach (var record in records)
        {
            var id = record.Id.Substring(1); // skip first № symbol

            Debug.WriteLine($"Process id: {id}");

            JsonObject json;
            try
            {
                json = grabber.GrabInfo(id, retries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fail to process id: {id}. {ex.Message}");
                Console.Error.WriteLine($"Fail to process id: {id}");
                continue;
            }

            string output = JsonSerializer.Serialize(json, options);

            fileStorageProvider.Store($"{id}.json", output);
        }
    }

    public enum OutFormat
    {
        files,
        zip,
    }

    static int Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        Option<string> idOption = new("--id")
        {
            Description = "Reg number.",
            Required = true,
        };

        Option<string> outdirOrStdoutOption = new("--outdir")
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
            outdirOrStdoutOption,
            retriesOption,
        };
        rootCommand.Add(grabCommand);

        grabCommand.SetAction(parseResult =>
        {
            var id = parseResult.GetRequiredValue(idOption);
            var outdir = parseResult.GetRequiredValue(outdirOrStdoutOption);
            var retries = parseResult.GetRequiredValue(retriesOption);

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

        Option<string> inputCSVOption = new("--input")
        {
            Description = "Path to merged.csv.",
            Required = true,
        };

        Option<OutFormat> outformatOption = new("--outformat")
        {
            Description = "Output format.",
            Required = true,
        };

        Option<string> outdirOption = new("--outdir")
        {
            Description = "Out directory.",
            Required = true,
        };

        Command grabAllCommand = new("grab-all", "Get info by ids from csv (after search)")
        {
            inputCSVOption,
            outformatOption,
            outdirOption,
            retriesOption,
        };
        rootCommand.Add(grabAllCommand);

        grabAllCommand.SetAction(parseResult =>
        {
            var input = parseResult.GetRequiredValue(inputCSVOption);
            var outformat = parseResult.GetRequiredValue(outformatOption);
            var outdir = parseResult.GetRequiredValue(outdirOption);
            var retries = parseResult.GetValue(retriesOption);

            if (!File.Exists(input))
            {
                Console.WriteLine($"File {input} is not exists");
                return 1;
            }

            outdir = Path.GetFullPath(outdir);
            try
            {
                Directory.CreateDirectory(outdir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                return 1;
            }

            GrabAll(input, outformat, outdir, retries);
            return 0;
        });

        return rootCommand.Parse(args).Invoke();
    }
}
