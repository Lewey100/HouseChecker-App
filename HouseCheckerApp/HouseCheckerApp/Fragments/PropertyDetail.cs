using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Android.Content;
using Android.Support.V4.App;
using HouseCheckerApp;
using Android.Graphics;
using System;
using System.Collections.Specialized;
using HouseChecker.Fragments;
using Plugin.TextToSpeech;

namespace HouseCheckerApp.Fragments
{

    public class PropertyDetail : Fragment
    {
        int accomId;
        Button displayAccom, addReview, returnToPrevious;
        TextView displayAddress, displayBills, displayWifi, displayPrice, displayBedrooms, displayBathrooms, displayRating;
        ImageView acommImage;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        List<Address> addressList;

        public static PropertyDetail NewInstance()
        {
            var frag1 = new PropertyDetail { Arguments = new Bundle() };
            return frag1;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.PropertyDetailView, container, false);
            accomId = int.Parse(Arguments.GetString("accomID"));
            addReview = v.FindViewById<Button>(Resource.Id.addReview);
            displayAccom = v.FindViewById<Button>(Resource.Id.displayAccom);
            displayAddress = v.FindViewById<TextView>(Resource.Id.displayAddress);
            displayBills = v.FindViewById<TextView>(Resource.Id.displayBills);
            displayWifi = v.FindViewById<TextView>(Resource.Id.displayWifi);
            displayPrice = v.FindViewById<TextView>(Resource.Id.displayPrice);
            displayBedrooms = v.FindViewById<TextView>(Resource.Id.displayBedrooms);
            displayBathrooms = v.FindViewById<TextView>(Resource.Id.displayBathrooms);
            displayRating = v.FindViewById<TextView>(Resource.Id.displayRating);
            LinearLayout reviewLayout = v.FindViewById<LinearLayout>(Resource.Id.reviewLayout);
            Button addToFavourites = v.FindViewById<Button>(Resource.Id.addFavourite);
            addressList = new List<Address>();

            // Gets all the views

            addReview.Click += delegate
            {
                FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
                addReview addReview = new addReview();
                Bundle bundle = new Bundle();
                bundle.PutString("accomID", accomId.ToString());
                addReview.Arguments = bundle;
                fragmentTx.Replace(Resource.Id.content_frame, addReview);
                fragmentTx.Commit();
            };

            // Takes you to the add a review page

            displayAccom.Click += async (sender, e) =>
            {
                string url = "http://housechecker.co.uk/api/export_property.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                addressList = JsonConvert.DeserializeObject<List<Address>>(jsonString);

                Address accomodation = addressList.Where(a => a.id == accomId).FirstOrDefault();

                ImageView imageView = v.FindViewById<ImageView>(Resource.Id.propertyImage);

                // Finds the accomodation that matches the ID that you have passed in

                try
                {
                    string imageURL = accomodation.image.Remove(0, 2);
                    imageURL = "http://housechecker.co.uk" + imageURL;
                    WebRequest request = WebRequest.Create(imageURL);
                    WebResponse resp = request.GetResponse();
                    Stream respStream = resp.GetResponseStream();
                    Bitmap bmp = BitmapFactory.DecodeStream(respStream);
                    Bitmap image = Bitmap.CreateScaledBitmap(bmp, 500, 500, false);
                    respStream.Dispose();
                    imageView.SetImageBitmap(image);
                    imageView.RefreshDrawableState();
                }
                catch (Exception error)
                {
                    imageView.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.propertyStock));
                }

                // Finds and displays the image

                string ratingUrl = "http://housechecker.co.uk/api/export_review.php";
                json = await FetchUserAsync(ratingUrl);

                jsonString = json.ToString();
                var reviewList = JsonConvert.DeserializeObject<List<Review>>(jsonString);

                // Gets all the reviewws and puts it into a review list

                List<Review> review = reviewList.Where(a => a.property_id == accomodation.id).ToList();
                if (review.Count > 0)
                {
                    accomodation.rating = review.Average(a => a.rating);
                }
                else
                {
                    accomodation.rating = 0;
                }

                // Gets the rating from each review that is linked to the property and averages it

                displayRating.Text = "Average Rating: " + accomodation.rating;
                displayRating.Click += async delegate
                {
                    await CrossTextToSpeech.Current.Speak(displayRating.Text);
                };

                displayBills.Text = "Bills included: " + accomodation.bills;
                displayBills.Click += async delegate
                {
                    await CrossTextToSpeech.Current.Speak(displayBills.Text);
                };

                displayWifi.Text = "Wifi included: " + accomodation.wifi;
                displayWifi.Click += async delegate
                 {
                     await CrossTextToSpeech.Current.Speak(displayWifi.Text);
                 };

                displayPrice.Text = "Price per week: " + accomodation.price;
                displayPrice.Click += async delegate
                {
                    await CrossTextToSpeech.Current.Speak(displayPrice.Text);
                };

                displayBathrooms.Text = "Number of bathrooms: " + accomodation.bathrooms;
                displayBathrooms.Click += async delegate
                {
                    await CrossTextToSpeech.Current.Speak(displayBathrooms.Text);
                };

