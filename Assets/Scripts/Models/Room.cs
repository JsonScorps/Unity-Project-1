using UnityEngine;
using System.Collections.Generic;

public class Room {

	public float temperature;

	List<Tile> tiles;

	public Room() {
		tiles = new List<Tile> ();
	}

	public void AssignTile(Tile t) {
		if (tiles.Contains (t)) {
			//tile already part of this room
			return;
		}

		if (t.room != null) {
			//tile part of some other room
			t.room.tiles.Remove (t);
		}

		t.room = this;
		tiles.Add (t);
	}

	public void UnAssignAllTiles() {
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].room = tiles [i].world.GetOutsideRoom(); //assign to outside
		}
		tiles = new List<Tile> ();
	}

	public static void RoomFloodFill(Furniture srcFurniture) {

		World world = srcFurniture.tile.world;

		Room oldRoom = srcFurniture.tile.room;

		//start flood fill for each direction
		foreach (Tile t in srcFurniture.tile.getNeighbors()) {
			FloodFill (t, oldRoom);
		}

		srcFurniture.tile.room = null;
		oldRoom.tiles.Remove (srcFurniture.tile);

		if (oldRoom != world.GetOutsideRoom ()) {
			//oldRoom should not contain any tiles anymore
			//remove room from world
			if (oldRoom.tiles.Count > 0) {
				Debug.LogError ("oldRoom still contains tiles");
			}

			world.DeleteRoom (oldRoom);
		}
	}

	protected static void FloodFill(Tile tile, Room oldroom) {
		if (tile == null) {
			return;
		}

		if (tile.room != oldroom) {
			//tile already part of another new room
			return;
		}

		if (tile.furniture != null && tile.furniture.roomEnclosure) {
			//tile contains room enclosing furniture
			return;
		}

		if (tile.Type == TileType.Empty) {
			return;
		}

		//start FloodFill
		Room newRoom = new Room();
		Queue<Tile> tilesToCheck = new Queue<Tile> ();
		tilesToCheck.Enqueue(tile);

		while (tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue ();

			if (t.room == oldroom) {
				newRoom.AssignTile (t);

				Tile[] ns = t.getNeighbors ();
				foreach (Tile t2 in ns) {
					if (t2 == null || t2.Type == TileType.Empty) {
						//reached outside / edge of the map
						//reassign all tiles to outside room
						newRoom.UnAssignAllTiles ();
						return;
					}

					if (t2.room == oldroom && (t2.furniture == null || t2.furniture.roomEnclosure == false)) {
						tilesToCheck.Enqueue (t2);
					}
				}
			}
		}

		newRoom.temperature = oldroom.temperature;

		tile.world.AddRoom (newRoom);
	}

}
