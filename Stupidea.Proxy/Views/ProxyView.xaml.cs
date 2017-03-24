using ReactiveUI;
using Stupidea.Proxy.ViewModels;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Stupidea.Proxy.Views
{
    public partial class ProxyView : UserControl, IViewFor<IProxyViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel",
                                        typeof(IProxyViewModel),
                                        typeof(ProxyView),
                                        new PropertyMetadata(null));

        public ProxyView()
        {
            InitializeComponent();

            IDisposable activation = null;
            activation = this.WhenActivated(disposables =>
            {
                activation
                    .DisposeWith(disposables);

                this // ViewModel -> DataContext
                    .WhenAnyValue(v => v.ViewModel)
                    .BindTo(this, v => v.DataContext)
                    .DisposeWith(disposables);

                this
                    .BindCommand(ViewModel,
                        vm => vm.StartCommand,
                        v => v.StartButton)
                    .DisposeWith(disposables);

                this
                    .BindCommand(ViewModel,
                        vm => vm.StopCommand,
                        v => v.StopButton)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(v => v.ViewModel.IsStarted)
                    .Select(v => v ? Visibility.Visible : Visibility.Hidden)
                    .BindTo(this, v => v.GoofySvg.Visibility)
                    .DisposeWith(disposables);

                this
                    .WhenAnyValue(v => v.ViewModel.IsStarted)
                    .Select(v => v ? Visibility.Hidden : Visibility.Visible)
                    .BindTo(this, v => v.HappySvg.Visibility)
                    .DisposeWith(disposables);
            });
        }

        public IProxyViewModel ViewModel
        {
            get { return (IProxyViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IProxyViewModel)value; }
        }
    }
}