using Avalonia.Controls;
using Avalonia.Input;

namespace Pororoca.Desktop.Controls;

public sealed class HeaderNameTextBox : TextBox
{
    protected override Type StyleKeyOverride => typeof(TextBox);

    private bool autocompleteEnabled = true;

    protected override void OnTextInput(TextInputEventArgs e)
    {
        // rudimentary implementation of autocomplete
        // could be better, with a drop-down of available options
        string? autocomplete = null;

        if (string.IsNullOrEmpty(Text))
        {
            this.autocompleteEnabled = true;
        }

        if (this.autocompleteEnabled && Text is not null && e.Text is not null)
        {
            autocomplete = ProvideAutoCompleteHeaderName(Text + e.Text);
        }

        base.OnTextInput(e);

        // this needs to happen after base.OnTextInput(e),
        // otherwise there may be an exception regarding index out of range
        if (autocomplete is not null)
        {
            this.autocompleteEnabled = false;
            Text = autocomplete;
        }
    }

    private static string? ProvideAutoCompleteHeaderName(string headerNameBeingTyped) =>
        headerNameBeingTyped?.ToLowerInvariant() switch
        {
            "ac" => "Accept",
            //"accept-d" => "Accept-Datetime",
            //"accept-e" => "Accept-Encoding",
            //"accept-l" => "Accept-Language",
            "ca" => "Cache-Control",
            "coo" => "Cookie",
            "con" => "Connection",
            //"content-l" => "Content-Language",
            //"content-e" => "Content-Encoding",
            "da" => "Date",
            "fr" => "From",
            "ho" => "Host",
            "or" => "Origin",
            "pra" => "Pragma",
            "pre" => "Prefer",
            "pro" => "Proxy-Authorization",
            "ra" => "Range",
            "re" => "Referer",
            "us" => "User-Agent",
            "x-ap" => "X-Api-Key",
            "x-au" => "X-Auth-Token",
            "x-tok" => "X-Token",
            _ => null
        };
}