using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Text;

// Reference URL:
//	  http://blog.goo.ne.jp/torisu_tetosuki/e/209ad341d3ece2b1b4df24abf619d6e4
//	  http://mikudan.blog120.fc2.com/blog-entry-280.html

namespace MMD
{
	public class Format : IComparable
	{
		// ShiftJISからUTF-8に変換してstringで返す
		protected string ConvertByteToString(byte[] bytes)
		{
			// パディングの消去, 文字を詰める
			if (bytes[0] == 0) return "";
			int count;
			for (count = 0; count < bytes.Length; count++) if (bytes[count] == 0) break;
			byte[] buf = new byte[count];		// NULL文字を含めるとうまく行かない
			for (int i = 0; i < count; i++) {
				buf[i] = bytes[i];	
			}

#if UNITY_STANDALONE_OSX
			buf = Encoding.Convert(Encoding.GetEncoding(932), Encoding.UTF8, buf);
#else
			buf = Encoding.Convert(Encoding.GetEncoding(0), Encoding.UTF8, buf);
#endif
			return Encoding.UTF8.GetString(buf).Replace("\n", "");
		}

		protected float[] ReadSingles(BinaryReader bin, uint count)
		{
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return result;
		}
			
		protected Vector3 ReadSinglesToVector3(BinaryReader bin)
		{
			const int count = 3;
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return new Vector3(result[0], result[1], result[2]);
		}
			
		protected Vector2 ReadSinglesToVector2(BinaryReader bin)
		{
			const int count = 2;
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return new Vector2(result[0], result[1]);
		}
			
		protected Color ReadSinglesToColor(BinaryReader bin)
		{
			const int count = 4;
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return new Color(result[0], result[1], result[2], result[3]);
		}
			
		protected Color ReadSinglesToColor(BinaryReader bin, float fix_alpha)
		{
			const int count = 3;
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return new Color(result[0], result[1], result[2], fix_alpha);
		}

		protected uint[] ReadUInt32s(BinaryReader bin, uint count)
		{
			uint[] result = new uint[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadUInt32();
			}
			return result;
		}

		protected ushort[] ReadUInt16s(BinaryReader bin, uint count)
		{
			ushort[] result = new ushort[count];
			for (uint i = 0; i < count; i++)
			{
				result[i] = bin.ReadUInt16();
			}
			return result;
		}
		
		protected Quaternion ReadSinglesToQuaternion(BinaryReader bin)
		{
			const int count = 4;
			float[] result = new float[count];
			for (int i = 0; i < count; i++)
			{
				result[i] = bin.ReadSingle();
			}
			return new Quaternion(result[0], result[1], result[2], result[3]);
		}
		
		// ソート用の何か
		protected int count = 0;
		public int CompareTo(object obj)
		{
			return count - ((Format)obj).count;
		}
	}
	
	namespace PMD
	{
		// PMDファイルの読み込みクラス
		public class PMDLoader
		{
			public static PMDFormat Load(BinaryReader bin, GameObject caller, string path)
			{
				return new PMDFormat(bin, caller, path);
			}
		}

		/// <summary>
		/// シェーダの種類
		/// </summary>
		public enum ShaderType
		{
			Default,		/// Unityのデフォルトシェーダ
			HalfLambert,	/// もやっとしたLambertっぽくなる
			MMDShader		/// MMDっぽいシェーダ
		}
		
		// PMDのフォーマットクラス
		public class PMDFormat : MMD.Format
		{
			public string path;			// フルパス
			public string name;			// 拡張子とパス抜きのファイルの名前
			public string folder;		// ファイル名抜きのパス
			public GameObject caller;	// MMDLoaderScirptを適用したオブジェクト
			public ShaderType shader_type;	// シェーダの種類

			public FileStream fst;		// テスト用
			
			public Header head;
			public VertexList vertex_list;
			public FaceVertexList face_vertex_list;
			public MaterialList material_list;
			public BoneList bone_list;
			public IKList ik_list;
			public SkinList skin_list;
			public SkinNameList skin_name_list;
			public BoneNameList bone_name_list;
			public BoneDisplayList bone_display_list;
			public EnglishHeader eg_head;
			public EnglishBoneNameList eg_bone_name_list;
			public EnglishSkinNameList eg_skin_name_list;
			public EnglishBoneDisplayList eg_bone_display_list;
			public ToonTextureList toon_texture_list;
			public RigidbodyList rigidbody_list;
			public RigidbodyJointList rigidbody_joint_list;
			
			int read_count = 0;
			
