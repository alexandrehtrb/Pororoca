using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionVariablesViewModel : CollectionOrganizationItemViewModel
{
    [Reactive]
    public VariablesDataGridViewModel VariablesTableVm { get; set; }

    public CollectionVariablesViewModel(CollectionViewModel parentVm,
                                        PororocaCollection col) : base(parentVm, col.Name) =>
        VariablesTableVm = new(parentVm, col.Variables);
}