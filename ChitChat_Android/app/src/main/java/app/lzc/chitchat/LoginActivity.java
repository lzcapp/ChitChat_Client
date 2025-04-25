package app.lzc.chitchat;

import androidx.activity.EdgeToEdge;

import android.app.AlertDialog;
import android.content.Intent;
import android.content.res.Resources;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.AutoCompleteTextView;
import android.widget.EditText;

import androidx.appcompat.app.AppCompatActivity;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;

/**
 * @author rainy
 */
public class LoginActivity extends AppCompatActivity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_login);

        AutoCompleteTextView actvServerUrl = findViewById(R.id.actv_url);
        String[] suggestions = getResources().getStringArray(R.array.server_url_suggestions);
        ArrayAdapter<String> adapter = new ArrayAdapter<>(
                this,
                android.R.layout.simple_dropdown_item_1line,
                suggestions
        );
        actvServerUrl.setAdapter(adapter);
        actvServerUrl.setOnClickListener(v -> {
            actvServerUrl.showDropDown(); // 手动显示下拉框
        });
    }

    public void login(android.view.View view) {
        try {
            EditText etUserName = findViewById(R.id.username);
            String username = etUserName.getText().toString().trim();
            HubConnection hubConnection = HubConnectionBuilder.create("?username=" + username).build();
            hubConnection.start();
            if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED) {
                Intent intent = new Intent();

            } else {
                Resources res = this.getResources();;
                throw new Exception(res.getString(R.string.connect_failed));
            }
        } catch (Exception e) {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setIcon(android.R.drawable.ic_dialog_alert);
            builder.setTitle(R.string.error);
            builder.setMessage(e.getMessage());
            builder.setPositiveButton(R.string.ok, null);
            builder.setCancelable(false);
            builder.create().show();
        }
    }
}
