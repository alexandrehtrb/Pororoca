using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.UITesting.Robots;

public sealed class CollectionScopedAuthRobot : BaseNamedRobot
{
    internal RequestAuthRobot Auth { get; }

    public CollectionScopedAuthRobot(CollectionScopedAuthView rootView) : base(rootView) =>
        Auth = new(GetChildView<RequestAuthView>("reqAuthView")!);
}