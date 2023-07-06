using Pororoca.Desktop.Localization;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Localizer i18n { get; } = Localizer.Instance;
}