﻿using MahApps.Metro.Controls;
using ReactiveUI;
using Stupidea.Proxy.ViewModels;
using System;
using System.Reactive.Disposables;
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

        public MainWindow()
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
                    .OneWayBind(ViewModel,
                        vm => vm.Router,
                        v => v.ViewHost.Router)
                    .DisposeWith(disposables);
            });

            ViewModel = new MainViewModel();
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