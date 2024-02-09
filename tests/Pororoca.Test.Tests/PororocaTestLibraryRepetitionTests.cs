using System.Diagnostics;
using System.Net;
using System.Threading.Channels;
using Pororoca.Domain.Feature.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca;
using Xunit;
using Xunit.Abstractions;

namespace Pororoca.Test.Tests;

public class PororocaTestLibraryRepetitionTests
{
    private static readonly TimeSpan oneSecond = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan oneMinute = TimeSpan.FromMinutes(1);

    private readonly PororocaTest pororocaTest;
    private readonly ITestOutputHelper output;

    public PororocaTestLibraryRepetitionTests(ITestOutputHelper output)
    {
        string filePath = GetTestCollectionFilePath();
        this.pororocaTest = PororocaTest.LoadCollectionFromFile(filePath)
                                        .AndUseTheEnvironment("Local");
        if (OperatingSystem.IsLinux())
        {
            this.pororocaTest.AndDontCheckTlsCertificate();
        }
        this.output = output;
    }

    #region SIMPLE

    [Fact]
    public async Task Should_run_simple_repetition_http_1_1_successfully()
    {
        this.pororocaTest.SetCollectionVariable("MyTxt", "ABC");
        this.pororocaTest.SetCollectionVariable("MyInt", "123");

        this.output.WriteLine("Simple repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP1 SIMPLE");

        await AssertSimpleRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_run_simple_repetition_http_2_successfully()
    {
        this.pororocaTest.SetCollectionVariable("MyTxt", "ABC");
        this.pororocaTest.SetCollectionVariable("MyInt", "123");

        this.output.WriteLine("Simple repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP2 SIMPLE");

        await AssertSimpleRepetitionAsync(channelReader);
    }

    [Fact]
    public async Task Should_run_simple_repetition_http_3_successfully()
    {
        this.pororocaTest.SetCollectionVariable("MyTxt", "ABC");
        this.pororocaTest.SetCollectionVariable("MyInt", "123");

        this.output.WriteLine("Simple repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP3 SIMPLE");

        await AssertSimpleRepetitionAsync(channelReader);
    }

    private async Task AssertSimpleRepetitionAsync(ChannelReader<PororocaHttpRepetitionResult> channelReader)
    {
        int count = 0;
        TimeSpan sumOfAllDurations = TimeSpan.Zero;
        Stopwatch sw = new();
        sw.Start();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            count++;
            sumOfAllDurations += result.Response is not null ? result.Response.ElapsedTime : TimeSpan.Zero;
            string inputLine = "-";
            string elapsedTimeStr = result.Response is null ? "-" : FormatElapsedTime(result.Response.ElapsedTime);
            this.output.WriteLine($"Request {count}. Status code {result.Response?.StatusCode}, duration {elapsedTimeStr}.\n\tInput line: {inputLine}");

            Assert.NotNull(result.Response);
            Assert.Null(result.Response.Exception);
            Assert.True(result.Successful);
            Assert.Null(result.ValidationErrorCode);
            Assert.NotNull(result.Response);
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.Null(result.InputLine);
        }
        sw.Stop();
        this.output.WriteLine($"Simple repetition of {count} requests finished in {FormatElapsedTime(sw.Elapsed)}. Average response time: {FormatElapsedTime(sumOfAllDurations / count)}.");
        Assert.Equal(25, count);
    }

    #endregion

    #region SEQUENTIAL

    #region RAW JSON ARRAY INPUT DATA

