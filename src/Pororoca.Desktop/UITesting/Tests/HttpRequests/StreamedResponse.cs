using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Pororoca.Desktop.UITesting.Tests;

public sealed partial class HttpRequestsUITest : PororocaUITest
{
    private int streamedPartCount = 0;

    private async Task TestStreamedResponse(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        this.streamedPartCount = 0;

        await HttpRobot.HttpMethod.Select("GET");
        await HttpRobot.Url.ClearAndTypeText("{{BaseUrl}}/test/get/server-sent-events?durationInSeconds=6");
        await HttpRobot.SetEmptyBody();
        HttpRobot.ResBodyRawContent.TextChanged += Document_TextChanged;
        await HttpRobot.ClickOnSend();

        await Wait(2);

        HttpRobot.ResTitle.AssertContainsText("Receiving stream… 200 OK");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResHeaders);
        AssertContainsResponseHeader("Date");
        AssertContainsResponseHeader("Server", "Kestrel");
        AssertContainsResponseHeader("Content-Type", "text/event-stream");
        await HttpRobot.TabControlRes.Select(HttpRobot.TabResBody);

        HttpRobot.ResProgressBar.AssertIsVisible();
        HttpRobot.ResBodyExportLogToFile.AssertIsHidden();
        HttpRobot.ResBodySaveToFile.AssertIsHidden();

        await Wait(10);

        HttpRobot.ResProgressBar.AssertIsHidden();
        HttpRobot.ResBodyExportLogToFile.AssertIsVisible();
        HttpRobot.ResBodySaveToFile.AssertIsVisible();

        HttpRobot.ResBodyRawContent.TextChanged -= Document_TextChanged;

        string resBodyText = HttpRobot.ResBodyRawContent.Document.Text;
        int eventCount = Regex.Count(resBodyText, "event: ");
        int dataCount = Regex.Count(resBodyText, "data: ");
        AssertCondition(eventCount == dataCount);
        AssertCondition(eventCount == 6 || eventCount == 7);
        AssertCondition(dataCount == 6 || dataCount == 7);
        AppendToLog("Received " + eventCount + " events from stream.");
    }

    private void Document_TextChanged(object? sender, EventArgs e)
    {
        var doc = HttpRobot.ResBodyRawContent.Document;

        if (String.IsNullOrWhiteSpace(doc.Text))
            return;

        this.streamedPartCount++;
        string[] eventBlocks = doc.Text.TrimEnd().Split("\n\n", StringSplitOptions.TrimEntries);
        int eventNumber = eventBlocks.Length;
        string[] eventLines = eventBlocks[^1].Split('\n', StringSplitOptions.TrimEntries);

        //AppendToLog($"Asserting event {this.streamedPartCount} from stream:\n" + eventBlocks[^1]);
        AssertCondition(eventNumber == this.streamedPartCount);
        AssertCondition(2 == eventLines.Length);

        if (eventLines[0] == "event: cancellationRequested")
        {
            HttpRobot.ResBodyRawContent.TextChanged -= Document_TextChanged;
            return;
        }

        if (eventNumber % 3 == 0)
        {
            AssertCondition("event: dateTimeSync" == eventLines[0]);
            AssertCondition(eventLines[1].StartsWith("data: " + DateTimeOffset.Now.ToString("yyyy-MM-dd")));
        }
        else
        {
            AssertCondition("event: reactorStats" == eventLines[0]);
            AssertCondition(eventLines[1].StartsWith("data: "));
            var stats = JsonSerializer.Deserialize(eventLines[1][6..], TestJsonSrcGenContext.Instance.JsonNode)!;
            int tempInC = (int)stats["tempInC"]!;
            float pressureInKpa = (float)stats["pressureInKPa"]!;

            AssertCondition(400 <= tempInC && tempInC <= 500);
            AssertCondition(100.00f <= pressureInKpa && pressureInKpa <= 250.00f);
            string? expectedWarning = (tempInC, pressureInKpa) switch
            {
                ( > 470, > 240f) => "TEMPERATURE_AND_PRESSURE_TOO_HIGH",
                (_, > 240f) => "PRESSURE_TOO_HIGH",
                (_, < 120f) => "PRESSURE_TOO_LOW",
                ( > 470, _) => "TEMPERATURE_TOO_HIGH",
                ( < 420, _) => "TEMPERATURE_TOO_LOW",
                _ => null
            };
            AssertCondition(expectedWarning == (string?)stats["warning"]);
        }
    }
}

[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(JsonValue))]
internal partial class TestJsonSrcGenContext : JsonSerializerContext
{
    internal static readonly TestJsonSrcGenContext Instance = new();
}