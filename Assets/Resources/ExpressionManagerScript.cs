using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 表情の管理クラス
/// </summary>
public class ExpressionManagerScript : MonoBehaviour
{
	public int[] indices;

	public Vector3[] source;		// 元頂点, source_position

	public Vector3[] composite;

	public Vector3[] prev_comp;

	public Mesh mesh;	// メッシュ

	public MMDSkinsScript[] skin_script;	// 子供の表情のスクリプト配列

	//int lip_count = 0;
	//int eye_count = 0;
	//int eye_brow_count = 0;
	//int other_count = 0;

	void Init()
	{
		// meshの取得
		mesh = transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh;

		// 頂点インデックス取得
		indices = transform.FindChild("base").GetComponent<MMDSkinsScript>().targetIndices;

		skin_script = GetSkinScripts();		// 表情に付いているスクリプトを取得

		// 元頂点配列に入れていく
		source = new Vector3[indices.Length];
		for (int i = 0; i < indices.Length; i++)
			source[i] = mesh.vertices[indices[i]];

		// 合成するベクトル配列
		composite = new Vector3[indices.Length];
		for (int i = 0; i < indices.Length; i++)
			composite[i] = Vector3.zero;

		// 前のフレームの合成ベクトルはここでコピー
		prev_comp = new Vector3[indices.Length];
		Array.Copy(composite, prev_comp, indices.Length);

	}

	void Start()
	{
		Init();
	}

	// SkinScriptの配列を子供の表情から探して拾ってくる
	MMDSkinsScript[] GetSkinScripts()
	{
		// 表情のスクリプトを拾ってくる
		var scripts = new MMDSkinsScript[transform.GetChildCount()];
		for (int i = 0; i < scripts.Length; i++)
			scripts[i] = transform.GetChild(i).GetComponent<MMDSkinsScript>();

		return scripts;
	}

	void Update()
	{
		var vtxs = mesh.vertices;	// 配列を受け入れ

		// 表情ごとに計算する
		foreach (var s in this.skin_script)
		{
			s.Compute(composite);
			//bool computed_morph = s.Compute(composite);
			//if (computed_morph)
			//{
			//    // モーフした表情の種類によってカウント
			//    switch (s.skinType)
			//    {
			//        case MMDSkinsScript.SkinType.Eye:
			//            eye_count++;
			//            break;

			//        case MMDSkinsScript.SkinType.EyeBrow:
			//            eye_brow_count++;
			//            break;

			//        case MMDSkinsScript.SkinType.Lip:
			//            lip_count++;
			//            break;

			//        case MMDSkinsScript.SkinType.Other:
			//            other_count++;
			//            break;
			//    }
			//}
		}

		// ここで計算結果を入れていく
		for (int i = 0; i < indices.Length; i++)
		{
			if (prev_comp[i] != composite[i])
			{
				vtxs[indices[i]] = source[i] + composite[i];
			}
		}
		Array.Copy(composite, prev_comp, indices.Length);

		mesh.vertices = vtxs;	// ここで反映
		/*
		 * ノート
		 * どうやらsharedMeshはAssetを共有しているため
		 * シーン内に複数ある時に変形すると全体が変形してしまう
		 */
	}

	void OnApplicationQuit()
	{
		// アプリ終了時に頂点を元に戻す
		var vtxs = mesh.vertices;
		for (int i = 0; i < indices.Length; i++)
		{
			vtxs[indices[i]] = source[i];
		}
		mesh.vertices = vtxs;
	}
}
