using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Pororoca.Desktop.ViewModels;

namespace Pororoca.Desktop;

public class ViewLocator : IDataTemplate
{
    public IControl? Build(object? data)
    {
        string? name = data?.GetType()?.FullName?.Replace("ViewModel", "View");
        var type = name != null ? Type.GetType(name) : null;

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