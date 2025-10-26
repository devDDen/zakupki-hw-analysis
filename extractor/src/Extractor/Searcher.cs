using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using OpenQA.Selenium.Chrome;
using Extractor.pages;

namespace Extractor;

public class Searcher : IDisposable
{
    private ChromeDriver Driver { get; init; }
    private string Workdir { get; init; }
    private string Outdir { get; init; }
    private static readonly CsvConfiguration CsvCfg = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        NewLine = Environment.NewLine,
    };

    public class CsvData
    {
        [Name("Реестровый номер закупки")]
        public required string Id { get; set; }
        [Name("Дата размещения")]
        public required string PublishDate { get; set; }
    }

    public Searcher(string workdir)
    {
        var op = new ChromeOptions();
        op.AddArgument("--headless");

        op.AddUserProfilePreference("download.default_directory", workdir); // only absolute path is allowed
        op.AddUserProfilePreference("download.prompt_for_download", false);
        op.AddUserProfilePreference("download.directory_upgrade", true);

        Workdir = workdir;
        Driver = new ChromeDriver(options: op);

        Outdir = Path.Combine(Workdir, "out");
        Directory.CreateDirectory(Outdir);

        var uri = new UriBuilder($"https://zakupki.gov.ru/");
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Driver.Navigate().GoToUrl(uri.ToString());

        Utils.SSLCertPopupClose(Driver);
    }

    public void Search(string query, string? publishDateFrom = null, int retries = 1)
    {
        bool hasMore;
        string? publishDate = publishDateFrom;
        do
        {
            hasMore = Utils.ExecuteWithRetry(() => DownloadSearchResultsWithRetry(query, publishDate), retries);
            Thread.Sleep(TimeSpan.FromMilliseconds(1000)); // Wait for filnalizing downloads
            MergeCSVs();
            publishDate = GetLastPublishDateFromMergedCSV();
            Debug.WriteLine($"Last publish date: {publishDate ?? "null"}");
        } while (hasMore);
    }

    public string GetMergedFilePath()
    {
        return Path.Combine(Outdir, "merged.csv");
    }

    internal bool DownloadSearchResultsWithRetry(string query, string? publishDate)
    {
        try
        {
            return DownloadSearchResults(query, publishDate);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            RemoveCSVs();
            throw;
        }
    }

    internal bool DownloadSearchResults(string query, string? publishDate)
    {
        ExtendedSearch.ApplyFilters(Driver, query, publishDate);
        return ExtendedSearch.DownloadCurrentSearchResults(Driver);
    }

    internal void MergeCSVs()
    {
        Debug.WriteLine("Merge CSVs");

        var recordSet = new HashSet<CsvData>();

        var oldMergedFilePath = GetMergedFilePath();
        if (File.Exists(oldMergedFilePath))
        {
            using (var reader = new StreamReader(oldMergedFilePath))
            using (var csvReader = new CsvReader(reader, CsvCfg))
            {
                var records = csvReader.GetRecords<CsvData>();
                recordSet.UnionWith(records);
            }

            ObsoleteMergedFile();
        }

        var files = Directory.GetFiles(Workdir, "*.csv");
        foreach (var filepath in files)
        {
            Debug.WriteLine($"Merge {Path.GetFileName(filepath)}");
            using var reader = new StreamReader(filepath, Encoding.GetEncoding(1251));
            using var csvReader = new CsvReader(reader, CsvCfg);
            var records = csvReader.GetRecords<CsvData>();
            recordSet.UnionWith(records);
        }

        using (var writer = new StreamWriter(GetMergedFilePath()))
        using (var csvWriter = new CsvWriter(writer, CsvCfg))
        {
            csvWriter.WriteHeader<CsvData>();
            csvWriter.NextRecord();
            csvWriter.WriteRecords(recordSet);
        }

        RemoveCSVs();
    }

    internal string? GetLastPublishDateFromMergedCSV()
    {
        using var reader = new StreamReader(GetMergedFilePath());
        using var csvReader = new CsvReader(reader, CsvCfg);
        var records = csvReader.GetRecords<CsvData>();

        var result = records.MaxBy(record => DateOnly.Parse(record.PublishDate));
        return result?.PublishDate;
    }

    private void RemoveCSVs()
    {
        var files = Directory.GetFiles(Workdir, "*.csv");
        foreach (var file in files)
        {
            File.Delete(file);
        }
    }

    private void ObsoleteMergedFile()
    {
        string mergedPath = GetMergedFilePath();
        string oldMergedPath = $"{mergedPath}.old";

        if (File.Exists(mergedPath))
        {
            if (File.Exists(oldMergedPath))
            {
                File.Delete(oldMergedPath);
            }
            File.Move(mergedPath, oldMergedPath);
        }
    }

    public void Dispose()
    {
        Driver.Quit();
    }
}
