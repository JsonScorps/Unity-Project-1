using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

	Dictionary<Character, GameObject> characterGameObjectMap;
	Dictionary<string, Sprite> characterSprites;
	
	World world {
			get { return WorldController.Instance.world; }
	}


	// Use this for initialization
	void Start () {
		loadSprites ();
		
		characterGameObjectMap = new Dictionary<Character, GameObject> ();
		
		//register callback to update Gameobject
		world.RegisterCharacterCreated (OnCharacterCreated);

		foreach (Character c in world.characters) {
			OnCharacterCreated (c);
		}

		//c.SetDestination (world.GetTileAt (world.Width/2 + 5, world.Height/2) );

	}
	
	void loadSprites () {
		characterSprites = new Dictionary<string, Sprite> ();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Characters/");
			
		foreach(Sprite s in sprites) {
	 		//Debug.Log (s);
			characterSprites [s.name] = s;
		}
	}

	public void OnCharacterCreated(Character c) {
		// create new GameObject and add to scene
		GameObject c_go = new GameObject();

		// add object data/go pair to Dictionary
		characterGameObjectMap.Add( c, c_go);

		c_go.name = "Character";
		c_go.transform.SetParent (this.transform, true);
		c_go.transform.position = new Vector3( c.X, c.Y, 0);

		// add sprite renderer
		SpriteRenderer sr = c_go.AddComponent<SpriteRenderer>();
		sr.sprite= characterSprites["knight"];
		sr.sortingLayerName = "Characters";

		//register callback to update Gameobject on  object type change
		c.RegisterOnChangedCallback( OnCharacterChanged );
	}

	
	void OnCharacterChanged( Character c) {
		//update furniture graphics
		if(characterGameObjectMap.ContainsKey(c) == false) {
			Debug.LogError ("OnCharacterChanged -- can't change visuals");
			return;
		}

		GameObject c_go = characterGameObjectMap[c];

		//c_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture (c);

		c_go.transform.position = new Vector3( c.X, c.Y, 0);
	}
	
	
}
