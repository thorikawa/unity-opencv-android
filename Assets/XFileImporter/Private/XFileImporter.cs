using UnityEngine;
using System.Collections;

/*
 * 1. Google Sketchupのプラグインをインストール 
 * http://www.3drad.com/Google-SketchUp-To-DirectX-XNA-Exporter-Plug-in.htm
 * （プラグインはSketchupをインストールしたフォルダのPluginsフォルダにスクリプトを入れればOK） 
 * 
 * 2. Xファイルの出力 
 * 適当にフォルダを指定して出力 
 * 
 * 3. Projectへの読み込み 
 * 出力したフォルダごとProjectに投げればOK 
 * 
 * 4. スクリプトの適用 
 * 適当なGameObjectにこのスクリプトを適用させる 
 * xFile変数にロードしたXファイルをD&Dして実行すれば出来上がり 
 * 
 * 諸注意 
 * Xファイルは方言が多いので、現在は1.のプラグインで出力したファイルのみ対応 
 */

#if UNITY_EDITOR

public class XFileImporter {
	
	public Object xFile;
	
	Object prefab;
	Mesh mesh;
	Material[] material;
	
	// Use this for initialization
	public XFileImporter(Object xFile) {
		xfile.XFileConverter cnv = new xfile.XFileConverter(xFile);
		
		prefab = cnv.CreatePrefab();
		material = cnv.CreateMaterials();
		mesh = cnv.CreateMesh();
		cnv.ReplacePrefab(prefab, mesh, material);
	}
}

#endif