			void EntryPathes(string path)
			{
				this.path = path;
				string[] buf = path.Split('/');
				this.name = buf[buf.Length-1];
				this.name = name.Split('.')[0];		// .pmdを抜かす
				
				// PMDが格納されているフォルダ
				this.folder = buf[0];
				for (int i = 1; i < buf.Length-1; i++)
					this.folder += "/" + buf[i];
			}

			public PMDFormat(BinaryReader bin, GameObject caller, string path)
			{
				EntryPathes(path);
				
				this.caller = caller;
				
				try {
					this.head = new Header(bin);
					this.vertex_list = new VertexList(bin);
					this.face_vertex_list = new FaceVertexList(bin);
					this.material_list = new MaterialList(bin);
					this.bone_list = new BoneList(bin); 
					this.ik_list = new IKList(bin); read_count++;
					this.skin_list = new SkinList(bin); read_count++;
					this.skin_name_list = new SkinNameList(bin);
					this.bone_name_list = new BoneNameList(bin);
					this.bone_display_list = new BoneDisplayList(bin);
					this.eg_head = new EnglishHeader(bin);
					if (this.eg_head.english_name_compatibility != 0)
					{
						this.eg_bone_name_list = new EnglishBoneNameList(bin, bone_list.bone_count);
						this.eg_skin_name_list = new EnglishSkinNameList(bin, skin_list.skin_count);
						this.eg_bone_display_list = new EnglishBoneDisplayList(bin, bone_name_list.bone_disp_name_count);
					}
					this.toon_texture_list = new ToonTextureList(bin);
					this.rigidbody_list = new RigidbodyList(bin);
					this.rigidbody_joint_list = new RigidbodyJointList(bin);
				} catch {
					Debug.Log("Don't read full format");
				}
			}

			public class Header : MMD.Format
			{
				public byte[] magic; // "Pmd"
				public float version; // 00 00 80 3F == 1.00
				public string model_name;
				public string comment;

				public Header(BinaryReader bin)
				{
					this.magic = bin.ReadBytes(3);
					this.version = bin.ReadSingle();
					this.model_name = base.ConvertByteToString(bin.ReadBytes(20));
					this.comment = base.ConvertByteToString(bin.ReadBytes(256));
				}
			}

			public class VertexList : MMD.Format
			{
				public uint vert_count; // 頂点数
				public Vertex[] vertex;  // 頂点データ(38bytes/頂点)

				public VertexList(BinaryReader bin)
				{
					this.vert_count = bin.ReadUInt32();
					this.vertex = new Vertex[vert_count];
					for (int i = 0; i < this.vert_count; i++)
						this.vertex[i] = new Vertex(bin);
				}
			}

			public class Vertex : MMD.Format
			{
				public Vector3 pos; // x, y, z // 座標
				public Vector3 normal_vec; // nx, ny, nz // 法線ベクトル
				public Vector2 uv; // u, v // UV座標 // MMDは頂点UV
				public ushort[] bone_num; // ボーン番号1、番号2 // モデル変形(頂点移動)時に影響
				public byte bone_weight; // ボーン1に与える影響度 // min:0 max:100 // ボーン2への影響度は、(100 - bone_weight)
				public byte edge_flag; // 0:通常、1:エッジ無効 // エッジ(輪郭)が有効の場合

				public Vertex(BinaryReader bin)
				{
					this.pos = base.ReadSinglesToVector3(bin);
					this.normal_vec = base.ReadSinglesToVector3(bin);
					this.uv = base.ReadSinglesToVector2(bin);
					this.bone_num = base.ReadUInt16s(bin, 2);
					this.bone_weight = bin.ReadByte();
					this.edge_flag = bin.ReadByte();
				}
			}

			// 面頂点リスト
			public class FaceVertexList : MMD.Format
			{
				public uint face_vert_count; // 頂点数
				public ushort[] face_vert_index; // 頂点番号(3個/面)

				public FaceVertexList(BinaryReader bin)
				{
					this.face_vert_count = bin.ReadUInt32();
					this.face_vert_index = base.ReadUInt16s(bin, this.face_vert_count);
				}
			}

			public class MaterialList : MMD.Format
			{
				public uint material_count; // 材質数
				public Material[] material; // 材質データ(70bytes/material)

				public MaterialList(BinaryReader bin)
				{
					this.material_count = bin.ReadUInt32();
					this.material = new Material[this.material_count];
					for (int i = 0; i < this.material_count; i++)
						this.material[i] = new Material(bin);
				}
			}

