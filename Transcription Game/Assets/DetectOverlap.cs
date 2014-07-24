using UnityEngine;
using System.Collections;

public class DetectOverlap : MonoBehaviour {

	string string1 = "1";
	string string2 = "2";

	void OnCollisionEnter2D (Collision2D col)
	{
		//if(col.gameObject.name == "FragmentBrick")
		//{
			Debug.Log("Collision detected between \""+string1+"\" and \""+string2+"\".");
		//}
	}
}
