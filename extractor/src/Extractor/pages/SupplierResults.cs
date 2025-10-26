using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OpenQA.Selenium;

namespace Extractor.pages;

public class SupplierResults
{
    public class AuctionInfo
    {
        public bool Success { get; set; } = true;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reason { get; set; }
        public List<Participant>? Participants { get; set; }
    }

    public class Participant
    {
        public required string Id { get; set; }
        public required string Place { get; set; }
        public required string Bid { get; set; }
    }

    public static void GoToSupplierResults(IWebDriver driver, string id)
    {
        var uri = new UriBuilder($"https://zakupki.gov.ru/epz/order/notice/ea20/view/supplier-results.html?regNumber={id}");
        driver.Navigate().GoToUrl(uri.ToString());
    }

    public static JsonNode GrabSupplierResults(IWebDriver driver)
    {
        var (auctionInfo, protocolUri) = GetInfo(driver);
        auctionInfo.Participants = GetParticipants(driver);

        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.SerializeToNode(auctionInfo, serializeOptions)!;
    }

    static List<Participant> GetParticipants(IWebDriver driver)
    {
        var list = new List<Participant>();

        var bidsSection = GetBidsSection(driver);
        if (bidsSection == null)
        {
            return list;
        }

        var rows = bidsSection.FindElements(By.CssSelector(".blockInfo__table > .tableBlock__body > .tableBlock__row"));
        foreach (var row in rows)
        {
            var columns = row.FindElements(By.CssSelector(".tableBlock__col"));

            var participantId = columns[0].Text;
            var participantPlace = columns[1].Text;
            var participantBid = columns[2].Text;

            list.Add(new Participant
            {
                Id = participantId,
                Place = participantPlace,
                Bid = participantBid,
            });
        }

        return list;
    }

    static (AuctionInfo, Uri?) GetInfo(IWebDriver driver)
    {
        Uri? protocolUri = null;
        var auctionInfo = new AuctionInfo();

        var blocks = driver.FindElements(By.CssSelector(".container .blockInfo .col"));
        foreach (var block in blocks)
        {
            var title = block.FindElement(By.ClassName("blockInfo__title"));

            if (!title.Text.StartsWith("Результат определения поставщика"))
            {
                continue;
            }

            var sections = block.FindElements(By.ClassName("blockInfo__section"));
            foreach (var section in sections)
            {
                var sectionTitle = Utils.GetSectionTitle(section);
                var sectionInfo = Utils.GetSectionInfo(section);

                if (sectionTitle == null || sectionInfo == null)
                {
                    continue;
                }

                switch (sectionTitle.Text)
                {
                    case "Наименование протокола определения поставщика (подрядчика, исполнителя)":
                        var linkEl = sectionInfo.FindElement(By.CssSelector("a"));
                        protocolUri = new Uri(linkEl.GetAttribute("href")!);
                        break;
                    case "Основание признания торгов несостоявшимися":
                        auctionInfo.Success = false;
                        auctionInfo.Reason = sectionInfo.Text;
                        break;
                    default:
                        break;
                }
            }
        }

        return (auctionInfo, protocolUri);
    }

    static IWebElement? GetBidsSection(IWebDriver driver)
    {
        try
        {
            return driver.FindElement(By.CssSelector("div[id^=supplier-def-result-participant-table-]"));
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
}
