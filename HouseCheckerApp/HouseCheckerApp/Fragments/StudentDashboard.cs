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
using Android.Graphics;
using Plugin.TextToSpeech;

namespace HouseChecker.Fragments
{
    public class StudentDashboard : Fragment
    {
        //Setting the layout
        LinearLayout mainLayout;
        //Lists for address and reviews and coordinates
        List<Address> addressList;
        List<Review> reviewList;
        List<Coordinate> coordinates;
        //Variables
        double lat;
        double lon;
        int id;
        
        TextView jsonViewer;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        Plugin.Geolocator.Abstractions.Address userAddress;
        //Creating an instance of the dashboard
        public static StudentDashboard NewInstance()
        {
            var frag1 = new StudentDashboard { Arguments = new Bundle() };
            return frag1;
        }
        //Creates the main attributes of the page when it is loaded
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.studentDashboard, container, false);
            mainLayout = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            Button showReviews = v.FindViewById<Button>(Resource.Id.showReviews);
            Button showMap = v.FindViewById<Button>(Resource.Id.showMap);

            id = int.Parse(ap.getAccessKey());

            showMap.Click += async (sender, e) =>
            {
                await GetUserAddress();
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                addressList = JsonConvert.DeserializeObject<List<Address>>(jsonString).Where(o => o.city.Equals(userAddress.Locality)).ToList();
                await getCoordinates();

                string ratingUrl = "http://housechecker.co.uk/api/export_review.php";
                json = await FetchUserAsync(ratingUrl);

                jsonString = json.ToString();
                var reviewList = JsonConvert.DeserializeObject<List<Review>>(jsonString);

                // Getting information for properties and ratings and parsing them into a list

                foreach (var address in addressList)
                {
                    List<Review> review = reviewList.Where(a => a.property_id == address.id).ToList();
                    if (review.Count > 0)
                    {
                        address.rating = review.Average(a => a.rating);
                    }
                    else
                    {
                        address.rating = 0;
                    }
                }

                // Adding a rating to each accomodation

                List<Address> RatingSortedAddress = addressList.OrderByDescending(o => o.rating).Take(5).ToList();
                foreach (var coordinate in coordinates)
                {
                    var distance = CalculateDistance(userAddress.Latitude, userAddress.Longitude, coordinate.latitude, coordinate.longitude);
                    foreach (var address in RatingSortedAddress.Where(r => r.address1.Equals(coordinate.name)))
                    {
                        address.distance = distance;
                    }
                }

                // Sorts the list by rating and takes the highest 5
                // Works out a distance for each one using the calculate distance 

                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                lp.SetMargins(130, 20, 0, 0);           
                
                // Sets up the margins for padding

                foreach (var address in RatingSortedAddress)
                {
                    if (address.distance < 0.75)
                    {
                        // If the distance is less than 0.75
                        ImageView propertyImage = new ImageView(this.Context);
                        LinearLayout accomDisplay = new LinearLayout(this.Context);
                        TextView displayAddress = new TextView(this.Context);                      

                        accomDisplay.LayoutParameters = lp;
                        
                        displayAddress.TextSize = 16;                    

                        displayAddress.Text = "Address line 1: " + address.address1 + "\nAddress line 2: " 
                            + address.address2 + "\nCity: " + address.city + "\nPostcode: " + address.postcode + "\nRating: " 
                            + address.rating + "\nDistance: " + address.distance.ToString("F") + " km.";

                        // Creates the text that will contain the address

                        try
                        {
                            string imageURL = address.image.Remove(0, 2);
                            imageURL = "http://housechecker.co.uk" + imageURL;
                            WebRequest request = WebRequest.Create(imageURL);
                            WebResponse resp = request.GetResponse();
                            Stream respStream = resp.GetResponseStream();
                            Bitmap bmp = BitmapFactory.DecodeStream(respStream);
                            Bitmap image = Bitmap.CreateScaledBitmap(bmp, 500, 500, false);
                            respStream.Dispose();
                            propertyImage.SetImageBitmap(image);
                            propertyImage.RefreshDrawableState();
                        }
                        catch (Exception imageError)
                        {
                            propertyImage.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.propertyStock));
                        }

                        //  Typeface tf = Typeface.CreateFromAsset(Activity.Assets, "fonts/roboto.ttf");
                        LinearLayout.LayoutParams textParam = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
                        lp.SetMargins(0, 5, 0, 0);
                        TextView getFont = v.FindViewById<TextView>(Resource.Id.getFont);
                        displayAddress.Typeface = getFont.Typeface;
                        displayAddress.LayoutParameters = textParam;

                        // Formats all the display properties
                        
                        accomDisplay.AddView(propertyImage);
                        accomDisplay.AddView(displayAddress);

                        propertyImage.Click += delegate
                        {
                            FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                            PropertyDetail propertyDetail = new PropertyDetail();
                            Bundle bundle = new Bundle();
                            bundle.PutString("accomID", address.id.ToString());
                            propertyDetail.Arguments = bundle;
                            fragmentTx.Replace(Resource.Id.content_frame, propertyDetail);
                            fragmentTx.Commit();
                        };

                        // Adds an on click event to the image that takes you to the accomodation 

                        displayAddress.Click += async delegate
                        {
                            await CrossTextToSpeech.Current.Speak(displayAddress.Text);
                        };

                        mainLayout.AddView(accomDisplay);
                    }
                }


            };

            showMap.CallOnClick();
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

        public static double CalculateDistance(double sLatitude, double sLongitude, double eLatitude, double eLongitude)
        {
            var R = 6371;

            // Circumference of the world in KM 

            var dLongitude = deg2rad(eLongitude - sLongitude);
            var dLatitude = deg2rad(eLatitude - sLatitude);
            // Gets the distance between the accomodation and the user

            var result1 = Math.Sin(dLatitude / 2) * Math.Sin(dLatitude / 2) +
                          Math.Cos(deg2rad(sLatitude)) * Math.Cos(deg2rad(eLatitude)) *
                          Math.Sin(dLongitude / 2) * Math.Sin(dLongitude / 2);
            // Works out the result using pythagaros 

            var c = 2 * Math.Atan2(Math.Sqrt(result1), Math.Sqrt(1 - result1));
            var result2 = R * c;
            // Calculates the distance 

            return result2;
        }

        public static double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
            // Converts degree to radians
        }

        public async Task getCoordinates()
        {
            coordinates = new List<Coordinate>();
            GoogleGeocoder geocoder = new GoogleGeocoder() { ApiKey = "AIzaSyDluSC2z6JF_8GDjqnZ8A5itPnwI4EWD4A" };
            foreach (var mapPoint in addressList.Where(a => a.city.Equals(userAddress.Locality)))
            {
                string address = mapPoint.address1 + ", " + mapPoint.address2 + ", " + mapPoint.city + ", " + mapPoint.postcode;
                IEnumerable<GoogleAddress> addresses = await geocoder.GeocodeAsync(address);
                var latitude = addresses.FirstOrDefault().Coordinates.Latitude;
                var longitude = addresses.FirstOrDefault().Coordinates.Longitude;
                coordinates.Add(new Coordinate(latitude, longitude, mapPoint.address1));

            }
        }

        public async Task GetUserAddress()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 250;
            Plugin.Geolocator.Abstractions.Position position = await GetCurrentLocation();
            lon = position.Longitude;
            lat = position.Latitude;
            string mapkey = null;
            var address = await locator.GetAddressesForPositionAsync(position, mapkey);
            userAddress = address.FirstOrDefault();

            // Gets the users address
        }

        public async Task<Plugin.Geolocator.Abstractions.Position> GetCurrentLocation()
        {
            Plugin.Geolocator.Abstractions.Position position = null;
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 250;

            try
            {
                position = await locator.GetLastKnownLocationAsync();

                if (position != null)
                {
                    //got a cahched position, so let's use it.
                    return position;
                }

                if (!locator.IsGeolocationAvailable || !locator.IsGeolocationEnabled)
                {
                    //not available or enabled
                    return null;
                }

                position = await locator.GetPositionAsync(TimeSpan.FromSeconds(20), null, true);


            }
            catch (Exception ex)
            {
                //Display error as we have timed out or can't get location.
            }

            if (position == null)
                return null;

            var output = string.Format("Time: {0} \nLat: {1} \nLong: {2} \nAltitude: {3} \nAltitude Accuracy: {4} \nAccuracy: {5} \nHeading: {6} \nSpeed: {7}",
                position.Timestamp, position.Latitude, position.Longitude,
                position.Altitude, position.AltitudeAccuracy, position.Accuracy, position.Heading, position.Speed);


            return position;
        }

    }
    public class Address
    {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string image { get; set; }
        public int id { get; set; }
        public double rating { get; set; }
        public int reviewCount { get; set; }
        public string postcode { get; set; }
        public double distance { get; set; }
        public int user_id { get; set; }
    }

    public class Coordinate
    {
        public Coordinate(double latitude, double longitude, string name)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.name = name;
        }

        public double latitude { get; set; }
        public double longitude { get; set; }
        public string name { get; set; }
    }
}
