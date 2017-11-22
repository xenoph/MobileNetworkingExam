using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

	private NetworkController _nController;

	private void Start() {
		_nController = GetComponent<NetworkController>();
	}
	
	void Update () {
		if(Input.GetKeyUp(KeyCode.A)) {
			_nController.CheckForUsers();
		}
		if(Input.GetKeyUp(KeyCode.M)) {
			_nController.MatchWithOtherPlayer();
		}
	}
}