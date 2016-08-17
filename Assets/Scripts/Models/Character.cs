using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character: IXmlSerializable {

	public float X {
		get {
			return Mathf.Lerp (currTile.X, nextTile.X, movementPercentage);
			}
	}

	public float Y {
		get {
			return Mathf.Lerp (currTile.Y, nextTile.Y, movementPercentage);
			}
	}
				
	public Tile currTile {
		get; protected set;
	}
	Tile destTile;
	Tile nextTile; // next tile in pathfinding queue

	Path_AStar pathAStar;

	float movementPercentage;

	float speed = 3f; //tiles per second

	Action<Character> cbCharacterChanged;

	Job myJob;

	public Character() {
		// XML serialization only!
	}

	public Character(Tile tile) {
		currTile = destTile = nextTile = tile;
	}

	void Update_Job(float deltaTime) {
		
		//get a job !! 
		if (myJob == null) {

			//got a job
			myJob = currTile.world.jobQueue.Dequeue ();

			//go to the job
			if (myJob != null) {
				destTile = myJob.tile;
				myJob.RegisterJobCompleteCallback (OnJobEnded);
				myJob.RegisterJobCancelCallback (OnJobEnded);
			}
		}


		//already there?
		if (myJob != null && currTile == myJob.tile) {
			myJob.DoWork (deltaTime);
		}
			
	}

	public void AbandonJob() {
		nextTile = destTile = currTile;
		pathAStar = null;
		currTile.world.jobQueue.Enqueue (myJob);
		myJob = null;
	}

	void Update_Movement(float deltaTime) {

		if (currTile == destTile) {
			pathAStar = null;
			return;
		}

		if (nextTile == null || nextTile == currTile) {
			//get next tile from pathfinder
			if (pathAStar == null || pathAStar.Length() == 0) {
				//generate AStar path
				pathAStar = new Path_AStar(currTile.world, currTile, destTile);

				if (pathAStar.Length() == 0) {
					Debug.LogError ("Path_AStar -- no path found");
					AbandonJob ();
					pathAStar = null;
					return;
				}

				//discard first tile
				nextTile = pathAStar.Dequeue();
			}

			//get next waypoint from AStar
			nextTile = pathAStar.Dequeue();

			if (nextTile == currTile) {
				Debug.LogError ("Update_Movement -- nextTile = currTile");
			}
		}

		//if (pathAStar.Length () == 1) {
		//	return;
		//}

		//distance to destination
		float distToTravel = Mathf.Sqrt (
			Mathf.Pow (currTile.X - nextTile.X, 2) + 
			Mathf.Pow (currTile.Y - nextTile.Y, 2)
		);

		if (nextTile.IsEnterable() == Enterability.Never) {
			Debug.LogError ("trying to enter unwalkable tile");
			nextTile = null;
			pathAStar = null;
			return;
		}
		else if (nextTile.IsEnterable() == Enterability.Soon) {
			return;
		}

		//distance to travel this update
		float distThisFrame = speed / nextTile.movementCost * deltaTime;

		//percentage to travel this update
		float percThisFrame = distThisFrame / distToTravel;

		//Add to distance traveled
		movementPercentage += percThisFrame;

		if (movementPercentage >= 1) {
			//destination reached
			currTile = nextTile;

			movementPercentage = 0;
		}

	}

	public void Update(float deltaTime) {

		Update_Job (deltaTime);
		Update_Movement (deltaTime);

		if (cbCharacterChanged != null) {
			cbCharacterChanged (this);
		}


	}

	public void SetDestination(Tile tile) {
		if (currTile.IsNeighbor (tile, true) == false) {
			Debug.Log ("SetDestination -- destination not adjacent");
		}

		destTile = tile;
	}

	public void RegisterOnChangedCallback(Action<Character> cb) {
		cbCharacterChanged += cb;
	}

	public void UnregisterOnChangedCallback(Action<Character> cb) {
		cbCharacterChanged -= cb;
	}

	void OnJobEnded(Job j) {
		//job completed or cancelled
		if(j != myJob) {
			Debug.LogError ("Character on the wrong job");
			return;
		}

		myJob = null;

	}



	////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 
	/// 							SAVING & LOADING
	/// 
	////////////////////////////////////////////////////////////////////////////////////////////////////////


	public XmlSchema GetSchema() {
		return null;
	}

	public void WriteXml(XmlWriter writer) {
		writer.WriteAttributeString ("X", currTile.X.ToString());
		writer.WriteAttributeString ("Y", currTile.Y.ToString());
	}

	public void ReadXml(XmlReader reader) {

	}
}
