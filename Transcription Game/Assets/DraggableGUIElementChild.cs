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

	void Start(){
		parent = transform.parent;
		border.maxX = 1f;
		border.minX = 0f;
		border.minY = 0.12f;
		border.maxY = 1f;
	}
	
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
		
		//Update position again.
		parent.position = position;
		
		lastMousePosition = GetClampedMousePosition();
	}
	
	void OnMouseUp()
	{
		Debug.Log(lastMousePosition);
	}
}