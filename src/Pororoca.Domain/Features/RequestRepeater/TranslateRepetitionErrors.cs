namespace Pororoca.Domain.Features.RequestRepeater;

public static class TranslateRepetitionErrors
{
    public const string BaseHttpRequestNotSelected = "TranslateRepetition_BaseHttpRequestNotSelected";
    public const string BaseHttpRequestNotFound = "TranslateRepetition_BaseHttpRequestNotFound";
    public const string DelayCantBeNegative = "TranslateRepetition_DelayCantBeNegative";
    public const string MaximumRateCantBeNegative = "TranslateRepetition_MaximumRateCantBeNegative";
    public const string NumberOfRepetitionsMustBeAtLeast1 = "TranslateRepetition_NumberOfRepetitionsMustBeAtLeast1";
    public const string MaxDopMustBeAtLeast1 = "TranslateRepetition_MaxDopMustBeAtLeast1";
    public const string InputDataFileNotFound = "TranslateRepetition_InputDataFileNotFound";
    public const string InputDataInvalid = "TranslateRepetition_InputDataInvalid";
    public const string InputDataAtLeastOneLine = "TranslateRepetition_InputDataAtLeastOneLine";
    public const string UnknownError = "TranslateRepetition_UnknownError";
}