			public class Material : MMD.Format
			{
				public Color diffuse_color; // dr, dg, db // 減衰色
				public float alpha;
				public float specularity;
				public Color specular_color; // sr, sg, sb // 光沢色
				public Color mirror_color; // mr, mg, mb // 環境色(ambient)
				public byte toon_index; // toon??.bmp // 0.bmp:0xFF, 1(01).bmp:0x00 ・・・ 10.bmp:0x09
				public byte edge_flag; // 輪郭、影
				public uint face_vert_count; // 面頂点数 // インデックスに変換する場合は、材質0から順に加算
				public string texture_file_name; // テクスチャファイル名またはスフィアファイル名 // 20バイトぎりぎりまで使える(終端の0x00は無くても動く)
				public string sphere_map_name;	// スフィアマップ用
				
				/*
				テクスチャファイル名またはスフィアファイル名の補足：

				テクスチャファイルにスフィアファイルを乗算または加算する場合
				(MMD 5.12以降)
				"テクスチャ名.bmp*スフィア名.sph" で乗算
				"テクスチャ名.bmp*スフィア名.spa" で加算

				(MMD 5.11)
				"テクスチャ名.bmp/スフィア名.sph" で乗算

				(MMD 5.09あたり-)
				"テクスチャ名.bmp" または "スフィア名.sph"
				*/
				
				string CutTheUnknownDotSlash(string str)
				{
					string result = "";
					string[] buf = str.Split('/');
					if (buf[0] == ".") {
						result += buf[1];
						for (int i = 2; i < buf.Length; i++) {
							result += "/" + buf[i];
						}
					} else {
						result = str;
					}
					return result;
				}

				public Material(BinaryReader bin)
				{
					this.diffuse_color = base.ReadSinglesToColor(bin, 1);
					this.alpha = bin.ReadSingle();
					this.specularity = bin.ReadSingle();
					this.specular_color = base.ReadSinglesToColor(bin, 1);
					this.mirror_color = base.ReadSinglesToColor(bin, 1);
					this.toon_index = bin.ReadByte();
					this.edge_flag = bin.ReadByte();
					this.face_vert_count = bin.ReadUInt32();
					
					// テクスチャ名の抜き出し
					// スフィアマップも行う
					string buf = base.ConvertByteToString( bin.ReadBytes(20));
					
					//Debug by Wilfrem: テクスチャが無い場合を考慮していない
					//Debug by Wilfrem: テクスチャはfoo.bmp*bar.sphのパターンだけなのか？ bar.sph*foo.bmpのパターンがあり得るのでは？ 対策をしておくべき
					//Debug by GRGSIBERIA: スフィアマップとテクスチャが逆になる現象が発生したので修正
					//Debug by GRGSIBERIA: "./テクスチャ名"で始まるモデルで異常発生したので修正
					if(!string.IsNullOrEmpty(buf.Trim())){
						string[] textures = buf.Trim().Split('*');
						foreach(var tex in textures){
							string texNameEndAssignVar = "";
							string ext = Path.GetExtension(tex);
							if(ext == ".sph" || ext == ".spa"){
								this.sphere_map_name = tex;
							}/* else if (string.IsNullOrEmpty(tex)) {
								this.texture_file_name="";
							} */else {
								if (tex.Split('/')[0] == ".") {
									// テクスチャ名の後端に"./"があった場合の回避処理 
									string[] texNameBuf = tex.Split('/');
									for (int i = 1; i < texNameBuf.Length-1; i++)
										texNameEndAssignVar += texNameBuf[i] + "/";
									texNameEndAssignVar += texNameBuf[texNameBuf.Length-1];
								} else {
									// 特に異常がない場合はそのまま代入 
									texNameEndAssignVar = tex;
								}
								this.texture_file_name = texNameEndAssignVar;
							}
						}
					} else {
						this.sphere_map_name="";
						this.texture_file_name="";
					}
					if (string.IsNullOrEmpty(texture_file_name)) this.texture_file_name = "";
				}
			}

			public class BoneList : MMD.Format
			{
				public ushort bone_count; // ボーン数
				public Bone[] bone; // ボーンデータ(39bytes/bone)

				public BoneList(BinaryReader bin)
				{
					this.bone_count = bin.ReadUInt16();
					//Debug.Log("BoneCount:"+bone_count);
					this.bone = new Bone[this.bone_count];
					for (int i = 0; i < this.bone_count; i++)
						this.bone[i] = new Bone(bin);
				}
			}

			public class Bone : MMD.Format
			{
				public string bone_name; // ボーン名
				public ushort parent_bone_index; // 親ボーン番号(ない場合は0xFFFF)
				public ushort tail_pos_bone_index; // tail位置のボーン番号(チェーン末端の場合は0xFFFF) // 親：子は1：多なので、主に位置決め用
				public byte bone_type; // ボーンの種類
				public ushort ik_parent_bone_index; // IKボーン番号(影響IKボーン。ない場合は0)
				public Vector3 bone_head_pos; // x, y, z // ボーンのヘッドの位置

