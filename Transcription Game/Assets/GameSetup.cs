using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameSetup : MonoBehaviour {

	public Camera mainCam;

	Vector3 temp;
	
	string[] fragments = {"This is", "this is a", "is a sample", "sample."};
	public GameObject[] fragmentBricks = new GameObject[4];

	public float timer = 120f;

	Texture2D brickTexture;

	void makeBricks() {
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
		Vector2 letterOffset;

		UnityEngine.Object brickPrefab = Resources.Load ("fragmentBrick", typeof(GameObject));
		UnityEngine.Object letterPrefab = Resources.Load ("letter", typeof(GameObject));


		fragmentBricks = new GameObject[fragments.Length];

		//Initialize Menu Bar.

		for(int i = 0; i<fragments.Length; i++){
			//Erase old bricks
			Destroy(fragmentBricks[i]);

			//Draw new ones
			fragmentBricks[i] = Instantiate(brickPrefab) as GameObject;
			fragmentBricks[i].transform.parent = fragmentBrickParent.transform;
			fragmentBricks[i].name = fragments[i];
			fragmentBricks[i].guiText.text = fragments[i];

			//Scale the brick to fit the text exactly.
			textDimensions = fragmentBricks[i].guiText.GetScreenRect().width;
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
			GameObject[] charArr = new GameObject[fragments[i].Length];

			letterOffset = newOffset;

			//Loop through all the letters and readd them to the brick.
			//ERROR: lol this is pretty horrible but dont look at it
			for(int j = 0; j < fragments[i].Length; j++){
				//Create GUIText object.
				charArr[j] = Instantiate(letterPrefab) as GameObject;
				charArr[j].name = fragments[i].Substring(j, 1);

				//Put it under the GameObject parent.
				charArr[j].transform.parent = fragmentBricks[i].transform;
				charArr[j].transform.position = fragmentBricks[i].transform.position;

				//charArr[j].AddComponent<GUIText>();
				charArr[j].guiText.text = fragments[i].Substring(j, 1);
				charArr[j].guiText.fontSize = 50;
				charArr[j].guiText.anchor = TextAnchor.LowerLeft;
				charArr[j].guiText.alignment = TextAlignment.Center;

				//Fix the position.
				Debug.Log("Width of "+fragments[i].Substring(j, 1)+": "+charArr[j].guiText.GetScreenRect().width);
				charArr[j].guiText.pixelOffset = letterOffset;

				//Fix texture size.
				float letterDimensions = charArr[j].guiText.GetScreenRect().width;
				float letterDesiredScale = (float)letterDimensions/Screen.width;

				Rect tempPixelInset = charArr[j].transform.guiTexture.pixelInset;
				tempPixelInset.height = 64;
				tempPixelInset.width = charArr[j].guiText.GetScreenRect().width;
				tempPixelInset.x = letterOffset.x;
				tempPixelInset.y = letterOffset.y;
				charArr[j].transform.guiTexture.pixelInset = tempPixelInset;

				if(charArr[j].guiText.text == " ") letterOffset.x += 20;
				else letterOffset.x += charArr[j].guiText.GetScreenRect().width;

				charArr[j].guiText.text = fragments[i].Substring(j, 1); //????

				//Put the letter in front of the brick.
				charArr[j].layer = LayerMask.NameToLayer("Letters");

				Vector3 temp = charArr[j].transform.position;
				temp.z = 1;
				charArr[j].transform.position = temp;

				//Add on top of brick.
				Vector3 newPos = charArr[j].transform.localPosition;
				newPos.z = 1;
				charArr[j].transform.localPosition = newPos;

				//Add box collider.
				/*
				BoxCollider2D box = charArr[j].AddComponent<BoxCollider2D>();
				box.size = new Vector2(charArr[j].guiText.GetScreenRect().width, charArr[j].guiText.GetScreenRect().height);
				//attach a script for the collider.
				charArr[j].AddComponent("DetectOverlap");
				*/

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
		public GameObject conflictBrick;
		public int startIndex;
		public int endIndex;

		public Overlap(GameObject c, int start, int end){
			conflictBrick = c;
			startIndex = start;
			endIndex = end;
		}
	}
	
	public struct Brick{
		public GameObject brick;
		public int ID;
		public int startIndex;
		public int endIndex;

		public Brick(GameObject b, int i, int start, int end){
			brick = b;
			ID = i;
			startIndex = start;
			endIndex = end;
		}
	}

	public struct Letter{
		public GameObject letter;
		public int index;
		public GameObject brick;
		public int group;

		public Letter(GameObject singleLetter, int i, GameObject assignedBrick){
			letter = singleLetter;
			index = i;
			brick = assignedBrick;
			group = 0;
		}
	}

	void FindOverlap(){
		List<Brick> bricks = new List<Brick>();
		int index = 0;
		List<Letter> letters = new List<Letter>();

		//Results we want:
		//list of overlaps
		//parts of bricks overlapped
		//locations of overlaps

		//List of List of Objects
		//MAKE THIS PUBLIC!!!
		//(first list is overlaps, second list is bricks involved)
		//Object structure: brick ID, start index, end index

		//Loop through all the bricks to see which are in the construction area.
		for(int i = 0; i<fragmentBricks.Length; i++){
			if(fragmentBricks[i].transform.position.y == .2f){
				int startIndex = index;

				//Add letters to list of letters.
				foreach(Transform letter in fragmentBricks[i].transform){
					letters.Add(new Letter(letter.gameObject, index, fragmentBricks[i]));
					index++;
				}

				int endIndex = index - 1;

				//Add brick to our list of bricks.
				bricks.Add(new Brick(fragmentBricks[i], i, startIndex, endIndex));
				Debug.Log("Added brick: \""+fragmentBricks[i].name+"\"");
			}
		}
		//Order the list by x coordinate.
		letters.Sort(delegate(Letter a, Letter b){
			if(a.letter.transform.position.x < b.letter.transform.position.x) return 1;
			else return 2;
		});

		/*
		 * For each brick in the construction area, do the following:
			Find the first index.
			Iterate through list of IDs. Once a conflict has been detected, indicate the index where it begins and possibly where it ends.
			If a conflict already exists, then keep iterating to see if theres a bigger conflict.
		*/

		int currentGroup = 0;
		for(int i = 0; i<bricks.Count; i++){
			Brick brick = bricks[i];
			int start = brick.startIndex;
			int end = brick.endIndex;
			int next = start;

			bool startFound = false;

			//Loop through letter list in order of position.
			for(int j = 0; j<letters.Count; j++){
				Letter letter = letters[j];

				//If this letter has been recorded, get the group it's in.
				if(letter.group != 0){ 
					currentGroup = letter.group;
					continue;
				}

				//Once we find the position of the first letter
				if(startFound){
					if(letter.index == end){
						//Add to the current group.

						//No need to iterate through the remaining letters.
						break;
					}
					else{
						//Add it to the current group.
					}
				}

				//If we have not found the first letter yet
				else{
					//Check if this is the first letter.
					if(next == start){
						//Record the letter.
					}
					//If this letter is not the first letter, then move on to the next one.
					else continue;
				}
			}
		}

		//WHAT WE WANT IN THE END:
		//1. list of all overlaps
		//2. What parts of which bricks have been overlapped <-- do we need this?
		//3. Where the overlaps are visually.
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

	}

	void OnGUI(){
		TimeSpan time = TimeSpan.FromSeconds(timer);
		int seconds = time.Seconds;
		int minutes = time.Minutes;
		GUI.Label(new Rect(10, 10, Screen.width - 20, 30), minutes.ToString()+":"+seconds.ToString());
	}
}