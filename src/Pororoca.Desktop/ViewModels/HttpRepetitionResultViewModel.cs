using System.Net;
using Pororoca.Desktop.Localization;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using ReactiveUI.SourceGenerators;
using static Pororoca.Domain.Features.Common.HttpStatusCodeFormatter;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class HttpRepetitionResultViewModel : ViewModelBase
{
    [Reactive]
    public partial int IterationNumber { get; set; }

    [Reactive]
    public partial string ErrorDescriptionOrStatusCode { get; set; }

    [Reactive]
    public partial bool Successful { get; set; }

    public PororocaHttpRepetitionResult Result { get; }

    internal HttpRepetitionResultViewModel(int iterationNumber, PororocaHttpRepetitionResult result)
    {
        IterationNumber = iterationNumber;
        Result = result;
        long? elapsedTimeInMilliseconds = result.Response?.ElapsedTime is null ? null : (long)result.Response.ElapsedTime.TotalMilliseconds;
        if (result.Successful)
        {
            Successful = true;
            var statusCode = (HttpStatusCode)result.Response!.StatusCode!;
            ErrorDescriptionOrStatusCode = FormatHttpStatusCodeText(statusCode) + $" ({(long)elapsedTimeInMilliseconds!}ms)";
        }
        else
        {
            Successful = false;
            string errorMsg = result.ValidationErrorCode ??
             ((result.Response?.Exception is TaskCanceledException || result.Response?.Exception is OperationCanceledException) ?
               Localizer.Instance.HttpRepeater.RepetitionResultCancelled :
               Localizer.Instance.HttpRepeater.RepetitionResultException);
            ErrorDescriptionOrStatusCode = elapsedTimeInMilliseconds is null ? errorMsg : $"{errorMsg} ({(long)elapsedTimeInMilliseconds!}ms)";
        }
    }
}