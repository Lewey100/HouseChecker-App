using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Views;

using HouseCheckerApp.Fragments;

using Android.Support.Design.Widget;
using HouseChecker.Fragments;
using System.Json;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using HouseChecker;
using Android.Widget;

namespace HouseCheckerApp
{
    [Activity(Label = "@string/app_name", LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/Icon")]
    public class MainActivity : AppCompatActivity
    {
        string userType;
        AppPreferences ap = new AppPreferences(Application.Context);
        DrawerLayout drawerLayout;
        TextView usernameDisplay;
        NavigationView navigationView;
        string markerTitle;
        int accomID;

        IMenuItem previousItem;
        protected override void OnCreate(Bundle savedInstanceState)
        {
           
            userType = Intent.GetStringExtra("Type");
            // Gets the type of the user ie Student or Landlord 
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
            }

            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            // Creates the navigation bar for the program and sets it 

            //Set hamburger items menu
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);

            //setup navigation view
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);


            //handle navigation
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                if (previousItem != null)
                    previousItem.SetChecked(false);

                navigationView.SetCheckedItem(e.MenuItem.ItemId);

                previousItem = e.MenuItem;

                // Switches the result on what button is clicked on in the nav bar
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_home_1:
                        ListItemClicked(0);
                        break;
                    case Resource.Id.nav_properties:
                         Intent map = new Intent(this, typeof(DisplayMap));
                         StartActivity(map);
                         Finish();
                        //ListItemClicked(1);
                        break;
                    case Resource.Id.nav_search:
                        ListItemClicked(2);
                        break;
                    case Resource.Id.nav_profile:
                        ListItemClicked(3);
                        break;
                    case Resource.Id.nav_extra1:
                        ListItemClicked(4);
                        break;
                    case Resource.Id.nav_extra2:
                        ListItemClicked(5);
                        break;
                    case Resource.Id.nav_logout:
                        Intent i = new Intent(this, typeof(Login));
                        StartActivity(i);
                        Finish();
                        break;
                }


                drawerLayout.CloseDrawers();
            };

            if (userType == "Landlord")
            {
                IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                item1.SetTitle("Add Property");
                IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                item2.SetTitle("My Properties");
                // If the user type is Landlord, it sets the two extra features to these options
            }
            else
            {
                IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                item1.SetTitle("Favourite Properties");
                IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                item2.SetTitle("My Reviews");
                // If you're a student the options are different 
            }

            //if first time you will want to go ahead and click first item.
            if (savedInstanceState == null)
            {
                markerTitle = Intent.GetStringExtra("accomTitle");
                // Gets the marker title from the string
                if (markerTitle != null)
                // The marker is null if you havent clicked a marker on the Map page
                {
                    ListItemClicked(6);
                    // This takes you to the property page from the marker you have clicked on
                }else
                {
                    navigationView.SetCheckedItem(Resource.Id.nav_home_1);
                    ListItemClicked(0);
                    // Takes you to the home page which for us is the student or landlord dashboard
                }
                
            }
        }

        int oldPosition = -1;
        private async void ListItemClicked(int position)
        {
            Android.Support.V4.App.Fragment fragment = null;
            switch (position)
            {
                case 0:
                    int id = int.Parse(ap.getAccessKey());
                    string url = "http://housechecker.co.uk/api/export.php";
                    JsonValue json = await FetchUserAsync(url);

                    string jsonString = json.ToString();
                    List<Student> listOfLandlords = JsonConvert.DeserializeObject<List<Student>>(jsonString);
                    var user = listOfLandlords.Where(a => a.Id == id).FirstOrDefault();

                    if (user.Type == "Landlord")
                    {
                        IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                        item1.SetTitle("Add Property");
                        IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                        item2.SetTitle("My Properties");
                        // If the user type is Landlord, it sets the two extra features to these options
                    }
                    else
                    {
                        IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                        item1.SetTitle("Favourite Properties");
                        IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                        item2.SetTitle("My Reviews");
                        // If you're a student the options are different 
                    }
                    // Gets the list of users and finds the one that matches your ID

                    usernameDisplay = FindViewById<TextView>(Resource.Id.navBarHeader);
                    // Gets the display name for the nav bar 

                   
                    if (user.Type == "Landlord")
                    {
                        fragment = LandlordDashboard.NewInstance();
                        usernameDisplay.Text = user.CompanyName;
                        // Sets the name to company name if you are a landlord
                    }
                    else
                    {
                        fragment = StudentDashboard.NewInstance();
                        usernameDisplay.Text = user.Name;
                        // And to username if you are a student 
                    }
                    break;
                case 1:
                    fragment = PropertyDetail.NewInstance();
                    Bundle idBundle = new Bundle();
                    idBundle.PutString("accomID", "77");
                    fragment.Arguments = idBundle;
                    // This code calls the new fragment and places it on the screen aka loading a new page
                    break;
                case 2:
                    fragment = SearchPage.NewInstance();
                    break;
                case 3:
                    fragment = Profile.NewInstance();
                    break;
                case 4:
                    if(userType == "Landlord")
                    {
                        fragment = AddProperty.NewInstance();
                    }
                    else
                    {
                        fragment = DisplayFavourites.NewInstance();
                    }
                    break;
                case 5:
                    if (userType == "Landlord")
                    {
                        fragment = MyProperties.NewInstance();
                    }
                    else
                    {
                        fragment = MyReviews.NewInstance();
                    }
                    break;
                case 6:
                    id = int.Parse(ap.getAccessKey());
                    url = "http://housechecker.co.uk/api/export.php";
                    json = await FetchUserAsync(url);

                    jsonString = json.ToString();
                    listOfLandlords = JsonConvert.DeserializeObject<List<Student>>(jsonString);
                    user = listOfLandlords.Where(a => a.Id == id).FirstOrDefault();

                    if (user.Type == "Landlord")
                    {
                        IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                        item1.SetTitle("Add Property");
                        IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                        item2.SetTitle("My Properties");
                        // If the user type is Landlord, it sets the two extra features to these options
                    }
                    else
                    {
                        IMenuItem item1 = navigationView.Menu.FindItem(Resource.Id.nav_extra1);
                        item1.SetTitle("Favourite Properties");
                        IMenuItem item2 = navigationView.Menu.FindItem(Resource.Id.nav_extra2);
                        item2.SetTitle("My Reviews");
                        // If you're a student the options are different 
                    }
                    // Gets the list of users and finds the one that matches your ID

                    url = "http://housechecker.co.uk/api/display.php";
                    json = await FetchUserAsync(url);

                    jsonString = json.ToString();
                    var addressList = JsonConvert.DeserializeObject<List<Address>>(jsonString);

                    accomID = addressList.Where(a => a.address1 == markerTitle).FirstOrDefault().id;
                    fragment = PropertyDetail.NewInstance();
                    Bundle bundle = new Bundle();
                    bundle.PutString("accomID", accomID.ToString());
                    fragment.Arguments = bundle;
                    // Gets the accomodation that has been clicked on from the map page and then loads it
                    break;

            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
            // Commits the transaction
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
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

