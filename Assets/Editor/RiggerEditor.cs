using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiggerBehavior))]
public class RiggerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		// easy way to put an "hr" in inspector!
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		RiggerBehavior rigger = target as RiggerBehavior;

		if(!rigger.skinnedMeshRenderer)
		{
			rigger.meshFilter = EditorGUILayout.ObjectField("Mesh Filter", rigger.meshFilter, typeof(MeshFilter), true) as MeshFilter;
		}
		if(!rigger.meshFilter)
		{
			rigger.skinnedMeshRenderer = EditorGUILayout.ObjectField("Skinned Mesh", rigger.skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
		}

		if(rigger.meshFilter && !rigger.skinnedMeshRenderer && GUILayout.Button("Convert Mesh"))
		{
			rigger.ConvertMesh();
		}

		if(rigger.skinnedMeshRenderer && !rigger.skeleton && GUILayout.Button("Refresh Skeleton"))
		{
			rigger.RefreshSkeleton();
		}

		if(rigger.animator && !rigger.animator.avatar && rigger.skeleton.parent == rigger.animator.transform && GUILayout.Button("Add Avatar"))
		{
			rigger.AddAvatar();
		}

		if(rigger.skinnedMeshRenderer && rigger.skeleton && GUILayout.Button("Skin"))
		{
			rigger.Skin();
		}
	}

	private void OnSceneGUI()
	{
		RiggerBehavior rigger = target as RiggerBehavior;
		DrawSkeleton(rigger);
	}

	private void DrawSkeleton(RiggerBehavior rigger)
	{
		if(rigger.bones == null || (Event.current.type != EventType.Repaint))
		{
			return;
		}

		foreach(Transform b in rigger.bones)
		{
			Handles.color = Color.cyan;
			Handles.SphereHandleCap(0, b.position, Quaternion.identity, 0.02f, EventType.Repaint);
			Handles.color = Color.yellow;
			if(b.parent == rigger.skeleton)
			{
				Handles.color = Color.red;
			}
			Handles.DrawLine(b.position, b.parent.position);
		}
	}
}
