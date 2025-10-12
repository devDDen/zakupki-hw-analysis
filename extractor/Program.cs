using System.CommandLine;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using OpenQA.Selenium.Chrome;
using zakupki_extractor.pages;

namespace zakupki_extractor;

class Program
{
    static void GrabInfo(string id, string outdir)
    {
        var op = new ChromeOptions();
        op.AddArgument("--headless");

        var driver = new ChromeDriver(options: op);

        var json = Utils.ExecuteWithRetry(() => CommonInfo.GrabCommonInfo(driver, id));

        if (json["stage"]?.GetValue<string?>()?.Equals("Определение поставщика завершено") ?? false)
        {
            json["auction_info"] = Utils.ExecuteWithRetry(() => SupplierResults.GrabSupplierResults(driver, id));
        }

        driver.Quit();

        var encoderSettings = new TextEncoderSettings();
        encoderSettings.AllowRange(UnicodeRanges.BasicLatin);
        encoderSettings.AllowRange(UnicodeRanges.Cyrillic);
        encoderSettings.AllowRange(UnicodeRanges.MathematicalOperators);

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(encoderSettings)
        };

        string output = JsonSerializer.Serialize(json, options);

        if (outdir == "stdout")
        {
            Console.WriteLine(output);
        }
        else
        {
            try
            {
                File.WriteAllText($"{outdir}/{id}.json", output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine(output);
            }
        }
    }

    static int Main(string[] args)
    {
        Option<string> idOption = new("--id")
        {
            Description = "Reg number.",
            Required = true,
        };

        Option<string> outdirOption = new("--outdir")
        {
            Description = "Out directory or 'stdout'.",
            DefaultValueFactory = parseResult => "stdout",
        };

        RootCommand rootCommand = new("zakupki.gov.ru parser");

        Command grabCommand = new("grab", "Get info from zakupki.gov.ru by id")
        {
            idOption,
            outdirOption,
        };
        rootCommand.Add(grabCommand);

        grabCommand.SetAction(parseResult =>
        {
            var id = parseResult.GetRequiredValue(idOption);
            var outdir = parseResult.GetRequiredValue(outdirOption);

            if (outdir != "stdout")
            {
                outdir = Path.GetFullPath(outdir);
                try
                {
                    Directory.CreateDirectory(outdir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }

            GrabInfo(id, outdir);
        });

        return rootCommand.Parse(args).Invoke();
    }
}
