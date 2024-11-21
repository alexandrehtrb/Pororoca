using Xunit;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using System.Net;
using Pororoca.Domain.Tests.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca.Repetition;

public static class PororocaHttpRepetitionTests
{
    public static IEnumerable<object[]> GetAllTestInputDatas()
    {
        yield return new object[] { new PororocaRepetitionInputData(PororocaRepetitionInputDataType.RawJsonArray, "[]", null) };
        yield return new object[] { new PororocaRepetitionInputData(PororocaRepetitionInputDataType.File, null, "D:\\ARQUIVOS\\arq.txt") };
    }

    [Theory]
    [MemberData(nameof(GetAllTestInputDatas))]
    public static void Should_copy_full_rep_creating_new_instance(PororocaRepetitionInputData inputData)
    {
        // GIVEN
        PororocaHttpRepetition rep = new(
            Name: "name",
            BaseRequestPath: "basereqpath",
            RepetitionMode: PororocaRepetitionMode.Random,
            NumberOfRepetitions: 8000,
            MaxDop: 20,
            DelayInMs: 10,
            MaxRatePerSecond: 12,
            InputData: inputData
        );

        // WHEN
        var copy = rep.Copy();

        // THEN
        Assert.NotSame(rep, copy);
        Assert.Equal(rep.Name, copy.Name);
        Assert.Equal(rep.BaseRequestPath, copy.BaseRequestPath);
        Assert.Equal(rep.RepetitionMode, copy.RepetitionMode);
        Assert.Equal(rep.NumberOfRepetitions, copy.NumberOfRepetitions);
        Assert.Equal(rep.MaxDop, copy.MaxDop);
        Assert.Equal(rep.DelayInMs, copy.DelayInMs);
        Assert.Equal(rep.MaxRatePerSecond, copy.MaxRatePerSecond);
        Assert.NotSame(rep.InputData, copy.InputData);
        Assert.Equal(rep.InputData, copy.InputData);
    }

    [Fact]
    public static void Should_copy_empty_rep_creating_new_instance()
    {
        // GIVEN
        PororocaHttpRepetition rep = new(
            Name: "name",
            BaseRequestPath: "basereqpath",
            RepetitionMode: PororocaRepetitionMode.Simple,
            NumberOfRepetitions: 10,
            MaxDop: null,
            DelayInMs: null,
            MaxRatePerSecond: null,
            InputData: null
        );

        // WHEN
        var copy = rep.Copy();

        // THEN
        Assert.NotSame(rep, copy);
        Assert.Equal(rep.Name, copy.Name);
        Assert.Equal(rep.BaseRequestPath, copy.BaseRequestPath);
        Assert.Equal(rep.RepetitionMode, copy.RepetitionMode);
        Assert.Equal(rep.NumberOfRepetitions, copy.NumberOfRepetitions);
        Assert.Equal(rep.MaxDop, copy.MaxDop);
        Assert.Equal(rep.DelayInMs, copy.DelayInMs);
        Assert.Equal(rep.MaxRatePerSecond, copy.MaxRatePerSecond);
        Assert.Equal(rep.InputData, copy.InputData);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "abc", "text/plain")]
    [InlineData(HttpStatusCode.Accepted, "[]", "application/json")]
    [InlineData(HttpStatusCode.NoContent, null, null)]
    public static async Task Should_indicate_positive_http_2xx_status_code(HttpStatusCode statusCode, string? body, string? contentType)
    {
        // GIVEN
        var httpRes = PororocaHttpResponseTests.CreateTestHttpResponseMessage(body, contentType, null, statusCode);
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, TimeSpan.Zero, httpRes);
        var repRes = new PororocaHttpRepetitionResult(null, res, null);

        // WHEN AND THEN
        Assert.True(repRes.HasHttp2xxStatusCode);
    }

    [Fact]
    public static void Should_indicate_negative_http_2xx_status_code_if_validation_error_code_present()
    {
        // GIVEN
        var repRes = new PororocaHttpRepetitionResult(null, null, "some error");

        // WHEN AND THEN
        Assert.False(repRes.HasHttp2xxStatusCode);
    }

    [Fact]
    public static void Should_indicate_negative_http_2xx_status_code_if_request_failed()
    {
        // GIVEN
        var res = PororocaHttpResponse.Failed(null!, DateTimeOffset.Now, TimeSpan.Zero, new Exception());
        var repRes = new PororocaHttpRepetitionResult(null, res, null);

        // WHEN AND THEN
        Assert.False(repRes.HasHttp2xxStatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.Moved, "abc", "text/plain")]
    [InlineData(HttpStatusCode.BadRequest, "[]", "application/json")]
    [InlineData(HttpStatusCode.InternalServerError, null, null)]
    public static async Task Should_indicate_negative_http_2xx_status_code_if_status_code_not_success(HttpStatusCode statusCode, string? body, string? contentType)
    {
        // GIVEN
        var httpRes = PororocaHttpResponseTests.CreateTestHttpResponseMessage(body, contentType, null, statusCode);
        var res = await PororocaHttpResponse.SuccessfulAsync(null!, DateTimeOffset.Now, TimeSpan.Zero, httpRes);
        var repRes = new PororocaHttpRepetitionResult(null, res, null);

        // WHEN AND THEN
        Assert.False(repRes.HasHttp2xxStatusCode);
    }
}