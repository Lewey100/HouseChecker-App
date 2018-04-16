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
using Android.Graphics;

namespace HouseCheckerApp.Fragments
{

    public class MyProperties : Fragment
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
        //Creating an instance of the dashboard
        public static MyProperties NewInstance()
        {
            var frag1 = new MyProperties { Arguments = new Bundle() };
            return frag1;
        }
        //Creates the main attributes of the page when it is loaded
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.myProperties, container, false);
            mainLayout = v.FindViewById<LinearLayout>(Resource.Id.mainLayout);
            Button displayProperties = v.FindViewById<Button>(Resource.Id.showProperty);

            id = int.Parse(ap.getAccessKey());

            displayProperties.Click += async delegate
            {
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url);
                string jsonString = json.ToString();
                //Getting the properties from the database and putting it into the list
                propertyList = JsonConvert.DeserializeObject<List<Address>>(jsonString);
                //Getting each property for the current landlord
                foreach (var accomodation in propertyList.Where(a => a.user_id.Equals(id)))
                {
                    //Creating the layout for the properties
                    LinearLayout newLayout = new LinearLayout(this.Context);
                    newLayout.Orientation = Orientation.Vertical;
                    TextView displayAddress = new TextView(this.Context);
                    displayAddress.Text = "Address: " + accomodation.address1 + ", " + accomodation.address2 + "\nCity: " +
                        accomodation.city + "\nPostcode " + accomodation.postcode;
                    displayAddress.Typeface = Typeface.DefaultBold;
                    displayAddress.TextSize = 16;
                    displayAddress.Gravity = GravityFlags.Center;

                    //Button to allow for more informaiton

                    ImageView imageView = new ImageView(this.Context);

                    try
                    {
                        string imageURL = accomodation.image.Remove(0, 2);
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
                        bundle.PutString("accomID", accomodation.id.ToString());
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
            displayProperties.CallOnClick();
            return v;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }
        //Function that interacts with the database
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
