using Xunit;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;

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
        Assert.Equal(rep.InputData, copy.InputData);
    }
}