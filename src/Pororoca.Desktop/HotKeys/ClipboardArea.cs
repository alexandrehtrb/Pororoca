using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.HotKeys;

public sealed class ClipboardArea : ViewModelBase
{
    internal static readonly ClipboardArea Instance = new();

    private readonly List<object> copiedDomainObjs = new();

    internal List<CollectionOrganizationItemViewModel>? ItemsMarkedForCut { get; set; }

    [Reactive]
    public bool CanPasteEnvironment { get; private set; }

    [Reactive]
    public bool CanPasteCollectionFolderOrRequest { get; private set; }

    [Reactive]
    public bool CanPasteWebSocketClientMessage { get; private set; }

    public bool OnlyHasCopiesOfWebSocketClientMessages =>
        this.copiedDomainObjs.TrueForAll(x => x is PororocaWebSocketClientMessage);

    public void ClearCopiedItems()
    {
        this.copiedDomainObjs.Clear();
        CanPasteEnvironment = false;
        CanPasteCollectionFolderOrRequest = false;
        CanPasteWebSocketClientMessage = false;
    }

    public void PushToCopy(params object[] domainObjsToCopy)
    {
        this.copiedDomainObjs.Clear();
        this.copiedDomainObjs.AddRange(domainObjsToCopy);
        CanPasteEnvironment = this.copiedDomainObjs.Any(o => o is PororocaEnvironment);
        CanPasteCollectionFolderOrRequest = this.copiedDomainObjs.Any(o => o is PororocaCollectionFolder || o is PororocaRequest);
        CanPasteWebSocketClientMessage = this.copiedDomainObjs.Any(o => o is PororocaWebSocketClientMessage);
    }

    public IList<object> FetchCopiesOfFoldersAndReqs() =>
        this.copiedDomainObjs.Where(o => o is PororocaCollectionFolder || o is PororocaRequest)
                             .Select(o =>
                             {
                                 if (o is PororocaCollectionFolder f)
                                     return (object)f.Copy();
                                 else if (o is PororocaRequest r)
                                     return (object)r.CopyAbstract();
                                 else
                                     throw new InvalidCastException();
                             })
                             .ToList();

    public IList<PororocaEnvironment> FetchCopiesOfEnvironments() =>
        this.copiedDomainObjs.Where(o => o is PororocaEnvironment)
                        .Select(o => ((PororocaEnvironment)o).Copy(preserveId: false))
                        .ToList();

    public IList<PororocaWebSocketClientMessage> FetchCopiesOfWebSocketClientMessages() =>
        this.copiedDomainObjs.Where(o => o is PororocaWebSocketClientMessage)
                        .Select(o => ((PororocaWebSocketClientMessage)o).Copy())
                        .ToList();
}