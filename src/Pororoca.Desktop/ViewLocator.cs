using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop;

public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        string? name = data?.GetType()?.FullName?.Replace("ViewModel", "View");
#pragma warning disable IL2057
        var type = name != null ? Type.GetType(name) : null;
#pragma warning restore IL2057

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data) =>
        data is ViewModelBase;
}