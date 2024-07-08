using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChitChat_Client_WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private HubConnection _connection;

        private string url;
        private string username;

        public MainWindow() {
            url = "";
            username = "";
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            var loginWindow = new LoginWindow {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            try {
                do {
                    if (loginWindow.ShowDialog() == true) {
                        url = loginWindow.Url;
                        username = loginWindow.Username;
                    } else {
                        Environment.Exit(-1);
                    }
                } while (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(username));
            } catch (Exception) {
                return;
            }

            if (url.EndsWith("/") && !url.EndsWith("/chitchat")) {
                url += "chitchat";
            }

            _connection = new HubConnectionBuilder().WithUrl($"{url}?username={username}").Build();

            _connection.Closed += async (_) => {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };

            _connection.On<string, string>("UserJoined", (connectionID, user) => {
                Dispatcher.Invoke(() => {
                    DateTime currentDate = DateTime.Now;
                    var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var newMessage = $"[{formattedDate}] [{user}] joined the server.";
                    lstMessage.Items.Add(newMessage);// Add items to the ComboBox
                    listbox.Items.Add(new ComboBoxItem { Content = user, Tag = connectionID });
                });
            });

            _connection.On<string, string>("UserLeft", (connectionID, user) => {
                Dispatcher.Invoke(() => {
                    DateTime currentDate = DateTime.Now;
                    var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var newMessage = $"[{formattedDate}] [{user}] left the server.";
                    lstMessage.Items.Add(newMessage);
                    foreach (ListBoxItem userItem in listbox.Items) {
                        if (userItem.Tag.ToString() == connectionID) {
                            listbox.Items.Remove(userItem);
                        }
                    }
                });
            });

            _connection.On<string, string>("ReceiveMessage", (user, message) => {
                Dispatcher.Invoke(() => {
                    DateTime currentDate = DateTime.Now;
                    var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var newMessage = user.Equals(username) ? $"[{formattedDate}] you: {message}" : $"[{formattedDate}] {user}: {message}";
                    lstMessage.Items.Add(newMessage);
                });
            });

            try {
                await _connection.StartAsync();
                MessageBox.Show("Connection started.", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                btnSend.IsEnabled = true;
                btnReconnect.IsEnabled = true;
                listbox.Items.Add(new ComboBoxItem { Content = "All", Tag = "all" });
                listbox.SelectedValue = "All";
            } catch (Exception ex) {
                lstMessage.Items.Add(ex.Message);
            }
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e) {
            try {
                if (listbox.SelectedItem != null) {
                    var selectedUser = ((ListBoxItem)listbox.SelectedItem).Tag.ToString();
                    if (selectedUser == "all") {
                        await _connection.InvokeAsync("SendMessage", username, txtMessage.Text);
                    } else {
                        await _connection.InvokeAsync("SendPrivateMessage", selectedUser, username, txtMessage.Text);
                    }
                } else {
                    await _connection.InvokeAsync("SendMessage", username, txtMessage.Text);
                }
                txtMessage.Text = "";
            } catch (Exception ex) {
                lstMessage.Items.Add(ex.Message);
            }
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                BtnSend_Click(sender, e);
            }
        }

        private void BtnReconnect_Click(object sender, RoutedEventArgs e) {

        }
    }
}