                displayBedrooms.Text = "Number of bedrooms: " + accomodation.bedrooms;
                displayBedrooms.Click += async delegate
                 {
                     await CrossTextToSpeech.Current.Speak(displayBedrooms.Text);
                 };

                displayAddress.Text = "Address: " + accomodation.address1 + ", " + accomodation.address2 + ", " +
                        accomodation.city + ", " + accomodation.postcode;
                displayAddress.Click += async delegate
                 {
                     await CrossTextToSpeech.Current.Speak(displayAddress.Text);
                 };

                // Creates an on click event that reads out whatever the text says

                TextView reviewTitle = new TextView(this.Context);
                reviewTitle.Text = "Most recent reviews: ";
                reviewTitle.TextSize = 32;
                reviewTitle.Typeface = Typeface.DefaultBold;
                reviewTitle.Gravity = GravityFlags.Center;
                reviewLayout.AddView(reviewTitle);

                // Creates a title for the reviews section 

                foreach (var accomReview in review.OrderBy(a => a.date_of_review).Take(20))
                {
                    TextView newReviewText = new TextView(this.Context);

                    newReviewText.Text = "Title: " + accomReview.title + "\nComment: " 
                        + accomReview.comment + "\nRating: " + accomReview.rating;

                    newReviewText.TextSize = 24;
                    newReviewText.Gravity = GravityFlags.Center;

                    reviewLayout.AddView(newReviewText);

                    // Gets the top 20 reviews for this accomodation, ordered by date, and creates a text view for them
                }

                url = "http://housechecker.co.uk/api/export_favourite.php";
                json = await FetchUserAsync(url);
                jsonString = json.ToString();
                var favouriteList = JsonConvert.DeserializeObject<List<Favourite>>(jsonString);
                int userID = Int32.Parse(ap.getAccessKey());

                url = "http://housechecker.co.uk/api/export.php";
                json = await FetchUserAsync(url);
                jsonString = json.ToString();
                Student user = JsonConvert.DeserializeObject<List<Student>>(jsonString).Where(a => a.Id == userID).FirstOrDefault();

                // Gets the favourite list and decides if the user is a student or not 

                if(user.Type == "Student")
                {
                    addToFavourites.Visibility = ViewStates.Visible;
                }
                
                if(user.Type == "Landlord")
                {
                    addReview.Visibility = ViewStates.Invisible;
                }

                // Landlords cant add a favourite

                var accomFavourite = favouriteList.Where(a => a.user_id == userID
                    && a.accom_id ==accomodation.id).FirstOrDefault();

                // Gets the value for a saved favourite for that accomodation and user

                if(accomFavourite != null)
                {
                    // If a favourite exists it means the user has added it to their favourites
                    addToFavourites.Text = "Remove from favourites";
                    addToFavourites.Click += async delegate
                    {
                        url = "http://housechecker.co.uk/api/delete_favourite.php";
                        json = await DeleteFavourite(url, accomFavourite.favourite_id);
                        // This will remove the favourited accomodation from their list
                        Activity.RunOnUiThread(() =>
                        {
                            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(Activity);
                            Android.App.AlertDialog alert = dialog.Create();
                            dialog.SetCancelable(false);
                            alert.SetTitle("Complete");
                            alert.SetMessage("Successfully removed from favourites");
                            alert.SetButton("OK", (c, ev) =>
                            {
                                // Ok button click task  
                            });
                            alert.Show();
                        });
                        // Shows a pop up window informing the user it has worked
                    };
                }
                else
                {
                    addToFavourites.Text = "Add to Favourites";
                    addToFavourites.Click += async delegate
                    {
                        url = "http://housechecker.co.uk/api/import_favourite.php";
                        json = await AddFavourite(url, accomodation.id);
                        // This will add the property to the list
                        Activity.RunOnUiThread(() =>
                        {
                            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(Activity);
                            Android.App.AlertDialog alert = dialog.Create();
                            dialog.SetCancelable(false);
                            alert.SetTitle("Complete");
                            alert.SetMessage("Successfully added to favourites");
                            alert.SetButton("OK", (c, ev) =>
                            {
                                // Ok button click task  
                            });
                            alert.Show();
                        });
                    };
                }
                
            };
            displayAccom.CallOnClick();
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
        private async Task<JsonValue> DeleteFavourite(string url, int favouriteid)
        {
            string success = "";

            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["favourite_id"] = favouriteid.ToString();
                var response = webClient.UploadValues(url, "POST", data);

            }
            return success;
            // Calls the DELETE php page using the URL 
        }

        private async Task<JsonValue> AddFavourite(string url, int accomid)
        {
            string success = "";

            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["user_id"] = ap.getAccessKey();
                data["accom_id"] = accomid.ToString();
                var response = webClient.UploadValues(url, "POST", data);

            }
            // Calls the ADD php using the URL 
            return success;
        }
        public class Address
        {
            public string address1 { get; set; }
            public string address2 { get; set; }
            public string bathrooms { get; set; }
            public string bedrooms { get; set; }
            public string bills { get; set; }
            public string city { get; set; }
            public int id { get; set; }
            public string image { get; set; }
            public string postcode { get; set; }
            public string price { get; set; }
            public string type_of_accomodation { get; set; }
            public string wifi { get; set; }
            public double rating { get; set; }
        }       
    }
}