				/*
				・ボーンの種類
				0:回転 1:回転と移動 2:IK 3:不明 4:IK影響下 5:回転影響下 6:IK接続先 7:非表示 8:捻り 9:回転運動
				*/

				public Bone(BinaryReader bin)
				{
					this.bone_name = base.ConvertByteToString(bin.ReadBytes(20));
					this.parent_bone_index = bin.ReadUInt16();
					this.tail_pos_bone_index = bin.ReadUInt16();
					this.bone_type = bin.ReadByte();
					this.ik_parent_bone_index = bin.ReadUInt16();
					this.bone_head_pos = base.ReadSinglesToVector3(bin);
				}
			}

			public class IKList : MMD.Format
			{
				public ushort ik_data_count; // IKデータ数
				public IK[] ik_data; // IKデータ((11+2*ik_chain_length)/IK)

				public IKList(BinaryReader bin)
				{
					this.ik_data_count = bin.ReadUInt16();
					//Debug.Log("IKDataCount:"+ik_data_count);
					this.ik_data = new IK[this.ik_data_count];
					for (int i = 0; i < this.ik_data_count; i++)
						this.ik_data[i] = new IK(bin);
				}
			}

			public class IK : MMD.Format
			{
				public ushort ik_bone_index; // IKボーン番号
				public ushort ik_target_bone_index; // IKターゲットボーン番号 // IKボーンが最初に接続するボーン
				public byte ik_chain_length; // IKチェーンの長さ(子の数)
				public ushort iterations; // 再帰演算回数 // IK値1
				public float control_weight; // IKの影響度 // IK値2
				public ushort[] ik_child_bone_index; // IK影響下のボーン番号

				public IK(BinaryReader bin)
				{
					this.ik_bone_index = bin.ReadUInt16();
					this.ik_target_bone_index = bin.ReadUInt16();
					this.ik_chain_length = bin.ReadByte();
					this.iterations = bin.ReadUInt16();
					this.control_weight = bin.ReadSingle();
					this.ik_child_bone_index = base.ReadUInt16s(bin, this.ik_chain_length);
				}
			}

			public class SkinList : MMD.Format
			{
				public ushort skin_count; // 表情数
				public SkinData[] skin_data; // 表情データ((25+16*skin_vert_count)/skin)

				public SkinList(BinaryReader bin)
				{
					this.skin_count = bin.ReadUInt16();
					//Debug.Log("SkinCount:"+skin_count);
					this.skin_data = new SkinData[this.skin_count];
					for (int i = 0; i < this.skin_count; i++)
						this.skin_data[i] = new SkinData(bin);
				}
			}

			public class SkinData : MMD.Format
			{
				public string skin_name; //　表情名
				public uint skin_vert_count; // 表情用の頂点数
				public byte skin_type; // 表情の種類 // 0：base、1：まゆ、2：目、3：リップ、4：その他
				public SkinVertexData[] skin_vert_data; // 表情用の頂点のデータ(16bytes/vert)

				public SkinData(BinaryReader bin)
				{
					this.skin_name = base.ConvertByteToString( bin.ReadBytes(20));
					this.skin_vert_count = bin.ReadUInt32();
					this.skin_type = bin.ReadByte();
					this.skin_vert_data = new SkinVertexData[this.skin_vert_count];
					for (int i = 0; i < this.skin_vert_count; i++)
						this.skin_vert_data[i] = new SkinVertexData(bin);
				}
			}

			public class SkinVertexData : MMD.Format
			{
				// 実際の頂点を参照するには
				// int num = vertex_count - skin_vert_count;
				// skin_vert[num]みたいな形で参照しないと無理
				public uint skin_vert_index; // 表情用の頂点の番号(頂点リストにある番号)
				public Vector3 skin_vert_pos; // x, y, z // 表情用の頂点の座標(頂点自体の座標)

				public SkinVertexData(BinaryReader bin)
				{
					this.skin_vert_index = bin.ReadUInt32();
					this.skin_vert_pos = base.ReadSinglesToVector3(bin);
				}
			}
			
			// 表情用枠名
			public class SkinNameList : MMD.Format
			{
				public byte skin_disp_count;
				public ushort[] skin_index;		// 表情番号
				
				public SkinNameList(BinaryReader bin)
				{
					this.skin_disp_count = bin.ReadByte();
					this.skin_index = base.ReadUInt16s(bin, this.skin_disp_count);
				}
			}
			
			// ボーン用枠名
			public class BoneNameList : MMD.Format
			{
				public byte bone_disp_name_count;
				public string[] disp_name;		// 50byte
				
