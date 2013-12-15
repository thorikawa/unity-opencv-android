using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MMDSkinsScript : MonoBehaviour
{
	// 表情の種類
	public enum SkinType
	{
		Base,
		EyeBrow,
		Eye,
		Lip,
		Other,
	}

	// 全ての頂点データからターゲットとなる頂点インデックス
	public int[] targetIndices;

	// モーフ先へのベクトル
	public Vector3[] morphTarget;

	// 表情の種類
	public SkinType skinType;

	// 前フレームのウェイト値
	float prev_weight = 0;

	// Use this for initialization
	void Start () 
	{
		
	}

	// モーフの計算
	public bool Compute(Vector3[] composite)
	{
		bool computed_morph = false;	// 計算したかどうか

		float weight = transform.localPosition.z;

		if (weight != prev_weight)
		{
			computed_morph = true;
			for (int i = 0; i < targetIndices.Length; i++)
				composite[targetIndices[i]] = morphTarget[i] * weight;
		}

		prev_weight = weight;
		return computed_morph;
	}

	public void Compute(Vector3[] vtxs, int[] indices, Vector3[] source)
	{
		float weight = transform.localPosition.z;

		if (weight != prev_weight)
		{
			for (int i = 0; i < targetIndices.Length; i++)
			{
				//vtxs[targetIndices[i]] += morphTarget[i] * weight * 0.1f;
				vtxs[indices[targetIndices[i]]] = source[targetIndices[i]] + morphTarget[i] * weight * 0.1f;
			}
		}

		prev_weight = weight;
	}
}
