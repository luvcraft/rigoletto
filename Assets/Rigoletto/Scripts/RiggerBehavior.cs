using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RiggerBehavior : MonoBehaviour
{
	[HideInInspector]
	public MeshFilter meshFilter;
	[HideInInspector]
	public SkinnedMeshRenderer skinnedMeshRenderer;
	[HideInInspector]
	public Animator animator;
	[HideInInspector]
	public Transform skeleton;

	public Avatar referenceAvatar;
	public RuntimeAnimatorController defaultController;
	public Transform referenceSkeleton;
	public List<Transform> bones = new List<Transform>();
	public bool symmetrical = true;

	private Dictionary<Transform, Transform> bonePairs = new Dictionary<Transform, Transform>();
	private Transform rootTransform = null;

	private void OnDrawGizmos()
	{
		if(!skinnedMeshRenderer)
		{
			skeleton = null;
			bones.Clear();
		}

		if(skeleton && (bones == null || bones.Count < 1 || bonePairs.Count < 1))
		{
			bones = new List<Transform>(skeleton.GetComponentsInChildren<Transform>(true));
			bones.Remove(skeleton);
			SetBonePairs();
		}

		DrawSkeleton();

		if(symmetrical)
		{
			BonePairCheck();
		}
	}

	private void SetBonePairs()
	{
		bonePairs = new Dictionary<Transform, Transform>();

		bool pair = false;
		for(int i = 0; i < bones.Count - 1; i++)
		{
			for(int j = i + 1; j < bones.Count; j++)
			{
				pair = false;
				if(bones[i].name == bones[j].name.Replace("Left", "Right"))
				{
					pair = true;
				}
				else if(bones[j].name == bones[i].name.Replace("Left", "Right"))
				{
					pair = true;
				}
				if(pair && !bonePairs.ContainsKey(bones[i]))
				{
					bonePairs.Add(bones[i], bones[j]);
					bonePairs.Add(bones[j], bones[i]);
				}
			}
		}
	}

	private void DrawSkeleton()
	{
		foreach(Transform b in bones)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(b.position, 0.01f);
			Gizmos.color = Color.yellow;
			if(b.parent == skeleton)
			{
				Gizmos.color = Color.red;
			}
			Gizmos.DrawLine(b.position, b.parent.position);
		}
	}

	private void BonePairCheck()
	{
		if(!Selection.activeGameObject || !bones.Contains(Selection.activeGameObject.transform))
		{
			return;
		}

		Transform bone = Selection.activeGameObject.transform;
		if(bonePairs.ContainsKey(bone))
		{
			bonePairs[bone].position = new Vector3(-bone.position.x, bone.position.y, bone.position.z);
		}
	}

	private void CheckRootTransform(Transform childTransform)
	{
		if(!rootTransform)
		{
			rootTransform = childTransform.parent;
		}
		if(!rootTransform)
		{
			rootTransform = (new GameObject()).transform;
			rootTransform.name = "Character";
			rootTransform.SnapToZero();
			childTransform.parent = rootTransform;
		}

		if(PrefabUtility.IsPartOfAnyPrefab(rootTransform.gameObject))
		{
			PrefabUtility.UnpackPrefabInstance(rootTransform.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		}

		if(!skeleton)
		{
			if(skinnedMeshRenderer && skinnedMeshRenderer.rootBone)
			{
				skeleton = skinnedMeshRenderer.rootBone.parent;
			}
			else
			{
				skeleton = Instantiate(referenceSkeleton);
				skeleton.name = "Skeleton";
				skeleton.gameObject.SetActive(true);
				skeleton.parent = rootTransform;
				skeleton.SnapToZero();
			}
		}

		if(!animator)
		{
			animator = rootTransform.GetComponent<Animator>();
		}
		if(!animator)
		{
			animator = rootTransform.gameObject.AddComponent<Animator>();
		}
	}

	public void ConvertMesh()
	{
		if(!meshFilter)
		{
			Debug.LogWarning("no mesh renderer!");
			return;
		}

		CheckRootTransform(meshFilter.transform);

		skinnedMeshRenderer = (new GameObject()).AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer.name = "Skinned Mesh";
		skinnedMeshRenderer.transform.parent = meshFilter.transform.parent;
		skinnedMeshRenderer.transform.SnapToZero();
		skinnedMeshRenderer.sharedMesh = Instantiate(meshFilter.sharedMesh);
		skinnedMeshRenderer.sharedMesh.name = "Editable Mesh";
		SaveAsset(skinnedMeshRenderer.sharedMesh);

		MeshRenderer meshrenderer = meshFilter.GetComponent<MeshRenderer>();
		if(meshrenderer)
		{
			skinnedMeshRenderer.sharedMaterials = meshrenderer.sharedMaterials;
		}

		DestroyImmediate(meshFilter.gameObject);
	}

	/// <summary>
	/// Save a generated asset to "Assets/Rigoletto/Generated/"
	/// You can later move it wherever you want
	/// </summary>
	/// <param name="asset"></param>
	private void SaveAsset(Object asset)
	{
		string path = "Assets/Rigoletto/Generated/";
		if(!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
		{
			AssetDatabase.CreateFolder("Assets/Rigoletto", "Generated");
		}
		string name = asset.name;
		int i = 0;
		while(AssetDatabase.LoadAssetAtPath(path + name + ".asset", typeof(Object)) && i < 10)
		{
			name = asset.name + " " + i.ToString();
			i++;
		}
		AssetDatabase.CreateAsset(asset, path + name + ".asset");
	}

	/// <summary>
	/// Triggered by inspector button press
	/// Refreshes the skeleton, and also checks to make sure the mesh is editable
	/// Typically we'll only get here if we start with a skinned mesh, skipping the
	/// ...step of converting from a MeshFilter
	/// </summary>
	public void RefreshSkeleton()
	{
		CheckRootTransform(skinnedMeshRenderer.transform);

		// also check to make sure mesh is editable while we're here
		if(AssetDatabase.Contains(skinnedMeshRenderer.sharedMesh))
		{
			Debug.Log("Mesh not editable. Fixing.");
			Mesh mesh = Instantiate(skinnedMeshRenderer.sharedMesh);
			skinnedMeshRenderer.sharedMesh = mesh;
			skinnedMeshRenderer.sharedMesh.name = "Editable Mesh";
		}
	}

	public void AddAvatar()
	{
		animator.runtimeAnimatorController = defaultController;

		HumanDescription humanDescription = referenceAvatar.humanDescription;
		animator.avatar = AvatarBuilder.BuildHumanAvatar(animator.gameObject, humanDescription);
		animator.avatar.name = "Generated Avatar";
		SaveAsset(animator.avatar);
	}

	public void Skin()
	{
		CheckRootTransform(skinnedMeshRenderer.transform);

		if(!skinnedMeshRenderer.rootBone)
		{
			skinnedMeshRenderer.rootBone = bones[0];
		}

		Mesh mesh = skinnedMeshRenderer.sharedMesh;
		List<BoneWeight> weights = new List<BoneWeight>();

		skinnedMeshRenderer.bones = bones.ToArray();

		int closestBone;
		float closestDistance;
		foreach(Vector3 v in mesh.vertices)
		{
			closestBone = 0;
			closestDistance = float.PositiveInfinity;

			for(int b = 0; b < skinnedMeshRenderer.bones.Length; b++)
			{
				float d = Vector3.Distance(v, skinnedMeshRenderer.bones[b].position);
				if(d < closestDistance)
				{
					closestBone = b;
					closestDistance = d;
				}
			}

			BoneWeight w = new BoneWeight
			{
				boneIndex0 = closestBone,
				weight0 = 1
			};
			weights.Add(w);
		}

		mesh.boneWeights = weights.ToArray();

		List<Matrix4x4> bindPoses = new List<Matrix4x4>();
		foreach(Transform b in skinnedMeshRenderer.bones)
		{
			Matrix4x4 p = b.worldToLocalMatrix * skeleton.localToWorldMatrix;
			bindPoses.Add(p);
		}

		mesh.bindposes = bindPoses.ToArray();

		skinnedMeshRenderer.sharedMesh = mesh;

		// for some reason the bounding box is wrong if this is false
		skinnedMeshRenderer.updateWhenOffscreen = true;
	}
}

public static class TransformExtensions
{
	/// <summary>Snaps the transform's local position and rotation to zero and local scale to one</summary>
	public static void SnapToZero(this Transform source)
	{
		source.localPosition = Vector3.zero;
		source.localScale = Vector3.one;
		source.localRotation = Quaternion.identity;
	}
}
