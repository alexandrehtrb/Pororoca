using System.Runtime.CompilerServices;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;

namespace Pororoca.Desktop.TextEditorConfig;

internal static class TextEditorUtils
{
    internal const string XmlSyntaxLangId = "xml";

    // shared by all editors: the strategy keeps no per-editor state
    private static readonly XmlFoldingStrategy XmlFoldingStrategy = new();
    private static readonly ConditionalWeakTable<TextEditor, FoldingManager> XmlFoldingManagers = new();

    /// <summary>
    /// Installs XML folding on the editor when enabled, and removes it when disabled.
    /// While enabled, folds are recalculated whenever the document is replaced or its text changes.
    /// </summary>
    internal static void SetEditorXmlFolding(this TextEditor editor, bool enabled)
    {
        if (enabled)
        {
            editor.DocumentChanged -= OnEditorDocumentChangedForXmlFolding;
            editor.DocumentChanged += OnEditorDocumentChangedForXmlFolding;
            editor.TextChanged -= OnEditorTextChangedForXmlFolding;
            editor.TextChanged += OnEditorTextChangedForXmlFolding;

            if (!XmlFoldingManagers.TryGetValue(editor, out _))
                InstallXmlFoldingManager(editor);
        }
        else
        {
            editor.DocumentChanged -= OnEditorDocumentChangedForXmlFolding;
            editor.TextChanged -= OnEditorTextChangedForXmlFolding;
            UninstallXmlFoldingManager(editor);
        }
    }

    private static void OnEditorDocumentChangedForXmlFolding(object? sender, EventArgs e)
    {
        // the document was replaced, so the folding manager must be bound to the new one
        var editor = (TextEditor)sender!;
        if (XmlFoldingManagers.TryGetValue(editor, out _))
        {
            UninstallXmlFoldingManager(editor);
            InstallXmlFoldingManager(editor);
        }
    }

    private static void OnEditorTextChangedForXmlFolding(object? sender, EventArgs e) =>
        UpdateXmlFoldings((TextEditor)sender!);

    private static void InstallXmlFoldingManager(TextEditor editor)
    {
        var foldingManager = FoldingManager.Install(editor.TextArea);
        XmlFoldingManagers.Remove(editor);
        XmlFoldingManagers.Add(editor, foldingManager);
        UpdateXmlFoldings(editor);
    }

    private static void UninstallXmlFoldingManager(TextEditor editor)
    {
        if (XmlFoldingManagers.TryGetValue(editor, out var foldingManager))
        {
            FoldingManager.Uninstall(foldingManager);
            XmlFoldingManagers.Remove(editor);
        }
    }

    private static void UpdateXmlFoldings(TextEditor editor)
    {
        if (XmlFoldingManagers.TryGetValue(editor, out var foldingManager))
            XmlFoldingStrategy.UpdateFoldings(foldingManager, editor.Document);
    }

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
                string scopeName = TextEditorConfiguration.DefaultRegistryOptions!.Value!.GetScopeByLanguageId(updatedSyntaxLangId);
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
            return XmlSyntaxLangId;
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