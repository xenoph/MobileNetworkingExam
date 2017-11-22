using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class NetworkController : MonoBehaviour {

	public SocketIOComponent Socket;

	private string _socketID;
	//private void _playerID; maybe?

	private void Start(){
		Socket = GetComponent<SocketIOComponent>();
		SetSocketConnections();
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
		Debug.Log("Did not find a match");
	}

	private void OnMatchedPlayer(SocketIOEvent obj) {
		Debug.Log("Found a match");
	}

	private JSONObject CreateJSON() {
		var json = new JSONObject();
		json.AddField("SocketID", _socketID);
		return json;
	}
}