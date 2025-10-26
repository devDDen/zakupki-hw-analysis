using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

[assembly: InternalsVisibleTo("zakupki_extractor.Tests")]
namespace zakupki_extractor.pages;

public partial class ExtendedSearch
{
    [GeneratedRegex(@"(более\s)?(?<number>((\d{1,3}\s)*(\d{1,3})))\s(записей)")]
    private static partial Regex TotalNumberRegex();

    public static void ApplyFilters(IWebDriver driver, string query, string? publishDateFrom)
    {
        var queryParams = new Dictionary<string, string>();
        queryParams["searchString"] = query;
        queryParams["morphology"] = "on";
        queryParams["sortDirection"] = "true";
        queryParams["sortBy"] = "PUBLISH_DATE";
        queryParams["fz44"] = "on";

        if (publishDateFrom != null)
        {
            queryParams["publishDateFrom"] = publishDateFrom;
        }

        queryParams["ca"] = "on"; // работа комиссии
        queryParams["af"] = "on"; // подача заявок
        queryParams["pc"] = "on"; // закупка отменена
        queryParams["pa"] = "on"; // закупка завершена

        queryParams["currencyIdGeneral"] = "-1";

        var uri = new UriBuilder("https://zakupki.gov.ru/epz/order/extendedsearch/results.html");
        uri.Query = string.Join('&', queryParams.Select(pair => $"{pair.Key}={pair.Value}"));

        driver.Navigate().GoToUrl(uri.ToString());
    }

    public static bool DownloadCurrentSearchResults(WebDriver driver)
    {
        var downloadButton = driver.FindElement(By.ClassName("downLoad-search"));
        downloadButton.Click();

        SelectDownloadFilters(driver);

        bool hasMore = true;

        var total = driver.FindElement(By.ClassName("search-results__total"));
        int? totalNumber = ExtractTotalNumber(total.Text);
        ArgumentNullException.ThrowIfNull(totalNumber);

        var csvs = driver.FindElements(By.CssSelector(".link, .csvDownload"));
        foreach (var csvLink in csvs)
        {
            driver.ExecuteScript("arguments[0].scrollIntoView(true);", csvLink);

            string? upperNumberAttr = csvLink.GetAttribute("data-to");
            if (upperNumberAttr != null)
            {
                int upperNumber = int.Parse(upperNumberAttr);
                hasMore &= upperNumber % 500 == 0 && upperNumber != totalNumber;
            }

            Debug.WriteLine("Start downloading file");
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            csvLink.Click(); // waiting for download

            stopWatch.Stop();
            Debug.WriteLine($"Download completed in {stopWatch.Elapsed.Milliseconds}ms");
        }

        return hasMore;
    }

    internal static void SelectDownloadFilters(IWebDriver driver)
    {
        var checkbox = driver.FindElement(By.CssSelector("#csvSettingTree .dynatree-checkbox")); // select all
        checkbox.Click();

        var cont = driver.FindElement(By.Id("btn-primary"));
        cont.Click();
    }

    internal static int? ExtractTotalNumber(string totalText)
    {
        var extractTotalNumber = TotalNumberRegex();
        Match totalNumberMatch = extractTotalNumber.Match(totalText);
        if (totalNumberMatch.Success)
        {
            var matchStr = totalNumberMatch.Groups["number"].Value;
            int number = int.Parse(matchStr.Replace(" ", ""));
            return number;
        }
        return null;
    }
}
