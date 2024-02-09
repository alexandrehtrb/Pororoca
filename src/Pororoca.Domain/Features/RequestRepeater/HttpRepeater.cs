using System.Threading.Channels;
using Pororoca.Domain.Feature.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;

namespace Pororoca.Domain.Features.RequestRepeater;

public static class HttpRepeater
{
    public static ChannelReader<PororocaHttpRepetitionResult> StartRepetition(
        IPororocaRequester requester,
        IEnumerable<PororocaVariable> collectionEffectiveVars,
        PororocaVariable[][]? resolvedInputData,
        PororocaRequestAuth? collectionScopedAuth,
        PororocaHttpRepetition rep,
        PororocaHttpRequest baseReq,
        CancellationToken cancellationToken)
    {
        var channel = CreateChannel(rep, resolvedInputData);

        Task.Factory.StartNew(async () =>
        {
            try
            {
                await (rep.RepetitionMode switch
                {
                    PororocaRepetitionMode.Simple => requester.RunSimpleRepetitionAsync(channel.Writer, collectionEffectiveVars, collectionScopedAuth, rep, baseReq, cancellationToken),
                    PororocaRepetitionMode.Sequential => requester.RunSequentialRepetitionAsync(channel.Writer, collectionEffectiveVars, resolvedInputData!, collectionScopedAuth, rep, baseReq, cancellationToken),
                    PororocaRepetitionMode.Random => requester.RunRandomRepetitionAsync(channel.Writer, collectionEffectiveVars, resolvedInputData!, collectionScopedAuth, rep, baseReq, cancellationToken),
                    _ => Task.CompletedTask
                });
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                var response = PororocaHttpResponse.Failed(null, DateTimeOffset.Now, TimeSpan.Zero, ex);
                PororocaHttpRepetitionResult r = new(null, response, TranslateRepetitionErrors.UnknownError);
                await channel.Writer.WriteAsync(r);
            }
            finally { channel.Writer.Complete(); }
        }, TaskCreationOptions.LongRunning);

        return channel.Reader;
    }

    private static Channel<PororocaHttpRepetitionResult> CreateChannel(PororocaHttpRepetition rep, PororocaVariable[][]? resolvedInputData)
    {
        int numberOfReps = rep.RepetitionMode switch
        {
            PororocaRepetitionMode.Simple => (int)rep.NumberOfRepetitions!,
            PororocaRepetitionMode.Sequential => resolvedInputData!.Length,
            PororocaRepetitionMode.Random => (int)rep.NumberOfRepetitions!,
            _ => (int)rep.NumberOfRepetitions!
        };
        bool isSingleThread = rep.RepetitionMode == PororocaRepetitionMode.Sequential || rep.MaxDop == 1;

        BoundedChannelOptions channelOpts = new(numberOfReps)
        {
            SingleReader = true,
            SingleWriter = isSingleThread
        };
        var channel = Channel.CreateBounded<PororocaHttpRepetitionResult>(channelOpts);
        return channel;
    }

    private static async Task RunSimpleRepetitionAsync(
        this IPororocaRequester requester,
        ChannelWriter<PororocaHttpRepetitionResult> channelWriter,
        IEnumerable<PororocaVariable> collectionEffectiveVars,
        PororocaRequestAuth? collectionScopedAuth,
        PororocaHttpRepetition rep,
        PororocaHttpRequest baseReq,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource internalCts = new();
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, cancellationToken);

        ParallelOptions options = new();
        options.MaxDegreeOfParallelism = (int)rep.MaxDop!;
        options.CancellationToken = combinedCts.Token; // will stop either externally or by a validation error

