using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.RequestRepeater;
using Xunit;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionValidator;

namespace Pororoca.Domain.Tests.Features.RequestRepeater;

public static class HttpRepetitionValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public static async Task Should_disallow_repetition_without_a_selected_base_request_path(string? baseReqPath)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { BaseRequestPath = baseReqPath! };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.BaseHttpRequestNotSelected, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Fact]
    public static async Task Should_disallow_repetition_if_base_request_is_not_found()
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = null;
        var rep = MakeExampleRep();

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.BaseHttpRequestNotFound, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Fact]
    public static async Task Should_disallow_repetition_with_negative_delay_in_ms()
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { DelayInMs = -11 };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.DelayCantBeNegative, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Fact]
    public static async Task Should_disallow_repetition_with_negative_maximum_rate_per_second()
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { MaxRatePerSecond = -11 };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.MaximumRateCantBeNegative, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public static async Task Should_disallow_repetition_with_number_of_repetitions_less_than_1(int numReps)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { NumberOfRepetitions = numReps };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.NumberOfRepetitionsMustBeAtLeast1, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public static async Task Should_disallow_repetition_with_max_dop_less_than_1(int maxDop)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { MaxDop = maxDop };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.MaxDopMustBeAtLeast1, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Sequential)]
    [InlineData(PororocaRepetitionMode.Random)]
    public static async Task Should_disallow_sequential_and_random_repetitions_without_input_data(PororocaRepetitionMode repMode)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with
        {
            RepetitionMode = repMode,
            InputData = null
        };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.InputDataInvalid, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("/fafa99.json")]
    public static async Task Should_disallow_repetition_if_input_data_file_is_not_found(string? inputDataFilePath)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { InputData = new(PororocaRepetitionInputDataType.File, null, inputDataFilePath) };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.InputDataFileNotFound, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Fact]
    public static async Task Should_disallow_repetition_if_input_data_is_empty_array()
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { InputData = new(PororocaRepetitionInputDataType.RawJsonArray, "[]", null) };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.InputDataAtLeastOneLine, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("{}")]
    [InlineData("{\"v1\":\"aaa\"}")]
    [InlineData("[\"v1\",\"aaa\"]")]
    public static async Task Should_disallow_repetition_if_input_data_is_invalid_json(string? rawJsonArray)
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with { InputData = new(PororocaRepetitionInputDataType.RawJsonArray, rawJsonArray, null) };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.False(valid);
        Assert.Equal(TranslateRepetitionErrors.InputDataInvalid, errorCode);
        Assert.Null(resolvedInputData);
    }

    [Fact]
    public static async Task Should_allow_valid_simple_repetition()
    {
        // GIVEN
        PororocaVariable[] effVars = [];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with
        {
            RepetitionMode = PororocaRepetitionMode.Simple,
            InputData = null
        };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);
        Assert.Null(resolvedInputData);
    }

    [Theory]
    [InlineData(PororocaRepetitionMode.Sequential, PororocaRepetitionInputDataType.RawJsonArray)]
    [InlineData(PororocaRepetitionMode.Sequential, PororocaRepetitionInputDataType.File)]
    [InlineData(PororocaRepetitionMode.Random, PororocaRepetitionInputDataType.RawJsonArray)]
    [InlineData(PororocaRepetitionMode.Random, PororocaRepetitionInputDataType.File)]
    public static async Task Should_allow_valid_sequential_and_random_repetitions_and_resolve_raw_and_file_input_data_correctly(PororocaRepetitionMode repMode, PororocaRepetitionInputDataType inputDataType)
    {
        // GIVEN
        PororocaVariable[] effVars = [
            new(true, "KEY_123", "123", true),
            new(true, "InputDataDir", GetTestFilesDirPath(), true)
        ];
        PororocaHttpRequest? baseReq = new(string.Empty);
        var rep = MakeExampleRep() with
        {
            RepetitionMode = repMode,
            InputData = inputDataType == PororocaRepetitionInputDataType.RawJsonArray ?
                        PororocaRepetitionInputData.MakeRawJsonInputDataExample("comments are allowed") :
                        new(PororocaRepetitionInputDataType.File, null, "{{InputDataDir}}/InputData2.json")
        };

        // WHEN
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(effVars, baseReq, rep, default);

        // THEN
        Assert.True(valid);
        Assert.Null(errorCode);

        Assert.NotNull(resolvedInputData);
        Assert.Equal(3, resolvedInputData.Length);

        Assert.Equal(2, resolvedInputData[0].Length);
        var v = resolvedInputData[0][0];
        Assert.Equal((true, "Var1", "ABC"), (v.Enabled, v.Key, v.Value));
        v = resolvedInputData[0][1];
        Assert.Equal((true, "Var2", "123"), (v.Enabled, v.Key, v.Value));

        Assert.Equal(2, resolvedInputData[1].Length);
        v = resolvedInputData[1][0];
        Assert.Equal((true, "Var1", "DEF"), (v.Enabled, v.Key, v.Value));
        v = resolvedInputData[1][1];
        Assert.Equal((true, "Var2", "456"), (v.Enabled, v.Key, v.Value));

        Assert.Equal(2, resolvedInputData[2].Length);
        v = resolvedInputData[2][0];
        Assert.Equal((true, "Var1", "GHI"), (v.Enabled, v.Key, v.Value));
        v = resolvedInputData[2][1];
        Assert.Equal((true, "Var2", "789"), (v.Enabled, v.Key, v.Value));
    }

    private static PororocaHttpRepetition MakeExampleRep() => new(string.Empty)
    {
        BaseRequestPath = "New HTTP request",
        RepetitionMode = PororocaRepetitionMode.Random,
        NumberOfRepetitions = 25,
        MaxDop = 3,
        DelayInMs = null,
        InputData = PororocaRepetitionInputData.MakeRawJsonInputDataExample("comments are allowed")
    };
}