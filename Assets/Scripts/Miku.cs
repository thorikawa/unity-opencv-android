using UnityEngine;
using System.Collections;

public class Miku : MonoBehaviour
{
	private Matrix4x4 inverseMatrix;
	
	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
	
	void JavaMessage (string message)
	{ 
		Debug.Log ("message from java: " + message);
		string[] rects = message.Split (new char[] {'-'});
		if (rects [0].Equals ("X")) {
			renderer.enabled = false;
			return;
		}
		renderer.enabled = true;
		Rect r = getRect (rects [0]);
		Vector3 c = getCenter (r);
		// Debug.Log ("center: " + c.ToString());
		
		Plane hPlane = new Plane (Vector3.forward, Vector3.zero);
		float distance = 0; 
		Ray ray = Camera.main.ScreenPointToRay (c);
		if (hPlane.Raycast (ray, out distance)) {
			Vector3 newPos = ray.GetPoint (distance);
			newPos.x += 7.0F;
			// Debug.Log (newPos);
			transform.position = newPos;
			
		}
	}

	Rect getRect (string s)
	{
		string[] datas = s.Split (new char[] {'_'});
		float x = float.Parse (datas [0]);
		float y = float.Parse (datas [1]);
		float w = float.Parse (datas [2]);
		float h = float.Parse (datas [3]);
		return new Rect (x, y, w, h);
	}

	Vector3 getCenter (Rect rect)
	{
		return new Vector3 ((rect.xMin + rect.xMax) / 2.0F, (rect.yMin + rect.yMax) / 2.0F, 0.0F);
	}
}
