
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Google.MaterialDesign.Icons
{

public class MaterialIconSelectionWindow : EditorWindow
{
	private int iconSize = 60;
	private float iconZoom = 1f;
	private Vector2 scrollPos = Vector2.zero;

	private string selected;
	private System.Action<string> OnSelectionChanged;

	private Font MaterialIconsRegular;
	private codepointData[] CodepointsCollection;

	private GUIStyle toolbarSeachTextField;
	private GUIStyle toolbarSeachCancelButton;
	private GUIStyle toolbarSeachCancelButtonEmpty;
//	private GUIStyle toolbarLabel;
	private GUIStyle iconImage;
	private GUIStyle iconLabel;
	private string filterText = string.Empty;
	private bool showNames = true;
	private bool filterGotFocus = false;

	public void LoadDependencies(Font MaterialIconsRegular)
	{
		this.MaterialIconsRegular = MaterialIconsRegular;

		string fontPath = AssetDatabase.GetAssetPath(MaterialIconsRegular);
		string codepointsPath = Path.GetDirectoryName(fontPath) + "/codepoints";
	
		List<codepointData> tempList = new List<codepointData>();

		foreach(string codepoint in File.ReadAllLines(codepointsPath))
		{
			string[] data = codepoint.Split(' ');
			tempList.Add(new codepointData(data[0].ToLower().Replace('_', ' '), data[1]));
		}

		CodepointsCollection = tempList.ToArray();
	}

	static public void Init(Font MaterialIconsRegular, string preSelect, System.Action<string> callback)
	{
		MaterialIconSelectionWindow window = EditorWindow.GetWindow<MaterialIconSelectionWindow>(true);
		window.LoadDependencies(MaterialIconsRegular);
		window.selected = preSelect;
		window.OnSelectionChanged = callback;
	}

	private void OnEnable()
	{
		base.titleContent = new GUIContent("Icon Selection");
		base.minSize = new Vector2(410f, 529f);
	}

	private void OnGUI()
	{
		if((toolbarSeachTextField == null) || (iconImage == null))
		{
			toolbarSeachTextField = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSeachTextField"));
			toolbarSeachCancelButton = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSeachCancelButton"));
			toolbarSeachCancelButtonEmpty = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("ToolbarSeachCancelButtonEmpty"));
//			toolbarLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
			iconImage = new GUIStyle() { font = MaterialIconsRegular, alignment = TextAnchor.MiddleCenter };
			iconLabel = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, wordWrap = true };
		}

		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

		GUI.SetNextControlName("MaterialIconFilter");
		filterText = EditorGUILayout.TextField(filterText, toolbarSeachTextField);

		Rect clearRect = GUILayoutUtility.GetRect(20f, 14f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		clearRect.y += 1.5f;

		if(GUI.Button(clearRect, GUIContent.none, string.IsNullOrEmpty(filterText) ? toolbarSeachCancelButtonEmpty : toolbarSeachCancelButton))
		{
			filterText = string.Empty;
			GUI.FocusControl(null);
		}

		if(!filterGotFocus)
		{
			EditorGUI.FocusTextInControl("MaterialIconFilter");
			filterGotFocus = true;
		}

//		EditorGUILayout.LabelField("Names", toolbarLabel, GUILayout.Width(44f));
//		showNames = EditorGUILayout.Toggle(showNames, GUILayout.Width(20f));

//		EditorGUILayout.LabelField("Zoom", toolbarLabel, GUILayout.Width(40f));
//		Rect zrect = GUILayoutUtility.GetRect(160f, EditorStyles.toolbar.fixedHeight, GUILayout.ExpandWidth(false));

//		iconZoom = GUI.HorizontalSlider(zrect, iconZoom, 0.5f, 3f);
		iconImage.fontSize = Mathf.FloorToInt((iconSize - 10) * iconZoom);

		EditorGUILayout.EndHorizontal();

		if((CodepointsCollection == null) || (CodepointsCollection.Length == 0))
		{
			//show warning
			return;
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		EditorGUILayout.BeginHorizontal();

		int k = 0;
		foreach(var data in CodepointsCollection)
		{
			if(!string.IsNullOrEmpty(filterText) && filterText.Split(' ').Any( filter => data.name.IndexOf(filter.ToLower()) < 0 ))
				continue;

			if((++k * ((iconSize * iconZoom) + 8f)) + 16f > base.position.width)
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
				k = 1;
			}

			GUILayout.Space(4f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(4f);

			Rect irect = GUILayoutUtility.GetRect(iconSize * iconZoom, iconSize * iconZoom, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

			if(selected == data.codeGUIContent.text)
			{
				EditorGUI.DrawRect(irect, Color.cyan);
			}

			if(GUI.Button(irect, GUIContent.none, GUIStyle.none))
			{
				selected = data.codeGUIContent.text;
				OnSelectionChanged.Invoke(selected);
			}

			GUI.Label(irect, data.codeGUIContent, iconImage);

			if(showNames)
			{
				Rect lrect = GUILayoutUtility.GetRect(iconSize * iconZoom - 4f, iconLabel.CalcHeight(data.nameGUIContent, iconSize * iconZoom - 4f), iconLabel, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
				lrect.x = irect.x;
				lrect.width = irect.width;
				GUI.Label(lrect, data.nameGUIContent, iconLabel);
			}

			EditorGUILayout.EndVertical();
		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.EndScrollView();
	}
}

[System.Serializable]
public class codepointData
{
	public string name { get; private set; }
	public string code { get; private set; }
	public GUIContent nameGUIContent { get; private set; }
	public GUIContent codeGUIContent { get; private set; }

	public codepointData(string name, string code)
	{
		this.name = name;
		this.code = code;
		this.nameGUIContent = new GUIContent(name);
		this.codeGUIContent = new GUIContent(System.Text.RegularExpressions.Regex.Unescape(@"\u" + code));
	}
	
}

}
