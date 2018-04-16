using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

using System.Threading.Tasks;
using HouseCheckerApp;
using System.Json;

namespace HouseCheckerApp
{
    [Activity(Label = "Register")]
    
    public class Register : Activity, View.IOnClickListener
    {
        //Variables
        EditText firstname, secondname,university, username, password, companyname, address_1, address_2, cityinput, postcodeinput, phoneinput, emailinput;
        Button register;
        RadioGroup profileType;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Setting the context view
            SetContentView(Resource.Layout.register);
            //Setting the variables to the appropriate axml ID's
            profileType = FindViewById<RadioGroup>(Resource.Id.getType);
            firstname = (EditText)FindViewById(Resource.Id.firstName);
            secondname = (EditText)FindViewById(Resource.Id.secondName);
            university = (EditText)FindViewById(Resource.Id.university);
            username = (EditText)FindViewById(Resource.Id.user);
            password = (EditText)FindViewById(Resource.Id.pass);
            companyname = (EditText)FindViewById(Resource.Id.company_name);
            address_1 = (EditText)FindViewById(Resource.Id.address1);
            address_2 = (EditText)FindViewById(Resource.Id.address2);
            cityinput = (EditText)FindViewById(Resource.Id.city);
            postcodeinput = (EditText)FindViewById(Resource.Id.postcode);
            phoneinput = (EditText)FindViewById(Resource.Id.phone);
            emailinput = (EditText)FindViewById(Resource.Id.email);
            register = (Button)FindViewById(Resource.Id.bRegister);
            //Setting a listener for the register button
            register.SetOnClickListener(this);
            //Setting the values from the profile type to there variables
            RadioButton radio_landlord = FindViewById<RadioButton>(Resource.Id.getLandlordValue);
            RadioButton radio_student = FindViewById<RadioButton>(Resource.Id.getStudentValue);
            radio_landlord.Click += RadioButtonClick;
            radio_student.Click += RadioButtonClick;
            //Making all of the non-changing variables visible
            username.Visibility = ViewStates.Visible;
            password.Visibility = ViewStates.Visible;
            address_1.Visibility = ViewStates.Visible;
            address_2.Visibility = ViewStates.Visible;
            cityinput.Visibility = ViewStates.Visible;
            postcodeinput.Visibility = ViewStates.Visible;
            phoneinput.Visibility = ViewStates.Visible;
            emailinput.Visibility = ViewStates.Visible;
            //Sending an email to the user welcoming them to the app
            register.Click += async (sender, e) =>
            {
                string url = "http://housechecker.co.uk/api/import.php?";
                string data = await FetchUserAsync(url);
                //Attaching the username if they are a student
                if((FindViewById<RadioButton>(profileType.CheckedRadioButtonId)).Text == "Student")
                {
                    string message = "Welcome, " + username.Text + ", thanks for signing up to House Checker";
                    string subject = "Account created";
                    string to = emailinput.Text;
                    url = "http://housechecker.co.uk/api/email.php";
                    data = await ConfirmationEmail(url, to, message, subject);
                }
                //Attaching the company name if they are a landlord
                else
                {
                    string message = "Welcome, " + companyname.Text + ", thanks for signing up to House Checker";
                    string subject = "Account created";
                    string to = emailinput.Text;
                    url = "http://housechecker.co.uk/api/email.php";
                    data = await ConfirmationEmail(url, to, message, subject);
                }
            };
        }
        //Changing the visibilty of particular fields depending on the profile type
        private void RadioButtonClick(object sender, EventArgs e)
        {
            string type = (FindViewById<RadioButton>(profileType.CheckedRadioButtonId)).Text;
            RadioButton rb = (RadioButton)sender;
            //If they are a student the first name, second name and university are visible and the compnany name is gone.
            if (type == "Student")
            {
                firstname.Visibility = ViewStates.Visible;
                secondname.Visibility = ViewStates.Visible;
                university.Visibility = ViewStates.Visible;
                companyname.Visibility = ViewStates.Gone;
            }
            //If they are a landlord the company name is visible and the first name, second name are gone.
            if (type == "Landlord")
            {
                companyname.Visibility = ViewStates.Visible;
                firstname.Visibility = ViewStates.Gone;
                secondname.Visibility = ViewStates.Gone;
                university.Visibility = ViewStates.Gone;
            }
        }
        public void OnClick(View v)
        {
        }
        //Sending the information to the database
        private async Task<string> FetchUserAsync(string url)
        {
            string success = "";
      
            string fname = firstname.Text;
            string sname = secondname.Text;
            string uni = university.Text;
            string user = username.Text;
            string pass = password.Text;
            string company_name = companyname.Text;
            string address1 = address_1.Text;
            string address2 = address_2.Text;
            string city = cityinput.Text;
            string postcode = postcodeinput.Text;
            string phone = phoneinput.Text;
            string email = emailinput.Text;
            string type = (FindViewById<RadioButton>(profileType.CheckedRadioButtonId)).Text;

            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["firstname"] = fname;
                data["lastname"] = sname;
                data["university"] = uni;
                data["username"] = user;
                data["password"] = pass;
                data["company_name"] = company_name;
                data["address1"] = address1;
                data["address2"] = address2;
                data["city"] = city;
                data["postcode"] = postcode;
                data["phone"] = phone;
                data["email"] = email;
                data["type"] = type;

                var response = webClient.UploadValues(url, "POST", data);
                Intent i = new Intent(this, typeof(Login));
                StartActivity(i);
                Finish();
            }
                return success;
        }
        //Sending the user a confirmaiton email
        private async Task<JsonValue> ConfirmationEmail(string url, string to, string message, string subject)
        {
            string success = "";
            using (var webClient = new WebClient())
            {
                var data = new NameValueCollection();
                data["to"] = to;
                data["subject"] = subject;
                data["message"] = message;
                var response = webClient.UploadValues(url, "POST", data);
            }
            return success;
        }
    }
}
