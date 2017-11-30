using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System.Linq;

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
		json.AddField("playerName", _playerName);
		Socket.Emit("MatchPlayers", json);
	}

	/// <summary>
	/// Sends the changes to the cardList to the server so it can be passed to the opponent.
	/// </summary>
	/// <param name="cardList"></param>
	public void SendMove(string placement) {
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		json.AddField("cardPlacement", placement);
		Socket.Emit("MadeMove", json);
	}

	/// <summary>
	/// Sends a resignation from the game to the server
	/// </summary>
	public void SendResignation() {
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("Resign", json);
	}

	/// <summary>
	/// If the player times out of their round.
	/// </summary>
	public void TimedOut() {
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("TimedOut", json);
	}

	/// <summary>
	/// Send a message that the player did not match cards
	/// </summary>
	public void SendNoCardMatch() {
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("NoCardMatch", json);
	}

	/// <summary>
	/// Send a message that the player matched two cards
	/// </summary>
	public void SendCardMatch() {
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("CardMatch", json);
	}

	/// <summary>
	/// Sets all the incoming connections from the server
	/// </summary>
	private void SetSocketConnections() {
		Socket.On("init", OnInit);
		Socket.On("registered", OnRegister);

		Socket.On("noUsers", OnNoUsersOnline);
		Socket.On("onlineUsers", OnUsersOnline);
		Socket.On("noMatch", OnNoMatch);
		Socket.On("matchedPlayer", OnMatchedPlayer);

		Socket.On("playerResigned", OnResign);
		Socket.On("playerLeft", OnPlayerLeave);
		Socket.On("movedone", OnReceivedMove);
		Socket.On("noCardMatch", OnNoCardMatch);
		Socket.On("cardMatch", OnCardMatch);

		Socket.On("timedOut", OnTimedOut);
	}

	/// <summary>
	/// Initial connection from server.
	/// </summary>
	/// <param name="obj"></param>
	private void OnInit(SocketIOEvent obj) {
		Debug.Log("init");
	}

	/// <summary>
	/// When the opponent resigns
	/// </summary>
	/// <param name="obj"></param>
	private void OnResign(SocketIOEvent obj) {
		_matchController.OpponentResigned();
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
		Invoke("CheckForUsers", 5f);
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
		_matchController.SetUpMatch(numList, obj.data["starting"].b);
	}

	/// <summary>
	/// When the opponent has made their move.
	/// </summary>
	/// <param name="obj"></param>
	private void OnReceivedMove(SocketIOEvent obj) {
		var placement = int.Parse(obj.data["cardPlacement"].str);
		_matchController.OpponentMoved(int.Parse(obj.data["cardPlacement"].str));
	}

	/// <summary>
	/// When the opponent failed to match two cards
	/// </summary>
	/// <param name="obj"></param>	
	private void OnNoCardMatch(SocketIOEvent obj) {
		_matchController.OpponentDidNotMatch();
	}

	/// <summary>
	/// Opponent made a match
	/// </summary>
	/// <param name="obj"></param>
	private void OnCardMatch(SocketIOEvent obj) {
		_matchController.OpponentMatched();
	}

	/// <summary>
	/// Opponent left game outside the resign button
	/// Using the same method as resign, but keeping a different server call in case of change
	/// </summary>
	/// <param name="obj"></param>	
	private void OnPlayerLeave(SocketIOEvent obj) {
		_matchController.OpponentResigned();
	}

	/// <summary>
	/// When the network controller is disabled we know that the player has quit the game
	/// Check if the match canvas is active before sending
	/// </summary>
	private void OnApplicationQuit() {
		if(!_interfaceController.MatchCanvas.activeSelf) { return; }
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("QuitGame", json);
	}

	/// <summary>
	/// When the opponent timed out.
	/// </summary>
	/// <param name="obj"></param>
	private void OnTimedOut(SocketIOEvent obj) {
		_matchController.OpponentResigned();
	}

	/// <summary>
	/// When the network controller is disabled we know that the player has quit the game
	/// Check if the match canvas is active before sending
	/// </summary>
	private void OnDisable() {
		Debug.Log("OnDisable");
		if(!_interfaceController.MatchCanvas.activeSelf) { return; }
		var json = CreateJSON();
		json.AddField("oppSocket", _opponentSocketID);
		Socket.Emit("QuitGame", json);
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