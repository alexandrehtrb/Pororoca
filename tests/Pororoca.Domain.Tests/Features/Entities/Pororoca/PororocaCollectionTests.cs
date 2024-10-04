using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca;

public static class PororocaCollectionTests
{
    #region VARIABLE RESOLUTION

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public static void If_str_to_replace_templates_null_or_whitespace_then_return_empty_str(string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        string resolvedStr = IPororocaVariableResolver.ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        if (strToReplaceTemplates != null)
        {
            Assert.Equal(strToReplaceTemplates, resolvedStr);
        }
        else
        {
            Assert.Equal(string.Empty, resolvedStr);
        }
    }

    [Theory]
    [InlineData("v1/{{k3}}", "{{k1}}/{{k3}}")]
    [InlineData("v1/{{k4}}", "{{k1}}/{{k4}}")]
    public static void Should_use_collection_vars_to_resolve_template_if_no_env(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        col.Environments.Clear();
        col.Environments.AddRange(Array.Empty<PororocaEnvironment>());

        // WHEN
        string resolvedStr = IPororocaVariableResolver.ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        Assert.Equal(expectedResult, resolvedStr);
    }

    [Theory]
    [InlineData("v1/{{k3}}", "{{k1}}/{{k3}}")]
    [InlineData("v1/{{k4}}", "{{k1}}/{{k4}}")]
    public static void Should_use_collection_vars_to_resolve_template_if_no_selected_env(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        col = col with
        {
            Environments = col.Environments.Select(e => e with { IsCurrent = false }).ToList()
        };

        // WHEN
        string resolvedStr = IPororocaVariableResolver.ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        Assert.Equal(expectedResult, resolvedStr);
    }

    [Theory]
    [InlineData("v0/v1env1/v3env1", "{{k0}}/{{k1}}/{{k3}}")]
    [InlineData("v0/v1env1/{{k4}}", "{{k0}}/{{k1}}/{{k4}}")]
    [InlineData("k0k1k4", "k0k1k4")]
    [InlineData("k0v1env1k4", "k0{{k1}}k4")]
    public static void Should_use_selected_env_vars_with_collection_vars_to_resolve_template(string expectedResult, string? strToReplaceTemplates)
    {
        // GIVEN
        var col = CreateTestCollection();
        col = col with
        {
            Environments = col.Environments.Select(e => e with
            {
                IsCurrent = (e.Name == "MyEnvironment1")
            }).ToList()
        };

        // WHEN
        string resolvedStr = IPororocaVariableResolver.ReplaceTemplates(strToReplaceTemplates, ((IPororocaVariableResolver)col).GetEffectiveVariables());

        // THEN
        // Should use only enabled vars
        // Should use only vars from selected environment
        // Selected environment enabled vars override collection enabled vars
        Assert.Equal(expectedResult, resolvedStr);
    }

    #endregion

    #region FIND REQUEST IN COLLECTION

    [Fact]
    public static void Should_find_http_request_in_collection_in_root_folder()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.FindRequestInCollection<PororocaHttpRequest>(r => r.Name == "HttpReq0");

