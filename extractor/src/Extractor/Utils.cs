using System.Diagnostics;
using OpenQA.Selenium;

namespace Extractor;

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
                    Debug.WriteLine($"Retry {tries}, wait {retryInterval}s");
                    Thread.Sleep(retryInterval);
                }
                return func();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Retry got an exception {e.Message}");
                if (e.Message.Contains("An unknown exception was encountered sending an HTTP request to the remote WebDriver server for URL"))
                {
                    throw;
                }
                lastException = e;
                tries++;
                retryInterval *= 2;
            }
        } while (tries < numberOfRetries);

        throw new InvalidOperationException($"Error after {tries} tries: {lastException}");
    }

    public static void ExecuteWithRetry(Action func, int numberOfRetries = 1)
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
                    Debug.WriteLine($"Retry {tries}, wait {retryInterval}s");
                    Thread.Sleep(retryInterval);
                }
                func();
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Retry got an exception {e.Message}");
                if (e.Message.Contains("An unknown exception was encountered sending an HTTP request to the remote WebDriver server for URL"))
                {
                    throw;
                }
                lastException = e;
                tries++;
                retryInterval *= 2;
            }
        } while (tries < numberOfRetries);

        throw new InvalidOperationException($"Error after {tries} tries: {lastException}");
    }

    public static IWebElement? GetSectionTitle(IWebElement parent)
    {
        try
        {
            return parent.FindElement(By.ClassName("section__title"));
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    public static IWebElement? GetSectionInfo(IWebElement parent)
    {
        try
        {
            return parent.FindElement(By.ClassName("section__info"));
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    public static IWebElement? FindElement(IWebElement parent, By by)
    {
        try
        {
            return parent.FindElement(by);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
