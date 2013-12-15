#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class PMDLoaderScript {

	//--------------------------------------------------------------------------------
	// ファイル読み込み
	
	GameObject obj;
	public Object pmd;
	public bool rigidFlag;		// 物理や剛体のオン・オフ
	public MMD.PMD.ShaderType shader_type;
	public bool use_mecanim;
	public bool use_ik;

	FileStream fst;		// テスト用

	BinaryReader LoadFile(Object obj, string path) {
		FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
		this.fst = f;
		BinaryReader r = new BinaryReader(f);
		return r;
	}
		
	// PMDファイル読み込み
	void LoadPMDFile() {
		string path = AssetDatabase.GetAssetPath(this.pmd);
		BinaryReader bin = this.LoadFile(this.pmd, path);
		MMD.PMD.PMDFormat format = MMD.PMD.PMDLoader.Load(bin, obj, path);
		BurnUnityFormatForPMD(format);
		bin.Close();
	}
		
	// Use this for initialization
	public PMDLoaderScript (Object pmdFile, MMD.PMD.ShaderType shader_type, bool rigidFlag, bool use_mecanim, bool use_ik) {
		this.pmd = pmdFile;
		this.rigidFlag = rigidFlag;
		this.shader_type = shader_type;
		this.use_mecanim = use_mecanim;
		this.use_ik = use_ik;

		if (this.pmd != null) {
			LoadPMDFile();
		}
	}

	//--------------------------------------------------------------------------------
	// PMDファイルの読み込み
	
	Mesh mesh;
	Material[] materials;
	GameObject[] bones;
	GameObject[] rigids;
	
	void CreatePrefab(MMD.PMD.PMDFormat format)
	{
		Object prefab = PrefabUtility.CreateEmptyPrefab(format.folder + "/" + format.name + ".prefab");
#if UNITY_4_0 || UNITY_3_5
		// 4.0からPrefabUtilityを使えとのこと
		//   3.5.6f4でもコンソールで警告がでていたので3.5もこっち
		PrefabUtility.ReplacePrefab(format.caller, prefab);
#elif UNITY_3_4
		EditorUtility.ReplacePrefab(format.caller, prefab);
#endif
	}
	
	void EndOfScript(MMD.PMD.PMDFormat format)
	{
		AssetDatabase.Refresh();
		
		this.mesh = null;
		this.materials = null;
		this.bones = null;
		this.pmd = null;
	}
	
	// PMDファイルをUnity形式に変換
	void BurnUnityFormatForPMD(MMD.PMD.PMDFormat format) {
		format.fst = this.fst;
		obj = new GameObject(format.name);
		format.caller = obj;
		format.shader_type = this.shader_type;
		
		MMD.PMD.PMDConverter conv = new MMD.PMD.PMDConverter();
		
		this.mesh = conv.CreateMesh(format);				// メッシュの生成・設定
		this.materials = conv.CreateMaterials(format);		// マテリアルの生成・設定
		this.bones = conv.CreateBones(format);				// ボーンの生成・設定

		// バインドポーズの作成
		conv.BuildingBindpose(format, this.mesh, this.materials, this.bones);
		obj.AddComponent<Animation>();	// アニメーションを追加

		MMDEngine engine = obj.AddComponent<MMDEngine>();

		// IKの登録
		if(this.use_ik)
			engine.ik_list = conv.EntryIKSolver(format, this.bones);

		// 剛体関連
		if (this.rigidFlag)
		{
			try
			{
				this.rigids = conv.CreateRigids(format, bones);
				conv.AssignRigidbodyToBone(format, this.bones, this.rigids);
				conv.SetRigidsSettings(format, this.bones, this.rigids);
				conv.SettingJointComponent(format, this.bones, this.rigids);

				// 非衝突グループ
				List<int>[] ignoreGroups = conv.SettingIgnoreRigidGroups(format, this.rigids);
				int[] groupTarget = conv.GetRigidbodyGroupTargets(format, rigids);

				MMDEngine.Initialize(engine, groupTarget, ignoreGroups, this.rigids);
			}
			catch { }
		}

#if UNITY_4_0
		AvatarSettingScript avt_setting = new AvatarSettingScript(format.caller);
		avt_setting.SettingAvatar();
#endif

		var window = LoadedWindow.Init();
		window.Text = format.head.comment;
		
		CreatePrefab(format);
		EndOfScript(format);
	}
}
#endif