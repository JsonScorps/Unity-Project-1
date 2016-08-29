using UnityEngine;
using System.Collections;

public static class FurnitureActions {

	public static void Door_UpdateAction(Furniture furn,float deltaTime) {
		//Debug.Log ("Door_UpdateAction");

		if (furn.GetParameter ("is_opening") >= 1) {
			furn.ChangeParameter ("openness", deltaTime* 4);
			if (furn.GetParameter ("openness") >= 1) {
				furn.SetParameter ("is_opening", 0);
			}
		} 
		else {
			furn.ChangeParameter ("openness", deltaTime * -4);
		}

		furn.SetParameter ("openness",Mathf.Clamp01 (furn.GetParameter ("openness")));

		if (furn.cbOnChanged != null) {
			furn.cbOnChanged (furn);
		}
	}

	public static Enterability Door_IsEnterable(Furniture furn) {
		furn.SetParameter ("is_opening", 1);

		if (furn.GetParameter ("openness") >= 1) {
			return Enterability.Yes;
		}

		return Enterability.Soon;
	}


	public static void JobComplete_Furniture(Job job) {
		WorldController.Instance.world.PlaceFurniture (job.jobObjectType, job.tile);

		job.tile.pendingFurnitureJob = null;
	}
}
