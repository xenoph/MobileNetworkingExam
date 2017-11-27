using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class NetworkController : MonoBehaviour {

	public SocketIOComponent Socket;

	private InterfaceController _interfaceController;
	private MatchController _matchController;

	private string _socketID;
	private string _playerName;

	private string _opponentSocketID;
	private string _opponentPlayerName;

	private void Awake() {
		_interfaceController = GetComponent<InterfaceController>();
		_matchController = GetComponent<MatchController>();
	}

	private void Start(){
		Socket = GetComponent<SocketIOComponent>();
		SetSocketConnections();
	}

	public void Login(string name) {
		_playerName = name;
		RegisterUser();
		CheckForUsers();
	}

	/// <summary>
	/// Checks with the server if any other players are currently onlye
	/// </summary>
	public void CheckForUsers() {
		var json = CreateJSON();
		Socket.Emit("CheckCurrentUsers", json);
	}
	
	/// <summary>
	/// Sends request to match with another online player
	/// </summary>
	public void MatchWithOtherPlayer() {
		var json = CreateJSON();
		Socket.Emit("MatchPlayers", json);
	}

	/// <summary>
	/// Sets all the incoming connections from the server
	/// </summary>
	private void SetSocketConnections() {
		Socket.On("init", OnInit);
		Socket.On("registered", OnRegister);

		Socket.On("noMatch", OnNoMatch);
		Socket.On("matchedPlayer", OnMatchedPlayer);

		Socket.On("noUsers", OnNoUsersOnline);
		Socket.On("onlineUsers", OnUsersOnline);
	}

	/// <summary>
	/// Initial connection from server.
	/// </summary>
	/// <param name="obj"></param>
	private void OnInit(SocketIOEvent obj) {
		Debug.Log("init");
	}

	/// <summary>
	/// "Registers" the users by saving the player name and sending a notification to the server
	/// </summary>
	private void RegisterUser() {
		var json = new JSONObject();
		json.AddField("playerName", _playerName);
		Socket.Emit("PlayerLogin", json);
	}

	/// <summary>
	/// Return connection from server after registration to get socket id.
	/// </summary>
	/// <param name="obj"></param>
	private void OnRegister(SocketIOEvent obj) {
		_socketID = obj.data["socketid"].str;
	}

	/// <summary>
	/// When a matching failed for any reason
	/// </summary>
	/// <param name="obj"></param>
	private void OnNoMatch(SocketIOEvent obj) {
		_interfaceController.MatchingFailed();
	}

	/// <summary>
	/// When the server has no other users online
	/// </summary>
	/// <param name="obj"></param>
	private void OnNoUsersOnline(SocketIOEvent obj) {
		_interfaceController.SetNoPlayersAvailable();
	}

	/// <summary>
	/// When the server has other users online
	/// </summary>
	/// <param name="obj"></param>
	private void OnUsersOnline(SocketIOEvent obj) {
		_interfaceController.SetPlayersAvailable();
	}

	/// <summary>
	/// When a match happens. If the local player is not the one requesting,
	/// it should check if they're in lobby or not.
	/// </summary>
	/// <param name="obj"></param>
	private void OnMatchedPlayer(SocketIOEvent obj) {
		if(!_interfaceController.LobbyCanvas.activeSelf) {
			_interfaceController.MatchingFailed();
			return;
		}

		var numList = new List<float>();
		Debug.Log(obj.data["matchArray"].Count);
		for(int i = 0; i < obj.data["matchArray"].Count; i++) {
			numList.Add(obj.data["matchArray"][i].n);
		}
		Debug.Log("iterated numbers");
		_opponentSocketID = obj.data["opponentID"].str;
		_matchController.SetUpMatch(numList);
	}

	/// <summary>
	/// Creates a JSONObject with the players socketid.
	/// </summary>
	/// <returns></returns>
	private JSONObject CreateJSON() {
		var json = new JSONObject();
		json.AddField("SocketID", _socketID);
		return json;
	}
}