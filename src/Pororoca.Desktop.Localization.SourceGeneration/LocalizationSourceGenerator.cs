using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace Pororoca.Desktop.Localization.SourceGeneration;

[Generator]
public sealed class LocalizationSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // define the execution pipeline here via a series of transformations:

        var assemblyNameProvider = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);

        var keyFilesContentsProvider = context.AdditionalTextsProvider
                                      .Where(static file => file.Path.EndsWith("i18n_keys.json"))
                                      .Select(static (file, ct) => file.GetText(ct)!.ToString())
                                      .Select(static (text, _) => JsonSerializer.Deserialize<string[]>(text)!)
                                      .Collect();

        var langFilesProvider = context.AdditionalTextsProvider
                                       .Where(static file => file.Path.EndsWith(".i18n_lang.json"));

        var allLangsProvider = langFilesProvider
                                .Select(static (file, _) => LanguageExtensions.GetLanguageByLCID(Path.GetFileNameWithoutExtension(file.Path).Split('.')[0]))
                                .Collect();

        var allLangsAndAllKeysProvider = allLangsProvider.Combine(keyFilesContentsProvider).Combine(assemblyNameProvider);

        // read their contents and save their name
        var langFilesWithContentsProvider = langFilesProvider
            .Select(static (f, ct) => (name: Path.GetFileNameWithoutExtension(f.Path), content: f.GetText(ct)!.ToString()))
            .Combine(assemblyNameProvider);

        // generate classes that contains contexts and their keys and values for each language file
        context.RegisterSourceOutput(langFilesWithContentsProvider, (spc, x) =>
        {
            string name = x.Left.name, content = x.Left.content, assemblyName = x.Right!;
            string lcid = name.Split('.')[0];
            var lang = LanguageExtensions.GetLanguageByLCID(lcid);
            var langDict = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;
            spc.AddSource($"{lang}Strings.g.cs", BuildCodeForLanguageClass(assemblyName, lang, langDict));
        });

        context.RegisterSourceOutput(allLangsAndAllKeysProvider, (spc, x) =>
        {
            string assemblyName = x.Right!;
            var allLangs = x.Left.Left;
            string[] allKeysWithContexts = x.Left.Right[0]!;
            string[] allContexts = allKeysWithContexts.Select(ck => ck.Split('/')[0]!).Distinct().ToArray();
            List<(string ctx, string[] ctxKeys)> contextsWithKeys = new();
            foreach (string? ctx in allContexts)
            {
                string[] ctxKeys = allKeysWithContexts
                    .Where(ck => ck.StartsWith(ctx + '/'))
                    .Select(ck => ck.Split('/')[1])
                    .ToArray();

                contextsWithKeys.Add((ctx, ctxKeys));
            }

            spc.AddSource("Contexts.g.cs", BuildCodeForContextsClasses(assemblyName, contextsWithKeys));
            spc.AddSource("Language.g.cs", BuildCodeForLanguagesEnum(assemblyName, allLangs));
            spc.AddSource("Localizer.g.cs", BuildCodeForLocalizerClass(assemblyName, allLangs, contextsWithKeys));
        });
    }

    private string BuildCodeForLanguagesEnum(string assemblyName, ImmutableArray<Language> langs)
    {
        StringBuilder sb = new($"namespace {assemblyName}.Localization;\npublic enum Language\n{{\n");
        foreach (var lang in langs)
        {
            sb.AppendLine($"\t{lang},");
        }
        sb.AppendLine("}");
        sb.AppendLine("public static class LanguageExtensions\n{");
        sb.AppendLine("\tpublic static string ToLCID(this Language lang) => lang switch\n\t{");
        foreach (var lang in langs)
        {
            sb.AppendLine($"\t\tLanguage.{lang} => \"{lang.ToLCID().ToLowerInvariant()}\",");
        }
        sb.AppendLine("\t\t_ => \"en-gb\",\n\t};\n");
        sb.AppendLine("\tpublic static Language GetLanguageByLCID(string lcid) => lcid.ToLowerInvariant() switch\n\t{");
        foreach (var lang in langs)
        {
            sb.AppendLine($"\t\t\"{lang.ToLCID().ToLowerInvariant()}\" => Language.{lang},");
        }
        sb.AppendLine("\t\t_ => throw new KeyNotFoundException($\"No language found for LCID '{lcid}'.\")\n\t};");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string BuildCodeForLocalizerClass(string assemblyName, ImmutableArray<Language> langs, List<(string ctx, string[] ctxKeys)> contextsWithKeys)
    {
        StringBuilder sb = new($"using ReactiveUI;\nnamespace {assemblyName}.Localization;\npublic sealed class Localizer\n{{");
        foreach (string ctx in contextsWithKeys.Select(x => x.ctx))
        {
            sb.AppendLine($"\n\tpublic {ctx}Context {ctx} {{ get; }} = new();");
        }

        sb.AppendLine("\n\tprivate readonly List<Action> languageChangedSubscriptions = new();");
        sb.AppendLine("\tpublic void SubscribeToLanguageChange(Action onLanguageChanged) => this.languageChangedSubscriptions.Add(onLanguageChanged);\n");
        sb.AppendLine("\tprivate Language currentLanguageField;");
        sb.AppendLine("\tpublic Language CurrentLanguage\n\t{");
        sb.AppendLine("\t\tget => this.currentLanguageField;");
        sb.AppendLine("\t\tset\n\t\t{");
        // The line below is commented because the default initial value is 0.
        // When starting the program, CurrentLanguage will be 0, that corresponds to the first Language in the enum,
        // however, the strings are not loaded yet and we need to load them when setting CurrentLanguage at the startup.
        // if (this.currentLanguageField is value) return
        sb.AppendLine("\t\t\tthis.currentLanguageField = value;");
        sb.AppendLine("\t\t\tswitch(value)\n\t\t\t{");
        foreach (var lang in langs)
        {
            sb.AppendLine($"\t\t\t\tcase Language.{lang}:");
            foreach (var ctxWithKeys in contextsWithKeys)
            {
                string ctx = ctxWithKeys.ctx;
                foreach (string key in ctxWithKeys.ctxKeys)
                {
                    sb.AppendLine($"\t\t\t\t\t{ctx}.{key} = {lang}Strings.{ctx}.{key};");
                }
            }
            sb.AppendLine($"\t\t\t\t\tbreak;");
        }
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t\tthis.languageChangedSubscriptions.ForEach(sub => sub.Invoke());");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        sb.AppendLine("\n\tprivate Localizer() {}");
        sb.AppendLine("\n\tpublic static readonly Localizer Instance = new();");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildCodeForLanguageClass(string assemblyName, Language lang, Dictionary<string, string> kvs)
    {
        StringBuilder sb = new($"namespace {assemblyName}.Localization;\npublic static class {lang}Strings\n{{\n");
        string[] contexts = kvs.Keys.Select(k => k.Split('/')[0]).Distinct().ToArray();
        foreach (string ctx in contexts)
        {
            sb.AppendLine($"\tpublic static class {ctx}\n\t{{");
            var ctxKvs = kvs.Where(kv => kv.Key.StartsWith(ctx + '/'));
            foreach (var ctxKv in ctxKvs)
            {
                string[] contextAndKey = ctxKv.Key.Split('/');
                string key = contextAndKey[1], value = ctxKv.Value;
                sb.AppendLine($"\t\tpublic const string {key} = \"{value}\";");
            }
            sb.AppendLine("\t}");
        }
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string BuildCodeForContextsClasses(string assemblyName, List<(string ctx, string[] ctxKeys)> contextsWithKeys)
    {
        StringBuilder sb = new($"using ReactiveUI;\nnamespace {assemblyName}.Localization;\n");
        foreach (var (ctx, ctxKeys) in contextsWithKeys)
        {
            sb.AppendLine(BuildCodeForContextClass(ctx, ctxKeys));
        }
        return sb.ToString();
    }

    private string BuildCodeForContextClass(string ctx, string[] contextKeys)
    {
        StringBuilder sb = new($"public sealed class {ctx}Context : ReactiveObject\n{{");

        foreach (string key in contextKeys)
        {
            string keyField = key.ToLower() + "Field";
            sb.AppendLine($"\n\tprivate string {keyField};");
            sb.AppendLine($"\tpublic string {key} {{ get => this.{keyField}; set => this.RaiseAndSetIfChanged(ref this.{keyField}, value); }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}