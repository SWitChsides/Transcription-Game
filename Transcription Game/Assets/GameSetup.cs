using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour {

	public enum BrickState {IN, OUT};

	public Camera mainCam;
	public BoxCollider2D topWall;
	public BoxCollider2D bottomWall;
	public BoxCollider2D leftWall;
	public BoxCollider2D rightWall;

	Vector3 temp;
	
	string[] fragments = {"This is", "this is a", "is a sample", "sample."};
	public GameObject[] fragmentBricks = new GameObject[4];

	//NOTE: clear this out when reinitializing the bricks!
	public Hashtable brickSizes = new Hashtable();

	public float timer = 120f;

	Texture2D brickTexture;

	public GameObject movedBrick = null;
	public BrickState brickstate = BrickState.OUT;
	public List<GameObject> brickList = new List<GameObject>();

	List<Overlap> overlapList = new List<Overlap>();

	void makeBricks() {
		//Clear brickSizes.
		brickSizes = new Hashtable();

		//DEBUG: Initialize our sample brick set.
		//Find the GameObject to contain all the bricks in.
		GameObject fragmentBrickParent = GameObject.Find("fragmentBricks");

		//Vector containing the next position to place the brick. Used to stagger the bricks.
		float nextX = 0f;
		float nextY = .8f;
		Vector3 nextPosition = new Vector3(nextX, nextY, 0f);

		//Stuff for fitting the brick to the text.
		float desiredScale = 1f;
		float prevTextDimensions = 0f;
		float textDimensions = 0f;
		float brickSize = 273f;
		Vector2 newOffset;
		Vector2 wordOffset;


		fragmentBricks = new GameObject[fragments.Length];

		//Initialize City Grounds and Construction Zone boundaries.
		//Move each wall to its edge location
		topWall.size = new Vector2 (mainCam.ScreenToWorldPoint(new Vector3(Screen.width * 2f, 0f, 0f)).x, 1f);
		topWall.center = new Vector2 (0f, mainCam.ScreenToWorldPoint(new Vector3(0f, Screen.height, 0f)).y + 0.5f);
		
		bottomWall.size = new Vector2 (mainCam.ScreenToWorldPoint (new Vector3 (Screen.width * 2, 0f, 0f)).x, 1f);
		bottomWall.center = new Vector2 (0f, mainCam.ScreenToWorldPoint (new Vector3( 0f, 0f, 0f)).y - 0.5f);
		
		leftWall.size = new Vector2(1f, mainCam.ScreenToWorldPoint(new Vector3(0f, Screen.height*2f, 0f)).y);;
		leftWall.center = new Vector2(mainCam.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x - 0.5f, 0f);
		
		rightWall.size = new Vector2(1f, mainCam.ScreenToWorldPoint(new Vector3(0f, Screen.height*2f, 0f)).y);
		rightWall.center = new Vector2(mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0f, 0f)).x + 0.5f, 0f);

		//Initialize Menu Bar.

		for(int i = 0; i<fragments.Length; i++){
			//Erase old bricks
			Destroy(fragmentBricks[i]);

			//Draw new ones
			fragmentBricks[i] = Instantiate(Resources.Load("fragmentBrick", typeof(GameObject))) as GameObject;
			fragmentBricks[i].transform.parent = fragmentBrickParent.transform;
			fragmentBricks[i].name = fragments[i];
			fragmentBricks[i].guiText.text = fragments[i];

			//Scale the brick to fit the text exactly.
			textDimensions = fragmentBricks[i].guiText.GetScreenRect().width;
			//Save the brick size for later use.
			brickSizes.Add(fragmentBricks[i], textDimensions);
			desiredScale = (float)textDimensions/Screen.width;
			fragmentBricks[i].transform.localScale = new Vector3(desiredScale-.12f, 0f, 0f);

			//Offset GUIText so its aligned with the GameObject correctly.
			newOffset = fragmentBricks[i].guiText.pixelOffset;
			newOffset.x = (brickSize - textDimensions)/2f;
			fragmentBricks[i].guiText.pixelOffset = newOffset;

			//Change position so its adjacent to next brick.
			//If the brick overflows the row, put it on the next line.
			//Bit of a hack but leave me alone I've been here for hours
			if(i != 0)
				nextX += (((prevTextDimensions/2 + textDimensions/2) /Screen.width) + 0.05f);
			else nextX += (((textDimensions/2) /Screen.width) + 0.05f);
			
			if((nextX + ((textDimensions/Screen.width) + 0.15f)) > 1f){
				nextX = (((textDimensions/2) /Screen.width) + 0.05f);
				nextY -= .15f;
			}
			
			nextPosition.x = nextX;
			nextPosition.y = nextY;
			fragmentBricks[i].transform.position = nextPosition;


			//Create an array of GUIText objects.
			//note: rename charArr to wordArr
			string[] wordArray = fragments[i].Split(" ".ToCharArray());
			GameObject[] charArr = new GameObject[wordArray.Length];

			wordOffset = newOffset;

			//Loop through all the words and readd them to the brick.
			//ERROR: lol this is pretty horrible but dont look at it
			for(int j = 0; j < wordArray.Length; j++){
				//Create GUIText object.
				charArr[j] = new GameObject();
				charArr[j].name = wordArray[j];
				charArr[j].AddComponent<GUITexture>();
				charArr[j].guiTexture.texture = brickTexture;

				//Put it under the GameObject parent.
				charArr[j].transform.parent = fragmentBricks[i].transform;
				charArr[j].transform.position = fragmentBricks[i].transform.position;

				charArr[j].AddComponent<GUIText>();
				charArr[j].guiText.text = wordArray[j];
				charArr[j].guiText.fontSize = 50;
				charArr[j].guiText.anchor = TextAnchor.LowerLeft;
				charArr[j].guiText.alignment = TextAlignment.Center;

				//Fix the position.
				Debug.Log("Width of "+wordArray[j]+": "+charArr[j].guiText.GetScreenRect().width);
				charArr[j].guiText.pixelOffset = wordOffset;
				//if(charArr[j].guiText.text == " ") wordOffset.x += 20;
				wordOffset.x += (charArr[j].guiText.GetScreenRect().width+20);

				//Fix texture size.
				//charArr[j].guiTexture.texture.height = (int)charArr[j].guiText.GetScreenRect().height;
				//charArr[j].guiTexture.texture.width = (int)charArr[j].guiText.GetScreenRect().width;

				Vector3 temp = charArr[j].transform.position;
				temp.z = 1;
				charArr[j].transform.position = temp;

				//Add on top of brick.
				Vector3 newPos = charArr[j].transform.localPosition;
				newPos.z = 1;
				charArr[j].transform.localPosition = newPos;

				//Add box collider.
				BoxCollider2D box = charArr[j].AddComponent<BoxCollider2D>();
				box.size = new Vector2(charArr[j].guiText.GetScreenRect().width, charArr[j].guiText.GetScreenRect().height);
				//attach a script for the collider.
				charArr[j].AddComponent("DetectOverlap");

				//Add dragging script.
				charArr[j].AddComponent("DraggableGUIElementChild");
			}
			//Delete the original GUIText.
			Destroy(fragmentBricks[i].guiText);

			//Initialize box collider.
			//ERROR: Still being tested.

			//Set previous text size for the next element.
			prevTextDimensions = textDimensions;
		}
	}


	//**********************************************************OVERLAP ALGORITHM

	public struct Overlap{
		public List<string> firstChunk;
		public List<string> secondChunk;
		public float startOverlap;
		public float endOverlap;

		public Overlap(List<string> first, List<string> second, float start, float end){
			firstChunk = first;
			secondChunk = second;
			startOverlap = start;
			endOverlap = end;
		}
	}

	//This is called when a brick has moved in or out of the construction zone.
	void FindOverlap(){
		overlapList = new List<Overlap> ();

		//put words in order
		//for each brick, look at the end point and look at the next brick's start point
		for(int i = 0; i < brickList.Count-1; i++){
			GameObject prevBrick = brickList[i];
			GameObject nextBrick = brickList[i+1];

			Vector3 temp = new Vector3((float)brickSizes[prevBrick], 0, 0);
			Vector3 temp2 = new Vector3((float)brickSizes[nextBrick], 0, 0);

			float prevBrickEnd = prevBrick.transform.position.x + (Camera.main.ScreenToViewportPoint(temp).x/2);
			float nextBrickStart = nextBrick.transform.position.x - (Camera.main.ScreenToViewportPoint(temp2).x/2);

			Debug.Log("Overlap at "+i+": ("+prevBrickEnd+", "+nextBrickStart+")");

			//if next brick start point < prev brick end point, there is overlap
			if(nextBrickStart < prevBrickEnd){
				//get all the words between the two points (can loop through the two overlapping bricks)
				List<string> firstList = new List<string>();
				List<string> secondList = new List<string>();

				float startOverlap = 0f;
				float endOverlap = 0f;

				float tempCandidate;

				//NOTE: these need to be fixed. 
				Debug.Log("Children of "+prevBrick.name+": ");
				foreach (Transform child in prevBrick.transform){
					temp = new Vector3(child.gameObject.guiText.GetScreenRect().width, 0, 0);
					tempCandidate = child.position.x - Camera.main.ScreenToViewportPoint(temp).x/2;
					Debug.Log (child.position.x);
					if(child.position.x > nextBrickStart){
						Debug.Log (child.gameObject.name);
						if(startOverlap == 0f){
							startOverlap = tempCandidate;
						}
						firstList.Add(child.gameObject.name);
					}
				}

				Debug.Log("Children of "+nextBrick.name+": ");
				foreach (Transform child in nextBrick.transform){
					tempCandidate = child.position.x + (child.gameObject.guiText.GetScreenRect().width/2f);
					Debug.Log (child.position.x);
					if(child.position.x < prevBrickEnd){
						Debug.Log (child.gameObject.name);
						endOverlap = tempCandidate;
						firstList.Add(child.gameObject.name);
					}
				}

				//store in an object that contains overlap start point, overlap end point, first chunk, second chunk
				Overlap newOverlap = new Overlap(firstList, secondList, startOverlap, endOverlap);

				overlapList.Add(newOverlap);
			}
		}



	}

	//******************************************************END OVERLAP ALGORITHM

	void Start(){
		brickTexture = (Texture2D)Resources.Load ("brick");
		makeBricks();
	}
	
	// Update is called once per frame
	void Update () {
		//FindOverlap();
		timer -= Time.deltaTime;

		if (movedBrick != null) {
			Debug.Log(movedBrick);
			Debug.Log (brickstate);
			FindOverlap();
			movedBrick = null;
		}

	}

	void OnGUI(){
		GUIStyle style = new GUIStyle ();
		style.fontSize = 100;
		style.alignment = TextAnchor.UpperLeft;
		TimeSpan time = TimeSpan.FromSeconds(timer);
		int seconds = time.Seconds;
		int minutes = time.Minutes;

		if(seconds >= 0)GUI.Label(new Rect(100, -10, Screen.width, 0), minutes.ToString()+":"+seconds.ToString(), style);
	}
}