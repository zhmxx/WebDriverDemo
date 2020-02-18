using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WebDriverDemo
{
    public class WebDriverViewModel : INotifyPropertyChanged
    {
        private ChromeDriver chromeDriver;
        private string address = @"localhost:8088";
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
                OnPropertChanged("Address");
            }
        }

        private string script = @"navigator.userAgent";
        public string Script
        {
            get
            {
                return script;
            }
            set
            {
                script = value;
                OnPropertChanged("Script");
            }
        }

        private bool isAsync = false;
        public bool IsAsync 
        { 
            get
            {
                return isAsync;
            }
            set
            {
                isAsync = value;
                OnPropertChanged("IsAsync");
            }
        }

        public RelayCommand AttachCommand { get; set; }
        public RelayCommand EvalCommand { get; set; }

        public WebDriverViewModel()
        {
            AttachCommand = new RelayCommand(x => Attach());
            EvalCommand = new RelayCommand(x => Eval());
        }

        public void Attach()
        {
            try
            {
                string driverExe = "chromedriver.exe";
                string driverFullPath = Path.GetFullPath(driverExe);
                if (!string.IsNullOrEmpty(driverFullPath))
                    driverFullPath = driverFullPath.Substring(0, driverFullPath.Length - driverExe.Length);
                var chromeDriverService = ChromeDriverService.CreateDefaultService(driverFullPath);
                chromeDriverService.HideCommandPromptWindow = true;

                var chromeOptions = new ChromeOptions();
                chromeOptions.DebuggerAddress = Address;
                chromeOptions.AddWindowType("webview");
                chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions);
                if (chromeDriver != null)
                    MessageBox.Show("Attached");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }

        public void Eval()
        {
            try
            {
                if (IsAsync)
                {
                    string asyncScript = String.Format(@"var ret = {0}; var callback = arguments[arguments.length-1]; callback(ret);", Script);
                    object ret = chromeDriver.ExecuteAsyncScript(asyncScript);
                    MessageBox.Show(ret as string);
                }
                else
                {
                    string syncScript = String.Format(@"return {0}", Script);
                    object ret = chromeDriver.ExecuteScript(syncScript);
                    MessageBox.Show(ret as string);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify of Property Changed event
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }


    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
