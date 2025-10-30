namespace Extractor.CLI;

internal partial class ExtractorCLI
{
    public static int RunMergeCsvCmd(string output, string[] input)
    {
        try
        {
            var dirname = Path.GetDirectoryName(output);
            if (!string.IsNullOrEmpty(dirname))
            {
                Directory.CreateDirectory(dirname);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fail to create directory for {output}: {ex.Message}");
            return 1;
        }

        try
        {
            MergeCsvCmd(output, input);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            return 2;
        }

        return 0;
    }

    static void MergeCsvCmd(string output, string[] input)
    {
        var merger = new CsvMerger<Searcher.CsvData>(Searcher.CsvCfg);
        merger.Merge(output, input);
    }
}
