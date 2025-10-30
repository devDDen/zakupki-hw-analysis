using CsvHelper;
using CsvHelper.Configuration;

namespace Extractor;

public class CsvMerger<T>(CsvConfiguration csvCfg)
{
    public CsvConfiguration CsvCfg { get; set; } = csvCfg;

    public void Merge(string outfile, IEnumerable<string> csvs)
    {
        bool isFirst = true;
        string[] firstHeader = null!;
        var allRecords = new HashSet<T>();

        foreach (var csv in csvs)
        {
            using var reader = new StreamReader(csv);
            using var csvReader = new CsvReader(reader, CsvCfg);

            if (CsvCfg.HasHeaderRecord)
            {
                csvReader.Read();
                csvReader.ReadHeader();
                var header = csvReader.HeaderRecord!;

                if (isFirst)
                {
                    firstHeader = header;
                    isFirst = false;
                }
                else
                {
                    if (!header.SequenceEqual(firstHeader))
                    {
                        Console.Error.WriteLine($"Skip: {csv} header is diffrent: '{ArrayToString(header)}' not equal '{ArrayToString(firstHeader)}'");
                        continue;
                    }
                }
            }

            var records = csvReader.GetRecords<T>();
            allRecords.UnionWith(records);
        }

        using var writer = new StreamWriter(outfile, append: false);
        using var csvWriter = new CsvWriter(writer, CsvCfg);
        csvWriter.WriteRecords(allRecords);
    }

    internal string ArrayToString(string[] arr)
    {
        return string.Join(CsvCfg.Delimiter, arr);
    }
}
