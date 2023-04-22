using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorUtils
{
    internal static void SetEditorRawContent(this TextEditor editor, string updatedContent)
    {
        if (updatedContent != editor.Text)
        {
            // the document needs to be entirely replaced,
            // a text substitution breaks the syntax
            editor.Document = new TextDocument(updatedContent);
            //editor.Document.Text = updatedResponseContent;
        }
    }

    internal static void SetEditorSyntax(this TextMate.Installation tmInstallation, ref string? currentSyntaxLangId, string? updatedContentType)
    {
        string? updatedSyntaxLangId = FindSyntaxLanguageIdForContentType(updatedContentType);

        // only setting a new syntax language if it changed
        if (currentSyntaxLangId != updatedSyntaxLangId)
        {
            if (updatedSyntaxLangId is null)
            {
                tmInstallation.SetGrammar(null);
            }
            else
            {
                string scopeName = TextEditorConfiguration.DefaultRegistryOptions!.GetScopeByLanguageId(updatedSyntaxLangId);
                tmInstallation.SetGrammar(scopeName);
            }
            currentSyntaxLangId = updatedSyntaxLangId;
        }
    }

    private static string? FindSyntaxLanguageIdForContentType(string? contentType)
    {
        // The list of available TextMate syntaxes can be extracted with:
        // TextEditorConfiguration.DefaultRegistryOptions!.GetAvailableLanguages()

        if (contentType is null)
            return null;
        else if (contentType.Contains("json"))
            return "jsonc";
        else if (contentType.Contains("xml"))
            return "xml";
        else if (contentType.Contains("html"))
            return "html";
        else if (contentType.Contains("javascript"))
            return "javascript";
        else if (contentType.Contains("css"))
            return "css";
        else if (contentType.Contains("markdown"))
            return "markdown";
        else if (contentType.Contains("yaml"))
            return "yaml";
        else
            return null;
    }
}