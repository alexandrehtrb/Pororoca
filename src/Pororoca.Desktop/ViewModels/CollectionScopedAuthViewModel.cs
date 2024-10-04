using Pororoca.Domain.Features.Entities.Pororoca;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.ViewModels;

public sealed class CollectionScopedAuthViewModel : CollectionOrganizationItemViewModel
{
    [Reactive]
    public RequestAuthViewModel AuthVm { get; set; } // TODO: Remove InheritedFromCollection option

    public CollectionScopedAuthViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                         PororocaCollection col) : base(parentVm, string.Empty) =>
        AuthVm = new(col.CollectionScopedAuth, false, () => { });
}