using OpenQA.Selenium;

namespace zakupki_extractor;

public class Utils
{
    public static string GetText(IWebElement el)
    {
        return (el.GetAttribute("textContent") ?? "").Trim();
    }

    public static void SSLCertPopupClose(IWebDriver driver)
    {
        try
        {
            var close = driver.FindElement(By.CssSelector("#sslCertificateChecker-right .closePopUp"));
            close.Click();
        }
        catch (NoSuchElementException)
        {
        }
    }

    public static TResult ExecuteWithRetry<TResult>(Func<TResult> func, int numberOfRetries = 1)
    {
        int tries = 0;
        var retryInterval = new TimeSpan(0, 0, 1);

        Exception? lastException = null;

        do
        {
            try
            {
                if (tries > 0)
                {
                    Thread.Sleep(retryInterval);
                }
                return func();
            }
            catch (Exception e)
            {
                lastException = e;
                tries++;
                retryInterval *= 2;
            }
        } while (tries < numberOfRetries);

        throw new InvalidOperationException($"Error after {tries} tries: {lastException}");
    }
}
