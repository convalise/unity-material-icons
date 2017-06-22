
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Google.MaterialDesign.Icons
{

public class MaterialIcon : Text
{
	protected override void Start()
	{
		base.Start();
	}

	#if UNITY_EDITOR
	protected override void Reset()
	{
		base.Reset();
		base.text = "\ue84d";
		base.font = null;
		base.alignment = TextAnchor.MiddleCenter;
		base.supportRichText = false;
		base.horizontalOverflow = HorizontalWrapMode.Overflow;
		base.verticalOverflow = VerticalWrapMode.Overflow;
		base.fontSize = Mathf.FloorToInt(Mathf.Min(base.rectTransform.sizeDelta.x, base.rectTransform.sizeDelta.y));
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		SetLayoutDirty();
	}
	#endif

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		base.fontSize = Mathf.FloorToInt(Mathf.Min(base.rectTransform.sizeDelta.x, base.rectTransform.sizeDelta.y));
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if(font == null)
			return;

		base.m_DisableFontTextureRebuiltCallback = true;

		TextGenerationSettings settings = base.GetGenerationSettings(base.rectTransform.rect.size);
		string str = System.Text.RegularExpressions.Regex.Unescape(base.text);

		base.cachedTextGenerator.Populate(str, settings);

		Vector2 refPoint = new Vector2(base.rectTransform.rect.xMin, base.rectTransform.rect.yMax);
		Vector2 roundingOffset = base.PixelAdjustPoint(refPoint) - refPoint;

		IList<UIVertex> verts = base.cachedTextGenerator.verts;
		float unitsPerPixel = 1f / pixelsPerUnit;

		UIVertex[] tempVerts = new UIVertex[4];

		toFill.Clear();
		for(int i = 0; i < verts.Count - 4; i++)
		{
			int tempVertsIndex = i & 3;

			tempVerts[tempVertsIndex] = verts[i];
			tempVerts[tempVertsIndex].position *= unitsPerPixel;
			tempVerts[tempVertsIndex].position.x += roundingOffset.x;
			tempVerts[tempVertsIndex].position.y += roundingOffset.y;

			if(tempVertsIndex == 3)
				toFill.AddUIVertexQuad(tempVerts);
		}

		base.m_DisableFontTextureRebuiltCallback = false;
	}

}

}
