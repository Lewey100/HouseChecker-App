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
using HouseChecker.Fragments;
using Plugin.TextToSpeech;
using Android.Graphics;

namespace HouseCheckerApp.Fragments
{
    public class LandlordDashboard : Fragment
    {
        //Setting the layout
        LinearLayout mainLayout;
        //Lists for properties
        List<Address> propertyList;
        //Variables
        int id;
        int propert_landlord_id;

        TextView jsonViewer;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        //Creating an instance of the dashboard's fragment
        public static LandlordDashboard NewInstance()
        {
            var frag1 = new LandlordDashboard { Arguments = new Bundle() };
            return frag1;
        }
        //Creates the main attributes of the page when it is loaded
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.landlordDashboard, container, false);
            mainLayout = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            Button navHome = v.FindViewById<Button>(Resource.Id.bProperties);
            Button displayUser = v.FindViewById<Button>(Resource.Id.displayUser);
            Button displayProperties = v.FindViewById<Button>(Resource.Id.showProperty);
            
            id = int.Parse(ap.getAccessKey()); //gets the current user's ID
            //This is an invisible button that is used so the list of properties is displayed when it loads
            displayProperties.Click += async delegate 
            {
                //Getting the php page with the data on for the properties
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url); //storing the data in a Json object using the asynchronous function written below
                string jsonString = json.ToString(); //converting the json to a string
                //Adding the properties to the list
                propertyList = JsonConvert.DeserializeObject<List<Address>>(jsonString); //Storing all the the individual property objects in the property list
                //Laying out the data
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                lp.SetMargins(130, 20, 0, 0);
                //Creating a title and setting the layout for it
                TextView propertyTitle = new TextView(this.Context);
                propertyTitle.Text = "Your Properties:";
                propertyTitle.TextSize = 20;
                propertyTitle.LayoutParameters = lp;
                mainLayout.AddView(propertyTitle);
                //Getting the php page with the data on for the reviews
                string ratingUrl = "http://housechecker.co.uk/api/export_review.php";
                json = await FetchUserAsync(ratingUrl);
                jsonString = json.ToString();
                var reviewList = JsonConvert.DeserializeObject<List<Review>>(jsonString); //storing all reviews in a list of reviews
                //Creating a new list with the properties in that the current landlord owns
                List<Address> propertiesOwned = propertyList.Where(a => a.user_id == id).ToList(); //Creating a seperate list of properties of the properties that the current user owns
                
                foreach (var address in propertiesOwned) //loops through each property in the User's property list
                {
                    double average_score;
                    List<Review> review = reviewList.Where(a => a.property_id == address.id).ToList(); //Gets all the reviews for each property in propertiesOwned
                    if(review.Count > 0) 
                    {
                         average_score = review.Average(a => a.rating); //If there is a review for the property, find the average of all the ratings in reviews
                    }
                    else
                    {
                        average_score = 0; //If there are no reviews, set the average to 0
                    }
                    
                    if (review != null)
                    {
                        address.rating = average_score; //If review list isn't empty, set the property's rating to the average, worked out above
                    }
                }

                List<Review> propertyReviews = new List<Review>();

