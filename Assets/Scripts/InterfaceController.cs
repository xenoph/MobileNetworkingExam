using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterfaceController : MonoBehaviour {

	public GameObject LobbyCanvas;
	public GameObject MatchCanvas;
	public GameObject LoginCanvas;
	public GameObject MainMenuCanvas;
	public GameObject WinningCanvas;
	public GameObject LoadingCanvas;
	public GameObject DiscCanvas;

	public Button FindMatchButton;
	public Button LoginButton;

	public InputField NameInput;

	public Text ErrorText;
	public Text LobbyHeadlineText;

	public Image[] CardSpawnLocations;

	private NetworkController _netController;
	private MatchController _matchController;


	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_matchController = GetComponent<MatchController>();
	}

	private void Start () {
		SetErrorText();
		SetLobbyHeadlineText();
		ToggleCanvas(MainMenuCanvas);
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
	public void MatchWithPlayer() {
		_netController.MatchWithOtherPlayer();
	}

	/// <summary>
	/// Toggles loading canvas before starting the actual match
	/// <summary>
	public void GetMatchCanvas() {
		ToggleCanvas(LoadingCanvas);
		StartCoroutine(DelayToggleScreen(3, MatchCanvas, _matchController.StartCountdown));
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
		SetLobbyHeadlineText("Other Players Online");
		SetMatchButton("Start Match", MatchWithPlayer);
	}

	/// <summary>
	/// Sends a query to the server to check if any players have come online
	/// </summary>
	public void RefreshPlayerList() {
		_netController.CheckForUsers();
	}

	/// <summary>
	/// Resets button and inform the player if the matching failed!
	/// </summary>
	public void MatchingFailed() {
		SetLobbyHeadlineText("Matching failed!");
		SetMatchButton("Refresh", RefreshPlayerList);
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

	public void WinningScreen() {
		ToggleCanvas(WinningCanvas);
	}

	public void LoadUp() {
		ToggleCanvas(LoginCanvas);
	}

	public void MainMenu() {
		ToggleCanvas(MainMenuCanvas);
	}
	
	/// <summary>
	/// When the opponent has disconnected.
	/// </summary>
	public void PlayerDisconnected() {
		ToggleCanvas(DiscCanvas);
		StartCoroutine(DelayToggleScreen(3, LobbyCanvas));
	}

	/// <summary>
	/// Resets the scene by reloading it.
	/// </summary>	
	public void SceneReset(){
		Scene loadedLevel = SceneManager.GetActiveScene ();
    	SceneManager.LoadScene (loadedLevel.buildIndex);
	}

	
	/// <summary>
	/// Toggles the canvases to the given one by first deactivating all,
	/// then activating the given one
	/// </summary>
	/// <param name="canvas"></param>
	private void ToggleCanvas(GameObject canvas) {
		var allCanvas = new GameObject[] { LoginCanvas, LobbyCanvas, MatchCanvas, MainMenuCanvas, WinningCanvas, LoadingCanvas, DiscCanvas };
		foreach(var can in allCanvas) {
			can.SetActive(false);
		}
		canvas.SetActive(true);
	}

	private IEnumerator DelayToggleScreen(int delay, GameObject screen, Action act = null) {
		yield return new WaitForSeconds(delay);
		ToggleCanvas(screen);
		if(act != null) {
			act();
		}
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