				public BoneNameList(BinaryReader bin)
				{
					this.bone_disp_name_count = bin.ReadByte();
					this.disp_name = new string[this.bone_disp_name_count];
					for (int i = 0; i < this.bone_disp_name_count; i++)
						this.disp_name[i] = base.ConvertByteToString(bin.ReadBytes(50));
				}
			}
			
			// ボーン枠用表示リスト
			public class BoneDisplayList : MMD.Format
			{
				public uint bone_disp_count;
				public BoneDisplay[] bone_disp;
				
				public BoneDisplayList(BinaryReader bin) 
				{
					this.bone_disp_count = bin.ReadUInt32();
					this.bone_disp = new MMD.PMD.PMDFormat.BoneDisplay[this.bone_disp_count];
					for (int i = 0; i < this.bone_disp_count; i++)
						bone_disp[i] = new BoneDisplay(bin);
				}
			}
			
			public class BoneDisplay : MMD.Format
			{
				public ushort bone_index;		// 枠用ボーン番号 
				public byte bone_disp_frame_index;	// 表示枠番号 
				
				public BoneDisplay(BinaryReader bin)
				{
					this.bone_index = bin.ReadUInt16();
					this.bone_disp_frame_index = bin.ReadByte();
				}
			}
			
			/// <summary>
			/// 英語表記用ヘッダ
			/// </summary>
			public class EnglishHeader : MMD.Format
			{
				public byte english_name_compatibility;	// 01で英名対応 
				public string model_name_eg;	// 20byte
				public string comment_eg;	// 256byte
				
				public EnglishHeader(BinaryReader bin)
				{
					this.english_name_compatibility = bin.ReadByte();
					
					if (this.english_name_compatibility != 0)
					{	// 英語名対応あり
						this.model_name_eg = base.ConvertByteToString(bin.ReadBytes(20));
						this.comment_eg = base.ConvertByteToString(bin.ReadBytes(256));
					}
				}
			}
			
			/// <summary>
			/// 英語表記用ボーンの英語名
			/// </summary>
			public class EnglishBoneNameList : MMD.Format
			{
				public string[] bone_name_eg;	// 20byte * bone_count
				
				public EnglishBoneNameList(BinaryReader bin, int boneCount)
				{
					this.bone_name_eg = new string[boneCount];
					for (int i = 0; i < boneCount; i++)
					{
						bone_name_eg[i] = base.ConvertByteToString(bin.ReadBytes(20));
					}
				}
			}
			
			public class EnglishSkinNameList : MMD.Format
			{
				// baseは英名が登録されない 
				public string[] skin_name_eg;	// 20byte * skin_count-1
				
				public EnglishSkinNameList(BinaryReader bin, int skinCount)
				{
					skin_name_eg = new string[skinCount];
					for (int i = 0; i < skinCount - 1; i++)
					{
						skin_name_eg[i] = base.ConvertByteToString(bin.ReadBytes(20));
					}
				}
			}
			
			public class EnglishBoneDisplayList : MMD.Format
			{
				public string[] disp_name_eg;	// 50byte * bone_disp_name_count
				
				public EnglishBoneDisplayList(BinaryReader bin, int boneDispNameCount)
				{
					disp_name_eg = new string[boneDispNameCount];
					for (int i = 0; i < boneDispNameCount; i++)
					{
						disp_name_eg[i] = base.ConvertByteToString(bin.ReadBytes(50));
					}
				}
			}
			
			public class ToonTextureList : MMD.Format
			{
				public string[] toon_texture_file;	// 100byte * 10個固定 
				
				public ToonTextureList(BinaryReader bin)
				{
					this.toon_texture_file = new string[10];
					for (int i = 0; i < this.toon_texture_file.Length; i++)
					{
						this.toon_texture_file[i] = base.ConvertByteToString(bin.ReadBytes(100));
					}
				}
			}
			
			public class RigidbodyList : MMD.Format
			{
				public uint rigidbody_count;
				public PMD.PMDFormat.Rigidbody[] rigidbody;
				
				public RigidbodyList(BinaryReader bin)
				{
					this.rigidbody_count = bin.ReadUInt32();
					this.rigidbody = new MMD.PMD.PMDFormat.Rigidbody[this.rigidbody_count];
					for (int i = 0; i < this.rigidbody_count; i++)
						this.rigidbody[i] = new MMD.PMD.PMDFormat.Rigidbody(bin);
				}
			}
			
