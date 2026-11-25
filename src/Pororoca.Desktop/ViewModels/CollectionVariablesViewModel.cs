using Pororoca.Desktop.ViewModels.DataGrids;
using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class CollectionVariablesViewModel : CollectionOrganizationItemViewModel
{
    [Reactive]
    public partial VariablesDataGridViewModel VariablesTableVm { get; set; }

    public CollectionVariablesViewModel(CollectionViewModel parentVm,
                                        PororocaCollection col) : base(parentVm, col.Name) =>
        VariablesTableVm = new(parentVm, col.Variables);
}