using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchController : MonoBehaviour {

	public Sprite CardBack;
	public Sprite[] CardFronts;

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

	private bool _playerTurn;
	private int _oppTurn = 0;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}

	public void SetUpMatch(List<float> cardList, bool starting) {
		_playerTurn = starting;
		_interfaceController.GetMatchCanvas();
        IterateCardList(cardList);
		SpawnCards();
	}

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
		}
		SpawnCardFront(cardType, go);
	}

	public void OpponentDidNotMatch() {
		EndTurn();
		_playerTurn = true;
	}

	public void OpponentMatched() {
		LockCard(_card1);
		LockCard(_card2);
		_playerTurn = true;
	}

	public void OpponentResigned() {
		EndMatch();
	}

	public void Resign() {
		_netController.SendResignation();
		EndMatch();
	}

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

	private void SpawnCards() {
		var locUsed = 0;
		foreach(var card in _cardList) {
			var img = _interfaceController.CardSpawnLocations[locUsed];
			img.sprite = CardBack;
			img.name = card.ToString() + locUsed.ToString();
			locUsed++;
		}
	}

	private void SpawnCardFront(char card, GameObject img) {
		var cardImg = CardFronts.Where(c => c.name == card.ToString()).FirstOrDefault();
		img.GetComponent<Image>().sprite = cardImg;
	}

	private void SpawnCardBack(GameObject card) {
		card.GetComponent<Image>().sprite = CardBack;
	}

	private void CheckForMatch() {
		if(_card1Type == _card2Type) {
			MadeMatch();
		} else {
			_netController.SendNoCardMatch();
			EndTurn();
		}
	}

	private void SendMove(string placement) {
		_netController.SendMove(placement);
	}

	private void MadeMatch() {
		LockCard(_card1);
		LockCard(_card2);
		_netController.SendCardMatch();
	}

	private void LockCard(GameObject card) {
		card.GetComponent<Button>().interactable = false;
	}

	private void EndTurn() {
		SpawnCardBack(_card1);
		SpawnCardBack(_card2);
		_turnedCards = 0;
	}

	private void EndMatch() {
		_cardList.Clear();
		_interfaceController.CloseMatch();
	}

	private void IterateCardList(List<float> cList) {
		_cardList = new List<float>();
		foreach(var card in cList) {
			_cardList.Add(card);
		}
	}
}