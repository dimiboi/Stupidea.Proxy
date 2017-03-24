using ReactiveUI;
using Splat;
using Stupidea.Proxy.Services;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Stupidea.Proxy.ViewModels
{
    public interface IProxyViewModel : IReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        bool IsStarted { get; }
        ReactiveCommand<Unit, Unit> StartCommand { get; }
        ReactiveCommand<Unit, Unit> StopCommand { get; }
    }

    public class ProxyViewModel : ReactiveObject, IProxyViewModel
    {
        private readonly IMainViewModel window;
        private readonly IProxyService proxy;
        private readonly IInteractionService interactions;

        private bool isStarted;

        public ProxyViewModel(IMainViewModel window = null,
                              IProxyService proxy = null,
                              IInteractionService interactions = null)
        {
            this.window = window ?? Locator.Current.GetService<IMainViewModel>();
            this.proxy = proxy ?? Locator.Current.GetService<IProxyService>();
            this.interactions = interactions ?? Locator.Current.GetService<IInteractionService>();

            this.WhenActivated(disposables =>
            {
                StartCommand = ReactiveCommand.Create(() =>
                {
                    this.proxy.Start();
                    IsStarted = true;
                },
                this.WhenAnyValue(vm => vm.IsStarted, v => !v))
                .DisposeWith(disposables);

                StopCommand = ReactiveCommand.Create(() =>
                {
                    this.proxy.Stop();
                    IsStarted = false;
                },
                this.WhenAnyValue(vm => vm.IsStarted))
                .DisposeWith(disposables);

                this
                    .interactions
                    .WindowClosing
                    .RegisterHandler(async intertaction =>
                    {
                        if (await StopCommand.CanExecute.FirstAsync())
                        {
                            await StopCommand.Execute(Unit.Default);
                        }

                        intertaction.SetOutput(Unit.Default);
                    })
                    .DisposeWith(disposables);
            });
        }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public IScreen HostScreen => window;

        public string UrlPathSegment => "proxy";

        public bool IsStarted
        {
            get { return isStarted; }
            private set { this.RaiseAndSetIfChanged(ref isStarted, value); }
        }

        public ReactiveCommand<Unit, Unit> StartCommand { get; private set; }

        public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    }
}