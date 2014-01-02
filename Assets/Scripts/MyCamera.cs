using UnityEngine;
using System.Collections.Generic;

public class MyCamera : MonoBehaviour
{
	AndroidJavaObject currentActivity;
	private static int w = 640;
	private static int h = 360;

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("camera start");
		
		AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer"); 
		currentActivity = jc.GetStatic<AndroidJavaObject> ("currentActivity"); 
		currentActivity.Call ("onUnityLoaded");

		float[] cameraParam = currentActivity.Call<float[]> ("getCameraParameters");
		Matrix4x4 proj = buildProjectionMatrix (cameraParam, w, h);
		camera.projectionMatrix = proj;
	}
	
	void Update ()
	{
		bool findMarker = currentActivity.Call<bool> ("getFindMarker");
		float[] transformation = currentActivity.Call<float[]> ("getTransformation");

		Matrix4x4 transMat;
		if (!findMarker || transformation == null) {
			SetMatrix (Matrix4x4.zero);
			return;
		} else {
			transMat = new Matrix4x4 ();
			for (int i=0; i<16; i++) {
				transMat [i] = transformation [i];
			}
			SetMatrix (transMat);
		}
	}
	
	private void SetMatrix (Matrix4x4 m)
	{
		// the following code is equals to "camera.worldToCameraMatrix = m;" ??
		Vector3 pos = new Vector3 (m.m03, m.m13, m.m23);
		Quaternion q = QuaternionFromMatrix (m);
		// fix rotate around y
		q.y = -q.y;
		m = Matrix4x4.TRS (pos, q, new Vector3 (1, 1, -1));

		// stand up miku
		Quaternion objRotate = Quaternion.Euler (-90, 0, 0);
		Matrix4x4 objRotateMatrix = Matrix4x4.TRS (Vector3.zero, objRotate, Vector3.one);
		m = m * objRotateMatrix;
		camera.worldToCameraMatrix = m;
	}

	Matrix4x4 buildProjectionMatrix (float[] cameraParam, float w, float h)
	{
		float near = 0.01f;  // Near clipping distance
		float far = 100f;  // Far clipping distance
    
		Matrix4x4 projectionMatrix = new Matrix4x4 ();
		// Camera parameters
		float f_x = cameraParam [0]; // Focal length in x axis
		float f_y = cameraParam [1]; // Focal length in y axis (usually the same?)
		float c_x = cameraParam [2]; // Camera primary point x
		float c_y = cameraParam [3]; // Camera primary point y

		projectionMatrix [0] = 2.0f * f_x / w;
		projectionMatrix [1] = 0.0f;
		projectionMatrix [2] = 0.0f;
		projectionMatrix [3] = 0.0f;
    
		projectionMatrix [4] = 0.0f;
		projectionMatrix [5] = 2.0f * f_y / h;
		projectionMatrix [6] = 0.0f;
		projectionMatrix [7] = 0.0f;
    
		projectionMatrix [8] = 2.0f * c_x / w - 1.0f;
		projectionMatrix [9] = 2.0f * c_y / h - 1.0f;
		projectionMatrix [10] = -(far + near) / (far - near);
		projectionMatrix [11] = -1.0f;
    
		projectionMatrix [12] = 0.0f;
		projectionMatrix [13] = 0.0f;
		projectionMatrix [14] = -2.0f * far * near / (far - near);
		projectionMatrix [15] = 0.0f;
		
		return projectionMatrix;
	}
	
	private static Quaternion QuaternionFromMatrix (Matrix4x4 m)
	{		
		Quaternion q = Quaternion.LookRotation (m.GetColumn (2), m.GetColumn (1));
		return q;
	}
}