        // THEN
        Assert.NotNull(req);
        Assert.Equal("HttpReq0", req.Name);
    }

    [Fact]
    public static void Should_find_http_request_in_collection_in_subfolder()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.FindRequestInCollection<PororocaHttpRequest>(r => r.Name == "HttpReq1/1");

        // THEN
        Assert.NotNull(req);
        Assert.Equal("HttpReq1/1", req.Name);
    }

    [Fact]
    public static void Should_find_ws_request_in_collection()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.FindRequestInCollection<PororocaWebSocketConnection>(r => r.Name == "Ws1");

        // THEN
        Assert.NotNull(req);
        Assert.Equal("Ws1", req.Name);
    }

    [Fact]
    public static void Should_find_http_repetition_in_collection()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.FindRequestInCollection<PororocaHttpRepetition>(r => r.Name == "Rep1");

        // THEN
        Assert.NotNull(req);
        Assert.Equal("Rep1", req.Name);
    }

    #endregion

    #region GET HTTP REQUEST BY PATH

    [Fact]
    public static void Should_get_http_request_by_path_in_root_folder()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.GetHttpRequestByPath("HttpReq0");

        // THEN
        Assert.NotNull(req);
        Assert.Equal("HttpReq0", req.Name);
    }

    [Fact]
    public static void Should_get_http_request_by_path_in_subfolder()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var req = col.GetHttpRequestByPath("Dir1/Dir11/HttpReq11"); // should ignore slashes in name

        // THEN
        Assert.NotNull(req);
        Assert.Equal("HttpReq1/1", req.Name);
    }

    #endregion

    #region LIST HTTP REQUESTS PATHS

    [Fact]
    public static void Should_list_http_requests_paths_in_collection_correctly()
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var paths = col.ListHttpRequestsPathsInCollection();

        // THEN
        // should consider slashes in name
        Assert.NotNull(paths);
        Assert.Equal(2, paths.Count);
        Assert.Equal("HttpReq0", paths[0]);
        Assert.Equal("Dir1/Dir11/HttpReq11", paths[1]);
    }

    #endregion

    #region COPY

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static void Should_copy_full_collection_creating_new_instances(bool preservingIds)
    {
        // GIVEN
        var col = CreateTestCollection();

        // WHEN
        var copy = col.Copy(preservingIds);

        // THEN
        Assert.NotSame(col, copy);
        if (preservingIds)
        {
            Assert.Equal(col.Id, copy.Id);
        }
        else
        {
            Assert.NotEqual(col.Id, copy.Id);
        }
        Assert.Equal(col.Name, copy.Name);

        Assert.Equal(col.CreatedAt, copy.CreatedAt);

        Assert.Equal(col.Variables, copy.Variables);
        Assert.NotSame(col.Variables, copy.Variables);
        Assert.Equal(col.Variables.Count, copy.Variables.Count);
        foreach (var (varCol, varCopy) in col.Variables.Zip(copy.Variables))
        {
            Assert.Equal(varCol, varCopy);
            Assert.NotSame(varCol, varCopy);
        }

        Assert.Equal(col.CollectionScopedAuth, copy.CollectionScopedAuth);
        Assert.NotSame(col.CollectionScopedAuth, copy.CollectionScopedAuth);

        Assert.NotNull(copy.CollectionScopedRequestHeaders);
        Assert.Equal(col.CollectionScopedRequestHeaders, copy.CollectionScopedRequestHeaders);
        Assert.NotSame(col.CollectionScopedRequestHeaders, copy.CollectionScopedRequestHeaders);
        Assert.Equal(col.CollectionScopedRequestHeaders!.Count, copy.CollectionScopedRequestHeaders!.Count);
        foreach (var (csrhCol, csrhCopy) in col.CollectionScopedRequestHeaders.Zip(copy.CollectionScopedRequestHeaders))
        {
            Assert.Equal(csrhCol, csrhCopy);
            Assert.NotSame(csrhCol, csrhCopy);
        }

        Assert.Equal(col.Requests, copy.Requests);
        Assert.NotSame(col.Requests, copy.Requests);
        Assert.NotSame(col.Requests[0], copy.Requests[0]);

        Assert.NotNull(copy.Environments);
        Assert.NotSame(col.Environments, copy.Environments);
        Assert.Equal(col.Environments.Count, copy.Environments.Count);
        foreach (var (envCol, envCopy) in col.Environments.Zip(copy.Environments))
        {
            Assert.NotSame(envCol, envCopy);
            if (preservingIds)
            {
                Assert.Equal(envCol.Id, envCopy.Id);
            }
            else
            {
                Assert.NotEqual(envCol.Id, envCopy.Id);
            }
            Assert.Equal(envCol.CreatedAt, envCopy.CreatedAt);
            Assert.Equal(envCol.Name, envCopy.Name);
            Assert.Equal(envCol.IsCurrent, envCopy.IsCurrent);
            Assert.Equal(envCol.Variables, envCopy.Variables);
            Assert.NotSame(envCol.Variables, envCopy.Variables);
            Assert.Equal(envCol.Variables.Count, envCopy.Variables.Count);
            foreach (var (varCol, varCopy) in envCol.Variables.Zip(envCopy.Variables))
            {
                Assert.Equal(varCol, varCopy);
                Assert.NotSame(varCol, varCopy);
            }
        }

        var copiedDir1 = Assert.Single(copy.Folders);
        Assert.NotNull(copiedDir1.Requests);
        Assert.Empty(copiedDir1.Requests);
        Assert.NotNull(copiedDir1.Folders);
        Assert.Equal(2, copiedDir1.Folders.Count);

        var copiedDir1_0 = copiedDir1.Folders[0];
        Assert.Equal("Dir1/0", copiedDir1_0.Name);
        Assert.NotNull(copiedDir1_0.Folders);
        Assert.Empty(copiedDir1_0.Folders);
        Assert.NotNull(copiedDir1_0.Requests);
        Assert.Equal(2, copiedDir1_0.Requests.Count);
        Assert.Equal(col.Folders[0].Folders[0].Requests[0], copiedDir1_0.Requests[0]);
        Assert.NotSame(col.Folders[0].Folders[0].Requests[0], copiedDir1_0.Requests[0]);
        Assert.Equal(col.Folders[0].Folders[0].Requests[1], copiedDir1_0.Requests[1]);
        Assert.NotSame(col.Folders[0].Folders[0].Requests[1], copiedDir1_0.Requests[1]);

        var copiedDir1_1 = copiedDir1.Folders[1];
        Assert.Equal("Dir1/1", copiedDir1_1.Name);
        Assert.NotNull(copiedDir1_1.Folders);
        Assert.Empty(copiedDir1_1.Folders);
        Assert.NotNull(copiedDir1_1.Requests);
        var copiedReqDir1_1 = Assert.Single(copiedDir1_1.Requests);
        Assert.Equal(col.Folders[0].Folders[1].Requests[0], copiedReqDir1_1);
        Assert.NotSame(col.Folders[0].Folders[1].Requests[0], copiedReqDir1_1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static void Should_copy_empty_collection_creating_new_instances(bool preservingIds)
    {
        // GIVEN
        PororocaCollection col = new(
            Id: Guid.NewGuid(),
            Name: "name",
            CreatedAt: DateTimeOffset.Now,
            Variables: [],
            CollectionScopedAuth: null,
            CollectionScopedRequestHeaders: null,
            Environments: [],
            Requests: [],
            Folders: []);

        // WHEN
        var copy = col.Copy(preservingIds);

        // THEN
        Assert.NotSame(col, copy);
        if (preservingIds)
        {
            Assert.Equal(col.Id, copy.Id);
        }
        else
        {
            Assert.NotEqual(col.Id, copy.Id);
        }
        Assert.Equal(col.Name, copy.Name);
        Assert.Equal(col.CreatedAt, copy.CreatedAt);
        Assert.Equal(col.Variables, copy.Variables);
        Assert.NotSame(col.Variables, copy.Variables);
        Assert.Equal(col.CollectionScopedAuth, copy.CollectionScopedAuth);
        Assert.Equal(col.CollectionScopedRequestHeaders, copy.CollectionScopedRequestHeaders);
        Assert.Equal(col.Environments, copy.Environments);
        Assert.NotSame(col.Environments, copy.Environments);
        Assert.Equal(col.Requests, copy.Requests);
        Assert.NotSame(col.Requests, copy.Requests);
        Assert.Equal(col.Folders, copy.Folders);
        Assert.NotSame(col.Folders, copy.Folders);
    }

    #endregion

    private static PororocaCollection CreateTestCollection() =>
        new(Id: Guid.NewGuid(),
            Name: "MyCollection",
            CreatedAt: DateTimeOffset.Now,
            Variables: [
                new(true, "k0", "v0", false),
                new(true, "k1", "v1", false),
                new(false, "k2", "v2", false)
            ],
            CollectionScopedAuth: PororocaRequestAuth.MakeBearerAuth("tkn"),
            CollectionScopedRequestHeaders: [new(true, "ColScopedHeader", "val")],
            Environments: [
                new(Id: Guid.NewGuid(),
                    CreatedAt: DateTimeOffset.Now,
                    Name: "MyEnvironment1",
                    IsCurrent: false,
                    Variables: [
                        new(true, "k1", "v1env1", false),
                        new(true, "k3", "v3env1", false),
                        new(false, "k4", "v4env1", false)
                    ]),
                new(Id: Guid.NewGuid(),
                    CreatedAt: DateTimeOffset.Now,
                    Name: "MyEnvironment2",
                    IsCurrent: true,
                    Variables: [
                        new(true, "k1", "v1env2", false),
                        new(true, "k3", "v3env2", false),
                        new(false, "k4", "v4env2", false)
                    ])
                ],
            Requests: [new PororocaHttpRequest("HttpReq0")],
            Folders: [
                new(Name: "Dir1",
                    Requests: [],
                    Folders: [
                        new(Name: "Dir1/0",
                            Folders: [],
                            Requests: [new PororocaWebSocketConnection("Ws1"),
                                       new PororocaHttpRepetition("Rep1")]),
                        new(Name: "Dir1/1",
                            Folders: [],
                            Requests: [ new PororocaHttpRequest("HttpReq1/1") ])
                    ])
            ]);
}