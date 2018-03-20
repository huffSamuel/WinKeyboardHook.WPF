using AobD.WinKeyboardHook.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AobD.WinKeyboardHook.Demo
{
    class MainViewModel : INotifyPropertyChanged
    {
        private IKeyboardInterceptor _interceptor;

        private string _text;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Text
        {
            get => _text;
            set
            {
                if(_text != value)
                {
                    _text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
                }
            }
        }

        public MainViewModel()
        {
            _interceptor = new KeyboardInterceptor();
            _interceptor.DisableRepeat = true;
            _interceptor.SuppressWindowsHandling = true;
            _interceptor.KeyDown += _interceptor_KeyDown;
            _interceptor.KeyUp += _interceptor_KeyUp;
        }

        private void _interceptor_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                var key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;

                Text += key.ToString() + " UP ";
            }
        }

        private void _interceptor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(!e.IsRepeat)
            {
                var key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;

                Text += key.ToString() + " DN ";
            }
        }

        public void BlockWindowsHandling()
        {
            _interceptor.StartCapturing();
        }

        public void UnblockWindowsHandling()
        {
            _interceptor.StopCapturing();
        }
    }
}
