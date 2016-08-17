using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

	public GameObject circleCursorPrefab;

	//mouse position in the world
	Vector3 lastFramePosition;
	Vector3 currFramePosition;

	//start position of left click drag
	Vector3 dragStartPosition;

	//list of preview objects
	List<GameObject> dragPreviewGameObjects;

	// Use this for initialization
	void Start () {
		dragPreviewGameObjects = new List<GameObject>();
	}

	/// <summary>
	/// Gets the mouse position in world space.
	/// </summary>
	public Vector3 GetMousePosition() {
		return currFramePosition;
	}

	public Tile GetMouseOverTile() {
		return WorldController.Instance.world.GetTileAt (
			Mathf.RoundToInt(currFramePosition.x),
			Mathf.RoundToInt(currFramePosition.y)
		);
	}

	// Update is called once per frame
	void Update () {

		currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		currFramePosition.z = 0;

		//UpdateCursor ();
		UpdateDragging ();
		UpdateCameraMovement ();

		lastFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		lastFramePosition.z = 0;
	}
		
	//click and drag
	void UpdateDragging () {

		//disable over UI
		if (EventSystem.current.IsPointerOverGameObject ()) {
			return;
		}

		//start drag
		if (Input.GetMouseButtonDown (0)) {
			dragStartPosition = currFramePosition;
		}

		int start_x = 	Mathf.RoundToInt(dragStartPosition.x);
		int end_x = 	Mathf.RoundToInt(currFramePosition.x);
		int start_y = 	Mathf.RoundToInt(dragStartPosition.y);
		int end_y = 	Mathf.RoundToInt(currFramePosition.y);
		//in case direction is changed
		if (end_x < start_x) {
			int tmp = end_x;
			end_x = start_x;
			start_x = tmp;
		}

		if (end_y < start_y) {
			int tmp = end_y;
			end_y = start_y;
			start_y = tmp;
		}

		//delete unnecessary previews
		dragPreviewGameObjects.ForEach(x => SimplePool.Despawn(x));
		dragPreviewGameObjects.Clear();﻿

		//drag area preview
		if (Input.GetMouseButton (0)) {
			//loop through all tiles
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.GetTileAt (x, y);
					if (t != null) {
						//display preview
						GameObject go = (GameObject)SimplePool.Spawn(circleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
						go.transform.SetParent (this.transform, true);
						dragPreviewGameObjects.Add (go);
					}
				}
			}
		}

		//end drag
		if (Input.GetMouseButtonUp (0)) {

			BuildController bc = GameObject.FindObjectOfType<BuildController> ();

			//loop through all tiles
			for (int x = start_x; x <= end_x; x++) {
				for (int y = start_y; y <= end_y; y++) {
					Tile t = WorldController.Instance.world.GetTileAt (x, y);
					if (t != null) {
						//call BuildControll.DoBuild
						bc.DoBuild(t);
					}
				}
			}
		}
	}
		
	//view paning
	void UpdateCameraMovement() {
		//right or midlle mouse button
		if (Input.GetMouseButton (1) || Input.GetMouseButton (2)) { 
			
			Vector3 diff = lastFramePosition - currFramePosition;
			Camera.main.transform.Translate (diff);
		}

		Camera.main.orthographicSize -= Input.GetAxis ("Mouse ScrollWheel") * Camera.main.orthographicSize;
	
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 3f, 25f);
	}

}
