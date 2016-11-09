using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System;
using System.Linq;
using System.Json;
using System.Text.RegularExpressions;
using Android.Preferences;
using Android.Content;

namespace SSHConnectAndroid
{
    [Activity(Label = "SSH Connect Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //private string baseURL = "http://192.168.0.10";
        Button btnConnect;
        Spinner spinnerKillProcess;
        Button btnKillProcess;
        Button btnRestart;
        Button btnShutdown;
        LinearLayout layoutSave;
        EditText txtUsername;
        EditText txtPassword;
        EditText txtAPIAddress;
        Button btnSave;
        ISharedPreferences preferences;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            btnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            btnConnect.Click += async (sender, e) =>
            {
                string url = txtAPIAddress.Text + "/sshconnect/connect";
                string result = await InitiateWebRequest(url);
                SetItemsVisibility(result);
            };
            
            spinnerKillProcess = FindViewById<Spinner>(Resource.Id.spinnerKillProcess);

            btnKillProcess = FindViewById<Button>(Resource.Id.btnKillProcess);
            btnKillProcess.Click += async (sender, e) =>
            {
                var selectedItem = spinnerKillProcess.SelectedItem.ToString();
                string url = txtAPIAddress.Text + "/sshconnect/killprocess/" + selectedItem;
                string result = await InitiateWebRequest(url);
            };

            btnRestart = FindViewById<Button>(Resource.Id.btnRestart);
            btnRestart.Click += async (sender, e) =>
            {
                string url = txtAPIAddress.Text + "/sshconnect/restart";
                string result = await InitiateWebRequest(url);
                SetItemsVisibility(result);
            };

            btnShutdown = FindViewById<Button>(Resource.Id.btnShutdown);
            btnShutdown.Click += async (sender, e) =>
            {
                string url = txtAPIAddress.Text + "/sshconnect/shutdown";
                string result = await InitiateWebRequest(url);
                SetItemsVisibility(result);
            };

            preferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            layoutSave = FindViewById<LinearLayout>(Resource.Id.layoutSave);
            txtUsername = FindViewById<EditText>(Resource.Id.txtUsername);
            txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            txtAPIAddress = FindViewById<EditText>(Resource.Id.txtAPIAddress);
            btnSave = FindViewById<Button>(Resource.Id.btnSave);
            btnSave.Click += delegate
            {
                // Save to preferences
                ISharedPreferencesEditor editor = preferences.Edit();
                editor.PutString("txtUsername", txtUsername.Text);
                editor.PutString("txtPassword", txtPassword.Text);
                editor.PutString("txtAPIAddress", txtAPIAddress.Text);
                editor.Commit();
            };

            txtUsername.Text = preferences.GetString("txtUsername", "");
            txtPassword.Text = preferences.GetString("txtPassword", "");
            txtAPIAddress.Text = preferences.GetString("txtAPIAddress", "");

            SetDefaultItemsVisibility();

            var listResult = await InitiateWebRequest(txtAPIAddress.Text + "/sshconnect/killprocesslist");
            string[] killProcessList = listResult.Split(',');
            for(int i = 0; i < killProcessList.Count(); i++)
            {
                killProcessList[i] = killProcessList[i].Trim('[', ']', '\\', '"');
            }
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, killProcessList);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerKillProcess.Adapter = adapter;
            
            string isConnectedResult = await InitiateWebRequest(txtAPIAddress.Text + "/sshconnect/isconnected");
            SetItemsVisibility(isConnectedResult);
        }

        private async Task<string> InitiateWebRequest(string url)
        {
            try
            {
                // Create an HTTP web request using the URL:
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.ContentType = "application/json";
                request.Method = "GET";

                // Add authentication
                string username = txtUsername.Text;
                string password = txtPassword.Text;
                string encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                request.Headers.Add("Authorization", "Basic " + encoded);

                // Send the request to the server and wait for the response:
                using (WebResponse response = await request.GetResponseAsync())
                {
                    // Get a stream representation of the HTTP web response:
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = reader.ReadToEnd();
                            return result;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return "false";
            }
        }

        private void SetDefaultItemsVisibility()
        {
            btnConnect.Visibility = Android.Views.ViewStates.Gone;
            spinnerKillProcess.Visibility = Android.Views.ViewStates.Gone;
            btnKillProcess.Visibility = Android.Views.ViewStates.Gone;
            btnRestart.Visibility = Android.Views.ViewStates.Gone;
            btnShutdown.Visibility = Android.Views.ViewStates.Gone;
            layoutSave.Visibility = Android.Views.ViewStates.Gone;
        }

        private void SetItemsVisibility(string value)
        {
            bool isConnected;
            if (Boolean.TryParse(value, out isConnected))
            {
                if (isConnected)
                {
                    btnConnect.Visibility = Android.Views.ViewStates.Gone;
                    spinnerKillProcess.Visibility = Android.Views.ViewStates.Visible;
                    btnKillProcess.Visibility = Android.Views.ViewStates.Visible;
                    btnRestart.Visibility = Android.Views.ViewStates.Visible;
                    btnShutdown.Visibility = Android.Views.ViewStates.Visible;
                    layoutSave.Visibility = Android.Views.ViewStates.Gone;
                }
                else
                {
                    btnConnect.Visibility = Android.Views.ViewStates.Visible;
                    spinnerKillProcess.Visibility = Android.Views.ViewStates.Gone;
                    btnKillProcess.Visibility = Android.Views.ViewStates.Gone;
                    btnRestart.Visibility = Android.Views.ViewStates.Gone;
                    btnShutdown.Visibility = Android.Views.ViewStates.Gone;
                    layoutSave.Visibility = Android.Views.ViewStates.Visible;
                }
            }
        }

    }
}

