using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;
using static Pororoca.Desktop.HotKeys.KeyValueParamsClipboardArea;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class RequestHeadersDataGridViewModel : BaseDataGridWithOperationsViewModel<RequestHeaderViewModel, PororocaKeyValueParam>
{
    public override SimpleClipboardArea<PororocaKeyValueParam> InnerClipboardArea =>
        KeyValueParamsClipboardArea.Instance;

    public RequestHeadersDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
    {
    }

    protected override RequestHeaderViewModel ToVm(PororocaKeyValueParam domainObj) =>
        new(Items, domainObj);

    protected override PororocaKeyValueParam ToDomain(RequestHeaderViewModel viewModel) =>
        viewModel.ToKeyValueParam();

    protected override PororocaKeyValueParam MakeCopy(PororocaKeyValueParam domainObj) =>
        domainObj.Copy();

    internal override async Task PasteAsync()
    {
        await base.PasteAsync();
        var headersFromSystemClipboard = await GetColonSeparatedValuesFromSystemClipboardAreaAsync();
        foreach (var h in headersFromSystemClipboard)
        {
            Items.Add(ToVm(h));
        }
    }
}