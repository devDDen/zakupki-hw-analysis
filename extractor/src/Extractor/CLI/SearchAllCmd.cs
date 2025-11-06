using System.Text;

namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    public static int RunSearchAllCmd(string queryfile, string baseWorkdir, string? publishDate, int retries)
    {
        try
        {
            queryfile = Path.GetFullPath(queryfile);
            baseWorkdir = Path.GetFullPath(baseWorkdir);
            Directory.CreateDirectory(baseWorkdir);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}");
            return 1;
        }

        try
        {
            var resultCsvPath = SearchAllCmd(queryfile, baseWorkdir, publishDate, retries);
            Console.WriteLine($"Search completed. 'merged.csv' location: {resultCsvPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            return 2;
        }

        return 0;
    }

    static string SearchAllCmd(string queryfile, string baseWorkdir, string? publishDate, int retries)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // For Windows-1251 encoding support

        var workdirs = new List<string>();
        var csvs = new List<string>();

        int i = 0;
        using var reader = new StreamReader(queryfile);
        do
        {
            var query = reader.ReadLine();
            if (string.IsNullOrEmpty(query)) { break; }

	        Console.WriteLine($"Processing query: '{query}'");

            try
            {
                i++;
                string workdir = Path.Combine(baseWorkdir, i.ToString());

                using var searcher = new Searcher(workdir);
                searcher.SearchAndMakeCsv(query, publishDate, retries);

                workdirs.Add(workdir);
                csvs.Add(searcher.GetMergedFilePath());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to process query {query}: {ex.Message}");
            }

        } while (!ShouldStop);

        var merger = new CsvMerger<Searcher.CsvData>(Searcher.CsvCfg);

        string outfile = Path.Combine(baseWorkdir, "mergerd.csv");
        merger.Merge(outfile, csvs);

        foreach (string workdir in workdirs)
        {
            Directory.Delete(workdir, recursive: true);
        }

        return outfile;
    }
}
