using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

	private NetworkController _nController;

	private void Start() {
		_nController = GetComponent<NetworkController>();
	}
	
	void Update () {
	}
}