
using System.Text.Json.Nodes;
using Extractor.pages;
using OpenQA.Selenium.Chrome;

namespace Extractor;

public class Grabber : IDisposable
{
    private ChromeDriver Driver { get; init; }
    private string Outdir { get; init; }

    public Grabber(string outdir)
    {
        var op = new ChromeOptions();
        op.AddArgument("--headless");

        Outdir = outdir;

        Driver = new ChromeDriver(options: op);

        var uri = new UriBuilder($"https://zakupki.gov.ru/");
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Driver.Navigate().GoToUrl(uri.ToString());

        Utils.SSLCertPopupClose(Driver);
    }

    public JsonObject GrabInfo(string id, int retries = 1)
    {
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Utils.ExecuteWithRetry(() => CommonInfo.GoToCommonInfo(Driver, id), retries);

        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);
        var json = Utils.ExecuteWithRetry(() => CommonInfo.GrabCommonInfo(Driver, id), retries);

        if (json["stage"]?.GetValue<string?>()?.Equals("Определение поставщика завершено") ?? false)
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Utils.ExecuteWithRetry(() => SupplierResults.GoToSupplierResults(Driver, id), retries);

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);
            json["auction_info"] = Utils.ExecuteWithRetry(() => SupplierResults.GrabSupplierResults(Driver), retries);
        }

        return json;
    }

    public void Dispose()
    {
        Driver.Quit();
    }
}
