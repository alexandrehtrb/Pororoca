using System.Collections.ObjectModel;

namespace Pororoca.Desktop.ViewModels;

internal static class ItemPositionHelper
{
    internal static int GetLastIndexOf<T>(this ObservableCollection<CollectionOrganizationItemViewModel> items) where T : CollectionOrganizationItemViewModel
    {
        var last = items.LastOrDefault(x => x is T);
        if (last is null) return -1;
        else return items.IndexOf(last);
    }
}