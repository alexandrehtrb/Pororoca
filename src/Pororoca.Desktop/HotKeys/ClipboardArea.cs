using Pororoca.Desktop.ViewModels;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using ReactiveUI.Fody.Helpers;

namespace Pororoca.Desktop.HotKeys;

public sealed class ClipboardArea : ViewModelBase
{
    internal static readonly ClipboardArea Instance = new();

    private readonly List<ICloneable> copiedDomainObjs = new();

    [Reactive]
    public bool CanPasteEnvironment { get; set; }

    [Reactive]
    public bool CanPasteCollectionFolderOrRequest { get; set; }

    [Reactive]
    public bool CanPasteWebSocketClientMessage { get; set; }

    public bool OnlyHasCopiesOfWebSocketClientMessages =>
        this.copiedDomainObjs.TrueForAll(x => x is PororocaWebSocketClientMessage);

    public void PushToCopy(params ICloneable[] domainObjsToCopy)
    {
        this.copiedDomainObjs.Clear();
        this.copiedDomainObjs.AddRange(domainObjsToCopy);
        CanPasteEnvironment = this.copiedDomainObjs.Any(o => o is PororocaEnvironment);
        CanPasteCollectionFolderOrRequest = this.copiedDomainObjs.Any(o => o is PororocaCollectionFolder || o is PororocaRequest);
        CanPasteWebSocketClientMessage = this.copiedDomainObjs.Any(o => o is PororocaWebSocketClientMessage);
    }

    public IList<PororocaCollectionItem> FetchCopiesOfFoldersAndReqs() =>
        this.copiedDomainObjs.Where(o => o is PororocaCollectionFolder || o is PororocaRequest)
                             .Select(o => o.Clone())
                             .Cast<PororocaCollectionItem>()
                             .ToList();

    public IList<PororocaEnvironment> FetchCopiesOfEnvironments() =>
        this.copiedDomainObjs.Where(o => o is PororocaEnvironment)
                        .Select(o => o.Clone())
                        .Cast<PororocaEnvironment>()
                        .ToList();

    public IList<PororocaWebSocketClientMessage> FetchCopiesOfWebSocketClientMessages() =>
        this.copiedDomainObjs.Where(o => o is PororocaWebSocketClientMessage)
                        .Select(o => o.Clone())
                        .Cast<PororocaWebSocketClientMessage>()
                        .ToList();
}