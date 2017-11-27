using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour {

	private NetworkController _netController;
	private InterfaceController _interfaceController;

	private void Awake() {
		_netController = GetComponent<NetworkController>();
		_interfaceController = GetComponent<InterfaceController>();
	}

	public void SetUpMatch(List<float> cardList) {
		_interfaceController.GetMatchCanvas();
		IterateCardList(cardList);
	}

	private void IterateCardList(List<float> cList) {
		foreach(var card in cList) {
			Debug.Log(card);
		}
	}
}