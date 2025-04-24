package app.lzc.chitchat;

import androidx.activity.EdgeToEdge;

import android.content.Intent;
import android.os.Bundle;
import android.widget.EditText;

import androidx.appcompat.app.AppCompatActivity;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;

public class LoginActivity extends AppCompatActivity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_login);
    }

    public void login(android.view.View view) {
        EditText etUserName = findViewById(R.id.username);
        String userName = etUserName.getText().toString().trim();
        HubConnection hubConnection = HubConnectionBuilder.create("https://chitchat.seeleo.com/chitchat?username=" + userName).build();
        hubConnection.start();
        if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
            Intent intent = new Intent();

        }
    }
}