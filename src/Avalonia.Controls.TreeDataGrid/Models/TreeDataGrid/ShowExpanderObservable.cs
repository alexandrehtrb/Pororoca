using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls.Experimental.Data.Core;
using Avalonia.Data;
using Avalonia.Experimental.Data;
using Avalonia.Experimental.Data.Core;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    internal class ShowExpanderObservable<TModel> : SingleSubscriberObservableBase<bool>,
        IObserver<BindingValue<bool>>,
        IObserver<BindingValue<IEnumerable<TModel>?>>
            where TModel : class
    {
        private readonly Func<TModel, IEnumerable<TModel>?> _childSelector;
        private readonly TypedBinding<TModel, bool>? _hasChildrenSelector;
        private TModel? _model;
        private IDisposable? _subscription;
        private INotifyCollectionChanged? _incc;

        public ShowExpanderObservable(
            Func<TModel, IEnumerable<TModel>?> childSelector,
            TypedBinding<TModel, bool>? hasChildrenSelector,
            TModel model)
        {
            _childSelector = childSelector;
            _hasChildrenSelector = hasChildrenSelector;
            _model = model;
        }

        protected override void Subscribed()
        {
            if (_model is null)
                throw new ObjectDisposedException(nameof(ShowExpanderObservable<TModel>));

            if (_hasChildrenSelector is not null)
                _subscription = _hasChildrenSelector?.Instance(_model).Subscribe(this);
            else
                // TODO: _childSelector needs to be made into a binding; leaving the observable
                // machinery in place for this to be turned into a subscription later.
                ((IObserver<BindingValue<IEnumerable<TModel>?>>)this).OnNext(new(_childSelector(_model)));
        }

        protected override void Unsubscribed()
        {
            _subscription?.Dispose();
            _subscription = null;
            _model = null;
        }

        void IObserver<BindingValue<bool>>.OnNext(BindingValue<bool> value)
        {
            if (value.HasValue)
                PublishNext(value.Value);
        }

        void IObserver<BindingValue<IEnumerable<TModel>?>>.OnNext(BindingValue<IEnumerable<TModel>?> value)
        {
            if (_incc is not null)
                _incc.CollectionChanged -= OnCollectionChanged;

            if (value.HasValue && value.Value is not null)
            {
                if (value.Value is INotifyCollectionChanged incc)
                {
                    _incc = incc;
                    _incc.CollectionChanged += OnCollectionChanged;
                }

                PublishNext(value.Value.Any());
            }
            else
            {
                PublishNext(false);
            }
        }

        void IObserver<BindingValue<bool>>.OnCompleted() { }
        void IObserver<BindingValue<IEnumerable<TModel>?>>.OnCompleted() { }
        void IObserver<BindingValue<bool>>.OnError(Exception error) { }
        void IObserver<BindingValue<IEnumerable<TModel>?>>.OnError(Exception error) { }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            PublishNext((sender as IEnumerable<TModel>)?.Any() ?? false);
        }
    }
}
