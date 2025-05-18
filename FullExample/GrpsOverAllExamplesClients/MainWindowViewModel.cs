using GrpsOverAllExamplesClients.Configurations;
using GrpsOverAllExamplesClients.Services;
using GrpsOverAllExamplesClients.Ui;
using GrpsOverAllExamplesClients.Ui.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows.Input;

namespace GrpsOverAllExamplesClients
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private ISuperShopClientService? _superShopClientService;
        private LocalLogger _localLogger;

        public MainWindowViewModel()
        {
            ConnectCommand = new RelayCommand(ExecuteConnectCommnad, null);
            GetSuperShopService().PropertyChanged += HandleSuperShopClientPropertyChanged;
            _localLogger = LocalLogger.Logger;
            _localLogger.PropertyChanged += HandleLogerPropertyChanged;
        }

        public string LogContent => _localLogger.LogContent;

        public bool IsConnected
        {
            get => GetSuperShopService().ServiceStarted;
        }

        public ICommand ConnectCommand { get; set; }

        private void ExecuteConnectCommnad(object? obj)
        {
            var client = GetSuperShopService();
            if (client.ServiceStarted)
            {
                client.Stop();
            }
            else
            {
                client.Start();
            }
        }

        private void HandleSuperShopClientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ISuperShopClientService.ServiceStarted))
            {
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        private void HandleLogerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalLogger.LogContent))
            {
                OnPropertyChanged(nameof(LogContent));
            }
        }

        private ISuperShopClientService GetSuperShopService()
        {
            if (_superShopClientService == null)
            {
                _superShopClientService = Config.ServiceProvider.GetRequiredService<ISuperShopClientService>();
            }

            return _superShopClientService;
        }
    }
}