                if (propertiesOwned.Count > 0) //If the landlord owns a property
                {

                    foreach (var property in propertiesOwned) //Loop through all the properties they own
                    {
                        propertyReviews.AddRange(reviewList.Where(a => a.property_id == property.id).ToList()); //Add all the revews for the property to a list 
                        property.reviewCount = reviewList.Where(a => a.property_id == property.id).Count(); //Sets how many reviews there are for a property
                    }

                    Address topRatedAccom = propertiesOwned.Where(o => o.user_id == id).OrderByDescending(o => o.rating).Take(1).FirstOrDefault(); //Gets the best accomodation by taking the top property from a list ordered by rating
                    Address lowestRatedAccom = propertiesOwned.Where(o => o.user_id == id).OrderBy(o => o.rating).Take(1).FirstOrDefault(); //Gets the worst property by taking the bottom property from a list ordered by rating
                    Address mostPopularAccom = propertiesOwned.OrderByDescending(a => a.reviewCount).Take(1).FirstOrDefault(); //Takes the most popular accomodation based upomn how many reviews it has received
                    Review mostRecentReview = propertyReviews.OrderByDescending(a => a.date_of_review).Take(1).FirstOrDefault(); //Gets the most recent review of a property


                    //Sets parameters for each property that has been defined above
                    TextView topRatedAccomDescp = new TextView(this.Context);
                    TextView lowestRatedAccomDesc = new TextView(this.Context);
                    TextView mostPopularAccomDesc = new TextView(this.Context);
                    TextView mostRecentReviewDesc = new TextView(this.Context);

                    TextView topRatedAccomTitle = new TextView(this.Context);
                    TextView lowestRatedAccomTitle = new TextView(this.Context);
                    TextView mostPopularAccomTitle = new TextView(this.Context);
                    TextView mostRecentReviewTitle = new TextView(this.Context);

                    topRatedAccomDescp.Text = "\nAccom Title: " + topRatedAccom.address1 + "\nPostcode: " + topRatedAccom.postcode;
                    lowestRatedAccomDesc.Text = "\nAccom Title: " + lowestRatedAccom.address1 + "\nPostcode: " + lowestRatedAccom.postcode;
                    mostPopularAccomDesc.Text = "\nAccom Title: " + mostPopularAccom.address1 + "\nPostcode: " + mostPopularAccom.postcode;
                    mostRecentReviewDesc.Text = "\nReview Title: " + mostRecentReview.title + "\nComment: " + mostRecentReview.comment;

                    topRatedAccomTitle.Text = "Top Rated Accomodation: ";
                    lowestRatedAccomTitle.Text = "Worst Rated Accomodation: ";
                    mostPopularAccomTitle.Text = "Most Popular Accomodation: ";
                    mostRecentReviewTitle.Text = "Most Recent Review: ";

                    topRatedAccomDescp.LayoutParameters = lp;
                    lowestRatedAccomDesc.LayoutParameters = lp;
                    mostPopularAccomDesc.LayoutParameters = lp;
                    mostRecentReviewDesc.LayoutParameters = lp;

                    topRatedAccomTitle.LayoutParameters = lp;
                    lowestRatedAccomTitle.LayoutParameters = lp;
                    mostPopularAccomTitle.LayoutParameters = lp;
                    mostRecentReviewTitle.LayoutParameters = lp;

                    topRatedAccomTitle.TextSize = 20;
                    lowestRatedAccomTitle.TextSize = 20;
                    mostPopularAccomTitle.TextSize = 20;
                    mostRecentReviewTitle.TextSize = 20;

                    //If the text has been clicked, Text to Speech will happen
                    topRatedAccomTitle.Click += async (senderSpeak, eSpeak) =>
                    {
                        string txt = "Your top rated accomodation is " + topRatedAccom.address1 + ", in, " + topRatedAccom.city;
                        await CrossTextToSpeech.Current.Speak(txt);
                    };

                    lowestRatedAccomTitle.Click += async (senderSpeak, eSpeak) =>
                    {
                        string txt = "Your lowest rated accomodation is " + lowestRatedAccom.address1 + ", in, " + lowestRatedAccom.city;
                        await CrossTextToSpeech.Current.Speak(txt);
                    };

                    mostPopularAccomTitle.Click += async (senderSpeak, eSpeak) =>
                    {
                        string txt = "Your most popular accomodation is " + mostPopularAccom.address1 + ", in, " + mostPopularAccom.city;
                        await CrossTextToSpeech.Current.Speak(txt);
                    };

                    mostRecentReviewTitle.Click += async (senderSpeak, eSpeak) =>
                    {
                        string txt = "Your most recent review was rated " + mostRecentReview.rating + ", in, " + mostRecentReview.comment;
                        await CrossTextToSpeech.Current.Speak(txt);
                    };
                    //Adds the properties to the page to display them on the screen
                    LinearLayout topRatedLayout = new LinearLayout(this.Context);
                    topRatedLayout.Orientation = Orientation.Vertical;
                    topRatedLayout.AddView(topRatedAccomTitle);
                    topRatedLayout.AddView(topRatedAccomDescp);
                    topRatedLayout.AddView(GetImage(topRatedAccom));

                    LinearLayout worstRatedLayout = new LinearLayout(this.Context);
                    worstRatedLayout.Orientation = Orientation.Vertical;
                    worstRatedLayout.AddView(lowestRatedAccomTitle);
                    worstRatedLayout.AddView(lowestRatedAccomDesc);
                    worstRatedLayout.AddView(GetImage(lowestRatedAccom));

                    LinearLayout mostPopularLayout = new LinearLayout(this.Context);
                    mostPopularLayout.Orientation = Orientation.Vertical;
                    mostPopularLayout.AddView(mostPopularAccomTitle);
                    mostPopularLayout.AddView(mostPopularAccomDesc);
                    mostPopularLayout.AddView(GetImage(mostPopularAccom));

                    LinearLayout reviewLayout = new LinearLayout(this.Context);
                    mainLayout.AddView(mostRecentReviewTitle);
                    mainLayout.AddView(mostRecentReviewDesc);

                    mainLayout.AddView(topRatedLayout);
                    mainLayout.AddView(worstRatedLayout);
                    mainLayout.AddView(mostPopularLayout);
                    mainLayout.AddView(reviewLayout);
                }
            };
           
            displayUser.Click += async delegate //Gets the current landlord's information when the page is loaded
            {
                string url = "http://housechecker.co.uk/api/landlord_export.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                List<Landlord> listOfLandlords = JsonConvert.DeserializeObject<List<Landlord>>(jsonString);
                var user = listOfLandlords.Where(a => a.Id == id).FirstOrDefault();
            };

            displayUser.CallOnClick(); //Automatically calls click methods on the invisible buttons to begin functionality
            displayProperties.CallOnClick();
            return v;
        }

        public ImageView GetImage(Address property)
        {
            ImageView propertyImage = new ImageView(this.Context);

            try
            {
                string imageURL = property.image.Remove(0, 2); //Removes the two ".." from the file path for easy image downloading
                imageURL = "http://housechecker.co.uk" + imageURL; //creates the correct file path to the image hosted online
                WebRequest request = WebRequest.Create(imageURL);
                WebResponse resp = request.GetResponse();
                Stream respStream = resp.GetResponseStream();
                Bitmap bmp = BitmapFactory.DecodeStream(respStream); //Create the bitmap of the data downloaded from the web
                Bitmap image = Bitmap.CreateScaledBitmap(bmp, 750, 750, false); //Gives the bitmap a size
                respStream.Dispose();
                propertyImage.SetImageBitmap(image); //Sets the image to be drawn on the app
                propertyImage.RefreshDrawableState();
            }
            catch (Exception imageError)
            {
                 propertyImage.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.propertyStock)); //If no image has been gotten, set the photo to a stock image
            }

            return propertyImage;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
        }

        private async Task<JsonValue> FetchUserAsync(string url) //Asynchronous function that takes the url of a webpage and returns the whole page as a json object
        {

            //Create a new web request to the url and define the process to return a json type
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url); 
            request.ContentType = "json";
            request.Method = "GET";

            //Wait until the app receives a response from the page
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    //Load the returned web page into a Json value and return it.
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    return jsonDoc;
                }
            }
        }


    }

    public class Landlord //Landlord Class, contains all neccessay Data
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public int Id { get; set; }
        public string Lastname { get; set; }
        public string Name { get; set; }
        public string Pass { get; set; }
        public string Uni { get; set; }
        public string Type { get; set; }
    }


}
