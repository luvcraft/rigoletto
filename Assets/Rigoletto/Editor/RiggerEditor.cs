using UnityEngine;
using UnityEditor;

namespace Rigoletto
{
	/// <summary>
	/// Editor for RiggerBehavior.
	/// Provides handy buttons to step through the conversion process!
	/// </summary>
	[CustomEditor(typeof(RiggerBehavior))]
	public class RiggerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			// easy way to put an "hr" in inspector!
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			RiggerBehavior rigger = target as RiggerBehavior;
			string instructions = "";

			if(!rigger.skinnedMeshRenderer)
			{
				rigger.meshFilter = EditorGUILayout.ObjectField("Mesh Filter", rigger.meshFilter, typeof(MeshFilter), true) as MeshFilter;
			}
			if(!rigger.meshFilter)
			{
				rigger.skinnedMeshRenderer = EditorGUILayout.ObjectField("Skinned Mesh", rigger.skinnedMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
			}

			if(!rigger.skinnedMeshRenderer && !rigger.meshFilter)
			{
				instructions = "Connect either a MeshFilter or SkinnedMeshRenderer to one of the fields above.";
			}

			if(rigger.meshFilter && !rigger.skinnedMeshRenderer)
			{
				if(GUILayout.Button("Convert Mesh"))
				{
					rigger.ConvertMesh();
				}
				instructions = "Click the \"Convert Mesh\" button to convert the MeshFilter to a SkinnedMeshRenderer.";
			}

			if(rigger.animator && !rigger.animator.avatar && rigger.skeleton && rigger.skeleton.parent == rigger.animator.transform)
			{
				if(GUILayout.Button("Add Avatar"))
				{
					rigger.AddAvatar();
				}
				instructions = "Click the \"Add Avatar\" button to add an avatar to the SkinnedMeshRenderer.";
			}

			if(rigger.skinnedMeshRenderer && rigger.skeleton)
			{
				if(GUILayout.Button("Skin"))
				{
					rigger.Skin();
				}
				instructions += "\n\nClick the \"Skin\" button to skin the SkinnedMeshRenderer to the skeleton.";
			}

			if(rigger.skinnedMeshRenderer)
			{
				if(GUILayout.Button("UnSkin"))
				{
					rigger.Unskin();
				}
				instructions += "\n\nClick the \"UnSkin\" button to unskin the SkinnedMeshRenderer from the skeleton.";
			}

			if(!string.IsNullOrEmpty(instructions))
			{
				EditorGUILayout.HelpBox(instructions.TrimStart('\n'), MessageType.Info);
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
}