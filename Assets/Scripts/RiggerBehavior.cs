using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RiggerBehavior : MonoBehaviour
{
	public MeshRenderer meshRenderer;
	public SkinnedMeshRenderer skinnedMeshRenderer;
	public Animator animator;

	public Avatar referenceAvatar;

	public Transform skeleton;

	public List<Transform> bones;
	public Dictionary<Transform, Transform> bonePairs = new Dictionary<Transform, Transform>();

	private void OnDrawGizmos()
	{
		if(skeleton && (bones == null || bones.Count < 1 || bonePairs.Count < 1))
		{
			bones = new List<Transform>(skeleton.GetComponentsInChildren<Transform>(true));
			bones.Remove(skeleton);
			SetBonePairs();
		}

		DrawSkeleton();
		BonePairCheck();
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

		Debug.Log((bonePairs.Count / 2).ToString() + " bone pairs found");
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

	public void Skin()
	{
		Debug.Log("skinning...");

		if(!skinnedMeshRenderer.rootBone)
		{
			skinnedMeshRenderer.rootBone = bones[0];
		}

		Mesh mesh = skinnedMeshRenderer.sharedMesh;
		List<BoneWeight> weights = new List<BoneWeight>();

		skinnedMeshRenderer.bones = bones.ToArray();

		Vector3 offset = Vector3.zero; // skinnedMeshRenderer.rootBone.position;
		int closestBone;
		float closestDistance;
		foreach(Vector3 v in mesh.vertices)
		{
			closestBone = 0;
			closestDistance = float.PositiveInfinity;

			for(int b = 0; b < skinnedMeshRenderer.bones.Length; b++)
			{
				float d = Vector3.Distance(v - offset, skinnedMeshRenderer.bones[b].position);
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
		mesh.RecalculateBounds();

		skinnedMeshRenderer.sharedMesh = mesh;

		Debug.Log("skinned!");
	}

	public void AddAvatar()
	{
		HumanDescription humanDescription = referenceAvatar.humanDescription;
		animator.avatar = AvatarBuilder.BuildHumanAvatar(animator.gameObject, humanDescription);
		animator.avatar.name = "Generated Avatar";
	}
}
