using UnityEngine;
using System.Collections;
using System;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;


public enum TileType { Empty, Dirt, Water, Sand, Rock };
public enum Enterability { Yes, Never, Soon};

public class Tile : IXmlSerializable {

	private TileType _type = TileType.Empty;
	public TileType Type {
		get { return _type; }
		set {
			TileType oldType = _type;
			_type = value;
			// tile type changed -> callback

			if(cbTileChanged != null && oldType != _type)
				cbTileChanged(this);
		}
	}

	// stuff lying around
	Inventory inventory;

	// solid stuff like walls, doors, etc
	public Furniture furniture {
		get; protected set;
	}

	public Room room;

	public Job pendingFurnitureJob;

	// We need to know the context in which we exist. Probably. Maybe.
	public World world 	{ get; protected set; }
	public int X 		{ get; protected set; }
	public int Y 		{ get; protected set; }

	const float bastTileMovementCost = 1;
	public float movementCost {
		get {
			if(Type == TileType.Water || Type == TileType.Empty) {
				return 0;
			}
			if (furniture == null) {
				return bastTileMovementCost;
			}
			return bastTileMovementCost * furniture.movementCost;
		}
	}

	// callback function on tile changes
	Action<Tile> cbTileChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="world">A World instance.</param>
	/// <param name="x">x coordinate.</param>
	/// <param name="y">y coordinate.</param>
	public Tile( World world, int x, int y ) {
		this.world = world;
		this.X = x;
		this.Y = y;
	}

	/// <summary>
	/// register a callback on tile change
	/// </summary>
	public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged += callback;
	}
	
	/// <summary>
	/// unregister a callback
	/// </summary>
	public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged -= callback;
	}

	public bool PlaceFurniture(Furniture objInstance) {
		if(objInstance == null) {
			//uninstall object
			furniture = null;
			return true;
		}

		if (furniture != null) {
			Debug.LogError ("tile already contains object");
			return false;
		}

		furniture = objInstance;
		return true;
	}

	public Enterability IsEnterable() {
		//returns true if tile enterable on call

		if (movementCost == 0) {
			return Enterability.Never;
		}

		if (furniture != null && furniture.isEnterable != null) {
			return furniture.isEnterable (furniture);
		}

		return Enterability.Yes;
	}

	public Tile North() {
		return world.GetTileAt (X, Y + 1);
	}
	public Tile East() {
		return world.GetTileAt (X + 1, Y);
	}
	public Tile South() {
		return world.GetTileAt (X, Y - 1);
	}
	public Tile West() {
		return world.GetTileAt (X - 1, Y);
	}

	//true if tiles adjacent
	public bool IsNeighbor(Tile tile, bool diagOkay = false) {

		int absX = Mathf.Abs (tile.X - this.X);
		int absY = Mathf.Abs (tile.Y - this.Y);

		return 
			//horizontal / vertical adjacency
			(absX + absY) == 1 ||
			//diagonal adjacency
			(diagOkay && absX == 1 && absY == 1);
	}

	/// <summary>
	/// Gets the neighbors.
	/// </summary>
	/// <returns>The neighbors.</returns>
	/// <param name="diagOkay">If set to <c>true</c> diagonal movement okay.</param>
	public Tile[] getNeighbors(bool diagOkay = false) {

		Tile[] ns;

		if (diagOkay == false) {
			ns = new Tile[4];
		}
		else {
			ns = new Tile[8];
		}

		Tile n;

		n = world.GetTileAt (X, Y + 1);
		ns [0] = n; //N
		n = world.GetTileAt (X + 1, Y);
		ns [1] = n; //E
		n = world.GetTileAt (X, Y - 1);
		ns [2] = n; //S
		n = world.GetTileAt (X - 1, Y);
		ns [3] = n; //W

		if(diagOkay == true) {
			n = world.GetTileAt (X + 1, Y + 1);
			ns [4] = n; //NE
			n = world.GetTileAt (X + 1, Y - 1);
			ns [5] = n; //SE
			n = world.GetTileAt (X - 1, Y - 1);
			ns [6] = n; //SW
			n = world.GetTileAt (X -1, Y + 1);
			ns [7] = n; //NW
		}

		return ns;
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
		writer.WriteAttributeString ("X", X.ToString());
		writer.WriteAttributeString ("Y", Y.ToString());
		writer.WriteAttributeString ("Type", ((int)Type).ToString());

	}

	public void ReadXml(XmlReader reader) {
		Type = (TileType) int.Parse(reader.GetAttribute ("Type"));
	}


	
}
