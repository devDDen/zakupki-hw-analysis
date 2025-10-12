using System.Text.Json.Nodes;
using OpenQA.Selenium;

namespace zakupki_extractor.pages;

public class CommonInfo
{
    public static JsonObject GrabCommonInfo(IWebDriver driver, string id)
    {
        var json = new JsonObject();
        json["id"] = id;

        var uri = new UriBuilder($"https://zakupki.gov.ru/epz/order/notice/ea20/view/common-info.html?regNumber={id}");
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);
        driver.Navigate().GoToUrl(uri.ToString());
        Utils.SSLCertPopupClose(driver);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(50);

        var blocks = driver.FindElements(By.CssSelector(".container .blockInfo .col"));
        foreach (var block in blocks)
        {
            var title = block.FindElement(By.ClassName("blockInfo__title"));

            if (title.Text == "Информация об объекте закупки")
            {
                json["products"] = ParseProducts(block);
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

                string val = sectionInfo.Text;
                switch (sectionTitle.Text)
                {
                    case "Способ определения поставщика (подрядчика, исполнителя)":
                        json["competition"] = val;
                        break;
                    case string when val.StartsWith("Наименование электронной площадки"):
                        json["platform"] = val;
                        break;
                    case "Организация, осуществляющая размещение":
                        json["organization"] = val;
                        break;
                    case "Место нахождения":
                        json["organization_address"] = val;
                        break;
                    case "Этап закупки":
                        json["stage"] = val;
                        break;
                    case "Регион":
                        json["region"] = val;
                        break;
                    case "Дата и время начала срока подачи заявок":
                        json["competition_start"] = val;
                        break;
                    case "Дата и время окончания срока подачи заявок":
                        json["competition_end"] = val;
                        break;
                    case "Дата подведения итогов определения поставщика (подрядчика, исполнителя)":
                        json["result_date"] = val;
                        break;
                    case "Начальная (максимальная) цена контракта":
                        json["price"] = val;
                        break;
                    case "Валюта":
                        json["currency"] = val;
                        break;
                    case "Срок исполнения контракта":
                        json["supply_date"] = val;
                        break;
                    case "Закупка за счет бюджетных средств":
                        json["budget_financing"] = val;
                        break;
                    default:
                        break;
                }
            }
        }
        return json;
    }

    static JsonArray ParseProducts(IWebElement block)
    {
        var array = new JsonArray();

        var trs = block.FindElements(By.CssSelector("#positionKTRU div[id^=purchaseObjectTruTable] > .blockInfo__table > .tableBlock__body > tr"));

        for (int i = 0; i < trs.Count; ++i)
        {
            var productInfo = trs[i];

            var productJson = new JsonObject();
            var columns = productInfo.FindElements(By.ClassName("tableBlock__col"));
            productJson["code"] = columns[1].Text;
            productJson["name"] = columns[2].Text;
            productJson["units"] = columns[3].Text;
            productJson["count"] = columns[4].Text;
            productJson["price"] = columns[5].Text;
            productJson["cost"] = columns[6].Text;

            if (i + 1 >= trs.Count || !trs[i + 1].GetAttribute("class")!.StartsWith("truInfo_"))
            {
                productJson["specifications"] = new JsonArray();
                array.Add(productJson);
                continue;
            }

            var productSpec = trs[++i];
            var productSpecifications = new JsonArray();
            var specRows = productSpec.FindElements(By.CssSelector(".tableBlock__col table > tbody > tr"));

            string prevName = "first";
            foreach (var spec in specRows)
            {
                var specification = new JsonObject();
                var specCols = spec.FindElements(By.CssSelector("td"));

                if (specCols.Count == 0)
                {
                    continue;
                }

                var specColsText = specCols.Select(el => Utils.GetText(el)).ToList();
                if (specCols.Count == 1)
                {
                    specification["name"] = prevName;
                    specification["value"] = specColsText[0];
                }
                else
                {
                    prevName = specColsText[0];
                    specification["name"] = specColsText[0];
                    specification["value"] = specColsText[1];
                    if (!string.IsNullOrEmpty(specColsText[2]))
                    {
                        specification["units"] = specColsText[2];
                    }
                }

                productSpecifications.Add(specification);
            }
            productJson["specifications"] = productSpecifications;
            array.Add(productJson);
        }

        return array;
    }
}
