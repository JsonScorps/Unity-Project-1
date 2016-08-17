using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking.Types;

public class TileSpriteController : MonoBehaviour {

	//tile sprites
	public Sprite dirtSprite;
	public Sprite waterSprite;
	public Sprite emptySprite;

	Dictionary<Tile, GameObject> tileGameObjectMap;

	World world {
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {

		// instantiate Dictionary mapping Gameobject and tile data
		tileGameObjectMap = new Dictionary<Tile, GameObject>();

		// create a GameObject for each tile
		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {
				// Get the tile data
				Tile tile_data = world.GetTileAt(x, y);

				// create new GameObject and add to scene
				GameObject tile_go = new GameObject();

				// add tile data/go pair to Dictionary
				tileGameObjectMap.Add(tile_data, tile_go);

				tile_go.name = "Tile_" + x + "_" + y;
				tile_go.transform.SetParent (this.transform, true);
				tile_go.transform.position = new Vector3( tile_data.X, tile_data.Y, 0);

				// add sprite renderer
				SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer> ();

				//if (tile_data.Type == TileType.Dirt) {
				//	sr.sprite = dirtSprite;
				//	sr.sortingLayerName = "Tiles";
				//} else if (tile_data.Type == TileType.Water) {
				//	sr.sprite = waterSprite;
				//	sr.sortingLayerName = "Tiles";
				//} else {
					sr.sprite = emptySprite;
					sr.sortingLayerName = "Tiles";
				//}

				OnTileChanged(tile_data);
			}
		}

		//register callback to update Gameobject on tile change
		world.RegisterTileChanged( OnTileChanged );

	}
		
	// callback function on tile change
	void OnTileChanged(Tile tile_data ) {

		if (tileGameObjectMap.ContainsKey(tile_data) == false) {
			Debug.LogError ("tileGameObjectMap does not contain tile_data");
			return;
		}

		GameObject tile_go = tileGameObjectMap [tile_data];

		if (tile_go == null) {
			Debug.LogError ("tileGameObjectMap returned GameObject null");
			return;
		}


		if(tile_data.Type == TileType.Dirt) {
			tile_go.GetComponent<SpriteRenderer>().sprite = dirtSprite;
		}
		else if( tile_data.Type == TileType.Water ) {
			tile_go.GetComponent<SpriteRenderer>().sprite = waterSprite;
		}
		else if( tile_data.Type == TileType.Empty ) {
			tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
		}
		else {
			Debug.LogError("OnTileTypeChanged - Unrecognized tile type");
		}
	}
}
