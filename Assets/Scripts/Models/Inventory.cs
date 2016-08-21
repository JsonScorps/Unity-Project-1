using UnityEngine;
using System.Collections;

//

public class Inventory {

	public string inventoryType = "Iron";

	public int maxStackSize = 50;
	public int stackSize = 1;

	public Tile tile;
	public Character character;

	public Inventory() {

	}

	protected Inventory(Inventory other) {
		inventoryType 	= other.inventoryType;
		maxStackSize 	= other.maxStackSize;
		stackSize 		= other.stackSize;
	}

	public virtual Inventory Clone() {
		return new Inventory (this);
	}
}