        await Parallel.ForAsync(0, (int)rep.NumberOfRepetitions!, options, async (i, ct) =>
        {
            bool valid = await ExecuteRepetitionIfValidAsync(channelWriter, requester, collectionEffectiveVars, null, collectionScopedAuth, rep, baseReq, ct);
            if (!valid) internalCts.Cancel();
        });
    }

    private static async Task RunSequentialRepetitionAsync(
        this IPororocaRequester requester,
        ChannelWriter<PororocaHttpRepetitionResult> channelWriter,
        IEnumerable<PororocaVariable> collectionEffectiveVars,
        PororocaVariable[][] resolvedInputData,
        PororocaRequestAuth? collectionScopedAuth,
        PororocaHttpRepetition rep,
        PororocaHttpRequest baseReq,
        CancellationToken cancellationToken)
    {
        // sequential --> not parallel
        foreach (var inputLine in resolvedInputData)
        {
            // external stop
            if (cancellationToken.IsCancellationRequested)
                break;

            bool valid = await ExecuteRepetitionIfValidAsync(channelWriter, requester, collectionEffectiveVars, inputLine, collectionScopedAuth, rep, baseReq, cancellationToken);
            if (!valid) break;
        }
    }

    private static async Task RunRandomRepetitionAsync(
        this IPororocaRequester requester,
        ChannelWriter<PororocaHttpRepetitionResult> channelWriter,
        IEnumerable<PororocaVariable> collectionEffectiveVars,
        PororocaVariable[][] resolvedInputData,
        PororocaRequestAuth? collectionScopedAuth,
        PororocaHttpRepetition rep,
        PororocaHttpRequest baseReq,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource internalCts = new();
        var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, cancellationToken);

        ParallelOptions options = new();
        options.MaxDegreeOfParallelism = (int)rep.MaxDop!;
        options.CancellationToken = combinedCts.Token; // will stop either externally or by a validation error

        await Parallel.ForAsync(0, (int)rep.NumberOfRepetitions!, options, async (i, ct) =>
        {
            // Random.Shared is thread-safe
            // https://learn.microsoft.com/en-us/dotnet/api/system.random.shared?view=net-8.0
            var inputLine = resolvedInputData.Length == 0 ?
                            null :
                            resolvedInputData[Random.Shared.Next(0, resolvedInputData.Length)];

            bool valid = await ExecuteRepetitionIfValidAsync(channelWriter, requester, collectionEffectiveVars, inputLine, collectionScopedAuth, rep, baseReq, ct);
            if (!valid) internalCts.Cancel();
        });
    }

    private static async Task<bool> ExecuteRepetitionIfValidAsync(ChannelWriter<PororocaHttpRepetitionResult> channelWriter, IPororocaRequester requester, IEnumerable<PororocaVariable> collectionEffectiveVars, PororocaVariable[]? inputLine, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRepetition rep, PororocaHttpRequest baseReq, CancellationToken cancellationToken)
    {
        var effectiveVars = CombineEffectiveVarsWithInputLine(collectionEffectiveVars, inputLine);

        if (!requester.IsValidRequest(effectiveVars, collectionScopedAuth, baseReq, out string? errorCode))
        {
            channelWriter.TryWrite(new(inputLine, null, errorCode));
            return false;
        }
        else
        {
            var res = await requester.RequestAsync(effectiveVars, collectionScopedAuth, baseReq, cancellationToken);
            channelWriter.TryWrite(new(inputLine, res, null));
        }

        if (rep.DelayInMs > 0L)
        {
            await Task.Delay((int)rep.DelayInMs!, cancellationToken);
        }

        return true;
    }

    internal static IEnumerable<PororocaVariable> CombineEffectiveVarsWithInputLine(IEnumerable<PororocaVariable> colEffectiveVars, PororocaVariable[]? inputLine)
    {
        if (inputLine is null)
            return colEffectiveVars;

        var colEffectiveVarsNotIntersectingInputLine = colEffectiveVars.Where(v => !inputLine.Any(ilv => ilv.Key == v.Key));
        return colEffectiveVarsNotIntersectingInputLine.Concat(inputLine);
    }

    public static TimeSpan EstimateRemainingTime(int total, int numberExecuted, TimeSpan elapsedSoFar) =>
        ((((float)total) / numberExecuted) - 1) * elapsedSoFar;
}