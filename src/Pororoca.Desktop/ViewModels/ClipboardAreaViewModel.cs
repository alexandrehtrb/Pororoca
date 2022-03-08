using System;
using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class ClipboardAreaViewModel : ReactiveObject
    {
        public static readonly ClipboardAreaViewModel Singleton = new();

        private ICloneable? _domainObjectToCopy = null;
        
        private ClipboardAreaViewModel()
        {
        }

        private bool _canPasteEnvironment;
        public bool CanPasteEnvironment
        {
            get => _canPasteEnvironment;
            private set
            {
                this.RaiseAndSetIfChanged(ref _canPasteEnvironment, value);
            }
        }

        private bool _canPasteCollectionFolderOrRequest;
        public bool CanPasteCollectionFolderOrRequest
        {
            get => _canPasteCollectionFolderOrRequest;
            private set
            {
                this.RaiseAndSetIfChanged(ref _canPasteCollectionFolderOrRequest, value);
            }
        }

        public void PushToCopy(ICloneable domainObjToCopy)
        {
            _domainObjectToCopy = domainObjToCopy;
            CanPasteEnvironment = domainObjToCopy is PororocaEnvironment;
            CanPasteCollectionFolderOrRequest = (domainObjToCopy is PororocaCollectionFolder || domainObjToCopy is PororocaRequest);
        }

        public ICloneable? FetchCopy() =>
            (ICloneable?)_domainObjectToCopy?.Clone();
    }
}