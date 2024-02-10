using Avalonia.Controls;
using AvaloniaEdit;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class HttpRepeaterRobot : BaseNamedRobot
{
    public HttpRepeaterRobot(HttpRepeaterView rootView) : base(rootView) { }

    internal Button ExportAllLogs => GetChildView<Button>("btExportAllLogs")!;
    internal Button ExportReport => GetChildView<Button>("btExportReport")!;
    internal Button RefreshBaseHttpRequestsList => GetChildView<Button>("btRefreshBaseHttpRequestsList")!;
    internal Button InputDataFileSearch => GetChildView<Button>("btRepetitionInputDataFileSearch")!;
    internal Button ResultDetailBodySaveToFile => GetChildView<Button>("btResBodySaveToFile")!;
    internal Button ResultDetailDisableTlsVerification => GetChildView<Button>("btResDisableTlsVerification")!;
    internal Button ResExportLogFile => GetChildView<Button>("btResExportLogFile")!;
    internal Button SaveAllResponses => GetChildView<Button>("btSaveAllResponses")!;
    internal Button StartRepetition => GetChildView<Button>("btStartRepetition")!;
    internal Button StopRepetition => GetChildView<Button>("btStopRepetition")!;
    internal ComboBox BaseHttpRequest => GetChildView<ComboBox>("cbBaseHttpRequest")!;
    internal ComboBox InputDataType => GetChildView<ComboBox>("cbRepetitionInputDataType")!;
    internal ComboBox RepetitionMode => GetChildView<ComboBox>("cbRepetitionMode")!;
    internal ComboBoxItem OptionInputDataTypeFile => GetChildView<ComboBoxItem>("cbiRepetitionInputDataTypeFile")!;
    internal ComboBoxItem OptionInputDataTypeRaw => GetChildView<ComboBoxItem>("cbiRepetitionInputDataTypeRaw")!;
    internal ComboBoxItem OptionRepetitionModeRandom => GetChildView<ComboBoxItem>("cbiRepetitionModeRandom")!;
    internal ComboBoxItem OptionRepetitionModeSequential => GetChildView<ComboBoxItem>("cbiRepetitionModeSequential")!;
    internal ComboBoxItem OptionRepetitionModeSimple => GetChildView<ComboBoxItem>("cbiRepetitionModeSimple")!;
    internal DataGrid ResultDetailHeaders => GetChildView<DataGrid>("dgResHeaders")!;
    internal DataGrid ResultDetailInputLine => GetChildView<DataGrid>("dgResInputLine")!;
    internal ListBox RepetitionResults => GetChildView<ListBox>("lbRepetitionResults")!;
    internal TextEditor ResponseBodyRawEditor => GetChildView<TextEditor>("ResponseBodyRawContentEditor")!;
    internal TabControl TabControlRepetition => GetChildView<TabControl>("tabControlRepetition")!;
    internal TabControl TabControlResultDetail => GetChildView<TabControl>("tabControlRes")!;
    internal TabItem TabItemRepetitionInputData => GetChildView<TabItem>("tabItemRepetitionInputData")!;
    internal TabItem TabItemRepetitionMode => GetChildView<TabItem>("tabItemRepetitionMode")!;
    internal TabItem TabItemResultDetailBody => GetChildView<TabItem>("tabItemResBody")!;
    internal TabItem TabItemResultDetailHeaders => GetChildView<TabItem>("tabItemResHeaders")!;
    internal TabItem TabItemResultDetailInputLine => GetChildView<TabItem>("tabItemResInputLine")!;
    internal TextBlock ErrorMessage => GetChildView<TextBlock>("tbErrorMsg")!;
    internal TextBox InputDataFileSrcPath => GetChildView<TextBox>("tbRepetitionInputDataFileSrcPath")!;
    internal TextBlock RepetitionStatusMessage => GetChildView<TextBlock>("tbRepetitionStatus")!;
    internal TextBlock ResultDetailTitle => GetChildView<TextBlock>("tbResTitle")!;
    internal TextEditor InputDataRawEditor => GetChildView<TextEditor>("teRepetitionInputDataRaw")!;
    internal NumericUpDown NumberOfRepetitions => GetChildView<NumericUpDown>("nudNumberOfRepetitions")!;
    internal NumericUpDown MaxDop => GetChildView<NumericUpDown>("nudMaxDop")!;
    internal NumericUpDown DelayInMs => GetChildView<NumericUpDown>("nudDelayInMs")!;
}