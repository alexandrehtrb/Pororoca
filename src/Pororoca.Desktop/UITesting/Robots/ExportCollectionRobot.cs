using Avalonia.Controls;
using Pororoca.Desktop.Converters;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class ExportCollectionRobot : BaseNamedRobot
{
    public ExportCollectionRobot(ExportCollectionView rootView)
     : base(rootView) { }

    internal Button GoBack => GetChildView<Button>("btGoBack")!;
    internal TextBlock CollectionName => GetChildView<TextBlock>("tbCollectionName")!;
    internal CheckBox IncludeCollectionSecretVariables => GetChildView<CheckBox>("cbIncludeSecretsWhenExporting")!;
    internal ComboBox Format => GetChildView<ComboBox>("cbExportFormat")!;
    internal ComboBoxItem FormatOptionPororoca => GetChildView<ComboBoxItem>("cbiExportFormatPororoca")!;
    internal ComboBoxItem FormatOptionPostman => GetChildView<ComboBoxItem>("cbiExportFormatPostman")!;
    internal DataGrid Environments => GetChildView<DataGrid>("dgEnvironmentsToInclude")!;

    internal async Task SetIncludeColSecretVars(bool includeColSecretVars)
    {
        IncludeCollectionSecretVariables.IsChecked = includeColSecretVars;
        await UITestActions.WaitAfterActionAsync();
    }

    internal Task SelectExportFormat(ExportCollectionFormat format) =>
        Format.Select(format == ExportCollectionFormat.Postman ?
            FormatOptionPostman :
            FormatOptionPororoca);

    internal async Task SetEnvironmentToExport(bool includeInExport, string envName, bool includeSecrets)
    {
        var envGrpVm = ((ExportCollectionViewModel)RootView!.DataContext!).EnvironmentsToExport;
        var env = envGrpVm.First(x => x.Name == envName).ExportEnvironmentVm;
        env.IncludeInCollectionExport = includeInExport;
        env.IncludeSecretVariables = includeSecrets;
        await UITestActions.WaitAfterActionAsync();
    }
}