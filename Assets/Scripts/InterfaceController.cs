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

	public void Login() {
		if(!ValidateName(NameInput.text)) { return; }
		SetLobbyHeadlineText("Welcome " + NameInput.text);
		ToggleCanvas(LobbyCanvas);
		_netController.Login(NameInput.text);
	}

	public void StartMatch() {
		_netController.MatchWithOtherPlayer();
	}

	public void SetNoPlayersAvailable() {
		SetMatchButton("Refresh", RefreshPlayerList);
		SetLobbyHeadlineText("No available users");
	}

	public void SetPlayersAvailable() {
		Debug.Log("Refreshing player list...");
	}

	public void RefreshPlayerList() {
		_netController.CheckForUsers();
	}

	private void SetMatchButton(string txt, Action act) {
		FindMatchButton.GetComponentInChildren<Text>().text = txt;
		FindMatchButton.onClick.RemoveAllListeners();
		FindMatchButton.onClick.AddListener(delegate { act(); });
	}

	private void ToggleCanvas(GameObject canvas) {
		var allCanvas = new GameObject[] { LoginCanvas, LobbyCanvas, MatchCanvas };
		foreach(var can in allCanvas) {
			can.SetActive(false);
		}
		canvas.SetActive(true);
	}

	private void SetLobbyHeadlineText(string txt = "") {
		LobbyHeadlineText.text = txt;
	}

	private void SetErrorText(string txt = "") {
		ErrorText.text = txt;
	}

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