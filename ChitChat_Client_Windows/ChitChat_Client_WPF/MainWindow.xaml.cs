using System.Collections;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ChitChat_Client_WPF {
    public class KeyValuePair {
        public string Key { get; set; }

        public string Value { get; set; }
    }

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

            _connection.On<string, string>("UserJoined", (connectionId, user) => {
                Dispatcher.Invoke(() => {
                    DateTime currentDate = DateTime.Now;
                    var formattedDate = currentDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var newMessage = $"[{formattedDate}] [{user}] joined the server.";
                    lstMessage.Items.Add(newMessage);// Add items to the ComboBox
                    listbox.Items.Add(new ListBoxItem { Content = user, Tag = connectionId });
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


            _connection.On<ArrayList>("UserList", (clients) => {
                Dispatcher.Invoke(() => {
                    listbox.Items.Clear();
                    listbox.Items.Add(new ListBoxItem { Content = "All", Tag = "all" });
                    for (int index = 0; index < clients.Count; index++) {
                        var str = clients[index].ToString();
                        KeyValuePair client = JsonConvert.DeserializeObject<KeyValuePair>(str);
                        var user = client.Value;
                        var connectionId = client.Key;
                        listbox.Items.Add(new ListBoxItem { Content = user, Tag = connectionId });
                        break;
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
                listbox.SelectedIndex = 0;
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
            Process.Start(Environment.ProcessPath);
            Application.Current.Shutdown();
        }
    }
}