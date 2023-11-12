using System;
using Avalonia.Controls.Experimental.Data.Core;
using Avalonia.Experimental.Data.Core;
using Avalonia.VisualTree;

#nullable enable

namespace Avalonia.Experimental.Data
{
    internal class ParentDataContextRoot<T> : SingleSubscriberObservableBase<T?>
        where T : class
    {
        private readonly Visual _source;

        public ParentDataContextRoot(Visual source)
        {
            _source = source;
        }

        protected override void Subscribed()
        {
            ((AvaloniaObject)_source).PropertyChanged += SourcePropertyChanged;
            StartListeningToDataContext(_source.GetVisualParent());
            PublishValue();
        }

        protected override void Unsubscribed()
        {
            ((AvaloniaObject)_source).PropertyChanged -= SourcePropertyChanged;
        }

        private void PublishValue()
        {
            var parent = _source.GetVisualParent() as StyledElement;

            if (parent?.DataContext is null)
            {
                PublishNext(null);
            }
            else if (parent.DataContext is T value)
            {
                PublishNext(value);
            }
            else
            {
                // TODO: Log DataContext is unexpected type.
            }
        }

        private void StartListeningToDataContext(Visual? visual)
        {
            if (visual is StyledElement styled)
            {
                styled.PropertyChanged += ParentPropertyChanged;
            }
        }

        private void StopListeningToDataContext(Visual? visual)
        {
            if (visual is StyledElement styled)
            {
                styled.PropertyChanged -= ParentPropertyChanged;
            }
        }

        private void SourcePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            // TODO: Double check this with @grokys
            if (e.Property == Visual.VisualParentProperty)
            {
                StopListeningToDataContext(_source.GetVisualParent());
                StartListeningToDataContext(_source.GetVisualParent());
                PublishValue();
            }
        }

        private void ParentPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == StyledElement.DataContextProperty)
            {
                PublishValue();
            }
        }
    }
}
