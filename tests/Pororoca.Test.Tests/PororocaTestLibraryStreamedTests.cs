using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Pororoca.Test.Tests;

public sealed class PororocaTestLibraryStreamedTests
{
    private readonly PororocaTest pororocaTest;

    public PororocaTestLibraryStreamedTests()
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local");
    }

    [Theory]
    [InlineData("SERVER-SENT EVENTS HTTP1")]
    [InlineData("SERVER-SENT EVENTS HTTP2")]
    [InlineData("SERVER-SENT EVENTS HTTP3")]
    public async Task Should_get_streamed_response_successfully(string reqName)
    {
        // GIVEN
        this.pororocaTest.SetEnvironmentVariable("Local", "DURATION_IN_SECONDS", "7");

        // WHEN
        var res = await this.pororocaTest.SendHttpRequestAsync(reqName, TestContext.Current.CancellationToken);

        // THEN
        Assert.NotNull(res);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal("text/event-stream", res.ContentType);
        Assert.True(res.IsStreamed);
        Assert.NotNull(res.StreamedBodyReader);

        Stopwatch sw = new();
        sw.Start();
        int partCount = 0;
        await foreach (string part in res.StreamedBodyReader!.ReadAllAsync(TestContext.Current.CancellationToken))
        {
            partCount++;
            string[] eventLines = part.TrimEnd().Split('\n', StringSplitOptions.TrimEntries);

            Assert.Equal(2, eventLines.Length);

            if (eventLines[0] == "event: cancellationRequested")
                break;

            if (partCount % 3 == 0)
            {
                Assert.Equal("event: dateTimeSync", eventLines[0]);
                Assert.StartsWith("data: " + DateTimeOffset.Now.ToString("yyyy-MM-dd"), eventLines[1]);
            }
            else
            {
                Assert.Equal("event: reactorStats", eventLines[0]);
                Assert.StartsWith("data: ", eventLines[1]);
                var stats = JsonSerializer.Deserialize(eventLines[1][6..], TestsJsonSrcGenContext.Default.ReactorStatus)!;
                Assert.InRange(stats.TempInC, 400, 500);
                Assert.InRange(stats.PressureInKPa, 100.00f, 250.00f);
                string? expectedWarning = (stats.TempInC, stats.PressureInKPa) switch
                {
                    ( > 470, > 240f) => "TEMPERATURE_AND_PRESSURE_TOO_HIGH",
                    (_, > 240f) => "PRESSURE_TOO_HIGH",
                    (_, < 120f) => "PRESSURE_TOO_LOW",
                    ( > 470, _) => "TEMPERATURE_TOO_HIGH",
                    ( < 420, _) => "TEMPERATURE_TOO_LOW",
                    _ => null
                };
                Assert.Equal(expectedWarning, stats.Warning);
            }
        }
        sw.Stop();
        Assert.Equal(7, partCount);
    }
}

public record ReactorStatus(int TempInC, float PressureInKPa, string? Warning);