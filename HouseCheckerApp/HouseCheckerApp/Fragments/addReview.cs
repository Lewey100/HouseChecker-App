using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Threading.Tasks;
using Android.Support.V4.App;
using HouseCheckerApp;
using System.Json;
using Newtonsoft.Json;
using HouseChecker.Fragments;

namespace HouseCheckerApp.Fragments
{
    public class addReview : Fragment
    {
        //Variables
        EditText input_review_title, input_review_Comment;
        RatingBar clean_Rating, landlord_Rating, value_Rating, location_Rating;
        Button b_add_review1;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        int accomId, userId;
        View v;
        //Creating a new instance of the addReview
        public static addReview NewInstance()
        {
            var frag1 = new addReview { Arguments = new Bundle() };
            return frag1;
        }
        //Creates the main attributes of the page when it is loaded
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            v = inflater.Inflate(Resource.Layout.addReview, container, false);
            accomId = int.Parse(Arguments.GetString("accomID"));
            userId = int.Parse(ap.getAccessKey());
            Button navHome = v.FindViewById<Button>(Resource.Id.bProperties);
            // setting the editText variables to the fields from axml
            input_review_title = (EditText)v.FindViewById(Resource.Id.reviewTitle);
            input_review_Comment = (EditText)v.FindViewById(Resource.Id.reviewComment);
            clean_Rating = (RatingBar)v.FindViewById(Resource.Id.cleanRating);
            landlord_Rating = (RatingBar)v.FindViewById(Resource.Id.landlordRating);
            value_Rating = (RatingBar)v.FindViewById(Resource.Id.valueRating);
            location_Rating = (RatingBar)v.FindViewById(Resource.Id.locationRating);
            b_add_review1 = (Button)v.FindViewById(Resource.Id.b_add_review);
            //Adding a review when the button is clicked
            b_add_review1.Click += async delegate
            {
                //Creating new variables for each field
                float clean = clean_Rating.Rating;
                float value = value_Rating.Rating;
                float location = location_Rating.Rating;
                float landlord = landlord_Rating.Rating;
                float avg_score = (clean + value + location + landlord) / 4;
                //Getting the url to create a new review
                string url = "http://housechecker.co.uk/api/new_review.php?";
                string data = await FetchUserAsync(url);
                //Getting the property details from the php page
                url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await GetData(url);
                string jsonString = json.ToString();
                //Creating a new list for the properties
                List<Address> propertyList = JsonConvert.DeserializeObject<List<Address>>(jsonString);

                Address selectedProperty = propertyList.Where(a => a.id == accomId).FirstOrDefault();
                //Getting the list of users to to then send an email welcoming then to the app
                url = "http://housechecker.co.uk/api/export.php";
                json = await GetData(url);
                jsonString = json.ToString();
                List<Student> userList = JsonConvert.DeserializeObject<List<Student>>(jsonString);

                var companySelected = userList.Where(a => a.Id == selectedProperty.user_id).FirstOrDefault();
                //Sending the email
                string message = "Hello, " + companySelected.CompanyName + ", someone has added a new review for your property: " +
                    selectedProperty.address1 + "\n They scored you an average of: " + avg_score;
                string subject = "New review";
                string to = companySelected.Email;
                url = "http://housechecker.co.uk/api/email.php";
                data = await SendEmail(url, to, message, subject);

                var user = userList.Where(a => a.Id == userId).FirstOrDefault();
                //Sending an email to the landlord to tell them that they have a new review
                message = "Hello, " + user.Firstname + ", thank you for adding a review for the property " + selectedProperty.address1;
                subject = "New review";
                to = user.Email;
                url = "http://housechecker.co.uk/api/email.php";
                data = await SendEmail(url, to, message, subject);
            };
            return v;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }
        //Sending the information to the database
        private async Task<string> FetchUserAsync(string url)
        {
            string success = "";

            string title = input_review_title.Text;
            string comment = input_review_Comment.Text;
            float clean = clean_Rating.Rating;
            float value = value_Rating.Rating;
            float location = location_Rating.Rating;
            float landlord = landlord_Rating.Rating;
            string str6 = userId.ToString();
            string str5 = accomId.ToString();
            string str1 = clean.ToString();
            string str2 = value.ToString();
            string str3 = location.ToString();
            string str4 = landlord.ToString();
            using (var webClient = new WebClient())
            {

                var data = new NameValueCollection();
                data["property_id"] = str5;
                data["user_id"] = str6;
                data["title"] = title;
                data["comment"] = comment;
                data["clean"] = str1;
                data["value"] = str2;
                data["landlord"] = str3;
                data["location"] = str4;

                var response = webClient.UploadValues(url, "POST", data);

                FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                addReview addReview = new addReview();
                Bundle bundle = new Bundle();
                bundle.PutString("accomID", str5);
                addReview.Arguments = bundle;
                fragmentTx.Replace(Resource.Id.content_frame, addReview);
                fragmentTx.Commit();
            }
            return success;
        }
        //Getting the data
        private async Task<JsonValue> GetData(string url)
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
        //Send an email to a user
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
    }
}