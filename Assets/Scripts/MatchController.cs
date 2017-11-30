using System.Collections;
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
	private float countdown = 10f;

	private NetworkController _netController;
	private InterfaceController _interfaceController;

	private List<float> _cardList;

	private int _turnedCards;

	private GameObject _card1;
	private char _card1Type;
	private char _card1Placement;

	private GameObject _card2;
	private char _card2Type;
	private char _card2Placement;

	private bool _opponentTurn;
	private bool _playerTurn;
	private int _oppTurn = 0;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}
	
	private void Update()
	{
		if(_playerTurn == true || _opponentTurn == true){
			countdown -= Time.deltaTime;
			Timer1.text = "" + Mathf.Round(countdown);
			Timer2.text = "" + Mathf.Round(countdown);
			if(countdown <= 0){
				if(_playerTurn){
					Debug.Log("timed out");
					_netController.TimedOut();
				}
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
		_interfaceController.GetMatchCanvas();
        IterateCardList(cardList);
		SpawnCards();
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
			_card1Placement = placement.ToString()[0];
		} else {
			_card2 = go;
			_card2Type = cardType;
			_card2Placement = placement.ToString()[0];
			_oppTurn = 0;
		}
		SpawnCardFront(cardType, go);
	}

	public void TurnSwitch()
	{
		if(_playerTurn == true){
			WhoTurn.text = "T U R N   A R O U N D";
			_playerTurn = false;
			_opponentTurn = true;
		} else {
			WhoTurn.text = "Y O U R   T U R N";
			_playerTurn = true;
			_opponentTurn = false;
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
	/// When the opponent matched their cards
	/// </summary>
	public void OpponentMatched() {
		LockCard(_card1);
		LockCard(_card2);
		TurnSwitch();
	}

	/// <summary>
	/// When the opponent timed out
	/// </summary>
	public void OpponentTimedOut() {
		EndTurn();
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
		if(!_playerTurn) { return; }
		var go = EventSystem.current.currentSelectedGameObject;
		var cardType = go.name[0];
		var cardPlacement = go.name[1];
		if(_turnedCards == 0) {
			_card1Type = cardType;
			_card1Placement = cardPlacement;
			_card1 = go;
			_turnedCards++;
		} else {
			_playerTurn = false;
			_card2Type = cardType;
			_card2Placement = cardPlacement;
			_card2 = go;
			Invoke("CheckForMatch", 1f);
		}

		SpawnCardFront(cardType, go);
		SendMove(cardPlacement.ToString());
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
		} else {
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
		LockCard(_card1);
		LockCard(_card2);
		ResetCardInfo();
		_netController.SendCardMatch();
		_turnedCards = 0;
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
		if(_card1 != null) {
			SpawnCardBack(_card1);
			SpawnCardBack(_card2);
		}
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
		ResetCardInfo();
		_cardList.Clear();
		_interfaceController.CloseMatch();
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