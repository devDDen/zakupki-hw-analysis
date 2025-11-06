
using System.Text.Json.Nodes;
using Extractor.pages;
using OpenQA.Selenium.Chrome;

namespace Extractor;

public class Grabber : IDisposable
{
    private ChromeDriver Driver { get; init; }

    public Grabber()
    {
        var op = new ChromeOptions();
        op.AddArgument("--headless");

        Driver = new ChromeDriver(options: op);

        var uri = new UriBuilder($"https://zakupki.gov.ru/");
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Driver.Navigate().GoToUrl(uri.ToString());

        Utils.SSLCertPopupClose(Driver);
    }

    public JsonObject GrabInfo(string id, int retries = 1)
    {
        var json = Utils.ExecuteWithRetry(() =>
        {
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            CommonInfo.GoToCommonInfo(Driver, id);

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);
            return CommonInfo.GrabCommonInfo(Driver, id);
        }, retries);

        if (json["stage"]?.GetValue<string?>()?.Equals("Определение поставщика завершено") ?? false)
        {
            json["auction_info"] = Utils.ExecuteWithRetry(() =>
            {
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                SupplierResults.GoToSupplierResults(Driver, id);

                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);
                return SupplierResults.GrabSupplierResults(Driver);
            }, retries);
        }

        return json;
    }

    public void Dispose()
    {
        Driver.Quit();
    }
}
