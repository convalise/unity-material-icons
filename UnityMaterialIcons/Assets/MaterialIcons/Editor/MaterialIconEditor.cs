
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Google.MaterialDesign.Icons
{

[CanEditMultipleObjects, CustomEditor(typeof(MaterialIcon))]
public class MaterialIconEditor : Editor
{
	private MaterialIcon icon;
	private bool fold;

	private Font MaterialIconsRegular;
	private GUIStyle iconImage;

	private void OnEnable()
	{
		string fontPath = null;

		foreach(string guid in AssetDatabase.FindAssets("MaterialIcons-Regular"))
		{
			string tempPath = AssetDatabase.GUIDToAssetPath(guid);

			if(tempPath.ToLower().EndsWith(".ttf") && File.Exists(Path.GetDirectoryName(tempPath) + "/codepoints"))
			{
				fontPath = tempPath;
				break;
			}
		}

		if(string.IsNullOrEmpty(fontPath))
			return;
		
		MaterialIconsRegular = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
		iconImage = new GUIStyle() { font = MaterialIconsRegular, alignment = TextAnchor.MiddleCenter, fontSize = 50 };
	}

	public override void OnInspectorGUI()
	{
		if(icon == null)
		{
			icon = target as MaterialIcon;
		}

		if((icon.font == null) && (MaterialIconsRegular != null))
		{
			icon.font = MaterialIconsRegular;
		}

		serializedObject.Update();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Icon");

		Rect irect = GUILayoutUtility.GetRect(60f, 60f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		irect.x -= 6f;

		if(GUI.Button(irect, GUIContent.none))
			MaterialIconSelectionWindow.Init(MaterialIconsRegular, icon.text, (selected) => icon.text = selected);
		GUI.Label(irect, icon.text, iconImage);

		EditorGUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.Space();

		if(fold = EditorGUILayout.Foldout(fold, "Base Text Editor"))
			base.OnInspectorGUI();
	}

	
}

}
