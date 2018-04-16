using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using HouseCheckerApp;
using HouseCheckerApp.Fragments;
using HouseChecker.Fragments;

namespace HouseCheckerApp
{
    [Activity(Label = "Login", MainLauncher = true)]

    public class Login : Activity
    {


        TextView jsonViewer;
        AppPreferences ap =new AppPreferences(Application.Context);

        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);
            EditText username = FindViewById<EditText>(Resource.Id.etLEmailAddress);
            EditText pass = FindViewById<EditText>(Resource.Id.etLPassword);
            Button login = FindViewById<Button>(Resource.Id.bLogin);
            Button register = FindViewById<Button>(Resource.Id.bRegister);
            jsonViewer = FindViewById<TextView>(Resource.Id.textView1);


            login.Click += async delegate
            {
                string url = "http://housechecker.co.uk/api/export.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                var items = JsonConvert.DeserializeObject<List<Student>>(jsonString);
                
                Boolean foundUser = false;
                foreach (Student landlord in items)
                // Goes through each user that is retrieved
                {
                    if (landlord.Name.Equals(username.Text) && landlord.Pass.Equals(pass.Text))
                    {
                        // If the info in the fields matches any of the users then stores the id 
                        string key = landlord.Id.ToString();
                        ap.saveAccessKey(key);
                        foundUser = true;
                        Intent i = new Intent(this, typeof(MainActivity));
                        i.PutExtra("Type", landlord.Type);
                        StartActivity(i);
                        Finish();
                    }
                }

                if (!foundUser)
                {
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                        AlertDialog alert = dialog.Create();
                        dialog.SetCancelable(false);
                        alert.SetTitle("Error");
                        alert.SetMessage("Incorrect login details");
                        alert.SetButton("OK", (c, ev) =>
                        {
                            // Ok button click task  
                        });
                        alert.Show();
                    });
                }
            };

            register.Click += delegate
            {
                Intent i = new Intent(this, typeof(Register));
                StartActivity(i);
                Finish();
            };
        }

        private async Task<JsonValue> FetchUserAsync(string url)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.ContentType = "json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    return jsonDoc;
                }
            }
        }



    }
}