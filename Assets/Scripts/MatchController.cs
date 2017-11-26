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

	public void SetUpMatch(string cardArray) {
		_interfaceController.GetMatchCanvas();
	}
}