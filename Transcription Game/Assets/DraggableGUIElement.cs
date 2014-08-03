using UnityEngine;
using System.Collections;

public class DraggableGUIElement : MonoBehaviour
{
	[System.Serializable]
	public class Border
	{
		public float minX,maxX,minY,maxY;
	}
	
	public Border border;
	
	Vector3 lastMousePosition;

	public GameSetup gamesetup;

	void OnMouseDown()
	{
		lastMousePosition = GetClampedMousePosition();

		GameObject fragmentBrickParent = GameObject.Find("fragmentBricks");
		GameObject fragmentBrickTopParent = GameObject.Find("fragmentBricks/top");

		/*
		if(fragmentBrickTopParent.transform.childCount > 0){
			Transform oldTop = fragmentBrickTopParent.transform.GetChild(0);
			oldTop.transform.parent = fragmentBrickParent.transform;
		}
		transform.parent = fragmentBrickTopParent.transform;
		*/
	}
	
	Vector3 GetClampedMousePosition()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x = Mathf.Clamp(mousePosition.x, 0f, Screen.width);
		mousePosition.y = Mathf.Clamp(mousePosition.y, 0f, Screen.height);
		
		return mousePosition;
	}

	//Detects whether the mouse cursor is actually within the GameObject.
	//ERROR: collider2D bounds seem to be (0,0), not sure why.
	bool isInGameObject(){
		Vector3 pos = transform.position;

		Collider2D collider2D= gameObject.GetComponent<Collider2D>();
		//float width = collider2D.bounds.size.x;
		//float height = collider2D.bounds.size.y;

		//Debug.Log ("width: "+width+"\nheight: "+height);

		return true;
	}
	
	void OnMouseDrag()
	{
		//Check if cursor is actually on the GameObject. If not, don't do anything.
		if(!isInGameObject()) return;

		//delta gets the change in position.
		Vector3 delta = GetClampedMousePosition() - lastMousePosition;
		
		delta = Camera.main.ScreenToViewportPoint(delta);

		//Update the GameObject's position.
		transform.position += delta;

		//Clamp the position to borders.
		Vector3 position = transform.position;
		position.x = Mathf.Clamp(position.x, border.minX, border.maxX);

		//Debug.Log(GetClampedMousePosition().y);

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
		transform.position = position;
		
		lastMousePosition = GetClampedMousePosition();
	}

	//Checks if the gameObject is in the list.
	bool isInList(){
		bool flag = false;
		for(int i = 0; i < gamesetup.brickList.Count; i++){
			if (gamesetup.brickList[i].GetInstanceID() == gameObject.GetInstanceID()){
				flag = true;
				break;
			}
		}

		return flag;
	}

	//Updates list when brick is moved around.
	void UpdateList(){

	}

	void OnMouseUp()
	{
		//If the user begins to drag the brick out but doesn't finish the action, snap it back in.
		Vector3 position = transform.position;
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
				Vector3 temp2 = new Vector3((float)gamesetup.brickSizes[gameObject], 0, 0);
				float endPos = startPos + Camera.main.ScreenToViewportPoint(temp).x/2 + Camera.main.ScreenToViewportPoint(temp2).x/2 + .01f;


				position.x = endPos;
			}

			//Add the brick to the list if it isn't already.
			if(!isInList()){
				gamesetup.brickList.Add(gameObject);
			}
			else Debug.Log ("Already in list!");
		}
		else{
			if(isInList()){
				gamesetup.brickList.Remove(gameObject);
				//reset position of all objects after it.
			}
		}

		transform.position = position;

		//Tell GM that a brick has moved.
		gamesetup.movedBrick = gameObject;
		Debug.Log(lastMousePosition);
	}

	void Start(){
		gamesetup = GameObject.Find("_GM").GetComponent<GameSetup>();
	}
}