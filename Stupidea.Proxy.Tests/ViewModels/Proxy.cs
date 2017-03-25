using FluentAssertions;
using Ninject;
using NSubstitute;
using Stupidea.Proxy.Services;
using Stupidea.Proxy.ViewModels;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ViewModels
{
    public class Proxy
    {
        private readonly IKernel kernel;

        private readonly IMainViewModel window;
        private readonly IProxyService proxy;
        private readonly IInteractionService interactions;

        public Proxy()
        {
            kernel = new StandardKernel();

            window = Substitute.For<IMainViewModel>();
            proxy = Substitute.For<IProxyService>();
            interactions = new InteractionService();

            kernel.Bind<IMainViewModel>()
                  .ToConstant(window);

            kernel.Bind<IProxyService>()
                  .ToConstant(proxy);

            kernel.Bind<IInteractionService>()
                  .ToConstant(interactions);

            kernel.Bind<IProxyViewModel>()
                  .To<ProxyViewModel>();
        }

        [Fact]
        public void SetsActivator()
            => kernel.Get<IProxyViewModel>()
                     .Activator.Should().NotBeNull();

        [Fact]
        public void SetsUrlPathSegment()
            => kernel.Get<IProxyViewModel>()
                     .UrlPathSegment.Should().Be(ProxyViewModel.UrlPathSegmentValue);

        [Fact]
        public void SetsHostScreen()
            => kernel.Get<IProxyViewModel>()
                     .HostScreen.Should().Be(window);

        [Fact]
        public async Task StartCommand_StartsProxy()
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            viewModel.Activator.Activate();

            // Act

            await viewModel.StartCommand.Execute();

            // Assert

            proxy.Received().Start();
        }

        [Fact]
        public async Task StartCommand_SetsIsStarted()
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            viewModel.Activator.Activate();

            // Act

            await viewModel.StartCommand.Execute();

            // Assert

            viewModel.IsStarted.Should().BeTrue();
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task StartCommand_CanExecute(bool isStarted, bool expected)
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            // Act

            viewModel.Activator.Activate();

            if (isStarted)
            {
                await viewModel.StartCommand.Execute();
            }

            var actual = await viewModel.StartCommand.CanExecute.FirstAsync();

            // Assert

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task StopCommand_StopsProxy()
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            // Act

            viewModel.Activator.Activate();

            await viewModel.StopCommand.Execute();

            // Assert

            proxy.Received().Stop();
        }

        [Fact]
        public async Task StopCommand_SetsIsStarted()
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            // Act

            viewModel.Activator.Activate();

            await viewModel.StopCommand.Execute();

            // Assert

            viewModel.IsStarted.Should().BeFalse();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task StopCommand_CanExecute(bool isStarted, bool expected)
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            // Act

            viewModel.Activator.Activate();

            if (isStarted)
            {
                await viewModel.StartCommand.Execute();
            }

            var actual = await viewModel.StopCommand.CanExecute.FirstAsync();

            // Assert

            actual.Should().Be(expected);
        }

        [Fact]
        public async Task IsStarted_RaisesPropertyChanged()
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            viewModel.MonitorEvents();

            // Act

            viewModel.Activator.Activate();

            await viewModel.StartCommand.Execute();

            // Assert

            viewModel
                .ShouldRaise(nameof(INotifyPropertyChanged.PropertyChanged))
                .WithSender(viewModel)
                .WithArgs<PropertyChangedEventArgs>(
                    a => a.PropertyName == nameof(IProxyViewModel.IsStarted));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WindowClosing_StopsProxy(bool isStarted)
        {
            // Arrange

            var viewModel = kernel.Get<IProxyViewModel>();

            // Act

            viewModel.Activator.Activate();

            if (isStarted)
            {
                await viewModel.StartCommand.Execute();
            }

            await interactions.WindowClosing.Handle(Unit.Default);

            // Assert

            if (isStarted)
            {
                proxy.Received().Stop();
            }
            else
            {
                proxy.DidNotReceive().Stop();
            }
        }
    }
}