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

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}

	public void SetUpMatch(List<float> cardList) {
		_interfaceController.GetMatchCanvas();
        IterateCardList(cardList);
		SpawnCards();
	}

	public void OpponentMoved() {

	}

	public void OpponentResigned() {
		EndMatch();
	}

	public void Resign() {
		_netController.SendResignation();
		EndMatch();
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

	private void TurnCard() {
		var go = EventSystem.current.currentSelectedGameObject;
		var cardType = go.name[0];
		var cardPlacement = go.name[1];
		if(_turnedCards == 0) {
			_card1Type = cardType;
			_card1Placement = cardPlacement;
			_card1 = go;
			_turnedCards++;
		} else {
			_card2Type = cardType;
			_card2Placement = cardPlacement;
			_card2 = go;
			Invoke("CheckForMatch", 1f);
		}

		SpawnCardFront(cardType, go);
	}

	private void SpawnCardFront(char card, GameObject img) {
		var cardImg = CardFronts.Where(c => c.name == card.ToString()).FirstOrDefault();
		img.GetComponent<Image>().sprite = cardImg;
	}

	private void CheckForMatch() {
		if(_card1Type == _card2Type) {
			MadeMatch();
		} else {
			EndTurn();
		}
	}

	private void MadeMatch() {

	}

	private void EndTurn() {

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