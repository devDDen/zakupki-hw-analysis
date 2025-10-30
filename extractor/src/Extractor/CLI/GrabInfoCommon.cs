using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Extractor.CLI;

internal enum OutFormatStdout
{
    file,
    zip,
    stdout,
}

internal enum OutFormat
{
    files,
    zip,
}

internal class GrabInfoCommon
{
    internal static JsonObject? GrabInfo(Grabber grabber, string id, int retries)
    {
        try
        {
            return grabber.GrabInfo(id, retries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fail to process id: {id}. {ex.Message}");
            Console.Error.WriteLine($"Fail to process id: {id}");
            return null;
        }
    }

    internal static string SerializeJson(JsonObject json)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        return JsonSerializer.Serialize(json, options);
    }

    internal static string MakeFilename(string id)
    {
        return $"{id}.json";
    }

    internal static string? PrepareDirectory(string dir)
    {
        try
        {
            return Directory.CreateDirectory(dir).FullName;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fail to create directory {dir}: {ex.Message}");
            return null;
        }
    }

    internal static string? PrepareDirectoryAndCheckZip(string zipfile)
    {
        if (Path.GetExtension(zipfile) != ".zip")
        {
            Console.Error.WriteLine($"Specified file is not .zip");
            return null;
        }

        try
        {
            var dirname = Path.GetDirectoryName(zipfile);
            if (!string.IsNullOrEmpty(dirname))
            {
                Directory.CreateDirectory(dirname);
            }
            return Path.GetFullPath(zipfile);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fail to create directory for {zipfile}: {ex.Message}");
            return null;
        }
    }
}
