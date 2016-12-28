using UnityEngine;
using System.Collections.Generic;
using System;

public class Job {
	// everything to do with queued jobs

	public Tile tile;
	float jobTime;
	public string jobObjectType { 
		get; protected set;
	}

	Action<Job> cbJobComplete;
	Action<Job> cbJobCancel;

	public Dictionary <string, Inventory> inventoryRequirements;

	public Job(Tile tile,string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements) {
		this.tile = tile;
		this.jobObjectType = jobObjectType;
		this.cbJobComplete += cbJobComplete; 
		this.jobTime = jobTime;

		this.inventoryRequirements = new Dictionary<string, Inventory> ();
		if (inventoryRequirements != null) {
			foreach (Inventory inv in inventoryRequirements) {
				this.inventoryRequirements [inv.inventoryType] = inv.Clone ();
			}
		}

	}

	protected Job(Job other) {
		this.tile = other.tile;
		this.jobObjectType = other.jobObjectType;
		this.cbJobComplete = other.cbJobComplete;
		//this.cbJobComplete = other.cbJobCancel;
		this.jobTime = other.jobTime;

		this.inventoryRequirements = new Dictionary<string, Inventory> ();
		if (inventoryRequirements != null) {
			foreach (Inventory inv in other.inventoryRequirements.Values) {
				this.inventoryRequirements [inv.inventoryType] = inv.Clone ();
			}
		}
	}

	virtual public Job Clone() {
		return new Job (this);
	}


	public void RegisterJobCompleteCallback(Action<Job> cb) {
		cbJobComplete += cb;
	}

	public void UnregisterJobCompleteCallback(Action<Job> cb) {
		cbJobComplete -= cb;
	}

	public void RegisterJobCancelCallback(Action<Job> cb) {
		cbJobCancel += cb;
	}

	public void UnregisterJobCancelCallback(Action<Job> cb) {
		cbJobCancel -= cb;
	}

	public void DoWork (float workTime) {
		jobTime -= workTime;

		if(jobTime <= 0) {
			if(cbJobComplete != null)
				cbJobComplete(this);
		}
	}

	public void CancelJob() {
		if(cbJobCancel != null)
			cbJobCancel(this);
	}

	public bool HasAllMaterials() {
		foreach (Inventory inv in inventoryRequirements.Values) {
			if (inv.maxStackSize > inv.stackSize)
				return false;
		}

		return true;
	}

	public bool DesiresInventoryType(Inventory inv) {
		if(inventoryRequirements.ContainsKey(inv.inventoryType) == false) {
			return false;
		}

		if(inventoryRequirements[inv.inventoryType].stackSize >= inventoryRequirements[inv.inventoryType].maxStackSize) {
			return false;
		}

		return true;
}
