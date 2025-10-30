using System.Text;

namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    public static int RunSearchCmd(string query, string workdir, string? publishDate, int retries)
    {
        try
        {
            workdir = Path.GetFullPath(workdir);
            Directory.CreateDirectory(workdir);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}");
            return 1;
        }

        try
        {
            var resultCsvPath = SearchCmd(query, workdir, publishDate, retries);
            Console.WriteLine($"Search completed. 'merged.csv' location: {resultCsvPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            return 2;
        }

        return 0;
    }

    static string SearchCmd(string query, string workdir, string? publishDate, int retries)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // For Windows-1251 encoding support

        using var searcher = new Searcher(workdir);
        searcher.SearchAndMakeCsv(query, publishDate, retries);
        return searcher.GetMergedFilePath();
    }
}
