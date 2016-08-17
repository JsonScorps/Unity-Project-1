using UnityEngine;
using System.Collections.Generic;
using Priority_Queue;
using System.Linq;

public class Path_AStar {

	Queue<Tile> path;

	public Path_AStar(World world, Tile startTile, Tile endTile) {

		//create new Graph
		if(world.tileGraph == null) {
			world.tileGraph = new Path_TileGraph (world);
		}

		Dictionary<Tile, Path_Node<Tile>> nodes = world.tileGraph.nodes;

		//invalid startTile
		if (nodes.ContainsKey (startTile) == false) {
			Debug.Log ("Path_AStar -- startTile missing");
			return;
		}

		//invalid endTile
		if (nodes.ContainsKey (endTile) == false) {
			Debug.Log ("Path_AStar -- endTile missing");
			return;
		}
			
		Path_Node<Tile> start = nodes [startTile]; 
		Path_Node<Tile> goal  = nodes [endTile];

		HashSet<Path_Node<Tile>> ClosedSet = new HashSet<Path_Node<Tile>> ();

		//List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>> ();
		//OpenSet.Add (start);

		SimplePriorityQueue<Path_Node<Tile>> OpenSet = new SimplePriorityQueue<Path_Node<Tile>> ();
		OpenSet.Enqueue (start, 0);

		Dictionary< Path_Node<Tile>, Path_Node<Tile> > Came_From = new Dictionary< Path_Node<Tile>, Path_Node<Tile> >();

		//g_score
		Dictionary< Path_Node<Tile>, float> g_score = new Dictionary<Path_Node<Tile>, float> ();
		foreach(Path_Node<Tile> n in nodes.Values) {
			g_score[n] = Mathf.Infinity;
		}
		g_score [start] = 0;

		//f_score
		Dictionary< Path_Node<Tile>, float> f_score = new Dictionary<Path_Node<Tile>, float> ();
		foreach(Path_Node<Tile> n in nodes.Values) {
			f_score[n] = Mathf.Infinity;
		}
		f_score [start] = heuristic_cost_estimate ( start, goal);


		while (OpenSet.Count > 0) {
			Path_Node<Tile> current = OpenSet.Dequeue();

			if (current == goal) {
				//destination reached
				reconstruct_path(Came_From, current);
			}

			ClosedSet.Add (current);

			foreach (Path_Edge<Tile> edge_neighbor in current.edges) {

				Path_Node<Tile> neighbor = edge_neighbor.node;

				if(ClosedSet.Contains(neighbor) == true) {
					continue;
				}

				float movement_cost_to_neighbor = neighbor.data.movementCost * dist_between (current, neighbor);

				float tentative_g_score = g_score [current] + movement_cost_to_neighbor;

				if(OpenSet.Contains(neighbor) == true && tentative_g_score >= g_score[neighbor]) {
					continue;
				}

				Came_From [neighbor] = current;
				g_score [neighbor] = tentative_g_score;
				f_score [neighbor] = g_score [neighbor] + heuristic_cost_estimate (neighbor, goal);

				if (OpenSet.Contains (neighbor) == false) {
					OpenSet.Enqueue (neighbor, f_score [neighbor]);
				} else {
					OpenSet.UpdatePriority (neighbor, f_score [neighbor]);
				}
			} // </foreach> 

		} // </while> loop endet without finding a path

	}

	float dist_between(Path_Node<Tile> a, Path_Node<Tile> b) {
		
		int absX = Mathf.Abs (a.data.X - b.data.X);
		int absY = Mathf.Abs (a.data.Y - b.data.Y);

		//direct neighbor distance = 1
		if((absX + absY) == 1) {
			return 1f;
		}

		//diag   neighbor distance = sqrt(2) ~ 1,414
		if (absX == 1 && absY == 1) {
			return 1.414f;
		}

		//actual math
		return Mathf.Sqrt (
			Mathf.Pow(a.data.X - b.data.X, 2) +
			Mathf.Pow(a.data.Y - b.data.Y, 2) 
		);

	}

	float heuristic_cost_estimate(Path_Node<Tile> a, Path_Node<Tile> b) {

		return Mathf.Sqrt (
			Mathf.Pow(a.data.X - b.data.X, 2) +
			Mathf.Pow(a.data.Y - b.data.Y, 2) 
		);
	}

	void reconstruct_path(
		Dictionary< Path_Node<Tile>, Path_Node<Tile> > Came_From,
		Path_Node<Tile> current)
	{
		Queue<Tile> total_path = new Queue<Tile>();
		total_path.Enqueue (current.data); //goal

		//backtrack path
		while(Came_From.ContainsKey(current)) {
			current = Came_From [current];
			total_path.Enqueue (current.data);
		}

		path = new Queue<Tile>(total_path.Reverse() ) ;
	}

	public Tile Dequeue() {
		return path.Dequeue();
	}

	public int Length() {
		if(path == null) {
			return 0;
		}

		return path.Count;
	}

}
