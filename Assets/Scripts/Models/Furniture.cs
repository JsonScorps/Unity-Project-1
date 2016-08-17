using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


public class Furniture : IXmlSerializable {

	//base tile, not occupied tiles
	public Tile tile {
		get; protected set;
	}

	//type of furniture
	public string furnitureType {
		get; protected set;
	}

	//walk speed dependend on environmental hazards
	//higher means slower, lower means faster
	//default 1
	//on walkSpeed == 0 tile is impassable
	public float movementCost {get; protected set;}

	//occupied/required area of furniture
	int width;
	int height;

	public bool linksToNeighbor {get; protected set;}
	public bool roomEnclosure   {get; protected set;}

	public Action<Furniture> cbOnChanged;
	Func<Tile, bool> funcPositionValidation;

	protected Action<Furniture, float> updateActions;
	protected Dictionary<string, float> furnParameters;

	public Func<Furniture, Enterability> isEnterable;

	public void Update(float deltaTime) {
		if (updateActions != null) {
			updateActions (this, deltaTime);
		}
	}

	public Furniture() {
		//use for serialization

		furnParameters = new Dictionary<string, float> ();
	}

	//Copy Constructor
	protected Furniture(Furniture other) {
		this.furnitureType 		= other.furnitureType;
		this.movementCost 		= other.movementCost;
		this.roomEnclosure 		= other.roomEnclosure;
		this.width 				= other.width;
		this.height 			= other.height;
		this.linksToNeighbor 	= other.linksToNeighbor;

		this.furnParameters = new Dictionary<string, float> (other.furnParameters);

		this.isEnterable = other.isEnterable;

		if(other.updateActions != null)
			this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();
	}

	virtual public Furniture Clone() {
		return new Furniture (this);
	}

	//create furniture from parameters
	public Furniture (string furnitureType, float walkSpeed = 100f, int width = 1, int height = 1,bool roomEnclosure = false, bool linksToNeighbor = false) {

		this.furnitureType 		= furnitureType;
		this.movementCost		= walkSpeed;
		this.width				= width;
		this.height 			= height;
		this.roomEnclosure 		= roomEnclosure;
		this.linksToNeighbor	= linksToNeighbor;

		this.funcPositionValidation = this.Default__IsValidPosition;

		furnParameters = new Dictionary<string, float> ();

	}

	static public Furniture PlaceInstance(Furniture proto, Tile tile) {
		if(proto.funcPositionValidation(tile) == false) {
			Debug.LogError ("PlaceInstance -- position invalid");
			return null;
		}

		Furniture furn = proto.Clone ();

		furn.tile = tile;

		if (tile.PlaceFurniture (furn) == false ) {
			//unable to place furniture
			return null;
		}

		if(furn.linksToNeighbor) {
			//update neighboring graphics

			Tile t;
			int x = tile.X;
			int y = tile.Y;

			t = tile.world.GetTileAt(x, y + 1);
			if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType) {
				t.furniture.cbOnChanged (t.furniture);
			}
			t = tile.world.GetTileAt(x + 1, y);
			if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType) {
				t.furniture.cbOnChanged (t.furniture);
			}
			t = tile.world.GetTileAt(x, y - 1);
			if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType) {
				t.furniture.cbOnChanged (t.furniture);
			}
			t = tile.world.GetTileAt(x - 1, y);
			if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType) {
				t.furniture.cbOnChanged (t.furniture);
			}
		}

		return furn;
	}

	public void RegisterOnChangedCallback(Action<Furniture> callbackfunc) {
		cbOnChanged += callbackfunc;
	}

	public void UnregisterOnChangedCallback(Action<Furniture> callbackfunc) {
		cbOnChanged -= callbackfunc;
	}

	public bool isValidPosition(Tile t) {
		return funcPositionValidation (t);
	}

	protected bool Default__IsValidPosition(Tile t) {
		if (t.Type == TileType.Water || t.Type == TileType.Empty) {
			return false;
		}

		if (t.furniture != null) {
			return false;
		}
		return true;
	}

	public float GetParameter(string key, float def = 0) {
		if(furnParameters.ContainsKey(key) == false) {
			return def;
		}

		return furnParameters [key];
	}

	public void SetParameter(string key, float value) {
		furnParameters [key] = value;
	}

	public void ChangeParameter(string key, float value) {
		if (furnParameters.ContainsKey (key) == false) {
			furnParameters [key] = value;
			return;
		}
		furnParameters [key] += value;
	}

	public void RegisterUpdateAction(Action<Furniture, float> a) {
		updateActions += a;
	}

	public void UnregisterUpdateAction(Action<Furniture, float> a) {
		updateActions -= a;
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
		writer.WriteAttributeString ("X", tile.X.ToString());
		writer.WriteAttributeString ("Y", tile.Y.ToString());
		writer.WriteAttributeString ("furnitureType", furnitureType);
		//writer.WriteAttributeString ("movementCost", movementCost.ToString());

		foreach (String k in furnParameters.Keys) {
			writer.WriteStartElement ("param");
			writer.WriteAttributeString ("name", k);
			writer.WriteAttributeString ("value", furnParameters[k].ToString());
			writer.WriteEndElement ();
		}

	}

	public void ReadXml(XmlReader reader) {
		//Debug.LogError ("Funiture.ReadXml -- UNUSED");
		//furnitureType = reader.GetAttribute ("furnitureType");
		//movementCost =  float.Parse(reader.GetAttribute ("movementCost"));

		if (reader.ReadToDescendant ("param")) {
			do {
				string k = reader.GetAttribute("name");
				float v = float.Parse(reader.GetAttribute("value"));

				furnParameters[k] = v;	
			} while(reader.ReadToNextSibling("param"));
		}
	}
		
}
