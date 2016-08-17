using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking.Types;

public class FurnitureSpriteController : MonoBehaviour {

	Dictionary<Furniture, GameObject> furnitureGameObjectMap;
	Dictionary<string, Sprite> furnitureSprites;

	World world {
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {

		loadSprites ();

		furnitureGameObjectMap = new Dictionary<Furniture, GameObject> ();

		//register callback to update Gameobject on furniture change
		world.RegisterFurnitureCreated (OnFurnitureCreated);

		foreach (Furniture furn in world.furnitures) {
			OnFurnitureCreated (furn);
		}

	}

	void loadSprites () {
		furnitureSprites = new Dictionary<string, Sprite> ();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furniture/");

		foreach(Sprite s in sprites) {
			//Debug.Log (s);
			furnitureSprites [s.name] = s;
		}
	}

	public void OnFurnitureCreated(Furniture furn) {
		// create new GameObject and add to scene
		GameObject furn_go = new GameObject();


		if (furn.furnitureType == "Door") {
			//default wall graphic is horizontal
			//check for verticality

			Tile n = world.GetTileAt (furn.tile.X, furn.tile.Y + 1);
			Tile s = world.GetTileAt (furn.tile.X, furn.tile.Y - 1);

			if (n != null && s != null && n.furniture != null && s.furniture != null &&
				n.furniture.furnitureType == "Wall" && s.furniture.furnitureType == "Wall") {

				furn_go.transform.rotation = Quaternion.Euler (0, 0, 90);
			}
		}



		// add object data/go pair to Dictionary
		furnitureGameObjectMap.Add( furn, furn_go);

		furn_go.name = furn.furnitureType + "_" + furn.tile.X + "_" + furn.tile.Y;
		furn_go.transform.SetParent (this.transform, true);
		furn_go.transform.position = new Vector3( furn.tile.X, furn.tile.Y, 0);

		// add sprite renderer
		SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
		sr.sprite = GetSpriteForFurniture (furn);
		sr.sortingLayerName = "Furniture";

		//register callback to update Gameobject on  object type change
		furn.RegisterOnChangedCallback( OnFurnitureChanged );
	}

	void OnFurnitureChanged( Furniture furn) {
		//update furniture graphics
		if(furnitureGameObjectMap.ContainsKey(furn) == false) {
			Debug.LogError ("OnFurnitureChanged -- can't change visuals");
			return;
		}

		GameObject furn_go = furnitureGameObjectMap[furn];
		furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture (furn);

	}

	public Sprite GetSpriteForFurniture( Furniture furn) {
		string spriteName = furn.furnitureType;

		if (furn.linksToNeighbor == false) {

			if (furn.furnitureType == "Door") {
				if (furn.GetParameter ("openness") < 0.1f) {
					spriteName = "Door";
				}
				else if (furn.GetParameter ("openness") < 0.5f) {
					spriteName = "Door_openness_1";
				}
				else if (furn.GetParameter ("openness") < 0.9f) {
					spriteName = "Door_openness_2";
				}
				else {
					spriteName = "Door_openness_3";
				}
			}


			return furnitureSprites [spriteName];
		} else {

			spriteName = furn.furnitureType + "_";

			// North, East, South, West
			int x = furn.tile.X;
			int y = furn.tile.Y;
			Tile t;

			t = world.GetTileAt(x, y + 1);
			if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType) {
				spriteName += "N";
			}
			t = world.GetTileAt(x + 1, y);
			if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType) {
				spriteName += "E";
			}
			t = world.GetTileAt(x, y - 1);
			if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType) {
				spriteName += "S";
			}
			t = world.GetTileAt(x - 1, y);
			if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType) {
				spriteName += "W";
			}


			if (furnitureSprites.ContainsKey (spriteName) == false) {
				Debug.LogError ("GetSpriteForInstalledObject -- No Sprite "+ spriteName +" found");
				return null;
			}

			return furnitureSprites [spriteName];
		}


	}

	public Sprite GetSpriteForFurniture( string objectType) {
		if (furnitureSprites.ContainsKey (objectType) ) {
			return furnitureSprites[objectType];
		}
		if (furnitureSprites.ContainsKey (objectType+"_") ) {
			return furnitureSprites[objectType+"_"];
		}
		
		Debug.LogError ("GetSpriteForInstalledObject -- No Sprite "+ objectType +" found");
		return null;
	}

}
