using System.ComponentModel;

namespace GrpsOverAllExamplesClients.Services
{
    internal class LocalLogger : INotifyPropertyChanged
    {
        private string _logContent = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static LocalLogger Logger { get;  set; } = new LocalLogger();

        public void Log(string message)
        {
            _logContent += $"{DateTime.Now.ToString("HH-mm-ss")}: {message}{Environment.NewLine}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogContent)));
        }

        public string LogContent
        {
            get => _logContent;
        }
    }
}
