using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class VariablesDataGridViewModel : BaseDataGridWithOperationsViewModel<VariableViewModel, PororocaVariable>
{
    public override SimpleClipboardArea<PororocaVariable> InnerClipboardArea =>
        VariablesClipboardArea.Instance;

    public VariablesDataGridViewModel(List<PororocaVariable> initialValues) : base(initialValues)
    {
    }

    protected override VariableViewModel ToVm(PororocaVariable domainObj) =>
        new(Items, domainObj);

    protected override PororocaVariable ToDomain(VariableViewModel viewModel) =>
        viewModel.ToVariable();

    protected override PororocaVariable MakeCopy(PororocaVariable domainObj) =>
        domainObj.Copy();
}