using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class NetworkController : MonoBehaviour {

	public SocketIOComponent Socket;

	private InterfaceController _interfaceController;

	private string _socketID;
	private string _playerName;

	private void Awake() {
		_interfaceController = GetComponent<InterfaceController>();
	}

	private void Start(){
		Socket = GetComponent<SocketIOComponent>();
		SetSocketConnections();
	}

	public void Login(string name) {
		_playerName = name;
		CheckForUsers();
	}

	public void CheckForUsers() {
		Debug.Log("Checking for users");
		var json = CreateJSON();
		Socket.Emit("CheckCurrentUsers", json);
	}
	
	public void MatchWithOtherPlayer() {
		var json = CreateJSON();
		Socket.Emit("MatchPlayers", json);
	}

	private void SetSocketConnections() {
		Socket.On("init", OnInit);
		Socket.On("registered", OnRegistered);
		Socket.On("allPlayers", OnGettingUsers);
		Socket.On("noMatch", OnNoMatch);
		Socket.On("matchedPlayer", OnMatchedPlayer);

		Socket.On("noUsers", OnNoUsersOnline);
		Socket.On("onlineUsers", OnUsersOnline);
	}

	private void OnInit(SocketIOEvent obj) {
		if(Socket != null) {
			var json = new JSONObject();
			Socket.Emit("PlayerLogin", json);
		}
	}

	private void OnRegistered(SocketIOEvent obj) {
		_socketID = obj.data["socketid"].str;
		Debug.Log(_socketID);
	}

	private void OnGettingUsers(SocketIOEvent obj) {
		Debug.Log(obj.data);
	}

	private void OnNoMatch(SocketIOEvent obj) {
	}

	private void OnNoUsersOnline(SocketIOEvent obj) {
		_interfaceController.SetNoPlayersAvailable();
	}

	private void OnUsersOnline(SocketIOEvent obj) {
		_interfaceController.SetPlayersAvailable();
	}

	private void OnMatchedPlayer(SocketIOEvent obj) {

	}

	private JSONObject CreateJSON() {
		var json = new JSONObject();
		json.AddField("SocketID", _socketID);
		return json;
	}
}