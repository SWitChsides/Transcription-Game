using UnityEngine;
using System.Collections;

public class DraggableGUIElementChild : MonoBehaviour
{
	[System.Serializable]
	public class Border
	{
		public float minX,maxX,minY,maxY;

		public Border(float x1,float x2,float y1,float y2){
			minX = x1;
			maxX = x2;
			minY = y1;
			maxY = y2;
		}
	}
	
	Border border = new Border(0f, 1f, 0.12f, 1f);
	
	Vector3 lastMousePosition;

	Transform parent;

	public GameSetup gamesetup;

	void PrintList(){
		Debug.Log ("==BRICK LIST==");
		for (int i = 0; i<gamesetup.brickList.Count; i++) {
			Debug.Log (gamesetup.brickList[i]);
		}
		Debug.Log ("==END==");
	}

	void Start(){
		parent = transform.parent;
		border.maxX = 1f;
		border.minX = 0f;
		border.minY = 0.12f;
		border.maxY = 1f;

		gamesetup = GameObject.Find("_GM").GetComponent<GameSetup>();
	}
	
	void OnMouseDown()
	{
		lastMousePosition = GetClampedMousePosition();
		
		GameObject fragmentBrickParent = GameObject.Find("fragmentBricks");
		GameObject fragmentBrickTopParent = GameObject.Find("fragmentBricks/top");
	}
	
	Vector3 GetClampedMousePosition()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x = Mathf.Clamp(mousePosition.x, 0f, Screen.width);
		mousePosition.y = Mathf.Clamp(mousePosition.y, 0f, Screen.height);
		
		return mousePosition;
	}

	//Checks if the gameObject is in the list.
	bool isInList(){
		bool flag = false;
		for(int i = 0; i < gamesetup.brickList.Count; i++){
			if (gamesetup.brickList[i].GetInstanceID() == parent.gameObject.GetInstanceID()){
				flag = true;
				break;
			}
		}
		
		return flag;
	}

	void OnMouseDrag()
	{
		//Check if cursor is actually on the GameObject. If not, don't do anything.
		//if(!isInGameObject()) return;
		
		//delta gets the change in position.
		Vector3 delta = GetClampedMousePosition() - lastMousePosition;
		
		delta = Camera.main.ScreenToViewportPoint(delta);
		
		//Update the GameObject's position.
		parent.position += delta;
		
		//Clamp the position to borders.
		Vector3 position = parent.position;
		position.x = Mathf.Clamp(position.x, border.minX, border.maxX);
		
		//Debug.Log(GetClampedMousePosition().y);
		
		//Snapping to Construction Area.
		if(GetClampedMousePosition().y < 300){
			//This condition is a way to disable snapping when dragging the piece back out.
			if(lastMousePosition.y > GetClampedMousePosition().y){
				position.y = 0.2f;
			}
		}
		
		else position.y = Mathf.Clamp(position.y, border.minY, border.maxY);

		//Snapping to Construction Area.
		if(GetClampedMousePosition().y < 300){
			//This condition is a way to disable snapping when dragging the piece back out.
			if(lastMousePosition.y > GetClampedMousePosition().y){
				position.y = 0.2f;
				gamesetup.brickstate = GameSetup.BrickState.IN;
			}
		}
		
		else {
			position.y = Mathf.Clamp(position.y, border.minY, border.maxY);
			gamesetup.brickstate = GameSetup.BrickState.OUT;
		}

		//Update position again.
		parent.position = position;
		
		lastMousePosition = GetClampedMousePosition();
	}
	
	void OnMouseUp()
	{
		//If the user begins to drag the brick out but doesn't finish the action, snap it back in.
		Vector3 position = parent.position;
		position.x = Mathf.Clamp(position.x, border.minX, border.maxX);
		
		if(GetClampedMousePosition().y < 300){

			position.y = 0.2f;
			
			//Snap the piece to the right x position, too.
			if(gamesetup.brickList.Count == 0){
				//If there are no other bricks in the construction zone, snap to the beginning.
				position.x = 0f;
			}
			else{
				//If there are, check where the brick is currently in relation to the last brick.
				GameObject last = gamesetup.brickList[gamesetup.brickList.Count - 1];
				float startPos = last.transform.position.x;
				
				Vector3 temp = new Vector3((float)gamesetup.brickSizes[last], 0, 0);
				Vector3 temp2 = new Vector3((float)gamesetup.brickSizes[parent.gameObject], 0, 0);
				float endPos = startPos + Camera.main.ScreenToViewportPoint(temp).x/2 + Camera.main.ScreenToViewportPoint(temp2).x/2 + .011f;
				
				if(parent.gameObject.transform.position.x > endPos) position.x = endPos;
				else if(parent.gameObject.transform.position.x < startPos) position.x = endPos;
			}

			//Add the brick to the list if it isn't already.
			if(!isInList()){
				gamesetup.brickList.Add(parent.gameObject);
			}
			else{
				int index = gamesetup.brickList.IndexOf(parent.gameObject);
				gamesetup.brickList.Remove(parent.gameObject);
				
				Vector3 temp = new Vector3((float)gamesetup.brickSizes[parent.gameObject], 0, 0);
				Vector3 temp2 = new Vector3(0,0,0); 
				if(index < gamesetup.brickList.Count) {
					temp2 = new Vector3((float)gamesetup.brickSizes[gamesetup.brickList[index]], 0, 0);
				}
				
				Vector3 offset = new Vector3(0,0,0);
				offset.x = Camera.main.ScreenToViewportPoint(temp).x/2 + Camera.main.ScreenToViewportPoint(temp2).x/2 + .011f;
				
				//reset position of all objects after it.
				for(int i = index; i < gamesetup.brickList.Count; i++){
					Vector3 tempPos = gamesetup.brickList[i].transform.position;
					tempPos -= offset;
					gamesetup.brickList[i].transform.position = tempPos;
				}
				
				gamesetup.brickList.Add(parent.gameObject);
			}
		}
		else{
			if(isInList()){
				int index = gamesetup.brickList.IndexOf(parent.gameObject);
				gamesetup.brickList.Remove(parent.gameObject);
				
				Vector3 temp = new Vector3((float)gamesetup.brickSizes[parent.gameObject], 0, 0);
				Vector3 temp2 = new Vector3(0,0,0); 
				if(index < gamesetup.brickList.Count) {
					temp2 = new Vector3((float)gamesetup.brickSizes[gamesetup.brickList[index]], 0, 0);
				}
				
				Vector3 offset = new Vector3(0,0,0);
				offset.x = Camera.main.ScreenToViewportPoint(temp).x/2 + Camera.main.ScreenToViewportPoint(temp2).x/2 + .011f;
				
				//reset position of all objects after it.
				for(int i = index; i < gamesetup.brickList.Count; i++){
					Vector3 tempPos = gamesetup.brickList[i].transform.position;
					tempPos -= offset;
					gamesetup.brickList[i].transform.position = tempPos;
				}
			}
		}
		
		parent.position = position;
		
		//Tell GM that a brick has moved.
		gamesetup.movedBrick = parent.gameObject;
		Debug.Log(position);

		PrintList();
	}
}