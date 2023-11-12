using System;
using System.Reactive.Subjects;
using System.Reflection;
using Avalonia.Data;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class CheckBoxCell : NotifyingBase, ICell, IDisposable
    {
        private readonly ISubject<BindingValue<bool?>>? _binding;
        private readonly IDisposable? _subscription;
        private bool? _value;

        public CheckBoxCell(bool? value)
        {
            _value = value;
            IsReadOnly = true;
        }

        public CheckBoxCell(
            ISubject<BindingValue<bool?>> binding,
            bool isReadOnly,
            bool isThreeState)
        {
            _binding = binding;
            IsReadOnly = isReadOnly;
            IsThreeState = isThreeState;

            _subscription = binding.Subscribe(x =>
            {
                if (x.HasValue)
                    Value = x.Value;
            });
        }

        public bool CanEdit => false;
        public BeginEditGestures EditGestures => BeginEditGestures.None;
        public bool SingleTapEdit => false;
        public bool IsReadOnly { get; }
        public bool IsThreeState { get; }

        public bool? Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value) && !IsReadOnly)
                    _binding!.OnNext(value);
            }
        }

        object? ICell.Value => Value;

        public void Dispose()
        {
            _subscription?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
