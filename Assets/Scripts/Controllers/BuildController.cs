using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildController : MonoBehaviour {

	//build furniture
	bool buildFurniture = false;

	//type of tile to build
	TileType buildTileType;

	//type of furniture to build
	string buildFurnitureType;

	// Use this for initialization
	void Start () {
		
	}

	public void SetMode_Dirt() {
		buildFurniture = false;
		buildTileType = TileType.Dirt;
	}

	public void SetMode_Bulldoze() {
		buildFurniture = false;
		buildTileType = TileType.Empty;
	}

	public void SetMode_Furniture(string furnitureType) {
		buildFurniture = true;
		buildFurnitureType = furnitureType;
	}

	public void DoBuild(Tile t) {
		if (buildFurniture == true) {
			//build furniture
			//WorldController.Instance.World.PlaceFurniture( buildFurnitureType, t);

			string furnitureType = buildFurnitureType;

			if (WorldController.Instance.world.isFurniturePlacementValid (furnitureType, t)
				&& t.pendingFurnitureJob == null) {
				//valid tile -> create job

				Job j;

				if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey (furnitureType)) {
					j = WorldController.Instance.world.furnitureJobPrototypes [furnitureType].Clone();

					j.tile = t;
				} 
				else {
					Debug.LogError ("No job prototype for '"+furnitureType+"'");
					j = new Job (t, furnitureType, FurnitureActions.JobComplete_Furniture, 0.1f, null);
				}

				t.pendingFurnitureJob = j;

				j.RegisterJobCancelCallback( (theJob) => {theJob.tile.pendingFurnitureJob = null; });
				//queue job
				WorldController.Instance.world.jobQueue.Enqueue (j);
			}

		} 
		else {
			//change tile to buildTileType 
			t.Type = buildTileType;
		}
	}

}
