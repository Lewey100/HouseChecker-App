using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using HouseChecker.Fragments;
using HouseCheckerApp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.Net.Http;
using Android.Media;

namespace HouseCheckerApp.Fragments
{
   
    public class AddProperty : Fragment
    {
        //Set definition of layouts and tools
        Button submit, addImage;
        EditText address1, address2, city, postcode, price;
        Spinner bedrooms, bathrooms, accomType;
        RadioGroup wifi, bills;
        View v;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        //intialise ID
        int id;

        //Set fragments
        public static AddProperty NewInstance()
        {
            var frag1 = new AddProperty { Arguments = new Bundle() };
            return frag1;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //Set new names to fields
            v = inflater.Inflate(Resource.Layout.AppPropertyView, container, false);
            submit = v.FindViewById<Button>(Resource.Id.submitProperty);
            address1 = v.FindViewById<EditText>(Resource.Id.getAddress1);
            address2 = v.FindViewById<EditText>(Resource.Id.getAddress2);
            city = v.FindViewById<EditText>(Resource.Id.getCity);
            price = v.FindViewById<EditText>(Resource.Id.getPrice);
            postcode = v.FindViewById<EditText>(Resource.Id.getPostcode);
            bedrooms = v.FindViewById<Spinner>(Resource.Id.bedroomSpinner);
            addImage = v.FindViewById<Button>(Resource.Id.addImage);
            //Get user ID from the previous page
            id = int.Parse(ap.getAccessKey());

            //Set for drop down boxs
            bedrooms.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(bedrooms_ItemSelected);
            var bedroomAdapter = ArrayAdapter.CreateFromResource(
                    Android.App.Application.Context, Resource.Array.bedroom_array, Android.Resource.Layout.SimpleSpinnerItem);

            bedroomAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bedrooms.Adapter = bedroomAdapter;

            bathrooms = v.FindViewById<Spinner>(Resource.Id.bathroomSpinner);

            bathrooms.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(bathrooms_ItemSelected);
            var bathroomsAdapter = ArrayAdapter.CreateFromResource(
                    Android.App.Application.Context, Resource.Array.bathroom_array, Android.Resource.Layout.SimpleSpinnerItem);

            bathroomsAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            bathrooms.Adapter = bathroomsAdapter;

            //set drop downs for accomodation type
            accomType = v.FindViewById<Spinner>(Resource.Id.typeSpinner);

            accomType.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(type_ItemSelected);
            var accomTypeAdapter = ArrayAdapter.CreateFromResource(
                    Android.App.Application.Context, Resource.Array.type_array, Android.Resource.Layout.SimpleSpinnerItem);

            accomTypeAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            accomType.Adapter = accomTypeAdapter;

            //Set wifi and bills to new name
            wifi = v.FindViewById<RadioGroup>(Resource.Id.getWifi);
            bills = v.FindViewById<RadioGroup>(Resource.Id.getBills);
             
            submit.Click += async delegate
            {
                //URL which will use php to add a new property
                string url = "http://housechecker.co.uk/api/new_property.php?";
                //Set data to fetched url
                string data = await FetchUserAsync(url);

                //Exporting user data
                url = "http://housechecker.co.uk/api/export.php";
                JsonValue json = await GetData(url);
                string jsonString = json.ToString();
                //Get list of users
                List<Student> userList = JsonConvert.DeserializeObject<List<Student>>(jsonString);

                var companySelected = userList.Where(a => a.Id == id).FirstOrDefault();

                //Set message which says that it has been added
                string message = "Hello, " + companySelected.CompanyName 
                    + ", you have successfully added the accomodation: " + address1.Text;
                string subject = "New property added";
                string to = companySelected.Email;
                //Send email to URL
                url = "http://housechecker.co.uk/api/email.php";
                data = await SendEmail(url, to, message, subject);

                //New fragment for when emailed
                FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                LandlordDashboard landlordDashboard = new LandlordDashboard();
               
                fragmentTx.Replace(Resource.Id.content_frame, landlordDashboard);
                fragmentTx.Commit();
            };
            return v;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           

        }

        //Set spinners
        private void type_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(Android.App.Application.Context, toast, ToastLength.Long).Show();
        }

        private void bathrooms_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(Android.App.Application.Context, toast, ToastLength.Long).Show();
        }

        private void bedrooms_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("The planet is {0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(Android.App.Application.Context, toast, ToastLength.Long).Show();
        }

        //Sending an email to the user who added the property
        private async Task<JsonValue> SendEmail(string url, string to, string message, string subject)
        {
            string success = "";

            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["to"] = to;
                data["subject"] = subject;
                data["message"] = message;
                var response = webClient.UploadValues(url, "POST", data);

            }
            return success;
        }
        //Get fetchuserasync and set the fields to new strings
        private async Task<string> FetchUserAsync(string url)
        {
            string success = "";
            string submitID = id.ToString();
            string submitAddress1 = address1.Text;
            string submitAddress2 = address2.Text;
            string submitCity = city.Text;
            string submitPostcode = postcode.Text;
            string submitPrice = price.Text;
            string submitBedrooms = (bedrooms.SelectedItem.ToString());
            string submitBathrooms = (bathrooms.SelectedItem.ToString());
            string submitType = accomType.SelectedItem.ToString();
            string submitBills = (v.FindViewById<RadioButton>(bills.CheckedRadioButtonId)).Text;
            string submitWifi = (v.FindViewById<RadioButton>(wifi.CheckedRadioButtonId)).Text;

            using (var webClient = new WebClient())
            {
                //FOr posting
                var data = new NameValueCollection();
                data["user_id"] = submitID;
                data["type_of_accomodation"] = submitType;
                data["address1"] = submitAddress1;
                data["address2"] = submitAddress2;
                data["city"] = submitCity;
                data["postcode"] = submitPostcode;
                data["bedrooms"] = submitBedrooms;
                data["bathrooms"] = submitBathrooms;
                data["price"] = submitPrice;
                data["wifi"] = submitWifi;
                data["bills"] = submitBills;
                var response = webClient.UploadValues(url, "POST", data);

            }
            return success;
        }

        private async Task<JsonValue> GetData(string url)
        {
            //Set the asyncs for json and get the data
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.ContentType = "json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (System.IO.Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    return jsonDoc;
                }
            }
        }

        public void OnClick(View v)
        {
            throw new System.NotImplementedException();
        }
    }
}