﻿using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticVerticalSizeEditor : Editor {

	public override void OnInspectorGUI () {
		
		DrawDefaultInspector ();

		if (GUILayout.Button ("Resize Buttons")) {
			((AutomaticVerticalSize)target).adjustSize ();
		}
	}
}
