using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {

	public static WorldController Instance { get; protected set; }

	// The world and tile data
	public World world { get; protected set; }

	static bool loadWorld = false;

	// Use this for initialization
	void Awake () {

		if (Instance != null) {
			Debug.LogError ("Why the fuck are there more than one world controller ?");
		} 
		else {
			Instance = this;
		}

		if (loadWorld == true) {
			loadWorld = false;
			CreateWorldFromSave ();
		} else {
			CreateEmptyWorld ();
		}

		// randomizes Map
		//world.RandomizeTiles();
	}

	void Update() {
		//time speed control
		world.Update (Time.deltaTime);
	}
		
	//returns world coordinates of tile
	public Tile GetTileAtWorldCoord(Vector3 coord) {
		int x = Mathf.RoundToInt (coord.x);
		int y = Mathf.RoundToInt (coord.y);

		return WorldController.Instance.world.GetTileAt (x, y);
	}

	public void CreateNewWorld() {
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void SaveWorld() {
		Debug.Log ("SaveWorld");

		XmlSerializer serializard = new XmlSerializer (typeof(World));
		TextWriter writer = new StringWriter ();
		serializard.Serialize (writer, world);
		writer.Close ();

		Debug.Log (writer.ToString() );

		PlayerPrefs.SetString ("SaveGame00", writer.ToString() );
	}

	public void LoadWorld() {
		Debug.Log ("LoadWorld");

		loadWorld = true;
		//clear old references
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}
		
	void CreateEmptyWorld() {
		//create empty world
		world = new World(100, 100);

		//center camera
		Camera.main.transform.position = new Vector3 (world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
	}

	void CreateWorldFromSave() {
		Debug.Log ("CreateWorldFromSave");

		//create world from save file
		XmlSerializer serializard = new XmlSerializer (typeof(World));
		TextReader reader = new StringReader (PlayerPrefs.GetString("SaveGame00"));
		world = (World) serializard.Deserialize (reader);
		reader.Close ();


		//center camera
		Camera.main.transform.position = new Vector3 (world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
	}
}