			/// <summary>
			/// 剛体
			/// </summary>
			public class Rigidbody : MMD.Format
			{
				public string rigidbody_name; // 諸データ：名称 ,20byte
				public int rigidbody_rel_bone_index;// 諸データ：関連ボーン番号 
				public byte rigidbody_group_index; // 諸データ：グループ 
				public ushort rigidbody_group_target; // 諸データ：グループ：対象 // 0xFFFFとの差
				public byte shape_type;  // 形状：タイプ(0:球、1:箱、2:カプセル)  
				public float shape_w;	// 形状：半径(幅) 
				public float shape_h;	// 形状：高さ 
				public float shape_d;	// 形状：奥行 
				public Vector3 pos_pos;	 // 位置：位置(x, y, z) 
				public Vector3 pos_rot;	 // 位置：回転(rad(x), rad(y), rad(z)) 
				public float rigidbody_weight; // 諸データ：質量 // 00 00 80 3F // 1.0
				public float rigidbody_pos_dim; // 諸データ：移動減 // 00 00 00 00
				public float rigidbody_rot_dim; // 諸データ：回転減 // 00 00 00 00
				public float rigidbody_recoil; // 諸データ：反発力 // 00 00 00 00
				public float rigidbody_friction; // 諸データ：摩擦力 // 00 00 00 00
				public byte rigidbody_type; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
				
				public Rigidbody(BinaryReader bin)
				{
					this.rigidbody_name = base.ConvertByteToString(bin.ReadBytes(20));
					this.rigidbody_rel_bone_index = bin.ReadUInt16();
					this.rigidbody_group_index = bin.ReadByte();
					this.rigidbody_group_target = bin.ReadUInt16();
					this.shape_type = bin.ReadByte();
					this.shape_w = bin.ReadSingle();
					this.shape_h = bin.ReadSingle();
					this.shape_d = bin.ReadSingle();
					this.pos_pos = base.ReadSinglesToVector3(bin);
					this.pos_rot = base.ReadSinglesToVector3(bin);
					this.rigidbody_weight = bin.ReadSingle();
					this.rigidbody_pos_dim = bin.ReadSingle();
					this.rigidbody_rot_dim = bin.ReadSingle();
					this.rigidbody_recoil = bin.ReadSingle();
					this.rigidbody_friction = bin.ReadSingle();
					this.rigidbody_type = bin.ReadByte();
				}
			}
			
			public class RigidbodyJointList : MMD.Format
			{
				public uint joint_count;
				public Joint[] joint;
				
				public RigidbodyJointList(BinaryReader bin)
				{
					this.joint_count = bin.ReadUInt32();
					this.joint = new MMD.PMD.PMDFormat.Joint[this.joint_count];
					for (int i = 0; i < this.joint_count; i++)
						this.joint[i] = new MMD.PMD.PMDFormat.Joint(bin);
				}
			}
			
			public class Joint : MMD.Format
			{
				public string joint_name;	// 20byte
				public uint joint_rigidbody_a; // 諸データ：剛体A 
				public uint joint_rigidbody_b; // 諸データ：剛体B 
				public Vector3 joint_pos; // 諸データ：位置(x, y, z) // 諸データ：位置合せでも設定可 
				public Vector3 joint_rot; // 諸データ：回転(rad(x), rad(y), rad(z)) 
				public Vector3 constrain_pos_1; // 制限：移動1(x, y, z) 
				public Vector3 constrain_pos_2; // 制限：移動2(x, y, z) 
				public Vector3 constrain_rot_1; // 制限：回転1(rad(x), rad(y), rad(z)) 
				public Vector3 constrain_rot_2; // 制限：回転2(rad(x), rad(y), rad(z)) 
				public Vector3 spring_pos; // ばね：移動(x, y, z) 
				public Vector3 spring_rot; // ばね：回転(rad(x), rad(y), rad(z)) 
				
				public Joint(BinaryReader bin)
				{
					this.joint_name = base.ConvertByteToString(bin.ReadBytes(20));
					this.joint_rigidbody_a = bin.ReadUInt32(); 
					this.joint_rigidbody_b = bin.ReadUInt32();
					this.joint_pos = base.ReadSinglesToVector3(bin);
					this.joint_rot = base.ReadSinglesToVector3(bin);
					this.constrain_pos_1 = base.ReadSinglesToVector3(bin);
					this.constrain_pos_2 = base.ReadSinglesToVector3(bin);
					this.constrain_rot_1 = base.ReadSinglesToVector3(bin);
					this.constrain_rot_2 = base.ReadSinglesToVector3(bin);
					this.spring_pos = base.ReadSinglesToVector3(bin);
					this.spring_rot = base.ReadSinglesToVector3(bin);
				}
			}
		}
	}
	namespace VMD
	{
		public class VMDLoader
		{
			static public VMD.VMDFormat Load(BinaryReader bin, string path, string clip_name) 
			{
				return new VMD.VMDFormat(bin, path, clip_name);
			}
		}
		
