using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class VariablesDataGridViewModel : BaseDataGridWithOperationsViewModel<VariableViewModel, PororocaVariable>
{
    public override SimpleClipboardArea<PororocaVariable> InnerClipboardArea =>
        VariablesClipboardArea.Instance;

    public VariablesDataGridViewModel(List<PororocaVariable>? initialValues = null) : base(initialValues)
    {
    }

    public List<PororocaVariable> GetVariables(bool includeSecretVariables) =>
        Items.Select(i =>
        {
            var v = ToDomain(i);
            return includeSecretVariables ? v : v.Censor();
        }).ToList();

    protected override VariableViewModel ToVm(PororocaVariable domainObj) =>
        new(Items, domainObj);

    protected override PororocaVariable ToDomain(VariableViewModel viewModel) =>
        viewModel.ToVariable();

    protected override PororocaVariable MakeCopy(PororocaVariable domainObj) =>
        domainObj.Copy();
}