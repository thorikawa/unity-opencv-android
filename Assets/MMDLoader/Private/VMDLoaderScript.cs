#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class VMDLoaderScript : MonoBehaviour {

	//--------------------------------------------------------------------------------
	// ファイル読み込み
	
	public Object vmd;
	public GameObject assign_pmd;	// 適用したいPMDファイル
	public string clip_name;	// クリップの名前
	public bool create_asset;

	BinaryReader LoadFile(Object obj, string path)
	{
		FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
		BinaryReader r = new BinaryReader(f);
		return r;
	}
	
	// VMDファイル読み込み
	void LoadVMDFile()
	{
		string path = AssetDatabase.GetAssetPath(this.vmd);
		BinaryReader bin = this.LoadFile(this.vmd, path);

		// パスからクリップ名を生成 
		string[] nameBuf = path.Split('/');
		string clipNameBuf = assign_pmd.name + "_" + nameBuf[nameBuf.Length - 1].Split('.')[0];
		BurnUnityFormatForVMD(MMD.VMD.VMDLoader.Load(bin, path, clipNameBuf));
		bin.Close();
	}
	
	// Use this for initialization
	public VMDLoaderScript(Object vmdFile, GameObject assignPmdPrefab, bool createAsset)
	{
		this.vmd = vmdFile;
		this.assign_pmd = assignPmdPrefab;
		this.create_asset = createAsset;

		if (this.vmd != null)
			LoadVMDFile();
	}

	//--------------------------------------------------------------------------------
	// VMDファイルの読み込み

	Animation anim = null;

	void BurnUnityFormatForVMD(MMD.VMD.VMDFormat format)
	{
		MMD.VMD.VMDConverter conv = new MMD.VMD.VMDConverter();
		conv.CreateAnimationClip(format, this.assign_pmd, this.anim, this.create_asset);
	}
}
#endif