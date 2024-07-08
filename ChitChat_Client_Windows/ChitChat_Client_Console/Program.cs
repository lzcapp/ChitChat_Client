using Microsoft.AspNetCore.SignalR.Client;

namespace ChitChat_Client_Console {
    internal class Program {
        HubConnection connection;

        static void Main(string[] args) {
            connection = new HubConnectionBuilder().WithUrl("http://localhost:53353/ChatHub").Build();
        }
    }
}