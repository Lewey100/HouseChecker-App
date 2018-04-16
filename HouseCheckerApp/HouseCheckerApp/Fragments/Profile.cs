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
using System.Collections.Specialized;

namespace HouseChecker.Fragments
{
    public class Profile : Fragment, View.IOnClickListener
    {

        TextView jsonViewer;
        AppPreferences ap = new AppPreferences(Android.App.Application.Context);
        int id;
        string editChoice = "";

        public void OnClick(View v)
        {

        }

        public static Profile NewInstance()
        {
            var frag1 = new Profile { Arguments = new Bundle() };
            return frag1;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.Profile, container, false);

            Button navHome = v.FindViewById<Button>(Resource.Id.bProperties);
            Button displayUser = v.FindViewById<Button>(Resource.Id.displayUser);
            Button submitEdit = v.FindViewById<Button>(Resource.Id.submitEdit);

            TextView displayFirstName = v.FindViewById<TextView>(Resource.Id.displayFirstName);
            TextView displayLastName = v.FindViewById<TextView>(Resource.Id.displayLastName);
            TextView displayEmail = v.FindViewById<TextView>(Resource.Id.displayEmail);
            TextView displayUni = v.FindViewById<TextView>(Resource.Id.displayUni);
            TextView displayNumber = v.FindViewById<TextView>(Resource.Id.displayNumber);
            TextView displayCompanyName = v.FindViewById<TextView>(Resource.Id.displayCompanyName);

            ImageView editFirstName = v.FindViewById<ImageView>(Resource.Id.editName);
            ImageView editLastName = v.FindViewById<ImageView>(Resource.Id.editLastName);
            ImageView editUni = v.FindViewById<ImageView>(Resource.Id.editUni);
            ImageView editEmail = v.FindViewById<ImageView>(Resource.Id.editEmail);
            ImageView editNumber = v.FindViewById<ImageView>(Resource.Id.editNumber);
            ImageView editCompanyName = v.FindViewById<ImageView>(Resource.Id.editCompanyName);
            LinearLayout editLayout = v.FindViewById<LinearLayout>(Resource.Id.editLayout);

            EditText submitEditValue = v.FindViewById<EditText>(Resource.Id.editValue);

            LinearLayout firstNameLayout = v.FindViewById<LinearLayout>(Resource.Id.firstNameLayout);
            LinearLayout lastNameLayout = v.FindViewById<LinearLayout>(Resource.Id.lastNameLayout);
            LinearLayout uniLayout = v.FindViewById<LinearLayout>(Resource.Id.uniLayout);
            LinearLayout phoneLayout = v.FindViewById<LinearLayout>(Resource.Id.numberLayout);
            LinearLayout emailLayout = v.FindViewById<LinearLayout>(Resource.Id.emailLayout);
            LinearLayout companyNameLayout = v.FindViewById<LinearLayout>(Resource.Id.companyNameLayout);

            // Retrieves all objects from the XAML page

            id = int.Parse(ap.getAccessKey());

            displayUser.Click += async (sender, e) =>
            {
                string url = "http://housechecker.co.uk/api/export.php";
                JsonValue json = await FetchUserAsync(url);

                string jsonString = json.ToString();
                List<Student> listOfStudents = JsonConvert.DeserializeObject<List<Student>>(jsonString);
                var user = listOfStudents.Where(a => a.Id == id).FirstOrDefault();
                if(user.Type == "Student")
                {
                    firstNameLayout.Visibility = ViewStates.Visible;
                    lastNameLayout.Visibility = ViewStates.Visible;
                    uniLayout.Visibility = ViewStates.Visible;
                    phoneLayout.Visibility = ViewStates.Visible;
                    emailLayout.Visibility = ViewStates.Visible;

                    displayFirstName.Text += user.Firstname;
                    displayLastName.Text += user.Lastname;
                    displayEmail.Text += user.Email;
                    displayUni.Text += user.Uni;
                    displayNumber.Text += user.Phone;

                    // If the user is a student select the correct fields to show
                }
                else
                {
                    phoneLayout.Visibility = ViewStates.Visible;
                    emailLayout.Visibility = ViewStates.Visible;
                    companyNameLayout.Visibility = ViewStates.Visible;

                    displayEmail.Text += user.Email;
                    displayNumber.Text += user.Phone;
                    displayCompanyName.Text += user.CompanyName;

                    // Same if the user is a landlord
                }
            };

            editFirstName.Click += delegate
            {
                editChoice = "first_name";
                editLayout.Visibility = ViewStates.Visible;
            };

            editLastName.Click += delegate
            {
                editChoice = "last_name";
                editLayout.Visibility = ViewStates.Visible;
            };

            editUni.Click += delegate
            {
                editChoice = "university";
                editLayout.Visibility = ViewStates.Visible;
            };

            editEmail.Click += delegate
            {
                editChoice = "email";
                editLayout.Visibility = ViewStates.Visible;
            };

            editNumber.Click += delegate
            {
                editChoice = "phone";
                editLayout.Visibility = ViewStates.Visible;
            };

            editCompanyName.Click += delegate
            {
                editChoice = "company_name";
                editLayout.Visibility = ViewStates.Visible;
            };

            // Adds a click event to each edit icon to allow the user to select what field they want to edit

            submitEdit.Click += async delegate
            {
                string url = "http://housechecker.co.uk/api/edit_user.php";
                string newValue = submitEditValue.Text;
                string data = await editValue(url, newValue);
            };

            // This submits the data to be edited

            displayUser.CallOnClick();
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

        private async Task<string> editValue(string url, string newValue)
        {
            string success = "";

            string newValueUpload = newValue;

            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["userid"] = id.ToString();
                data["value"] = newValue;
                // This is the new value
                data["field"] = editChoice;
                // This is the field to be edited

                var response = webClient.UploadValues(url, "POST", data);
            }

            FragmentTransaction fragmentTx = FragmentManager.BeginTransaction();
            Profile profile = new Profile();
            fragmentTx.Replace(Resource.Id.content_frame, profile);
            fragmentTx.Commit();

            return success;
           

        }
    }

    public class Student
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public int Id { get; set; }
        public string Lastname { get; set; }
        public string Name { get; set; }
        public string Pass { get; set; }
        public string Uni { get; set; }
        public string Type { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }
    }

}
