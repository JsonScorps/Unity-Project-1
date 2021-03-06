﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TileInfo : MonoBehaviour {

	Text myText;
	MouseController mouseController;

	// Use this for initialization
	void Start () {
		myText = GetComponent<Text> ();

		if (myText == null) {
			Debug.LogError ("TileInfo -- No text component found");
			this.enabled = false;
			return;
		}

		mouseController = GameObject.FindObjectOfType<MouseController> ();
		if (mouseController == null) {
			Debug.LogError ("TileInfo -- No MouseController found");
			return;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Tile t = mouseController.GetMouseOverTile ();
		myText.text = "Tile Type: " + t.Type.ToString (); 
	}
}