    [Fact]
    public async Task Should_run_sequential_repetition_input_data_from_raw_json_array_http_1_1_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP1 SEQUENTIAL FROM RAW JSON ARRAY");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_run_sequential_repetition_input_data_from_raw_json_array_http_2_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP2 SEQUENTIAL FROM RAW JSON ARRAY");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_run_sequential_repetition_input_data_from_raw_json_array_http_3_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP3 SEQUENTIAL FROM RAW JSON ARRAY");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    #endregion

    #region INPUT DATA FROM FILE

    [Fact]
    public async Task Should_run_sequential_repetition_input_data_from_file_http_1_1_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP1 SEQUENTIAL FROM FILE");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_run_sequential_repetition_input_data_from_file_http_2_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP2 SEQUENTIAL FROM FILE");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_run_sequential_repetition_input_data_from_file_http_3_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Sequential repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP3 SEQUENTIAL FROM FILE");

        await AssertSequentialRepetitionAsync(channelReader);
    }

    #endregion

    private async Task AssertSequentialRepetitionAsync(ChannelReader<PororocaHttpRepetitionResult> channelReader)
    {
        int count = 0;
        TimeSpan sumOfAllDurations = TimeSpan.Zero;
        Stopwatch sw = new();
        sw.Start();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            count++;
            sumOfAllDurations += result.Response is not null ? result.Response.ElapsedTime : TimeSpan.Zero;
            string inputLine = string.Join(", ", result.InputLine!.Select(v => v.Key + "=" + v.Value));
            string elapsedTimeStr = result.Response is null ? "-" : FormatElapsedTime(result.Response.ElapsedTime);
            this.output.WriteLine($"Request {count}. Status code {result.Response?.StatusCode}, duration {elapsedTimeStr}.\n\tInput line: {inputLine}");

            Assert.True(result.Successful);
            Assert.Null(result.ValidationErrorCode);
            Assert.NotNull(result.Response);
            Assert.NotNull(result.InputLine);
            switch (count)
            {
                case 1:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "ABC", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "123", true), result.InputLine[1]);
                    break;
                case 2:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "DEF", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "456", true), result.InputLine[1]);
                    break;
                case 3:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "GHI", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "789", true), result.InputLine[1]);
                    break;
                case 4:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "JKL", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "123", true), result.InputLine[1]);
                    break;
                case 5:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "MNO", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "456", true), result.InputLine[1]);
                    break;
                case 6:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "PQR", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "789", true), result.InputLine[1]);
                    break;
                case 7:
                    Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "STU", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "012", true), result.InputLine[1]);
                    break;
                case 8:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "VWX", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "345", true), result.InputLine[1]);
                    break;
                case 9:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "YZA", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "678", true), result.InputLine[1]);
                    break;
                case 10:
                    Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                    Assert.Equal(new PororocaVariable(true, "MyTxt", "BCD", true), result.InputLine[0]);
                    Assert.Equal(new PororocaVariable(true, "MyInt", "901", true), result.InputLine[1]);
                    break;
            }
        }
        sw.Stop();
        this.output.WriteLine($"Sequential repetition of {count} requests finished in {FormatElapsedTime(sw.Elapsed)}. Average response time: {FormatElapsedTime(sumOfAllDurations / count)}.");
        Assert.Equal(10, count);
    }

    #endregion

    #region RANDOM

    #region RAW JSON ARRAY INPUT DATA

    [Fact]
    public async Task Should_run_random_repetition_input_data_from_raw_json_array_http_1_1_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP1 RANDOM FROM RAW JSON ARRAY");

        await AssertRandomRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_run_random_repetition_input_data_from_raw_json_array_http_2_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP2 RANDOM FROM RAW JSON ARRAY");

        await AssertRandomRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_run_random_repetition_input_data_from_raw_json_array_http_3_successfully()
    {
        this.pororocaTest.SetCollectionVariable("KEY_123", "123");

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP3 RANDOM FROM RAW JSON ARRAY");

        await AssertRandomRepetitionAsync(channelReader);
    }

    #endregion

    #region FILE

    [Fact]
    public async Task Should_run_random_repetition_input_data_from_file_http_1_1_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP1 RANDOM FROM FILE");

        await AssertRandomRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp2]
    public async Task Should_run_random_repetition_input_data_from_file_http_2_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP2 RANDOM FROM FILE");

        await AssertRandomRepetitionAsync(channelReader);
    }

    [FactOnlyIfOSSupportsHttp3]
    public async Task Should_run_random_repetition_input_data_from_file_http_3_successfully()
    {
        this.pororocaTest.SetCollectionVariable("InputDataDir", GetTestFilesDir());

        this.output.WriteLine("Random repetition started");
        var channelReader = await this.pororocaTest.StartHttpRepetitionAsync("REPETITION HTTP3 RANDOM FROM FILE");

        await AssertRandomRepetitionAsync(channelReader);
    }

    #endregion

    private async Task AssertRandomRepetitionAsync(ChannelReader<PororocaHttpRepetitionResult> channelReader)
    {
        string[] possibleMyTxtValues = ["ABC","DEF","GHI","JKL","MNO","PQR","STU","VWX","YZA","BCD"];
        string[] possibleMyIntValues = ["123","456","789","012","345","678","901"];

        int count = 0;
        TimeSpan sumOfAllDurations = TimeSpan.Zero;
        Stopwatch sw = new();
        sw.Start();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            count++;
            sumOfAllDurations += result.Response is not null ? result.Response.ElapsedTime : TimeSpan.Zero;
            string inputLine = string.Join(", ", result.InputLine!.Select(v => v.Key + "=" + v.Value));
            string elapsedTimeStr = result.Response is null ? "-" : FormatElapsedTime(result.Response.ElapsedTime);
            this.output.WriteLine($"Request {count}. Status code {result.Response?.StatusCode}, duration {elapsedTimeStr}.\n\tInput line: {inputLine}");

            Assert.True(result.Successful);
            Assert.Null(result.ValidationErrorCode);
            Assert.NotNull(result.Response);
            Assert.NotNull(result.InputLine);

            var myTxtVar = result.InputLine.First(v => v.Key == "MyTxt");
            Assert.Contains(myTxtVar.Value, possibleMyTxtValues);
            var myIntVar = result.InputLine.First(v => v.Key == "MyInt");
            Assert.Contains(myIntVar.Value, possibleMyIntValues);

            Assert.Equal(myIntVar.Value == "012" ? HttpStatusCode.BadRequest : HttpStatusCode.OK, result.Response.StatusCode);
        }
        sw.Stop();
        this.output.WriteLine($"Random repetition of {count} requests finished in {FormatElapsedTime(sw.Elapsed)}. Average response time: {FormatElapsedTime(sumOfAllDurations / count)}.");
        Assert.Equal(25, count);
    }

    #endregion

    private static string FormatElapsedTime(TimeSpan elapsedTime) =>
        elapsedTime < oneSecond ?
        string.Format("{0}ms", (int)elapsedTime.TotalMilliseconds) :
        elapsedTime < oneMinute ? // More or equal than one second, but less than one minute
        string.Format("{0:0.00}s", elapsedTime.TotalSeconds) : // TODO: Format digit separator according to language
        string.Format("{0}min {1}s", elapsedTime.Minutes, elapsedTime.Seconds);
}