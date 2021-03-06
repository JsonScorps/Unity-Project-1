﻿using UnityEngine;
using System.Collections;

//

public class Inventory {

	public string inventoryType = "Iron";

	public int maxStackSize = 50;
	public int stackSize = 20;

	public Tile tile;
	public Character character;

	public Inventory() {

	}

	public Inventory(string inventoryType, int maxStackSize, int stackSize) {
		this.inventoryType 	= inventoryType;
		this.maxStackSize 	= maxStackSize;
		this.stackSize 		= stackSize;
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
