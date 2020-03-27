
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Google.MaterialDesign.Icons
{

[CustomEditor(typeof(MaterialIcon), true), CanEditMultipleObjects]
public class MaterialIconEditor : UnityEditor.UI.TextEditor
{
	private static readonly Color darkColor = new Color(0.196f, 0.196f, 0.196f);
	private static readonly Color lightColor = new Color(0.804f, 0.804f, 0.804f);
	
	private MaterialIcon icon;
	private Font MaterialIconsRegular;
	private GUIStyle iconStyle;
	private GUIContent iconTooltip;

	protected override void OnEnable()
	{
		base.OnEnable();
		icon = target as MaterialIcon;

		if(icon.font == null)
		{
			icon.LoadFont();
		}
		
		MaterialIconsRegular = icon.font;

		iconStyle = new GUIStyle();
		iconStyle.font = MaterialIconsRegular;
		iconStyle.fontSize = 46;
		iconStyle.alignment = TextAnchor.MiddleCenter;
		iconStyle.normal.textColor = iconStyle.active.textColor = iconStyle.focused.textColor = iconStyle.hover.textColor = EditorGUIUtility.isProSkin ? lightColor : darkColor;

		iconTooltip = new GUIContent(string.Empty, icon.iconUnicode);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		if(MaterialIconsRegular == null)
		{
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox("Could not find \"MaterialIcons-Regular\" font data.", MessageType.Error);
		}

		EditorGUILayout.Space();

		EditorGUI.BeginDisabledGroup(MaterialIconsRegular == null);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Icon");

		Rect irect = GUILayoutUtility.GetRect(60f, 60f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

		if(GUI.Button(irect, iconTooltip))
		{
			MaterialIconSelectionWindow.Init(MaterialIconsRegular, icon.text, (selected) => {
				Undo.RecordObject(icon, "Inspector");
				icon.text = selected;
			});
		}
		GUI.Label(irect, icon.text, iconStyle);

		EditorGUILayout.EndHorizontal();
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));

		EditorGUILayout.Space();

		Rect alignmentRect = GUILayoutUtility.GetRect(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
		DoTextAligmentControl(alignmentRect, serializedObject.FindProperty("m_FontData.m_Alignment"));

		serializedObject.ApplyModifiedProperties();
	}

	/// <summary> Reflection for the private synonymous method from the FontDataDrawer class. </summary>
	private static readonly MethodInfo DoHorizontalAligmentControl = typeof(UnityEditor.UI.FontDataDrawer).GetMethod("DoHorizontalAligmentControl", BindingFlags.NonPublic | BindingFlags.Static);

	/// <summary> Reflection for the private synonymous method from the FontDataDrawer class. </summary>
	private static readonly MethodInfo DoVerticalAligmentControl = typeof(UnityEditor.UI.FontDataDrawer).GetMethod("DoVerticalAligmentControl", BindingFlags.NonPublic | BindingFlags.Static);

	/// <summary> Workaround for the non-static private synonymous method from the FontDataDrawer class. </summary>
	private static void DoTextAligmentControl(Rect position, SerializedProperty alignment)
	{
		try
		{
			GUIContent guiContent = new GUIContent("Alignment");
			EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));
			EditorGUI.BeginProperty(position, guiContent, alignment);
			Rect rect = EditorGUI.PrefixLabel(position, guiContent);
			float size1 = 60f;
			float size2 = Mathf.Clamp(rect.width - size1 * 2f, 2f, 10f);
			Rect position2 = new Rect(rect.x, rect.y, size1, rect.height);
			Rect position3 = new Rect(position2.xMax + size2, rect.y, size1, rect.height);
			DoHorizontalAligmentControl.Invoke(null, new object[] { position2, alignment });
			DoVerticalAligmentControl.Invoke(null, new object[] { position3, alignment });
			EditorGUI.EndProperty();
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
		catch(System.Exception e)
		{
			Debug.LogException(e);
		}
	}

}

}
