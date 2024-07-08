using System.Windows;

namespace ChitChat_Client_WPF {
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window {
        public LoginWindow() {
            InitializeComponent();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        public string Url {
            get => TxtUrl.Text;
        }

        public string Username {
            get => TxtUsername.Text;
        }
    }
}