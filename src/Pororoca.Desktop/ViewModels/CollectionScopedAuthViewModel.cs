using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.SourceGenerators;

namespace Pororoca.Desktop.ViewModels;

public sealed partial class CollectionScopedAuthViewModel : CollectionOrganizationItemViewModel
{
    [Reactive]
    public partial RequestAuthViewModel AuthVm { get; set; } // TODO: Remove InheritedFromCollection option

    public CollectionScopedAuthViewModel(CollectionViewModel parentVm,
                                         PororocaCollection col) : base(parentVm, string.Empty) =>
        AuthVm = new(parentVm, col.CollectionScopedAuth, false, () => { });
}