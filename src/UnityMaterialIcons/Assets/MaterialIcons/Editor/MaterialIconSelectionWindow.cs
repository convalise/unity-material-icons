
using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Google.MaterialDesign.Icons
{

public class MaterialIconSelectionWindow : EditorWindow
{
	private static readonly Color darkColor = new Color(0.196f, 0.196f, 0.196f);
	private static readonly Color lightColor = new Color(0.804f, 0.804f, 0.804f);
	
	private int iconSize = 60;
	private int iconZoom = 1;
	private bool showNames = true;
	private Vector2 scrollPos = Vector2.zero;
	private string filterText = string.Empty;
	private bool filterGotFocus = false;

	private string selected;
	private int selectedIndex;
	private bool selectionKeep;
	private System.Action<string> OnSelectionChanged;

	private Font MaterialIconsRegular;
	private CodepointData[] CodepointsCollection;

	private GUIStyle toolbarSeachTextFieldStyle;
	private GUIStyle toolbarSeachCancelButtonStyle;
	private GUIStyle toolbarSeachCancelButtonEmptyStyle;
	private GUIStyle toolbarLabelStyle;
	private GUIStyle iconImageStyle;
	private GUIStyle iconLabelStyle;
	private GUIStyle iconSelectionStyle;

	public void LoadDependencies(Font MaterialIconsRegular)
	{
		showNames = EditorPrefs.GetBool("MaterialIconSelectionWindow.showNames", true);
		iconZoom = EditorPrefs.GetInt("MaterialIconSelectionWindow.iconZoom", 1);

		if(MaterialIconsRegular == null)
			return;
		
		this.MaterialIconsRegular = MaterialIconsRegular;

		string fontPath = AssetDatabase.GetAssetPath(MaterialIconsRegular);
		string codepointsPath = Path.GetDirectoryName(fontPath) + "/codepoints";
	
		List<CodepointData> tempList = new List<CodepointData>();

		foreach(string codepoint in File.ReadAllLines(codepointsPath))
		{
			string[] data = codepoint.Split(' ');
			tempList.Add(new CodepointData(data[0], data[1]));
		}

		CodepointsCollection = tempList.ToArray();
		selectedIndex = System.Array.FindIndex(CodepointsCollection, (data) => data.codeGUIContent.text == selected);
	}

	public static void Init(Font MaterialIconsRegular, string preSelect, System.Action<string> callback)
	{
		MaterialIconSelectionWindow window = EditorWindow.GetWindow<MaterialIconSelectionWindow>(true);
		window.selected = preSelect;
		window.OnSelectionChanged = callback;
		window.LoadDependencies(MaterialIconsRegular);
	}

	private void OnEnable()
	{
		base.titleContent = new GUIContent("Material Icon Selection");
		base.minSize = new Vector2(430f, 570f);
		selectionKeep = true;
	}

	private void OnGUI()
	{
		if((toolbarSeachTextFieldStyle == null) || (iconImageStyle == null))
		{
			toolbarSeachTextFieldStyle = new GUIStyle("ToolbarSeachTextField");
			toolbarSeachCancelButtonStyle = new GUIStyle("ToolbarSeachCancelButton");
			toolbarSeachCancelButtonEmptyStyle = new GUIStyle("ToolbarSeachCancelButtonEmpty");
			toolbarLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
			iconImageStyle = new GUIStyle() { font = MaterialIconsRegular, alignment = TextAnchor.MiddleCenter };
			iconLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.UpperCenter, wordWrap = true };
			iconSelectionStyle = new GUIStyle("RL Element");
			iconImageStyle.normal.textColor = iconLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? lightColor : darkColor;
		}

		OnHeaderGUI();

		if(MaterialIconsRegular == null)
		{
			EditorGUILayout.HelpBox("Could not find \"MaterialIcons-Regular\" font data.", MessageType.Error);
			return;
		}

		if((CodepointsCollection == null) || (CodepointsCollection.Length == 0))
		{
			EditorGUILayout.HelpBox("Could not find \"codepoints\" font data.", MessageType.Error);
			return;
		}

		OnBodyGUI();
	}

	private void OnHeaderGUI()
	{
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

		GUI.SetNextControlName("MaterialIconSelectionWindow.filterText");
		filterText = EditorGUILayout.TextField(filterText, toolbarSeachTextFieldStyle);

		Rect clearRect = GUILayoutUtility.GetRect(20f, 14f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		clearRect.y += 1.5f;

		GUI.Label(clearRect, GUIContent.none, toolbarSeachCancelButtonEmptyStyle);
		if(GUI.Button(clearRect, GUIContent.none, string.IsNullOrEmpty(filterText) ? toolbarSeachCancelButtonEmptyStyle : toolbarSeachCancelButtonStyle))
		{
			filterText = string.Empty;
			GUI.FocusControl(null);
		}

		if(!filterGotFocus)
		{
			EditorGUI.FocusTextInControl("MaterialIconSelectionWindow.filterText");
			filterGotFocus = true;
		}

		Rect nameRect = GUILayoutUtility.GetRect(64f, 14f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		EditorGUI.BeginChangeCheck();
		showNames = EditorGUI.Toggle(nameRect, showNames, EditorStyles.toolbarButton);
		if(EditorGUI.EndChangeCheck())
		{
			EditorPrefs.SetBool("MaterialIconSelectionWindow.showNames", showNames);
		}
		nameRect.y += 1.5f;
		EditorGUI.LabelField(nameRect, "Names", toolbarLabelStyle);

		Rect zoomRect = GUILayoutUtility.GetRect(32f, 14f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		EditorGUI.BeginChangeCheck();
		EditorGUI.Toggle(zoomRect, iconZoom == 2, EditorStyles.toolbarButton);
		if(EditorGUI.EndChangeCheck())
		{
			iconZoom = (iconZoom == 1) ? 2 : 1;
			EditorPrefs.SetInt("MaterialIconSelectionWindow.iconZoom", iconZoom);
		}
		zoomRect.y += 1.5f;
		EditorGUI.LabelField(zoomRect, "x2", toolbarLabelStyle);

		iconImageStyle.fontSize = Mathf.FloorToInt((iconSize - 10) * iconZoom);

		EditorGUILayout.EndHorizontal();
	}

	private void OnBodyGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
		GUILayout.Space(showNames ? 4f : 0f);

		int k = 0;
		int c = Mathf.FloorToInt((base.position.width - 16f) / ((iconSize * iconZoom) + 8f));
		for(int i = 0; i < CodepointsCollection.Length; i++)
		{
			var data = CodepointsCollection[i];

			if(!string.IsNullOrEmpty(filterText) && filterText.Split(' ').Any(filter => data.nameGUIContent.text.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) < 0))
				continue;

			if(k++ % c == 0)
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
				GUILayout.Space(showNames ? 4f : 0f);
			}

			GUILayout.Space(showNames ? 4f : 8f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(4f);

			Rect irect = GUILayoutUtility.GetRect(iconSize * iconZoom, iconSize * iconZoom, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
			Rect lrect = Rect.zero;
			if(showNames)
			{
				lrect = GUILayoutUtility.GetRect(iconSize * iconZoom - 4f, iconLabelStyle.CalcHeight(data.nameGUIContent, iconSize * iconZoom - 4f), iconLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
				lrect.x = irect.x;
				lrect.width = irect.width;
			}
			Rect brect = new Rect(irect) { height = irect.height + lrect.height };
			Rect hrect = new Rect(brect.x - 5, brect.y - 5, brect.width + 10, brect.height + 10);

			if(Event.current.type == EventType.Repaint)
			{
				if(i == selectedIndex)
				{
					iconSelectionStyle.Draw(hrect, false, true, true, true);
					if(selectionKeep)
					{
						scrollPos.y = Mathf.Clamp(scrollPos.y, irect.y + irect.height + lrect.height - base.position.height + EditorStyles.toolbar.fixedHeight, irect.y);
						selectionKeep = false;
						base.Repaint();
					}
				}
			}

			EditorGUILayout.EndVertical();

			GUI.Label(irect, data.codeGUIContent, iconImageStyle);
			if(showNames)
				GUI.Label(lrect, data.nameGUIContent, iconLabelStyle);

			if(GUI.Button(brect, GUIContent.none, GUIStyle.none))
			{
				selected = data.codeGUIContent.text;
				selectedIndex = i;
				OnSelectionChanged.Invoke(selected);
			}

		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		GUILayout.EndScrollView();
	}

	[System.Serializable]
	public class CodepointData
	{
		public string name { get; private set; }
		public string code { get; private set; }
		public GUIContent nameGUIContent { get; private set; }
		public GUIContent codeGUIContent { get; private set; }

		public CodepointData(string name, string code)
		{
			this.name = name;
			this.code = code;
			this.nameGUIContent = new GUIContent(string.Format("{0} ({1})", name.ToLowerInvariant().Replace('_', ' '), code));
			this.codeGUIContent = new GUIContent(char.ConvertFromUtf32(System.Convert.ToInt32(this.code, 16)), this.nameGUIContent.text);
		}

	}

}

}
