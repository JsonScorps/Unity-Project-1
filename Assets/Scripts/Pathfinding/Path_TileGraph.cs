using UnityEngine;
using System.Collections.Generic;

public class Path_TileGraph {

	//creates pathfindig graph for world
	//nodes = tiles
	public Dictionary<Tile, Path_Node<Tile>> nodes;

	public Path_TileGraph(World world) {


		nodes = new Dictionary<Tile, Path_Node<Tile>> ();

		//loop through tiles
		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {

				Tile t = world.GetTileAt (x, y);

				//movementSpeed 0 -> unpassable
				//if (t.movementCost > 0) {
					Path_Node<Tile> n = new Path_Node<Tile> ();
					n.data = t;
					nodes.Add (t, n);
				//}
				
			}
		}

		//loop through nodes
		foreach (Tile t in nodes.Keys) {
			Path_Node<Tile> n = nodes [t];

			List<Path_Edge<Tile> > edges = new List<Path_Edge<Tile>> ();

			//list of t'S neighbors
			Tile[] neighbors = t.getNeighbors (true);

			//if neighbor walkable -> create edge
			for (int i = 0; i < neighbors.Length; i++) {

				if (neighbors [i] != null && neighbors [i].movementCost > 0) {

					//prevent diagonal clipping
					if(isCLippingCorner(t, neighbors[i]) == true) {
						continue;
					}

					//create edge
					Path_Edge<Tile> e = new Path_Edge<Tile>();
					e.cost = neighbors [i].movementCost;
					e.node = nodes [neighbors [i]];

					//add edge to temp list
					edges.Add (e);
				}
			}

			n.edges = edges.ToArray();
		}
	}

	bool isCLippingCorner(Tile curr, Tile neigh) {
 
		int diffX = curr.X - neigh.X;
		int diffY = curr.Y - neigh.Y;

		if ((Mathf.Abs (diffX) + Mathf.Abs (diffY)) == 2) {
			//moving diagonally
			if (curr.world.GetTileAt (curr.X - diffX, curr.Y).movementCost == 0) {
				//east or west unwalkable
				return true;
			}

			if (curr.world.GetTileAt (curr.X, curr.Y - diffY).movementCost == 0) {
				//north or south unwalkable
				return true;
			}
		}

		return false;
		
	}
}
