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

	public bool PlaceInventory(Job j, Inventory inv) {

		if (j.inventoryRequirements.ContainsKey (inv.inventoryType) == false) {
			Debug.LogError ("trying to add wrong inventory");
			return false;
		}

		j.inventoryRequirements [inv.inventoryType].stackSize += inv.stackSize;

		if (j.inventoryRequirements [inv.inventoryType].maxStackSize > j.inventoryRequirements [inv.inventoryType].stackSize) {
			inv.stackSize = j.inventoryRequirements [inv.inventoryType].stackSize - j.inventoryRequirements [inv.inventoryType].maxStackSize;
			j.inventoryRequirements [inv.inventoryType].stackSize = j.inventoryRequirements [inv.inventoryType].maxStackSize;
		} else {
			inv.stackSize = 0;
		}


		if (inv.stackSize == 0) {
			if (inventories.ContainsKey (inv.inventoryType)) {
				inventories [inv.inventoryType].Remove (inv);
			}
		}

		return true;
	}

}
