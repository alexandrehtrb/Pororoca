using System.Text.Json;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Insomnia;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.Common.MimeTypesDetector;
using static Pororoca.Domain.Features.Entities.Pororoca.Http.PororocaHttpRequestBody;

namespace Pororoca.Domain.Features.ImportCollection;

public static class InsomniaCollectionV4Importer
{
    public static bool TryImportInsomniaCollection(string insomniaCollectionFileContent, out PororocaCollection? pororocaCollection)
    {
        try
        {
            var insomniaCol = JsonSerializer.Deserialize(insomniaCollectionFileContent, MainJsonCtxWithConverters.InsomniaCollectionV4);
            if (insomniaCol == null
             || insomniaCol.Type != "export")
            {
                pororocaCollection = null;
                return false;
            }

            return TryConvertToPororocaCollection(insomniaCol, out pororocaCollection);
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Error, "Failed to import Insomnia collection.", ex);
            pororocaCollection = null;
            return false;
        }
    }

    internal static bool TryConvertToPororocaCollection(InsomniaCollectionV4 insomniaCollection, out PororocaCollection? pororocaCollection)
    {
        try
        {
            // IMPORTANT!
            // removing resources that are not relevant (are null)
            // see InsomniaResourceJsonConverter class
            insomniaCollection.Resources = insomniaCollection.Resources.Where(r => r is not null).ToArray();

            // Always generating new id, in case user imports the same collection twice
            // This is to avoid overwriting when saving user collections
            var wrkspace = (InsomniaCollectionV4Workspace) (insomniaCollection.Resources.First(r => r is InsomniaCollectionV4Workspace));
            PororocaCollection myCol = new(
                Guid.NewGuid(),
                wrkspace.Name,
                DateTimeOffset.FromUnixTimeMilliseconds(wrkspace.Created));

            var envs = ReadEnvironments(insomniaCollection, wrkspace.Id);
            var (subdirs, reqs) = ReadResourceGroupReqsAndFolders(insomniaCollection, wrkspace.Id);

            myCol.Environments.AddRange(envs);
            myCol.Folders.AddRange(subdirs);
            myCol.Requests.AddRange(reqs);

            pororocaCollection = myCol;
            return true;
        }
        catch (Exception ex)
        {
            PororocaLogger.Instance?.Log(PororocaLogLevel.Error, "Failed to convert Insomnia collection to Pororoca.", ex);
            pororocaCollection = null;
            return false;
        }
    }

    internal static List<PororocaEnvironment> ReadEnvironments(InsomniaCollectionV4 col, string workspaceId) =>
        col.Resources
        .Where(r => r.ParentId == workspaceId && r.TypeAsEnum == InsomniaCollectionV4ResourceType.Environment)
        .Cast<InsomniaCollectionV4Environment>()
        .Select(ToEnvironment)
        .ToList();

    internal static PororocaEnvironment ToEnvironment(InsomniaCollectionV4Environment insomniaEnv) =>
        new(Id: Guid.NewGuid(),
            CreatedAt: DateTimeOffset.FromUnixTimeMilliseconds(insomniaEnv.Created),
            Name: insomniaEnv.Name,
            IsCurrent: false,
            Variables: insomniaEnv.Data.Select(kvp => new PororocaVariable(true, "_." + kvp.Key, kvp.Value.ToString(), true)).ToList());

    internal static PororocaCollectionFolder ToPororocaFolder(InsomniaCollectionV4 col, InsomniaCollectionV4RequestGroup reqGrp)
    {
        var (subdirs, reqs) = ReadResourceGroupReqsAndFolders(col, reqGrp.Id);
        return new(reqGrp.Name, subdirs.ToList(), reqs.ToList());
    }

    internal static (IEnumerable<PororocaCollectionFolder> Folders, IEnumerable<PororocaRequest> Reqs) ReadResourceGroupReqsAndFolders(InsomniaCollectionV4 col, string resourceGroupId)
    {
        var reqs = col.Resources
                      .Where(r => r.ParentId == resourceGroupId && r.TypeAsEnum == InsomniaCollectionV4ResourceType.Request)
                      .Cast<InsomniaCollectionV4Request>()
                      .Select(ToPororocaHttpReq)
                      .Cast<PororocaRequest>();

        var wss = col.Resources
                     .Where(r => r.ParentId == resourceGroupId && r.TypeAsEnum == InsomniaCollectionV4ResourceType.Websocket)
                     .Cast<InsomniaCollectionV4WebSocket>()
                     .Select(ToPororocaWs)
                     .Cast<PororocaRequest>();

        var subdirs = col.Resources
                         .Where(r => r.ParentId == resourceGroupId && r.TypeAsEnum == InsomniaCollectionV4ResourceType.RequestGroup)
                         .Cast<InsomniaCollectionV4RequestGroup>()
                         .Select(rg => ToPororocaFolder(col, rg));

        return new(subdirs, reqs.Concat(wss));
    }

    internal static PororocaHttpRequest ToPororocaHttpReq(InsomniaCollectionV4Request insomniaReq)
    {
        PororocaHttpRequestBody? body;
        switch (insomniaReq.Body?.MimeType)
        {
            case null:
                body = null;
                break;
            case "application/graphql":
                var gqlJson = JsonSerializer.Deserialize(insomniaReq.Body.Text!, MinifyingJsonCtx.DictionaryStringString)!;
                body = MakeGraphQlContent(gqlJson["query"], gqlJson["variables"]);
                break;
            case "multipart/form-data":
                body = MakeFormDataContent(insomniaReq.Body.Params!.Select(ToFormDataParam));
                break;
            case "application/x-www-form-urlencoded":
                body = MakeUrlEncodedContent(insomniaReq.Body.Params!.Select(ToKeyValueParam));
                break;
            case "application/octet-stream":
                string contentType = insomniaReq.Headers?.FirstOrDefault(h => h.Name == "Content-Type")?.Value ?? DefaultMimeTypeForBinary;
                body = MakeFileContent(insomniaReq.Body.FileName!, contentType);
                break;
            default:
                body = MakeRawContent(insomniaReq.Body.Text!, insomniaReq.Body.MimeType);
                break;
        }

        return new(
            Name: insomniaReq.Name,
            HttpVersion: 1.1m,
            HttpMethod: insomniaReq.Method,
            Url: insomniaReq.Url,
            Headers: ConvertHeaders(insomniaReq.Headers),
            Body: body,
            CustomAuth: ToAuth(insomniaReq.Authentication),
            ResponseCaptures: null);
    }

    internal static PororocaWebSocketConnection ToPororocaWs(InsomniaCollectionV4WebSocket insomniaWs) =>
        new(Name: insomniaWs.Name,
            HttpVersion: 1.1m,
            Url: insomniaWs.Url,
            Headers: ConvertHeaders(insomniaWs.Headers),
            CustomAuth: ToAuth(insomniaWs.Authentication));

    private static List<PororocaKeyValueParam>? ConvertHeaders(InsomniaCollectionV4NameValueParam[]? headers) =>
        headers
        ?.Where(h => !h.Name.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase))
        ?.Select(h => new PororocaKeyValueParam(!h.Disabled, h.Name, h.Value))
        ?.ToList();

    private static PororocaHttpRequestFormDataParam ToFormDataParam(InsomniaCollectionV4NameValueParam p) =>
        PororocaHttpRequestFormDataParam.MakeTextParam(!p.Disabled, p.Name, p.Value, DefaultMimeTypeForText);

    private static PororocaKeyValueParam ToKeyValueParam(InsomniaCollectionV4NameValueParam p) =>
        new(!p.Disabled, p.Name, p.Value);

    private static PororocaRequestAuth ToAuth(InsomniaCollectionV4RequestAuth? auth) =>
        (auth?.Type) switch
        {
            "basic" => PororocaRequestAuth.MakeBasicAuth(auth.Username!, auth.Password!),
            "bearer" => PororocaRequestAuth.MakeBearerAuth(auth.Token!),
            _ => PororocaRequestAuth.InheritedFromCollection,
        };
}