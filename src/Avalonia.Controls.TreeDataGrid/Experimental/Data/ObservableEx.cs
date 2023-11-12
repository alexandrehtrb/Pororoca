using System;
using System.Reactive.Disposables;

namespace Avalonia.Experimental.Data
{
    internal class ObservableEx
    {
        public static IObservable<T> SingleValue<T>(T value)
        {
            return new SingleValueImpl<T>(value);
        }
 
        private sealed class SingleValueImpl<T> : IObservable<T>
        {
            private readonly T _value;

            public SingleValueImpl(T value)
            {
                _value = value;
            }
            public IDisposable Subscribe(IObserver<T> observer)
            {
                observer.OnNext(_value);
                return Disposable.Empty;
            }
        }
    }
}
