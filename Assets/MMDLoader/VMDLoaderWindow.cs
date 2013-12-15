#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public class VMDLoaderWindow : EditorWindow {
	Object vmdFile;
	GameObject pmdPrefab;
	bool createAnimationFile;

	[MenuItem ("Plugins/MMD Loader/VMD Loader")]
	static void Init() {
		var window = (VMDLoaderWindow)EditorWindow.GetWindow<VMDLoaderWindow>(true, "VMDLoader");
		window.Show();
	}
	
	void OnGUI() {
		const int height = 20;

		pmdPrefab = EditorGUI.ObjectField(
			new Rect(0, 0, position.width - 16, height), "PMD Prefab", pmdPrefab, typeof(GameObject), false) as GameObject;
		vmdFile = EditorGUI.ObjectField(
			new Rect(0, height + 2, position.width - 16, height), "VMD File", vmdFile, typeof(Object), false);
		createAnimationFile = EditorGUI.Toggle(
			new Rect(0, height * 2 + 4, position.width - 16, height), "Create Asset", createAnimationFile);


		if (pmdPrefab != null && vmdFile != null) 
		{
			if (GUI.Button(new Rect(0, height * 3 + 6, position.width / 2, 16), "Convert"))
			{
				new VMDLoaderScript(vmdFile, pmdPrefab, createAnimationFile);
				vmdFile = null;
			}
		} 
		else 
		{
			if (pmdPrefab == null)
				EditorGUI.LabelField(new Rect(0, height * 3 + 6, position.width, height), "Missing", "Select PMD Prefab");
			else if (vmdFile == null)
				EditorGUI.LabelField(new Rect(0, height * 3 + 6, position.width, height), "Missing", "Select VMD File");
			else
				EditorGUI.LabelField(new Rect(0, height * 3 + 6, position.width, height), "Missing", "Select PMD and VMD");
		}
	}
}
#endif