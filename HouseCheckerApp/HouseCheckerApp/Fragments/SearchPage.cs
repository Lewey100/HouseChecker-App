using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Json;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Android.Speech;
using Android.Support.V4.App;
using Plugin.TextToSpeech;
using Android.Support.V7.App;
using HouseChecker.Fragments;
using Android.Graphics;

namespace HouseCheckerApp.Fragments
{
    class SearchPage : Fragment
    {
        TextView jsonViewer;
        EditText city;
        Button search;
        readonly int VOICE = 10;
        LinearLayout mainLayout;

        public static SearchPage NewInstance()
        {
            var frag1 = new SearchPage { Arguments = new Bundle() };
            return frag1;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.searchPage, container, false);
            city = v.FindViewById<EditText>(Resource.Id.getCity);
            search = v.FindViewById<Button>(Resource.Id.bSearch);
            jsonViewer = v.FindViewById<TextView>(Resource.Id.textView1);
            mainLayout = v.FindViewById<LinearLayout>(Resource.Id.searchLayout);
            // Gets all the elements on the page

            search.Click += async delegate
            {
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url);
                // Gets all the properties in the database

                string jsonString = json.ToString();
                var addressList = JsonConvert.DeserializeObject<List<Address>>(jsonString);
                // Creates a list of addresses from the JSON string through parsing

                foreach(var address in addressList)
                {
                    // Goes through each accomodation to see if the city matches the city searched for
                    if (address.city.ToUpper().Equals(city.Text.ToUpper()))
                    {
                        LinearLayout newLayout = new LinearLayout(this.Context);
                        newLayout.Orientation = Orientation.Vertical;
                        ImageView imageView = new ImageView(this.Context);
                        imageView.SetPadding(0, 100, 0, 0);
                        // Creates a new layout to store a single entry in the search results

                        try
                        {
                            string imageURL = address.image.Remove(0, 2);
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

                        TextView displayAddress = new TextView(this.Context);
                        displayAddress.Text = (address.address1 + " , " + address.address2);
                        displayAddress.Typeface = Typeface.DefaultBold;
                        displayAddress.TextSize = 16;
                        displayAddress.Gravity = GravityFlags.Center;
                        // Creates and formats a text field to display the address for the property

                        displayAddress.Click += async delegate
                        {
                            await CrossTextToSpeech.Current.Speak(displayAddress.Text);
                        };
                        // Using a plugin this reads out the text that is clicked on, in this case the address

                        imageView.Click += (senderNew, eNew) =>
                        {
                            FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                            PropertyDetail propertyDetail = new PropertyDetail();
                            Bundle bundle = new Bundle();
                            bundle.PutString("accomID", address.id.ToString());
                            propertyDetail.Arguments = bundle;
                            fragmentTx.Replace(Resource.Id.content_frame, propertyDetail);
                            fragmentTx.Commit();
                            // Creates a click event function for the image so it will go to the selected property
                        };

                        newLayout.AddView(imageView);
                        newLayout.AddView(displayAddress);
                        mainLayout.AddView(newLayout);
                        // Adding the views to the main Layout
                    }


                }
            };
         
            return v;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }


        protected void OnActivityResult(int requestCode, Android.App.Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Android.App.Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = city.Text + matches[0];
                        city.Text = textInput;
                        search.CallOnClick();
                    }
                    else
                    {
                        city.Text = "No speech was recognised";
                    }

                    OnActivityResult(requestCode, resultVal, data);
                }
            }
            // This function was from a plugin, but it reads out the text that is passed int it
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
        // A basic function that is repeated throughout our code. This runs a php function to GET data based on the url passed in



    }

}