using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable {

	// A two-dimensional array to hold our tile data.
	Tile[,] tiles;

	public List<Character> characters;
	public List<Furniture> furnitures;
	public List<Room> rooms;

	//pathfinding graph
	public Path_TileGraph tileGraph;
	//inventory manager
	public InventoryManager inventoryManager;

	Dictionary<string, Furniture> furniturePrototypes;

	// tile width of the world.
	public int Width { get; protected set; }

	// tile height of the world
	public int Height { get; protected set; }

	Action<Furniture> cbFurnitureCreated;
	Action<Character> cbCharacterCreated;
	Action<Inventory> cbInventoryCreated;
	Action<Tile> cbTileChanged;

	public JobQueue jobQueue;

	/// Initializes a new instance of the <see cref="World"/> class.
	public World(int width, int height) {
		SetupWorld (width, height);

		CreateCharacter (GetTileAt (Width/2, Height/2) );
	}

	//Default constructor for Xml Serialization
	public World() {

	}
		
	public Room GetOutsideRoom() {
		return rooms [0];
	}

	public void AddRoom(Room r) {
		rooms.Add (r);
	}

	public void DeleteRoom(Room r) {
		if (r == GetOutsideRoom ()) {
			Debug.LogError ("DeleteRoom -- tried to delete outside");
			return;
		}

		//remove room from list
		rooms.Remove (r);

		//reassign tiles from removed room to outside
		r.UnAssignAllTiles ();
	}

	void SetupWorld(int width, int height) {

		Width = width;
		Height = height;

		jobQueue = new JobQueue();

		tiles = new Tile[Width,Height];

		rooms = new List<Room> ();
		rooms.Add (new Room ());	//outside room

		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				tiles [x, y] = new Tile(this, x, y);
				tiles [x, y].Type = TileType.Empty; 
				tiles [x, y].RegisterTileTypeChangedCallback (OnTileChanged);
				tiles [x, y].room = GetOutsideRoom(); // default outside room 
			}
		}

		Debug.Log ("World created with " + (Width*Height) + " tiles.");

		CreateFurniturePrototypes ();

		characters 	= new List<Character> ();
		furnitures 	= new List<Furniture> ();
		inventoryManager = new InventoryManager ();

	}

	public void Update(float deltaTime) {
		foreach (Character c in characters) {
			c.Update (deltaTime);
		}

		foreach(Furniture f in furnitures) {
			f.Update (deltaTime);
		}
	}


	public Character CreateCharacter(Tile t) {
		Character c = new Character (t);
		characters.Add (c);
		if (cbCharacterCreated != null) {
			cbCharacterCreated (c);
		}

		return c;
	}

	void CreateFurniturePrototypes() {
		
		furniturePrototypes = new Dictionary<string, Furniture>();

		furniturePrototypes.Add ("Wall",
			new Furniture(
					"Wall",
					0,		//impassable
					1,		//width
					1,		//height
					true,	//roomEnclosure
					true	//linkstoNeighbor
		)
		);

		furniturePrototypes.Add ("Door",
			new Furniture(
				"Door",
				1,		//impassable
				1,		//width
				1,		//height
				true,	//roomEnclosure
				false	//linkstoNeighbor
			)
		);


		furniturePrototypes ["Door"].SetParameter ("openness", 0);
		furniturePrototypes ["Door"].SetParameter ("is_opening", 0);
		furniturePrototypes ["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);

		furniturePrototypes ["Door"].isEnterable = FurnitureActions.Door_IsEnterable;
	}
		
	// randomize map
	public void RandomizeTiles() {
		Debug.Log ("RandomizeTiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {

				if(UnityEngine.Random.Range(0, 2) == 0) {
					tiles[x,y].Type = TileType.Dirt;
				}
				else {
					tiles[x,y].Type = TileType.Water;
				}

			}
		}
	}
		
	/// Gets tile data at (x, y)
	/// <returns>The <see cref="Tile"/>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Tile GetTileAt(int x, int y) {
		if (x >= Width || x < 0 || y >= Height || y < 0) {
			//Debug.LogError ("Tile (" + x + "," + y + ") is out of range.");
			return null;
		} 
		else {
			//Debug.Log ("Tile (" + x + "," + y + ")");
			return tiles [x, y];
		}
	}

	public Furniture PlaceFurniture( string furnitureType, Tile t) {
		if (furniturePrototypes.ContainsKey (furnitureType) == false) {
			Debug.LogError ("furniturePrototypes does not contain proto for key:" + furnitureType);
			return null;
		}

		Furniture furn = Furniture.PlaceInstance (furniturePrototypes[furnitureType], t);

		if (furn == null) {
			return null;
		}

		furnitures.Add (furn);

		if (furn.roomEnclosure) {
			Room.RoomFloodFill (furn);
		}

		if (cbFurnitureCreated != null) {
			cbFurnitureCreated (furn);
			if (furn.movementCost != 1) {
				InvalidateTileGraph ();
			}
		}

		return furn;
	}

	public void RegisterFurnitureCreated (Action<Furniture> callbackfunc) {
		cbFurnitureCreated += callbackfunc;
	}

	public void UnregisterFurnitureCreated (Action<Furniture> callbackfunc) {
		cbFurnitureCreated -= callbackfunc;
	}
	public void RegisterCharacterCreated (Action<Character> callbackfunc) {
		cbCharacterCreated += callbackfunc;
	}
	
	public void UnregisterCharacterCreated (Action<Character> callbackfunc) {
		cbCharacterCreated -= callbackfunc;
	}

	public void RegisterInventoryCreated (Action<Inventory> callbackfunc) {
		cbInventoryCreated += callbackfunc;
	}

	public void UnregisterInventoryCreated (Action<Inventory> callbackfunc) {
		cbInventoryCreated -= callbackfunc;
	}

	public void RegisterTileChanged (Action<Tile> callbackfunc) {
		cbTileChanged += callbackfunc;
	}

	public void UnregisterTileChanged (Action<Tile> callbackfunc) {
		cbTileChanged -= callbackfunc;
	}

	void OnTileChanged (Tile t) {
		if (cbTileChanged == null) {
			return;
		}
		cbTileChanged (t);
		InvalidateTileGraph ();
	}

	public void InvalidateTileGraph() {
		tileGraph = null;
	}

	public bool isFurniturePlacementValid(string furnitureType, Tile t) {
		return furniturePrototypes [furnitureType].isValidPosition (t);
	}

	public Furniture GetFurniturePrototype(string objectType) {
		if(furniturePrototypes.ContainsKey(objectType) == false) {
			Debug.LogError ("GetFurniturePrototype -- No furniture with type: "+objectType);
			return null;
		}

		return furniturePrototypes[objectType];
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
		writer.WriteAttributeString ("Width", Width.ToString());
		writer.WriteAttributeString ("Height", Height.ToString());

		//tiles
		writer.WriteStartElement ("Tiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				if (tiles [x, y].Type != TileType.Empty) {
					writer.WriteStartElement ("Tile");
					tiles [x, y].WriteXml (writer);
					writer.WriteEndElement ();
				}
			}
		}
		writer.WriteEndElement ();

		//furniture
		writer.WriteStartElement ("Furnitures");
		foreach(Furniture furn in furnitures) {
			writer.WriteStartElement ("Furniture");
			furn.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteEndElement ();

		writer.WriteStartElement ("Characters");
		foreach(Character c in characters) {
			writer.WriteStartElement ("Character");
			c.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteEndElement ();
	}

	public void ReadXml(XmlReader reader) {
		Debug.Log ("ReadXml");

		Width = int.Parse (reader.GetAttribute ("Width"));
		Height = int.Parse (reader.GetAttribute ("Height"));

		SetupWorld (Width, Height);

		while (reader.Read ()) {
			switch (reader.Name) {
			case "Tiles":
				ReadXml_Tiles (reader);
				break;
			case "Furnitures":
				ReadXml_Furnitures (reader);
				break;
			case "Characters":
				ReadXml_Characters (reader);
				break;
			}
		}

		//DEBUG ONLY
		Inventory inv = new Inventory();
		Tile t = GetTileAt (Width / 2, Height / 2);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory();
		t = GetTileAt (Width / 2 - 1, Height / 2 + 1);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory();
		t = GetTileAt (Width / 2 + 1, Height / 2 + 1);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		//End Debug
	}

	void ReadXml_Tiles(XmlReader reader) {

		if (reader.ReadToDescendant ("Tile")) {
			//atleast one tile, start loop
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));
				tiles [x, y].ReadXml (reader); 
			} while (reader.ReadToNextSibling ("Tile"));

		}
			
	}

	void ReadXml_Furnitures(XmlReader reader) {

		if (reader.ReadToDescendant ("Furniture")) {
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));
					
				Furniture furn = PlaceFurniture (reader.GetAttribute ("furnitureType"), tiles [x, y]);
				furn.ReadXml (reader);
			} while(reader.ReadToNextSibling("Furniture"));
		}
	}

	void ReadXml_Characters(XmlReader reader) {

		if (reader.ReadToDescendant ("Character")) {
			do {
				int x = int.Parse (reader.GetAttribute ("X"));
				int y = int.Parse (reader.GetAttribute ("Y"));

				Character c = CreateCharacter (tiles [x, y]);	
				c.ReadXml (reader);
			} while(reader.ReadToNextSibling("Character"));
		}
	}

}
