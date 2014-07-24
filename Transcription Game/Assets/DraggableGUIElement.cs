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
			}
		}

		else position.y = Mathf.Clamp(position.y, border.minY, border.maxY);

		//Update position again.
		transform.position = position;
		
		lastMousePosition = GetClampedMousePosition();
	}

	void OnMouseUp()
	{
		Debug.Log(lastMousePosition);
	}
}