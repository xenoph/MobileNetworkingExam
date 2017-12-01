using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

	public Sprite CardBack;
	public Sprite[] CardFronts;
	public Text Timer1;
	public Text Timer2;
	public Text WhoTurn;
	public Text WinText1;
	public Text WinText2;

	public string PlayerName;
	public string OpponentName;

	private NetworkController _netController;
	private InterfaceController _interfaceController;

	private List<float> _cardList;

	private int _turnedCards;

	private GameObject _card1;
	private char _card1Type;

	private GameObject _card2;
	private char _card2Type;

	private bool _canClick;
	private bool _opponentTurn;
	private bool _playerTurn;
	private int _oppTurn = 0;

	private float countdown = 10f;
	private int OppMatches = 0;
	private int PlayMatches = 0;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}
	
	private void Update() {
		if(_playerTurn == true || _opponentTurn == true){
			countdown -= Time.deltaTime;
			Timer1.text = "" + Mathf.Round(countdown);
			Timer2.text = "" + Mathf.Round(countdown);
			if(countdown <= 0){
				EndTurn();
				TurnSwitch();
			}
		}
	}

	/// <summary>
	/// Set up the match canvas. The starting player is the one being challenged!
	/// This information should be given somewhere.
	/// </summary>
	/// <param name="cardList"></param>
	/// <param name="starting"></param>
	public void SetUpMatch(List<float> cardList, bool starting) {
		_playerTurn = starting;
		Invoke("SetNames", 1f);
		_interfaceController.GetMatchCanvas();
        IterateCardList(cardList);
		SpawnCards();
	}

	/// <summary>
	/// Starts the timer. Needs to be called from the interface controller when it unloads the loading screen
	/// </summary>
	public void StartCountdown() {
		if(_playerTurn) {
			_opponentTurn = false;
			_canClick = true;
		} else {
			WhoTurn.text = OpponentName.ToUpper() + " TURN";
			_opponentTurn = true;
		}
	}

	/// <summary>
	/// Simulate the opponent making a move by changing the card and storing the info
	/// </summary>
	/// <param name="placement"></param>
	public void OpponentMoved(int placement) {
		_oppTurn++;
		var go = _interfaceController.CardSpawnLocations[placement].gameObject;
		var cardType = go.name[0];
		if(_oppTurn == 1) {
			_card1 = go;
			_card1Type = cardType;
		} else {
			_card2 = go;
			_card2Type = cardType;
			_oppTurn = 0;
		}
		SpawnCardFront(cardType, go);
	}

	/// <summary>
	/// Switches the turn locally.
	/// </summary>
	public void TurnSwitch() {
		if(_playerTurn == true) {
			WhoTurn.text = OpponentName.ToUpper() + " TURN";
			_playerTurn = false;
			_opponentTurn = true;
		} else {
			WhoTurn.text = PlayerName.ToUpper() + " TURN";
			_playerTurn = true;
			_opponentTurn = false;
			_canClick = true;
		}
		countdown = 10f;
	}

	/// <summary>
	/// When the opponent did not match their cards
	/// </summary>
	public void OpponentDidNotMatch() {
		EndTurn();
		TurnSwitch();
	}

	/// <summary>
	/// If the opponent player disconnects, reset the cards and notify the player
	/// </summary>	
	public void OpponentDisconnected() {
		ResetCardInfo();
		_interfaceController.PlayerDisconnected();
	}

	/// <summary>
	/// When the opponent matched their cards
	/// </summary>
	public void OpponentMatched() {
		LockCard(_card1);
		LockCard(_card2);
		if(checkMatchNumber(false)) { EndMatch(); }
		else { TurnSwitch(); }
	}

	/// <summary>
	/// When the opponent resign - right now, just end match!
	/// </summary>
	public void OpponentResigned() {
		EndMatch();
	}

	/// <summary>
	/// Resign from the current match
	/// </summary>	
	public void Resign() {
		_netController.SendResignation();
		EndMatch();
	}

	/// <summary>
	/// Turn the card over and save the details of what it is - which is gotten from the name
	/// If it's turn 2, check for a match and end round
	/// </summary>
	public void TurnCard() {
		if(!_canClick) { return; }
		var go = EventSystem.current.currentSelectedGameObject;
		var cardType = go.name[0];
		var cardPlacement = go.name[1];
		if(_turnedCards == 0) {
			_card1Type = cardType;
			_card1 = go;
			_turnedCards++;
		} else {
			_canClick = false;
			_card2Type = cardType;
			_card2 = go;
			Invoke("CheckForMatch", 1f);
			
		}

		SpawnCardFront(cardType, go);
		SendMove(cardPlacement.ToString());
	}

	/// <summary>
	/// Checks the amount of matches made. Should give true if it has hit four, which is the max
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	private bool checkMatchNumber(bool player) {
		if(player) {
			PlayMatches++;
		} else { 
			OppMatches++; 
		}
		if(PlayMatches + OppMatches == 4)
			return true;

		return false;
	}

	/// <summary>
	/// Sets the player names in the headline
	/// </summary>	
	private void SetNames() {
		if(_playerTurn) {
			WhoTurn.text = PlayerName.ToUpper() + " TURN";
		} else {
			WhoTurn.text = OpponentName.ToUpper() + " TURN";
		}
	}

	/// <summary>
	/// "Spawn" in the cards by setting every img sprite to the cardback sprite.
	/// </summary>	
	private void SpawnCards() {
		var locUsed = 0;
		foreach(var card in _cardList) {
			var img = _interfaceController.CardSpawnLocations[locUsed];
			img.sprite = CardBack;
			img.name = card.ToString() + locUsed.ToString();
			locUsed++;
			img.gameObject.GetComponent<Button>().interactable = true;
		}
	}

	/// <summary>
	/// Changes the card to the front that corresponds with the card name (which is just a number)
	/// </summary>
	/// <param name="card"></param>
	/// <param name="img"></param>
	private void SpawnCardFront(char card, GameObject img) {
		var cardImg = CardFronts.Where(c => c.name == card.ToString()).FirstOrDefault();
		img.GetComponent<Image>().sprite = cardImg;
	}

	/// <summary>
	/// Spawn a single cardback in. Used after a match was not found.
	/// </summary>
	/// <param name="card"></param>
	private void SpawnCardBack(GameObject card) {
		card.GetComponent<Image>().sprite = CardBack;
	}

	/// <summary>
	/// Checks if the two turned cards match by comparing type
	/// </summary>
	private void CheckForMatch() {
		if(_card1Type == _card2Type) {
			MadeMatch();
			if(checkMatchNumber(true)) { EndMatch(); }
			else { TurnSwitch(); }
		} else {
			TurnSwitch();
			_netController.SendNoCardMatch();
			EndTurn();
		}
	}

	/// <summary>
	/// Send the move (turned card placement in the list) to the server
	/// </summary>
	/// <param name="placement"></param>
	private void SendMove(string placement) {
		_netController.SendMove(placement);
	}

	/// <summary>
	/// When a match was made. Should lock cards, reset info and notify server
	/// </summary>
	private void MadeMatch() {
		_turnedCards = 0;
		LockCard(_card1);
		LockCard(_card2);
		ResetCardInfo();
		_netController.SendCardMatch();
	}

	/// <summary>
	/// "Locks" the card by disabling interaction with the button.
	/// </summary>
	/// <param name="card"></param>
	private void LockCard(GameObject card) {
		card.GetComponent<Button>().interactable = false;
	}

	/// <summary>
	/// Ends a turn by spawning in cardbacks and reset info.
	/// </summary>
	private void EndTurn() {
		if(_card1 != null) { SpawnCardBack(_card1); }
		if(_card2 != null) { SpawnCardBack(_card2); }
		ResetCardInfo();
		_turnedCards = 0;
	}

	/// <summary>
	/// We reset the cards by nulling the gameobject and setting the types to two different chars
	/// </summary>
	private void ResetCardInfo() {
		_card1 = null;
		_card2 = null;
		_card1Type = '@';
		_card2Type = '!';
	}

	/// <summary>
	/// Ends the match
	/// </summary>
	private void EndMatch() {
		_interfaceController.WinningScreen();
		if(OppMatches > PlayMatches) {
				WinText1.text = ("Loser");
				WinText2.text = ("Loser");
			} else if(PlayMatches > OppMatches) {
				WinText1.text = ("Winner");
				WinText2.text = ("Winner");
			} else {
				WinText1.text = ("Tie");
				WinText2.text = ("Tie");
			}

		ResetCardInfo();
		_cardList.Clear();
	}

	/// <summary>
	/// Iterates through the card list and adds it to a ...list? 
	/// Not sure what the plan was here. Leaving it in case it comes back
	/// </summary>
	/// <param name="cList"></param>	
	private void IterateCardList(List<float> cList) {
		_cardList = new List<float>();
		foreach(var card in cList) {
			_cardList.Add(card);
		}
	}
}