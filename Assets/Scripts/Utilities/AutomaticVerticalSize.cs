using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AutomaticVerticalSize : MonoBehaviour {

	public float childheight = 30f;

	// Use this for initialization
	void Start () {
		adjustSize ();
	}

	//adjust menu size
	public void adjustSize() {
		Vector2 size = this.GetComponent<RectTransform> ().sizeDelta;
		size.y = this.transform.childCount * childheight;
		this.GetComponent<RectTransform> ().sizeDelta = size;
	}

}
