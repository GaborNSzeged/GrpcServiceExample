using GrpsOverAllExamplesClients.Enums;
using System.ComponentModel;

namespace GrpsOverAllExamplesClients.Services
{
    internal interface ISuperShopClientService : INotifyPropertyChanged, IDisposable
    {
        string Address { get; }
        bool ServiceStarted { get; set; }
        Task SetMockValue(ServiceHealthTypes healthType, int value);
        Task<string> GetInstantHealtCheck(ServiceHealthTypes serviceHealthType);
        void StartContinousHelathCheck(Action<string> logger);
        void StopContinousHelathCheck();
        public bool Start();
        public void Stop();
        Task<string> RegisterNewUser(string uerName, string userPsw);
        Task DisconnectAndReconnectWithAuthentication(string userName, string userPsw);
    }
}
