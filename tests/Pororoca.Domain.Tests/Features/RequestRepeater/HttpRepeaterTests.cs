using System.Diagnostics;
using System.Net;
using System.Text.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.RequestRepeater;
using Xunit;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepeater;
using static Pororoca.Domain.Tests.Features.RequestRepeater.HttpRepetitionReporterTests;

namespace Pororoca.Domain.Tests.Features.RequestRepeater;

public static class HttpRepeaterTests
{
    [Fact]
    public static async Task Should_run_simple_repetition_successfully()
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [new(true, "A", "K", true)];
        PororocaVariable[][]? resolvedInputData = null;
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(PororocaRepetitionMode.Simple);
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(colEffVars, colScopedAuth, baseReq, out _)
                 .Returns(true);
        var response1 = await MakeExampleResponseAsync(HttpStatusCode.OK, null);
        var response2 = await MakeExampleResponseAsync(HttpStatusCode.Accepted, null);
        var response3 = await MakeExampleResponseAsync(HttpStatusCode.NotAcceptable, null);
        requester.RequestAsync(colEffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response1), Task.FromResult(response2), Task.FromResult(response3));

        // WHEN
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }
        Assert.Equal(3, results.Count);
        Assert.Null(results[0].InputLine);
        Assert.Equal(response1, results[0].Response);
        Assert.Null(results[0].ValidationErrorCode);

        Assert.Null(results[1].InputLine);
        Assert.Equal(response2, results[1].Response);
        Assert.Null(results[1].ValidationErrorCode);

        Assert.Null(results[2].InputLine);
        Assert.Equal(response3, results[2].Response);
        Assert.Null(results[2].ValidationErrorCode);
    }

    [Fact]
    public static async Task Should_run_sequential_repetition_successfully()
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[][]? resolvedInputData =
        [
            [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)],
            [new(true, "Var1", "DEF", true), new(true, "Var2", "456", true)],
            [new(true, "Var1", "GHI", true), new(true, "Var2", "789", true)]
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = [
            new(true, "ColScopedHeader", "ColScopedValue")
        ];
        var rep = MakeExampleRep(PororocaRepetitionMode.Sequential);
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ReturnsForAnyArgs(true);

        var response1 = await MakeExampleResponseAsync(HttpStatusCode.OK, null);
        var req1EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "123"));
        requester.RequestAsync(req1EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response1));

        var response2 = await MakeExampleResponseAsync(HttpStatusCode.Accepted, null);
        var req2EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "456"));
        requester.RequestAsync(req2EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response2));

        var response3 = await MakeExampleResponseAsync(null, new TaskCanceledException());
        var req3EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "789"));
        requester.RequestAsync(req3EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response3));

        // WHEN
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }
        Assert.Equal(3, results.Count);
        Assert.True(Enumerable.SequenceEqual(resolvedInputData[0], results[0].InputLine!));
        Assert.Equal(response1, results[0].Response);
        Assert.Null(results[0].ValidationErrorCode);

        Assert.True(Enumerable.SequenceEqual(resolvedInputData[1], results[1].InputLine!));
        Assert.Equal(response2, results[1].Response);
        Assert.Null(results[1].ValidationErrorCode);

        Assert.True(Enumerable.SequenceEqual(resolvedInputData[2], results[2].InputLine!));
        Assert.Equal(response3, results[2].Response);
        Assert.Null(results[2].ValidationErrorCode);
    }

    [Fact]
    public static async Task Should_run_random_repetition_successfully()
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[][]? resolvedInputData =
        [
            [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)],
            [new(true, "Var1", "DEF", true), new(true, "Var2", "456", true)],
            [new(true, "Var1", "GHI", true), new(true, "Var2", "789", true)]
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(PororocaRepetitionMode.Sequential);
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ReturnsForAnyArgs(true);

        var response1 = await MakeExampleResponseAsync(HttpStatusCode.OK, null);
        var req1EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "123"));
        requester.RequestAsync(req1EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response1));

        var response2 = await MakeExampleResponseAsync(HttpStatusCode.Accepted, null);
        var req2EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "456"));
        requester.RequestAsync(req2EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response2));

        var response3 = await MakeExampleResponseAsync(null, new TaskCanceledException());
        var req3EffVars = Arg.Is<IEnumerable<PororocaVariable>>(il => il.Any(v => v.Key == "Var2" && v.Value == "789"));
        requester.RequestAsync(req3EffVars, colScopedAuth, colScopedReqHeaders, baseReq, Arg.Any<CancellationToken>())
                 .Returns(Task.FromResult(response3));

        // WHEN
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }

        Assert.Equal(3, results.Count);

        for (int i = 0; i < results.Count; i++)
        {
            Assert.Null(results[i].ValidationErrorCode);
            var res = results[i].Response;
            var il = results[i].InputLine!;
            if (res == response1)
            {
                Assert.True(Enumerable.SequenceEqual(resolvedInputData[0], il));
            }
            else if (res == response2)
            {
                Assert.True(Enumerable.SequenceEqual(resolvedInputData[1], il));
            }
            else if (res == response3)
            {
                Assert.True(Enumerable.SequenceEqual(resolvedInputData[2], il));
            }
            else
            {
                throw new Exception("Invalid response considering mocks");
            }
        }
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Simple)]
    [InlineData(PororocaRepetitionMode.Sequential)]
    [InlineData(PororocaRepetitionMode.Random)]
    public static async Task Should_for_invalid_request_cancel_repetition_and_complete_channel_successfully(PororocaRepetitionMode repMode)
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[][]? resolvedInputData =
        [
            [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)],
            [new(true, "Var1", "DEF", true), new(true, "Var2", "456", true)],
            [new(true, "Var1", "GHI", true), new(true, "Var2", "789", true)]
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(repMode);
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ReturnsForAnyArgs(false);

        // WHEN
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }

        var r = Assert.Single(results);
        Assert.Null(r.ValidationErrorCode); // there should be a validation error code, but can't be mocked here
        Assert.Null(r.Response);
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Simple, 500)]
    [InlineData(PororocaRepetitionMode.Sequential, 250)]
    [InlineData(PororocaRepetitionMode.Random, 1000)]
    public static async Task Should_delay_between_executions_correctly(PororocaRepetitionMode repMode, int delayInMs)
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[][]? resolvedInputData =
        [
            [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)]
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(repMode) with
        {
            NumberOfRepetitions = 1,
            DelayInMs = delayInMs
        };
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ReturnsForAnyArgs(true);

        // WHEN
        Stopwatch sw = new();
        sw.Start();
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }
        sw.Stop();

        var errorMargin = TimeSpan.FromMilliseconds(500); // this test was flaky
        Assert.True((sw.Elapsed + errorMargin) >= TimeSpan.FromMilliseconds(delayInMs));
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Simple, 1)]
    [InlineData(PororocaRepetitionMode.Sequential, 2)]
    [InlineData(PororocaRepetitionMode.Random, 3)]
    public static async Task Should_apply_rate_limiter_correctly(PororocaRepetitionMode repMode, int maximumRatePerSecond)
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[] inputLine = [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)];
        PororocaVariable[][]? resolvedInputData =
        [
            inputLine,inputLine,inputLine,inputLine,inputLine,inputLine
            // this is necessary to make 6 reqs for sequential rep
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(repMode) with
        {
            NumberOfRepetitions = 6,
            DelayInMs = 0,
            MaxDop = maximumRatePerSecond, // parallelism must not interfere with max rate
            MaxRatePerSecond = maximumRatePerSecond
        };
        TimeSpan expectedTotalTime = TimeSpan.FromSeconds((int)rep.NumberOfRepetitions! / maximumRatePerSecond);
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ReturnsForAnyArgs(true);

        // WHEN
        Stopwatch sw = new();
        sw.Start();
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }
        sw.Stop();

        var errorMargin = TimeSpan.FromSeconds(1); // this test was flaky
        Assert.True((sw.Elapsed + errorMargin) >= expectedTotalTime);
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Simple)]
    [InlineData(PororocaRepetitionMode.Sequential)]
    [InlineData(PororocaRepetitionMode.Random)]
    public static async Task Should_write_unknown_error_result_and_complete_channel(PororocaRepetitionMode repMode)
    {
        // GIVEN
        var requester = Substitute.For<IPororocaRequester>();
        PororocaVariable[] colEffVars = [];
        PororocaVariable[][]? resolvedInputData =
        [
            [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)]
        ];
        var colScopedAuth = PororocaRequestAuth.MakeBearerAuth("tkn");
        List<PororocaKeyValueParam>? colScopedReqHeaders = null;
        var rep = MakeExampleRep(repMode) with { NumberOfRepetitions = 1 };
        PororocaHttpRequest baseReq = new("basereq");

        requester.IsValidRequest(default!, default, default!, out _)
                 .ThrowsForAnyArgs(new JsonException());

        // WHEN
        var channelReader = StartRepetition(requester, colEffVars, resolvedInputData, colScopedAuth, colScopedReqHeaders, rep, baseReq, default);

        // THEN
        List<PororocaHttpRepetitionResult> results = new();
        await foreach (var result in channelReader.ReadAllAsync())
        {
            results.Add(result);
        }

        var r = Assert.Single(results);
        Assert.False(r.Successful);
        Assert.NotNull(r.Response);
        Assert.False(r.Response.Successful);
        Assert.IsType<JsonException>(r.Response.Exception);
        Assert.Equal(TimeSpan.Zero, r.Response.ElapsedTime);
        Assert.Equal(TranslateRepetitionErrors.UnknownError, r.ValidationErrorCode);
    }

    [Fact]
    public static void Should_combine_collection_effective_vars_with_null_input_line()
    {
        // GIVEN
        PororocaVariable[] colEffVars = [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)];
        PororocaVariable[]? inputLine = null;

        // WHEN
        var effVars = CombineEffectiveVarsWithInputLine(colEffVars, inputLine).ToArray();

        // THEN
        Assert.NotNull(effVars);
        Assert.Equal(effVars, colEffVars);
    }

    [Fact]
    public static void Should_combine_collection_effective_vars_with_input_line_unintersecting_variables()
    {
        // GIVEN
        PororocaVariable[] colEffVars = [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)];
        PororocaVariable[]? inputLine = [new(true, "Var3", "DEF", true), new(true, "Var4", "456", true)];

        // WHEN
        var effVars = CombineEffectiveVarsWithInputLine(colEffVars, inputLine).ToArray();

        // THEN
        Assert.NotNull(effVars);
        Assert.Equal(4, effVars.Length);
        Assert.Equal((true, "Var1", "ABC"), (effVars[0].Enabled, effVars[0].Key, effVars[0].Value));
        Assert.Equal((true, "Var2", "123"), (effVars[1].Enabled, effVars[1].Key, effVars[1].Value));
        Assert.Equal((true, "Var3", "DEF"), (effVars[2].Enabled, effVars[2].Key, effVars[2].Value));
        Assert.Equal((true, "Var4", "456"), (effVars[3].Enabled, effVars[3].Key, effVars[3].Value));
    }

    [Fact]
    public static void Should_combine_collection_effective_vars_with_input_line_intersecting_variables()
    {
        // GIVEN
        PororocaVariable[] colEffVars = [new(true, "Var1", "ABC", true), new(true, "Var2", "123", true)];
        PororocaVariable[]? inputLine = [new(true, "Var1", "DEF", true), new(true, "Var4", "456", true)];

        // WHEN
        var effVars = CombineEffectiveVarsWithInputLine(colEffVars, inputLine).ToArray();

        // THEN
        Assert.NotNull(effVars);
        Assert.Equal(3, effVars.Length);
        Assert.Equal((true, "Var2", "123"), (effVars[0].Enabled, effVars[0].Key, effVars[0].Value));
        Assert.Equal((true, "Var1", "DEF"), (effVars[1].Enabled, effVars[1].Key, effVars[1].Value));
        Assert.Equal((true, "Var4", "456"), (effVars[2].Enabled, effVars[2].Key, effVars[2].Value));
    }

    [Theory]
    [InlineData(9, 10, 1, 1)]
    [InlineData(2, 12, 6, 2)]
    public static void Should_estimate_remaining_time_correctly(int expected, int total, int executed, int elapsedSoFar) =>
        Assert.Equal(TimeSpan.FromSeconds(expected), EstimateRemainingTime(total, executed, TimeSpan.FromSeconds(elapsedSoFar)));

    private static PororocaHttpRepetition MakeExampleRep(PororocaRepetitionMode repMode = PororocaRepetitionMode.Random) => new(string.Empty)
    {
        BaseRequestPath = "New HTTP request",
        RepetitionMode = repMode,
        NumberOfRepetitions = 3,
        MaxDop = 1,
        DelayInMs = null,
        InputData = PororocaRepetitionInputData.MakeRawJsonInputDataExample("comments are allowed")
    };
}