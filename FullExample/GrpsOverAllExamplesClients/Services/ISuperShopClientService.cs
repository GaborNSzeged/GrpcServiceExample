using GrpsOverAllExamplesClients.Enums;
using System.ComponentModel;

namespace GrpsOverAllExamplesClients.Services
{
    internal interface ISuperShopClientService : INotifyPropertyChanged, IDisposable
    {
        bool ServiceStarted { get; set; }
        Task SetMockValue(ServiceHealthTypes healthType, int value);
        Task<string> GetInstantHealtCheck(ServiceHealthTypes serviceHealthType);
        void StartContinousHelathCheck(Action<string> logger);
        void StopContinousHelathCheck();
        public bool Start();
        public void Stop();
    }
}
