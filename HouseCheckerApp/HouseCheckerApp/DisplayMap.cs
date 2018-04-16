using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Json;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V4.View;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Linq;
using GoogleMaps.Geolocation;
using Plugin.Geolocator;
using System;
using Android.Content;
using Geocoding.Google;
using HouseCheckerApp;
using static Android.Gms.Maps.GoogleMap;
using HouseCheckerApp.Fragments;
using HouseChecker.Fragments;

namespace HouseCheckerApp
{
    [Activity(Label = "DisplayMap")]
    public class DisplayMap : Activity, IOnMapReadyCallback, IOnMarkerClickListener
    {
        private GoogleMap mMap;
        Button showMap, returnBtn;
        List<Address> addressList;
        double lan;
        double lon;
        Plugin.Geolocator.Abstractions.Address usersAddress;
        List<Coordinate> coordinates;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.DisplayMapView);
            showMap = (Button)FindViewById(Resource.Id.displayMap);
            returnBtn = (Button)FindViewById<Button>(Resource.Id.returnToApp);

            showMap.Click += async delegate
            {
                await GetUserAddress();
                string url = "http://housechecker.co.uk/api/display.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                addressList = JsonConvert.DeserializeObject<List<Address>>(jsonString);
                await getCoordinates();
                SetUpMap();
            };

            returnBtn.Click += delegate
            {
                Intent i = new Intent(this, typeof(MainActivity));
                StartActivity(i);
                Finish();
            };

            showMap.PerformClick();
        }

        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }

        // Sets up the map using the Fragment Manager and adding a fragment to the layout

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

        public async Task getCoordinates()
        {
            coordinates = new List<Coordinate>();
            GoogleGeocoder geocoder = new GoogleGeocoder() { ApiKey = "AIzaSyBQL6EeEsDIqm2FFp8hvcbgTReAAjFeIVc" };
            // Sets up the geocoder using a Google Play plugin and using a GMAPS API Key that I have generated for the app
            foreach (var mapPoint in addressList.Where(a => a.city.Equals(usersAddress.Locality)))
            {
                string address = mapPoint.address1 + ", " + mapPoint.address2 + ", " + mapPoint.city + ", " + mapPoint.postcode;
                // Builds the address based on the address
                IEnumerable<GoogleAddress> addresses = await geocoder.GeocodeAsync(address);
                // This geocoder is a function made by the plugin and returns a lat and long point
                var latitude = addresses.FirstOrDefault().Coordinates.Latitude;
                var longitude = addresses.FirstOrDefault().Coordinates.Longitude;
                // Gets the co ordinates
                coordinates.Add(new Coordinate(latitude, longitude, mapPoint.address1));
                // Adds it to a list 
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            foreach (var coordinate in coordinates)
            {
                MarkerOptions marker = new MarkerOptions();
                marker.SetPosition(new LatLng(coordinate.latitude, coordinate.longitude));
                marker.SetTitle(coordinate.name);
                googleMap.AddMarker(marker);
                // Converts the co ordinates into a marker for the map and adds that point
            }
            MarkerOptions usersMarker = new MarkerOptions();
            usersMarker.SetPosition(new LatLng(usersAddress.Latitude, usersAddress.Longitude));
            usersMarker.SetTitle("You");
            googleMap.AddMarker(usersMarker);
            // Gets the marker for your own position 

            LatLng location = new LatLng(usersAddress.Latitude, usersAddress.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(location);
            builder.Zoom(18);
            builder.Bearing(155);
            builder.Tilt(65);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
            // Positions and zooms the camera on your location

            googleMap.AnimateCamera(cameraUpdate);
            googleMap.SetOnMarkerClickListener(this);
            // Animates the camera to that point

            mMap = googleMap;
        }

        public bool OnMarkerClick(Marker marker)
        {
            if(marker.Title != "You")
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("accomTitle", marker.Title);
                StartActivity(intent);
                return true;

                // If click an accomodation marker then it will display that accomodation page 
            }
            else
            {
                return true;
            }
        }

        public async Task GetUserAddress()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 250;
            Plugin.Geolocator.Abstractions.Position position = await GetCurrentLocation();
            // This plugin function gets the users location and creates a variable for it 
            lon = position.Longitude;
            lan = position.Latitude;
            string mapkey = null;
            var address = await locator.GetAddressesForPositionAsync(position, mapkey);
            usersAddress = address.FirstOrDefault();
            // Passes the address back to the main code 
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

            // Builds the users address

            return position;
        }


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