		public class VMDFormat
		{
			public string name;
			public string path;
			public string folder;
			public string clip_name;
			public GameObject pmd;
			
			public Header header;
			public MotionList motion_list;
			public SkinList skin_list;
			public LightList light_list;
			public CameraList camera_list;
			public SelfShadowList self_shadow_list;
			
			int read_count = 0;
			
			void EntryPathes(string path)
			{
				this.path = path;
				string[] buf = path.Split('/');
				this.name = buf[buf.Length-1];
				this.name = name.Split('.')[0];		// .vmdを抜かす
				
				// VMDが格納されているフォルダ
				this.folder = buf[0];
				for (int i = 1; i < buf.Length-1; i++)
					this.folder += "/" + buf[i];
			}
			
			public VMDFormat(BinaryReader bin, string path, string clip_name)
			{
				// 読み込み失敗した場合はだいたいデータがない
				// 失敗しても読み込み続けることがあるので例外でキャッチして残りはnullにしておく
				try {
					this.clip_name = clip_name;
					this.header = new MMD.VMD.VMDFormat.Header(bin); read_count++;
					this.motion_list = new MMD.VMD.VMDFormat.MotionList(bin); read_count++;
					this.skin_list = new MMD.VMD.VMDFormat.SkinList(bin); read_count++;
					this.camera_list = new MMD.VMD.VMDFormat.CameraList(bin); read_count++;
					this.light_list = new MMD.VMD.VMDFormat.LightList(bin); read_count++;
					this.self_shadow_list = new MMD.VMD.VMDFormat.SelfShadowList(bin); read_count++;
				} catch (EndOfStreamException e) {
					Debug.Log(e.Message);
					if (read_count <= 0)
						this.header = null;
					if (read_count <= 1 || this.motion_list.motion_count <= 0)
						this.motion_list = null;
					if (read_count <= 2 || this.skin_list.skin_count <= 0)
						this.skin_list = null;
					if (read_count <= 3 || this.camera_list.camera_count <= 0)
						this.camera_list = null;
					if (read_count <= 4 || this.light_list.light_count <= 0)
						this.light_list = null;
					if (read_count <= 5 || this.self_shadow_list.self_shadow_count <= 0) 
						this.self_shadow_list = null;
				}
			}
			
			
			public class Header : MMD.Format
			{
				public string vmd_header; // 30byte, "Vocaloid Motion Data 0002"
				public string vmd_model_name; // 20byte
				
				public Header(BinaryReader bin)
				{
					this.vmd_header = base.ConvertByteToString(bin.ReadBytes(30));
					this.vmd_model_name = base.ConvertByteToString(bin.ReadBytes(20));
				}
			}
			
			public class MotionList : MMD.Format
			{
				public uint motion_count;
				public Dictionary<string, List<Motion>> motion;
				
				public MotionList(BinaryReader bin)
				{
					this.motion_count = bin.ReadUInt32();
					this.motion = new Dictionary<string, List<Motion>>();
					
					// 一度バッファに貯めてソートする
					Motion[] buf = new Motion[this.motion_count];
					for (int i = 0; i < this.motion_count; i++)
						buf[i] = new Motion(bin);
					Array.Sort(buf);
					
					// モーションの数だけnewされないよね？
					for (int i = 0; i < this.motion_count; i++) {
						try { this.motion.Add(buf[i].bone_name, new List<Motion>()); }
						catch {}
					}
					
					// dictionaryにどんどん登録
					for (int i = 0; i < this.motion_count; i++) 
						this.motion[buf[i].bone_name].Add(buf[i]);
				}
			}
			
			public class Motion : MMD.Format
			{
				public string bone_name;	// 15byte
				public uint flame_no;
				public Vector3 location;
				public Quaternion rotation;
				public byte[] interpolation;	// [4][4][4], 64byte
				
				public Motion(BinaryReader bin)
				{
					this.bone_name = base.ConvertByteToString(bin.ReadBytes(15));
					this.flame_no = bin.ReadUInt32();
					this.location = base.ReadSinglesToVector3(bin);
					this.rotation = base.ReadSinglesToQuaternion(bin);
					this.interpolation = bin.ReadBytes(64);
					this.count = (int)this.flame_no;
				}
				
				// なんか不便になりそうな気がして
				public byte GetInterpolation(int i, int j, int k)
				{
					return this.interpolation[i*16+j*4+k];
				}
				
				public void SetInterpolation(byte val, int i, int j, int k)
				{
					this.interpolation[i*16+j*4+k] = val;
				}
			}
			
