using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Android.Support.V4.App;
using HouseCheckerApp;
using Geocoding.Google;
using Plugin.Geolocator;
using HouseCheckerApp.Fragments;

namespace HouseChecker.Fragments
{
    public class MyReviews : Fragment
    {
        //Set Layouts
        LinearLayout mainLayout;
        //initialise the reviewlist from the url
        List<Review> reviewList;

        //Set app preferences
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        //Set ID
        int id;

        public void OnClick(View v)
        {

        }
        //Set fragments
        public static MyReviews NewInstance()
        {
            var frag1 = new MyReviews { Arguments = new Bundle() };
            return frag1;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //Set buttons and main layout
            View v = inflater.Inflate(Resource.Layout.myReviews, container, false);
            mainLayout = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            Button showReviews = v.FindViewById<Button>(Resource.Id.showReviews);
            //Get ID from previous page
            id = int.Parse(ap.getAccessKey());

            showReviews.Click += async delegate
            {
                //Grab url json data for the review tables
                string url = "http://housechecker.co.uk/api/export_review.php";
                //Get the json data 
                JsonValue json = await FetchUserAsync(url);
                //convert to string
                string jsonString = json.ToString();
                //Set the review list
                reviewList = JsonConvert.DeserializeObject<List<Review>>(jsonString);

                //For every review with user id that equals the current user id
                foreach (var reviews in reviewList.Where(a => a.user_id.Equals(id)))
                {
                    //Get url of the property table which shows every property
                    url = "http://housechecker.co.uk/api/export_property.php";
                    json = await FetchUserAsync(url);
                    Address accom = JsonConvert.DeserializeObject<List<Address>>(json.ToString())
                            .Where(a => a.id == reviews.property_id).FirstOrDefault();

                    //start dynamically creating layout of the reviews
                    LinearLayout reviewDisplay = new LinearLayout(this.Context);
                    reviewDisplay.Orientation = Orientation.Vertical;                         
                    
                    LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                    lp.SetMargins(100,20,0,0);

                    //Set accomodation title and display the review
                    TextView accomTitle = new TextView(this.Context);
                    TextView displayReview = new TextView(this.Context);

                    reviewDisplay.LayoutParameters = lp;

                    accomTitle.TextSize = 20;
                    displayReview.TextSize = 16;

                    //Display the reviews which belong to the user
                    displayReview.Text = "Review title: " + reviews.title + "\nReview Comment: " 
                        + reviews.comment + "\nReview Rating: " + reviews.rating;
                    accomTitle.Text = accom.address1;

                    reviewDisplay.AddView(accomTitle);
                    reviewDisplay.AddView(displayReview);


                    mainLayout.AddView(reviewDisplay);
                }
            };
            showReviews.CallOnClick();
            return v;

        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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

    //set the review fields and get set method them from the table
    public class Review
    {
        public int Id { get; set; }
        public int user_id { get; set; }
        public int property_id { get; set; }
        public string title { get; set; }
        public string comment { get; set; }
        public double rating { get; set; }
        public DateTime date_of_review { get; set; }
    }
}
