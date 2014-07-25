using UnityEngine;
using System.Collections;

public class MenuSetup : MonoBehaviour {

	GUIStyle style = new GUIStyle();
	GUIContent content = new GUIContent();
	public Texture btnTexture;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		style.normal.textColor = Color.black;
		style.alignment = TextAnchor.MiddleCenter;
		style.fontSize = 160;
		
		content.image = btnTexture;

		GUI.Label(new Rect(0, 300, Screen.width, 30), "Transcription Game", style);

		style.fontSize = 80;

		/*
		content.text = "Start";
		GUI.Button(new Rect(0, 650, Screen.width, 30), content, style);
		content.text = "Instructions";
		GUI.Button(new Rect(0, 750, Screen.width, 30), content, style);
		content.text = "High Scores";
		GUI.Button(new Rect(0, 850, Screen.width, 30), content, style);
		*/

		if (GUI.Button (new Rect (0, 650, Screen.width, 30), "Start", style)) {
			//start scene
			Application.LoadLevel("main");
		}

		GUI.Button(new Rect(0, 750, Screen.width, 30), "Instructions", style);
		GUI.Button(new Rect(0, 850, Screen.width, 30), "High Scores", style);
	}
}