			/// <summary>
			/// 表情リスト
			/// </summary>
			public class SkinList : MMD.Format
			{
				public uint skin_count;
				public Dictionary<string, List<SkinData>> skin;
				
				public SkinList(BinaryReader bin)
				{
					this.skin_count = bin.ReadUInt32();
					this.skin = new Dictionary<string, List<SkinData>>();
					
					// 一度バッファに貯めてソートする
					SkinData[] buf = new SkinData[this.skin_count];
					for (int i = 0; i < this.skin_count; i++)
						buf[i] = new SkinData(bin);
					Array.Sort(buf);
					
					// 全てのモーションを探索し、利用されているボーンを特定する
					for (int i = 0; i < this.skin_count; i++) {
						try { skin.Add(buf[i].skin_name, new List<SkinData>()); }
						catch { /*重複している場合はこの処理に入る*/ }
					}
					
					// 辞書に登録する作業
					for (int i = 0; i < this.skin_count; i++) 
						this.skin[buf[i].skin_name].Add(buf[i]);
				}
			}
			
			public class SkinData : MMD.Format
			{
				public string skin_name;	// 15byte
				public uint flame_no;
				public float weight;
				
				public SkinData(BinaryReader bin)
				{
					this.skin_name = base.ConvertByteToString(bin.ReadBytes(15));
					this.flame_no = bin.ReadUInt32();
					this.weight = bin.ReadSingle();
					this.count = (int)this.flame_no;
				}
			}
			
			public class CameraList : MMD.Format
			{
				public uint camera_count;
				public CameraData[] camera;
				
				public CameraList(BinaryReader bin)
				{
					this.camera_count = bin.ReadUInt32();
					this.camera = new CameraData[this.camera_count];
					for (int i = 0; i < this.camera_count; i++)
						this.camera[i] = new CameraData(bin);
					
					Array.Sort(camera);
				}
			}
			
			public class CameraData : MMD.Format
			{
				public uint flame_no;
				public float length;
				public Vector3 location;
				public Vector3 rotation;	// オイラー角, X軸は符号が反転している
				public byte[] interpolation;	// [6][4], 24byte(未検証)
				public uint viewing_angle;
				public byte perspective;	// 0:on 1:off
				
				public CameraData(BinaryReader bin)
				{
					this.flame_no = bin.ReadUInt32();
					this.length = bin.ReadSingle();
					this.location = base.ReadSinglesToVector3(bin);
					this.rotation = base.ReadSinglesToVector3(bin);
					this.interpolation = bin.ReadBytes(24);
					this.viewing_angle = bin.ReadUInt32();
					this.perspective = bin.ReadByte();
					this.count = (int)this.flame_no;
				}
				
				public byte GetInterpolation(int i, int j)
				{
					return this.interpolation[i*6+j];
				}
				
				public void SetInterpolation(byte val, int i, int j)
				{
					this.interpolation[i*6+j] = val;
				}
			}
			
			public class LightList : MMD.Format
			{
				public uint light_count;
				public LightData[] light;
				
				public LightList(BinaryReader bin)
				{
					this.light_count = bin.ReadUInt32();
					this.light = new LightData[this.light_count];
					for (int i = 0; i < this.light_count; i++)
						this.light[i] = new LightData(bin);
					
					Array.Sort(this.light);
				}
			}
			
			public class LightData : MMD.Format
			{
				public uint flame_no;
				public Color rgb;	// αなし, 256
				public Vector3 location;
				
				public LightData(BinaryReader bin)
				{
					this.flame_no = bin.ReadUInt32();
					this.rgb = base.ReadSinglesToColor(bin, 1);
					this.location = base.ReadSinglesToVector3(bin);
					this.count = (int)this.flame_no;
				}
			}
			
			public class SelfShadowList : MMD.Format
			{
				public uint self_shadow_count;
				public SelfShadowData[] self_shadow;
				
				public SelfShadowList(BinaryReader bin)
				{
					this.self_shadow_count = bin.ReadUInt32();
					this.self_shadow = new SelfShadowData[this.self_shadow_count];
					for (int i = 0; i < this.self_shadow_count; i++)
						this.self_shadow[i] = new SelfShadowData(bin);
					
					Array.Sort(this.self_shadow);
				}
			}
			
			public class SelfShadowData : MMD.Format
			{
				public uint flame_no;
				public byte mode; //00-02
				public float distance;	// 0.1 - (dist * 0.00001)
				
				public SelfShadowData(BinaryReader bin)
				{
					this.flame_no = bin.ReadUInt32();
					this.mode = bin.ReadByte();
					this.distance = bin.ReadSingle();
					this.count = (int)this.flame_no;
				}
			}
		}
	}
}