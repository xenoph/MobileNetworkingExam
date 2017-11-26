using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceController : MonoBehaviour {

	public GameObject LobbyCanvas;
	public GameObject MatchCanvas;
	public GameObject LoginCanvas;

	public Button FindMatchButton;
	public Button LoginButton;

	public InputField NameInput;

	public Text ErrorText;
	public Text LobbyHeadlineText;

	private NetworkController _netController;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
	}

	private void Start () {
		SetErrorText();
		SetLobbyHeadlineText();
		ToggleCanvas(LoginCanvas);
	}

	/// <summary>
	/// Should make sure name is valid before welcoming player into the lobby canvas.
	/// </summary>
	public void Login() {
		if(!ValidateName(NameInput.text)) { return; }
		SetLobbyHeadlineText("Welcome " + NameInput.text);
		ToggleCanvas(LobbyCanvas);
		_netController.Login(NameInput.text);
	}

	/// <summary>
	/// Starts a match with another online player
	/// </summary>
	public void StartMatch() {
		_netController.MatchWithOtherPlayer();
		ToggleCanvas(MatchCanvas);
	}

	/// <summary>
	/// Inform the player that there are no other players and give the player 
	/// an option to refresh.
	/// (Add timer to auto-refresh?)
	/// </summary>
	public void SetNoPlayersAvailable() {
		SetMatchButton("Refresh", RefreshPlayerList);
		SetLobbyHeadlineText("No available users");
	}

	/// <summary>
	/// Change lobby to give player the possibility to start a match
	/// </summary>
	public void SetPlayersAvailable() {
		SetMatchButton("Start Match", StartMatch);
	}

	/// <summary>
	/// Sends a query to the server to check if any players have come online
	/// </summary>
	public void RefreshPlayerList() {
		_netController.CheckForUsers();
	}

	/// <summary>
	/// Sets the lobby button text and changes listener to the given Action
	/// </summary>
	/// <param name="txt"></param>
	/// <param name="act"></param>	
	private void SetMatchButton(string txt, Action act) {
		FindMatchButton.GetComponentInChildren<Text>().text = txt;
		FindMatchButton.onClick.RemoveAllListeners();
		FindMatchButton.onClick.AddListener(delegate { act(); });
	}

	/// <summary>
	/// Toggles the canvases to the given one by first deactivating all,
	/// then activating the given one
	/// </summary>
	/// <param name="canvas"></param>
	private void ToggleCanvas(GameObject canvas) {
		var allCanvas = new GameObject[] { LoginCanvas, LobbyCanvas, MatchCanvas };
		foreach(var can in allCanvas) {
			can.SetActive(false);
		}
		canvas.SetActive(true);
	}

	/// <summary>
	/// Sets the headline text in lobby. If no parameter given, sets it to nothing
	/// </summary>
	/// <param name="txt"></param>
	private void SetLobbyHeadlineText(string txt = "") {
		LobbyHeadlineText.text = txt;
	}

	/// <summary>
	/// Sets the error text in login. If no parameter given, sets it to nothing
	/// </summary>
	/// <param name="txt"></param>
	private void SetErrorText(string txt = "") {
		ErrorText.text = txt;
	}

	/// <summary>
	/// Validates that the name input is not empty or above 8 letters.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	private bool ValidateName(string name) {
		if(name.Length > 8) {
			SetErrorText("Name too long!");
			return false;
		} else if(name == "") {
			SetErrorText("You didn't input a name");
			return false;
		}

		return true;
	}
}