using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class KeyValueParamsDataGridViewModel : BaseDataGridWithOperationsViewModel<KeyValueParamViewModel, PororocaKeyValueParam>
{
    protected override SimpleClipboardArea<PororocaKeyValueParam> InnerClipboardArea =>
        KeyValueParamsClipboardArea.Instance;

    public KeyValueParamsDataGridViewModel(List<PororocaKeyValueParam>? initialValues = null) : base(initialValues)
    {
    }

    protected override KeyValueParamViewModel ToVm(PororocaKeyValueParam domainObj) =>
        new(Items, domainObj);

    protected override PororocaKeyValueParam ToDomain(KeyValueParamViewModel viewModel) =>
        viewModel.ToKeyValueParam();

    protected override PororocaKeyValueParam MakeCopy(PororocaKeyValueParam domainObj) =>
        domainObj.Copy();
}