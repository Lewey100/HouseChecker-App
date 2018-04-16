using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Android.Support.V4.App;
using HouseChecker.Fragments;
using Android.Graphics;

namespace HouseCheckerApp.Fragments
{
    public class DisplayFavourites : Fragment
    {
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        //Set Fragments
        public static DisplayFavourites NewInstance()
        {
            var frag1 = new DisplayFavourites { Arguments = new Bundle() };
            return frag1;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            //Set Layouts
            View v = inflater.Inflate(Resource.Layout.DisplayFavouritesView, container, false);
            LinearLayout mainLayout = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            Button displayFavourites = v.FindViewById<Button>(Resource.Id.displayFavouritesList);


            //Get User ID
            int userID = int.Parse(ap.getAccessKey());

            //Displaying the favs
            displayFavourites.Click += async delegate
            {
                //URL is received for getting all propertys
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url);
                string jsonString = json.ToString();
                //Set the property list to convert the gathered url data
                var propertyList = JsonConvert.DeserializeObject<List<Address>>(jsonString);

                //Export favs from url
                url = "http://housechecker.co.uk/api/export_favourite.php";
                json = await FetchUserAsync(url);
                jsonString = json.ToString();
                var favouriteList = JsonConvert.DeserializeObject<List<Favourite>>(jsonString);
                var usersFavourites = favouriteList.Where(a => a.user_id == userID).ToList();

                //For every favourite in list
                foreach (var favourite in usersFavourites)
                {
                    //Get the selected property to match the user id of current user in fav table
                    var selectedAccom = propertyList.Where(a => a.id == favourite.accom_id).FirstOrDefault();
                    //Creating the layout for the properties
                    LinearLayout newLayout = new LinearLayout(this.Context);
                    newLayout.Orientation = Orientation.Vertical;
                    TextView displayAddress = new TextView(this.Context);
                    displayAddress.Text = "Address: " + selectedAccom.address1 + ", " + selectedAccom.address2 + "\nCity: " +
                        selectedAccom.city + "\nPostcode " + selectedAccom.postcode;
                    displayAddress.Typeface = Typeface.DefaultBold;
                    displayAddress.TextSize = 16;
                    displayAddress.Gravity = GravityFlags.Center;

                    //Button to allow for more informaiton

                    ImageView imageView = new ImageView(this.Context);

                    try
                    {
                        string imageURL = selectedAccom.image.Remove(0, 2);
                        imageURL = "http://housechecker.co.uk" + imageURL;
                        WebRequest request = WebRequest.Create(imageURL);
                        // Creates the image URL based from what is stored in the database
                        WebResponse resp = request.GetResponse();
                        Stream respStream = resp.GetResponseStream();
                        Bitmap bmp = BitmapFactory.DecodeStream(respStream);
                        Bitmap image = Bitmap.CreateScaledBitmap(bmp, 500, 500, false);
                        // Resizes the image to fit
                        respStream.Dispose();
                        imageView.SetImageBitmap(image);
                        imageView.RefreshDrawableState();
                        // Creates it and sets it to the view
                    }
                    catch (Exception e)
                    {
                        imageView.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.propertyStock));
                    }

                    imageView.Click += (senderNew, eNew) =>
                    {
                        FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                        PropertyDetail propertyDetail = new PropertyDetail();
                        Bundle bundle = new Bundle();
                        bundle.PutString("accomID", selectedAccom.id.ToString());
                        propertyDetail.Arguments = bundle;
                        fragmentTx.Replace(Resource.Id.content_frame, propertyDetail);
                        fragmentTx.Commit();
                        // Creates a click event function for the image so it will go to the selected property
                    };

                    newLayout.AddView(imageView);
                    newLayout.AddView(displayAddress);
                    mainLayout.AddView(newLayout);
                }
            };

            displayFavourites.CallOnClick();
            return v;
        }

        //Get JSON url
        private async Task<JsonValue> FetchUserAsync(string url)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.ContentType = "json";
            request.Method = "GET";
            //get the web response
            using (WebResponse response = await request.GetResponseAsync())
            {
                //get stream
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    return jsonDoc;
                }
            }
        }
    }
}

//get favourites from url
public class Favourite
{
    public int favourite_id { get; set; }
    public int user_id { get; set; }
    public int accom_id { get; set; }
}