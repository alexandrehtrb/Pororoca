using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Pororoca.Test.Tests;

public sealed class PororocaTestLibraryLargeRepetitionTests
{
    private readonly PororocaTest pororocaTest;
    private readonly ITestOutputHelper output;

    public PororocaTestLibraryLargeRepetitionTests(ITestOutputHelper output)
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath);

        if (OperatingSystem.IsLinux())
        {
            this.pororocaTest.AndDontCheckTlsCertificate();
        }
        this.output = output;
    }

    [Fact(Skip = "No need to always run")]
    public Task Should_run_large_repetition_http_1_1_successfully() =>
        RunRepetitionAsync("MASSIVE REPS HTTP1");

    [FactOnlyIfOSSupportsHttp2(Skip = "No need to always run")]
    public Task Should_run_large_repetition_http_2_successfully() =>
        RunRepetitionAsync("MASSIVE REPS HTTP2");

    [FactOnlyIfOSSupportsHttp3(Skip = "No need to always run")]
    public Task Should_run_large_repetition_http_3_successfully() =>
        RunRepetitionAsync("MASSIVE REPS HTTP3");

    private async Task RunRepetitionAsync(string repName)
    {
        Stopwatch sw = new();
        int count = 0;

        this.output.WriteLine($"Starting {repName}...");
        sw.Restart();
        var repChannelReader = await this.pororocaTest.StartHttpRepetitionAsync(repName, default);
        await foreach (var _ in repChannelReader.ReadAllAsync(default)) { count++; }
        sw.Stop();
        this.output.WriteLine($"Finished {repName}: {count} reps, elapsed time: {sw.Elapsed:hh':'mm':'ss}");
    }
}