using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
public class XFileImporterWindow : EditorWindow {
	Object xFile = null;
	
	[MenuItem ("Plugins/XFile Importer")]
	static void Init() {
		var window = (XFileImporterWindow)EditorWindow.GetWindow<XFileImporterWindow>(true, "XFile Importer");
		window.Show();
	}
	
	void OnGUI() {
		const int height = 20;
		
		xFile = EditorGUI.ObjectField(
			new Rect(0, 0, position.width-16, height), "XFile" ,xFile, typeof(Object));
		
		if (xFile != null) {
			if (GUI.Button(new Rect(0, height+2, position.width/2, height), "Convert")) {
				XFileImporter imp = new XFileImporter(xFile);
				xFile = null;		// 読み終わったので空にする 
			}
		} else {
			EditorGUI.LabelField(new Rect(0, height+2, position.width, height), "Missing", "Select XFile");
		}
	}
}
#endif