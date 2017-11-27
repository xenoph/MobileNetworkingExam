using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour {

	private NetworkController _netController;
	private InterfaceController _interfaceController;

	private List<float> _cardList;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}

	public void SetUpMatch(List<float> cardList) {
		_interfaceController.GetMatchCanvas();
		IterateCardList(cardList);
	}

	private void IterateCardList(List<float> cList) {
		_cardList = new List<float>();
		foreach(var card in cList) {
			_cardList.Add(card);
		}
	}
}