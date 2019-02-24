using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiggerBehavior))]
public class RiggerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		RiggerBehavior rigger = target as RiggerBehavior;


		if(rigger.skinnedMeshRenderer && GUILayout.Button("Skin"))
		{
			rigger.Skin();
		}

		if(GUILayout.Button("Add Avatar"))
		{
			rigger.AddAvatar();
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
