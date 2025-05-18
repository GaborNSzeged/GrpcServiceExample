using GrpsOverAllExamplesClients.Configurations;
using GrpsOverAllExamplesClients.Enums;
using GrpsOverAllExamplesClients.Services;
using GrpsOverAllExamplesClients.Ui.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace GrpsOverAllExamplesClients.Ui.HealthCheck
{
    internal class HealthCheckControlViewModel : BaseViewModel
    {
        private bool _continuousHealthCheckIsRunning;
        private ISuperShopClientService? _superShopClientService;
        private LocalLogger _localLoger;

        public HealthCheckControlViewModel()
        {
            _localLoger = LocalLogger.Logger;
            CheckHealthCheckCommand = new RelayCommand(ExecuteCheckHealthCheckCommand, CanExecuteCheckHealthCheckCommand);
            ClearLogCommand = new RelayCommand(ExecuteClearLogCommand, null);
            StartContinuousHealtCheckCommand = new RelayCommand(ExecuteStartContinuousHealtCheckCommand, null);
            StopContinuousHealtCheckCommand = new RelayCommand(ExecuteStopContinuousHealtCheckCommand, null);
            SetServerMockValueCommand = new RelayCommand(ExecuteSetServerMockValueCommand, null);
        }

        public ICommand CheckHealthCheckCommand { get; set; }
        public ICommand ClearLogCommand { get; set; }
        public ICommand StartContinuousHealtCheckCommand { get; set; }
        public ICommand StopContinuousHealtCheckCommand { get; set; }
        public ICommand SetServerMockValueCommand { get; set; }

        public List<ServiceHealthTypes> ServiceHealthTypes { get; set; } = new List<ServiceHealthTypes>() {
            Enums.ServiceHealthTypes.Memory, Enums.ServiceHealthTypes.Cpu, Enums.ServiceHealthTypes.Disk};

        public List<int> ServerMockValues { get; set; } = new List<int>() { 10, 50, 90 };

        public int SelectedValueToServerMock { get; set; }


        public ServiceHealthTypes SelectedHealtCheckTypeToTest { get; set; }
        public ServiceHealthTypes SelectedHealtCheckTypeToServerMock { get; set; }

        private string _instantHealtCheckResult = "---";
        public string InstantHealtCheckResult
        {
            get => _instantHealtCheckResult;
            set => SetValue(ref _instantHealtCheckResult, value);
        }

        private string _healtCheckLog = string.Empty;
        public string HealtCheckLog
        {
            get => _healtCheckLog;
            set => SetValue(ref _healtCheckLog, value);
        }

        private bool CanExecuteCheckHealthCheckCommand(object? arg)
        {
            return true;
        }

        private void ExecuteCheckHealthCheckCommand(object? obj)
        {
            RefreshHealtCheck();
        }

        private void ExecuteClearLogCommand(object? obj)
        {
            HealtCheckLog = string.Empty;
        }

        private void ExecuteStartContinuousHealtCheckCommand(object? obj)
        {
            StartContinousHealtCheck();
        }

        private void ExecuteStopContinuousHealtCheckCommand(object? obj)
        {
            var superShopClient = GetSuperShopService();
            superShopClient.StopContinousHelathCheck();
            _continuousHealthCheckIsRunning = false;
        }

        private async void ExecuteSetServerMockValueCommand(object? obj)
        {
            var superShopClient = GetSuperShopService();
            try
            {
                await superShopClient.SetMockValue(SelectedHealtCheckTypeToServerMock, SelectedValueToServerMock);
                _localLoger.Log($"Mock value sent: {SelectedHealtCheckTypeToServerMock} {SelectedValueToServerMock}");
            }
            catch (Exception ex)
            {
                _localLoger.Log($"Could not send mock value to server: {ex.Message}");
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

        private async void RefreshHealtCheck()
        {
            var superShopClient = GetSuperShopService();

            try
            {
                string healtCheckResult = await superShopClient.GetInstantHealtCheck(SelectedHealtCheckTypeToTest);
                InstantHealtCheckResult = $"{DateTime.Now.ToString("mm:ss")}:  {healtCheckResult}";
            }
            catch (Exception ex)
            {
                _localLoger.Log($"Could not refresh Healt Check: {ex.Message}");
            }
        }

        private void StartContinousHealtCheck()
        {
            if (_continuousHealthCheckIsRunning)
            {
                return;
            }
            var superShopClient = GetSuperShopService();

            try
            {
                superShopClient.StartContinousHelathCheck(RefreshHealthCheckLog);
                _continuousHealthCheckIsRunning = true;
            }
            catch (Exception ex)
            {
                _localLoger.Log($"Could not start continous healt check. {ex.Message}");
            }
        }

        private void RefreshHealthCheckLog(string statusLog)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                HealtCheckLog += $"{DateTime.Now.ToString("mm:ss")}: {statusLog}{Environment.NewLine}";
            }));
        }
    }
}
