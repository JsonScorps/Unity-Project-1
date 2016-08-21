using UnityEngine;
using System.Collections.Generic;

public class InventoryManager {


	public Dictionary<string,List<Inventory>> inventories;

	public InventoryManager() {
		inventories = new Dictionary<string,List<Inventory>> ();
	}

	public bool PlaceInventory(Tile t, Inventory inv) {

		bool tileWasEmpty = t.inventory == null;

		if (t.PlaceInventory (inv) == false) {
			//placing unsuccessful
			return false;
		}

		if (inv.stackSize == 0) {
			if (inventories.ContainsKey (t.inventory.inventoryType)) {
				inventories [inv.inventoryType].Remove (inv);
			}
		}

		if(tileWasEmpty) {
			if (inventories.ContainsKey (t.inventory.inventoryType) == false) {
				inventories [t.inventory.inventoryType] = new List<Inventory> ();
			}
			inventories [t.inventory.inventoryType].Add (t.inventory);
		}

		return true;
	}

}
