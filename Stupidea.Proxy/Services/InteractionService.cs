using ReactiveUI;
using System.Reactive;

namespace Stupidea.Proxy.Services
{
    public interface IInteractionService
    {
        Interaction<Unit, Unit> WindowClosing { get; }
    }

    public class InteractionService : IInteractionService
    {
        public Interaction<Unit, Unit> WindowClosing { get; } = new Interaction<Unit, Unit>();
    }
}