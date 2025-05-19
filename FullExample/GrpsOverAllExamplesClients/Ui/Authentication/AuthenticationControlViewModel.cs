using Grpc.Core;
using Grpc.Net.Client;
using GrpsOverAllExamplesClients.Configurations;
using GrpsOverAllExamplesClients.Services;
using GrpsOverAllExamplesClients.Ui.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

namespace GrpsOverAllExamplesClients.Ui.Authentication
{
    internal class AuthenticationControlViewModel : BaseViewModel
    {
        private LocalLogger _localLoger;
        private ISuperShopClientService? _superShopClientService;

        public AuthenticationControlViewModel()
        {
            _localLoger = LocalLogger.Logger;
            RegisterCommad = new RelayCommand(ExecuteRegisterCommad, null);
            LoginCommad = new RelayCommand(ExecuteLoginCommad, null);

        }

        public string NewUserName { get; set; } = string.Empty;
        public string NewUserPsw { get; set; } = string.Empty;
        public string OldUserName { get; set; } = string.Empty;
        public string OldUserPsw { get; set; } = string.Empty;
        public ICommand RegisterCommad { get; set; }
        public ICommand LoginCommad { get; set; }

        private async void ExecuteRegisterCommad(object? obj)
        {
            var service = GetSuperShopService();
            if (service == null)
            {
                _localLoger.Log($"Cannot execute Login command. Service was null");
                return;
            }
            try
            {
                string response = await service.RegisterNewUser(NewUserName, NewUserPsw);
                if (string.IsNullOrEmpty(response))
                {
                    _localLoger.Log($"New user added. Name {NewUserName}");
                }
                else
                {
                    _localLoger.Log(response);
                }
            }
            catch (Exception ex)
            {
                _localLoger.Log(ex.Message);
            }
        }

        private async void ExecuteLoginCommad(object? obj)
        {
            var service = GetSuperShopService();
            if (service == null)
            {
                _localLoger.Log($"Cannot execute Login command. Service was null");
                return;
            }

            await service.DisconnectAndReconnectWithAuthentication(OldUserName, OldUserPsw);
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
