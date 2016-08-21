using UnityEngine;
using System.Collections.Generic;

public class InventorySpriteController : MonoBehaviour {

	Dictionary<Inventory, GameObject> inventoryGameObjectMap;
	Dictionary<string, Sprite> inventorySprites;
	
	World world {
			get { return WorldController.Instance.world; }
	}


	// Use this for initialization
	void Start () {
		loadSprites ();
		
		inventoryGameObjectMap = new Dictionary<Inventory, GameObject> ();
		
		//register callback to update Gameobject
		world.RegisterInventoryCreated (OnInventoryCreated);

		foreach (string invType in world.inventoryManager.inventories.Keys) {
			foreach (Inventory inv in world.inventoryManager.inventories[invType]) {
				OnInventoryCreated (inv);
			}
		}

		//c.SetDestination (world.GetTileAt (world.Width/2 + 5, world.Height/2) );

	}
	
	void loadSprites () {
		inventorySprites = new Dictionary<string, Sprite> ();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Inventory/");
			
		foreach(Sprite s in sprites) {
	 		//Debug.Log (s);
			inventorySprites [s.name] = s;
		}
	}

	public void OnInventoryCreated(Inventory inv) {
		// create new GameObject and add to scene
		GameObject inv_go = new GameObject();

		// add object data/go pair to Dictionary
		inventoryGameObjectMap.Add( inv, inv_go);

		inv_go.name = inv.inventoryType;
		inv_go.transform.SetParent (this.transform, true);
		inv_go.transform.position = new Vector3( inv.tile.X, inv.tile.Y, 0);

		// add sprite renderer
		SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
		sr.sprite= inventorySprites[inv.inventoryType];
		sr.sortingLayerName = "Inventory";

		//register callback to update Gameobject on  object type change
		//inv.RegisterOnChangedCallback( OnCharacterChanged );
	}

	
	void OnInventoryChanged( Inventory inv) {
		//update furniture graphics
		if(inventoryGameObjectMap.ContainsKey(inv) == false) {
			Debug.LogError ("OnCharacterChanged -- can't change visuals");
			return;
		}

		GameObject c_go = inventoryGameObjectMap[inv];

		//c_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture (c);

		c_go.transform.position = new Vector3( inv.tile.X, inv.tile.Y, 0);
	}
	
	
}
