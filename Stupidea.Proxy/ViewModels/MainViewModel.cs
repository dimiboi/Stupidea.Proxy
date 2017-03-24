using ReactiveUI;
using Splat;
using Stupidea.Proxy.Services;
using Stupidea.Proxy.Views;

namespace Stupidea.Proxy.ViewModels
{
    public interface IMainViewModel : IReactiveObject, IScreen
    {
    }

    public class MainViewModel : ReactiveObject, IMainViewModel
    {
        private readonly RoutingState router;

        public MainViewModel(IMutableDependencyResolver resolver = null,
                             RoutingState router = null)
        {
            resolver = resolver ?? Locator.CurrentMutable;

            this.router = router ?? new RoutingState();

            RegisterDependencies(resolver);

            Router.Navigate.Execute(resolver.GetService<IProxyViewModel>());
        }

        public RoutingState Router => router;

        private void RegisterDependencies(IMutableDependencyResolver resolver)
        {
            // Services

            resolver.RegisterLazySingleton(() => new ProxyService(),
                                                 typeof(IProxyService));
            // Screen

            resolver.RegisterConstant(this,
                                      typeof(IScreen));

            resolver.RegisterConstant(this,
                                      typeof(IMainViewModel));
            // ViewModels

            resolver.Register(() => new ProxyViewModel(),
                                    typeof(IProxyViewModel));

            // Views

            resolver.Register(() => new ProxyView(),
                                    typeof(IViewFor<ProxyViewModel>));
        }
    }
}