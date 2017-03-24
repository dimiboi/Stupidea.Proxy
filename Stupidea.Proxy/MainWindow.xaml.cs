using MahApps.Metro.Controls;
using ReactiveUI;
using Splat;
using Stupidea.Proxy.Services;
using Stupidea.Proxy.ViewModels;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace Stupidea.Proxy
{
    public partial class MainWindow : MetroWindow, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel",
                                        typeof(IMainViewModel),
                                        typeof(MainWindow),
                                        new PropertyMetadata(null));

        private readonly IInteractionService interactions;

        public MainWindow() : this(null)
        {
        }

        public MainWindow(IInteractionService interactions)
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            this.interactions = interactions ?? Locator.Current.GetService<IInteractionService>();

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
                    .Events()
                    .Closing
                    .Subscribe(async e => await this.interactions
                                                    .WindowClosing.Handle(Unit.Default))
                    .DisposeWith(disposables);

                this
                    .OneWayBind(ViewModel,
                        vm => vm.Router,
                        v => v.ViewHost.Router)
                    .DisposeWith(disposables);
            });
        }

        public IMainViewModel ViewModel
        {
            get { return (IMainViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IMainViewModel)value; }
        }
    }
}