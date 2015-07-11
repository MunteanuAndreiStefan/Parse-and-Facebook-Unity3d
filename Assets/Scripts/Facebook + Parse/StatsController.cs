using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parse;
using System;
using Facebook;
using Facebook.MiniJSON;
using System.Linq;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class StatsController : MonoBehaviour {

	public GameObject loggedInGUI;
	public GameObject loggedOutGUI;
	public GameObject ProfilImageFacebookGameObject;
	public GameObject NameFacebookGameObject;
	public GameObject LocationFacebookGameObject;
	public GameObject GenderFacebookGameObject;
	public GameObject BirthdayFacebookGameObject;
	public GameObject NotLogInMessageGameObject;
	private Text NameFacebookComponentText;
	private Text LocationFacebookComponentText;
	private Text GenderFacebookComponentText;
	private Text BirthdayFacebookComponentText;
	

	// Use this for initialization
	void Start () {
		NameFacebookComponentText = NameFacebookGameObject.GetComponent<Text>();
		LocationFacebookComponentText = LocationFacebookGameObject.GetComponent<Text>();
		GenderFacebookComponentText = GenderFacebookGameObject.GetComponent<Text>();
		BirthdayFacebookComponentText = BirthdayFacebookGameObject.GetComponent<Text>();

		if (FB.IsLoggedIn) {
			showLoggedIn();
			if (ParseUser.CurrentUser == null) {
				StartCoroutine("ParseLogin");
			} else {
				UpdateProfile();
			}
		} else {
			showLoggedOut();
		}

	}

	void Awake()
	{
		DontDestroyOnLoad (transform.gameObject);
		enabled = false;
		FB.Init(SetInit, OnHideUnity);
	}

	private void SetInit()
	{
		enabled = true;
	}

	private void OnHideUnity(bool isGameShown) {
		if (!isGameShown)
		{

			Time.timeScale = 0;
		}
		else
		{

			Time.timeScale = 1;
		}
	}

	private IEnumerator ParseLogin() {
		if (FB.IsLoggedIn) {
			// Logging to Parse
			var loginTask = ParseFacebookUtils.LogInAsync(FB.UserId, 
			                                              FB.AccessToken, 
			                                              DateTime.Now);
			while (!loginTask.IsCompleted) yield return null;
			// Login completed
			if (loginTask.IsFaulted || loginTask.IsCanceled) {
				// There was an error logging to Parse
				foreach(var e in loginTask.Exception.InnerExceptions) {
					ParseException parseException = (ParseException) e;
					Debug.Log("ParseLogin: error message " + parseException.Message);
					Debug.Log("ParseLogin: error code: " + parseException.Code);
				}
			} else {
				FB.API("/me", HttpMethod.GET, FBAPICallback);
				UpdateProfile();
			}
		}
	}

	public void FBLogin() {
		FB.Login("user_about_me, user_birthday, user_location", FBLoginCallback);
	}

	private void FBLoginCallback(FBResult result) {
		if(FB.IsLoggedIn) {
			showLoggedIn();
			StartCoroutine("ParseLogin");
		} else {
			Debug.Log ("FBLoginCallback: User canceled login");
		}
	}

	public void ParseFBLogout() {
		FB.Logout();
		ParseUser.LogOutAsync();
		showLoggedOut();
	}

	private void FBAPICallback(FBResult result)
	{
		if (!String.IsNullOrEmpty(result.Error)) {
			Debug.Log ("FBAPICallback: Error getting user info: + "+ result.Error);
			ParseFBLogout();
		} else {
			var resultObject = Json.Deserialize(result.Text) as Dictionary<string, object>;
			var userProfile = new Dictionary<string, string>();
			
			userProfile["facebookId"] = getDataValueForKey(resultObject, "id");
			userProfile["name"] = getDataValueForKey(resultObject, "name");
			object location;
			if (resultObject.TryGetValue("location", out location)) {
				userProfile["location"] = (string)(((Dictionary<string, object>)location)["name"]);
			}
			userProfile["gender"] = getDataValueForKey(resultObject, "gender");
			userProfile["birthday"] = getDataValueForKey(resultObject, "birthday");
			if (userProfile["facebookId"] != "") {
				userProfile["pictureURL"] = "https://graph.facebook.com/" + userProfile["facebookId"] + "/picture?type=large&return_ssl_resources=1";
			}
			
			var emptyValueKeys = userProfile
				.Where(pair => String.IsNullOrEmpty(pair.Value))
					.Select(pair => pair.Key).ToList();
			foreach (var key in emptyValueKeys) {
				userProfile.Remove(key);
			}
			
			StartCoroutine("saveUserProfile", userProfile);
		}
	}

	private IEnumerator saveUserProfile(Dictionary<string, string> profile) {
		var user = ParseUser.CurrentUser;
		user["profile"] = profile;
		if (user.IsKeyDirty("profile")) {
			var saveTask = user.SaveAsync();
			while (!saveTask.IsCompleted) yield return null;
			UpdateProfile();
		}
	}

	private string getDataValueForKey(Dictionary<string, object> dict, string key) {
		object objectForKey;
		if (dict.TryGetValue(key, out objectForKey)) {
			return (string)objectForKey;
		} else {
			return "";
		}
	}

	private void UpdateProfile() {
		//cached info
		var user = ParseUser.CurrentUser;
		IDictionary<string, string> userProfile = user.Get<IDictionary<string, string>>("profile");
		NameFacebookComponentText.text = userProfile["name"];
		LocationFacebookComponentText.text = userProfile.ContainsKey("location") ? userProfile["location"] : "";
		GenderFacebookComponentText.text = userProfile.ContainsKey("gender") ? userProfile["gender"] : "";
		BirthdayFacebookComponentText.text = userProfile.ContainsKey("birthday") ? userProfile["birthday"] : "";
		FB.API (GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, GetTheProfilePhoto);
	}

	void GetTheProfilePhoto(FBResult result)
	{
		
		if(result.Error != null)
		{
			Debug.Log ("Problem with the picture get");
			
			FB.API (GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, GetTheProfilePhoto);
			return;
		}
		
		Image UserAvatar = ProfilImageFacebookGameObject.GetComponent<Image>();
		UserAvatar.sprite = Sprite.Create (result.Texture, new Rect(0,0,128,128), new Vector2(0,0));
		
	}
	
	public static string GetPictureURL(string facebookID, int? width = null, int? height = null, string type = null)
	{
		string url = string.Format("/{0}/picture", facebookID);
		string query = width != null ? "&width=" + width.ToString() : "";
		query += height != null ? "&height=" + height.ToString() : "";
		query += type != null ? "&type=" + type : "";
		if (query != "") url += ("?g" + query);
		return url;
	}

	private void showLoggedIn() {
		loggedOutGUI.SetActive (false);
		loggedInGUI.SetActive (true);
	}

	private void showLoggedOut() {
		loggedOutGUI.SetActive (true);
		loggedInGUI.SetActive (false);
	}


	public void AddDataUser(int win=0, int lose=0){
		string UserName=ParseUser.CurrentUser.Username;
		Debug.Log("Save data start");
			ParseObject dataObject = new ParseObject ("Stats");
			dataObject["user"]= UserName;
			dataObject["win"]= win;
			dataObject["lose"]= lose;
			dataObject.ACL = new ParseACL (ParseUser.CurrentUser);
		Debug.Log("All the data are in local storage.");
			System.Threading.Tasks.Task saveTask = 	dataObject.SaveAsync();
		Debug.Log("All the data saved into the Cloud");
	}

	public void AddDataUserForTestButton(){
		AddDataUser ();  // Unity bug ?
	}

	public void FindTheUserData(){
		string UserName=ParseUser.CurrentUser.Username;
		Debug.Log (UserName);
		var query = ParseObject.GetQuery ("Stats").WhereEqualTo("user", UserName);
		query.FirstAsync().ContinueWith(t => {
			ParseObject TheRow = t.Result;
			string content1 = TheRow.Get<string> ("user");
			int content2 = TheRow.Get<int> ("win");
			Debug.Log(content1+"         "+content2.ToString());
		});
	}

	public void FindAndDeleteDataUser(){
		string UserName=ParseUser.CurrentUser.Username;
		Debug.Log (UserName);
		Debug.Log("The data will be deleted");
		var query = ParseObject.GetQuery ("Stats")
			.WhereEqualTo("user", UserName);
		query.FirstAsync().ContinueWith(t =>{
			ParseObject TheRow = t.Result;
			TheRow.DeleteAsync();
			System.Threading.Tasks.Task saveTask = TheRow.SaveAsync();
			Debug.Log("The data was deleted");
		});
	}

	public void UpdateUserData(){
		FindAndDeleteDataUser ();
		AddDataUser ();
	}

	public void ShareWithFriends()
	{
		FB.Feed (
			linkCaption: "My name is Steven :)",
			picture: "https://scontent-fra3-1.xx.fbcdn.net/hphotos-xta1/v/t1.0-9/11041230_789344587822041_7573596991053821171_n.jpg?oh=0107d0b498f23da9f5d8ff640e6f21c3&oe=55E90E26",
			linkName: "Some text again..."
			);
	}
	
	public void InviteFriends()
	{
		FB.AppRequest(
			message: "Something about my awesome game",
			title: "Some message.."
			);
	}

	public void AddCharacterData(){
		FindAndDeleteCharacterData();
		Debug.Log("Save data start");
		ParseObject dataObject = new ParseObject ("Character");
		dataObject ["user"] = ParseUser.CurrentUser.Username;
		dataObject ["SomeData"] = "ABC";
		dataObject.ACL = new ParseACL (ParseUser.CurrentUser);
		Debug.Log("All the data are in local storage.");
		System.Threading.Tasks.Task saveTask = 	dataObject.SaveAsync();
		Debug.Log("All the data saved into the Cloud");
	}
	
	public void FindAndDeleteCharacterData(){   // I RECOMMEND YOU TO IMPLEMENT THIS FUNCTION ON BEFORE SAVE ON TE CLOUD CODE.
		string UserName=ParseUser.CurrentUser.Username;
		Debug.Log (UserName);
		Debug.Log("The data will be deleted");
		var query = ParseObject.GetQuery ("Character").WhereEqualTo("user", UserName);
		query.FirstAsync().ContinueWith(t =>{
			ParseObject TheRow = t.Result;
			TheRow.DeleteAsync();
			System.Threading.Tasks.Task saveTask = TheRow.SaveAsync();
			Debug.Log("The data was deleted");
		});
	}
	
	public void GetCharacterData(){
		string UserName=ParseUser.CurrentUser.Username;
		var query = ParseObject.GetQuery ("Character").WhereEqualTo ("user", UserName);
		query.FirstAsync().ContinueWith(t =>{
			ParseObject TheRow = t.Result;
			PutOnGameObject.MyKindOfThread.ExecuteOnMainThreadAtFirstUpdate(() => {
				PlayerPrefs.SetString("UserName",TheRow.Get<string>("user"));
				PlayerPrefs.SetString("Some",TheRow.Get<string>("SomeData")); // On windows you can find it: regedit -> Hkey_Current_user/Software/Company Name/Product Name
				Debug.Log(PlayerPrefs.GetString("UserName"));  // :)
			});
		});
	}

}
