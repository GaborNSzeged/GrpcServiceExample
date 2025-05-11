using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GrpsOverAllExamplesClients.Ui
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void SetValue(ref string field, string value, [CallerMemberName] string propertyName = "")
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        //public bool SetValue(ref object field, object value, [CallerMemberName] string propertyName = "")
        //{
        //    if (field == value)
        //    {
        //        return false;
        //    }

        //    field = value;
        //    OnPropertyChanged(propertyName);

        //    return true;
        //}

        protected void OnPropertyChanged([CallerMemberName] string  propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
