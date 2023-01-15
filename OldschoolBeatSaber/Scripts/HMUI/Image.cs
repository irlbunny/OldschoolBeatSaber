using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace HMUI
{
	public class Image : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
	{
		public enum Type
		{
			Simple = 0,
			Sliced = 1,
			Tiled = 2,
			Filled = 3
		}

		public enum FillMethod
		{
			Horizontal = 0,
			Vertical = 1,
			Radial90 = 2,
			Radial180 = 3,
			Radial360 = 4
		}

		public enum OriginHorizontal
		{
			Left = 0,
			Right = 1
		}

		public enum OriginVertical
		{
			Bottom = 0,
			Top = 1
		}

		public enum Origin90
		{
			BottomLeft = 0,
			TopLeft = 1,
			TopRight = 2,
			BottomRight = 3
		}

		public enum Origin180
		{
			Bottom = 0,
			Left = 1,
			Top = 2,
			Right = 3
		}

		public enum Origin360
		{
			Bottom = 0,
			Right = 1,
			Top = 2,
			Left = 3
		}

		[FormerlySerializedAs("m_Frame")]
		[SerializeField]
		private Sprite m_Sprite;

		[NonSerialized]
		private Sprite m_OverrideSprite;

		[SerializeField]
		private Type m_Type;

		[SerializeField]
		private bool m_PreserveAspect;

		[SerializeField]
		private bool m_FillCenter = true;

		[SerializeField]
		private FillMethod m_FillMethod = FillMethod.Radial360;

		[Range(0f, 1f)]
		[SerializeField]
		private float m_FillAmount = 1f;

		[SerializeField]
		private bool m_FillClockwise = true;

		[SerializeField]
		private int m_FillOrigin;

		private float m_EventAlphaThreshold = 1f;

		private static readonly Vector2[] s_VertScratch = new Vector2[4];

		private static readonly Vector2[] s_UVScratch = new Vector2[4];

		private static readonly Vector2[] s_UV2Scratch = new Vector2[4];

		private static readonly Vector3[] s_Xy = new Vector3[4];

		private static readonly Vector3[] s_Uv = new Vector3[4];

		public Sprite sprite
		{
			get
			{
				return m_Sprite;
			}
			set
			{
				if (SetPropertyUtility.SetClass(ref m_Sprite, value))
				{
					SetAllDirty();
				}
			}
		}

		public Sprite overrideSprite
		{
			get
			{
				return (!(m_OverrideSprite == null)) ? m_OverrideSprite : sprite;
			}
			set
			{
				if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value))
				{
					SetAllDirty();
				}
			}
		}

		public Type type
		{
			get
			{
				return m_Type;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_Type, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public bool preserveAspect
		{
			get
			{
				return m_PreserveAspect;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_PreserveAspect, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public bool fillCenter
		{
			get
			{
				return m_FillCenter;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_FillCenter, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public FillMethod fillMethod
		{
			get
			{
				return m_FillMethod;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_FillMethod, value))
				{
					SetVerticesDirty();
					m_FillOrigin = 0;
				}
			}
		}

		public float fillAmount
		{
			get
			{
				return m_FillAmount;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_FillAmount, Mathf.Clamp01(value)))
				{
					SetVerticesDirty();
				}
			}
		}

		public bool fillClockwise
		{
			get
			{
				return m_FillClockwise;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_FillClockwise, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public int fillOrigin
		{
			get
			{
				return m_FillOrigin;
			}
			set
			{
				if (SetPropertyUtility.SetStruct(ref m_FillOrigin, value))
				{
					SetVerticesDirty();
				}
			}
		}

		public float eventAlphaThreshold
		{
			get
			{
				return m_EventAlphaThreshold;
			}
			set
			{
				m_EventAlphaThreshold = value;
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (overrideSprite == null)
				{
					if (material != null && material.mainTexture != null)
					{
						return material.mainTexture;
					}
					return Graphic.s_WhiteTexture;
				}
				return overrideSprite.texture;
			}
		}

		public bool hasBorder
		{
			get
			{
				if (overrideSprite != null)
				{
					return overrideSprite.border.sqrMagnitude > 0f;
				}
				return false;
			}
		}

		public float pixelsPerUnit
		{
			get
			{
				float num = 100f;
				if ((bool)sprite)
				{
					num = sprite.pixelsPerUnit;
				}
				float num2 = 100f;
				if ((bool)base.canvas)
				{
					num2 = base.canvas.referencePixelsPerUnit;
				}
				return num / num2;
			}
		}

		public virtual float minWidth
		{
			get
			{
				return 0f;
			}
		}

		public virtual float preferredWidth
		{
			get
			{
				if (overrideSprite == null)
				{
					return 0f;
				}
				if (type == Type.Sliced || type == Type.Tiled)
				{
					return DataUtility.GetMinSize(overrideSprite).x / pixelsPerUnit;
				}
				return overrideSprite.rect.size.x / pixelsPerUnit;
			}
		}

		public virtual float flexibleWidth
		{
			get
			{
				return -1f;
			}
		}

		public virtual float minHeight
		{
			get
			{
				return 0f;
			}
		}

		public virtual float preferredHeight
		{
			get
			{
				if (overrideSprite == null)
				{
					return 0f;
				}
				if (type == Type.Sliced || type == Type.Tiled)
				{
					return DataUtility.GetMinSize(overrideSprite).y / pixelsPerUnit;
				}
				return overrideSprite.rect.size.y / pixelsPerUnit;
			}
		}

		public virtual float flexibleHeight
		{
			get
			{
				return -1f;
			}
		}

		public virtual int layoutPriority
		{
			get
			{
				return 0;
			}
		}

		protected Image()
		{
			base.useLegacyMeshGeneration = false;
		}

		public virtual void OnBeforeSerialize()
		{
		}

		public virtual void OnAfterDeserialize()
		{
			if (m_FillOrigin < 0)
			{
				m_FillOrigin = 0;
			}
			else if (m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1)
			{
				m_FillOrigin = 0;
			}
			else if (m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1)
			{
				m_FillOrigin = 0;
			}
			else if (m_FillOrigin > 3)
			{
				m_FillOrigin = 0;
			}
			m_FillAmount = Mathf.Clamp(m_FillAmount, 0f, 1f);
		}

		private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
		{
			Vector4 vector = ((!(overrideSprite == null)) ? DataUtility.GetPadding(overrideSprite) : Vector4.zero);
			Vector2 vector2 = ((!(overrideSprite == null)) ? new Vector2(overrideSprite.rect.width, overrideSprite.rect.height) : Vector2.zero);
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			int num = Mathf.RoundToInt(vector2.x);
			int num2 = Mathf.RoundToInt(vector2.y);
			Vector4 vector3 = new Vector4(vector.x / (float)num, vector.y / (float)num2, ((float)num - vector.z) / (float)num, ((float)num2 - vector.w) / (float)num2);
			if (shouldPreserveAspect && vector2.sqrMagnitude > 0f)
			{
				float num3 = vector2.x / vector2.y;
				float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
				if (num3 > num4)
				{
					float height = pixelAdjustedRect.height;
					pixelAdjustedRect.height = pixelAdjustedRect.width * (1f / num3);
					pixelAdjustedRect.y += (height - pixelAdjustedRect.height) * base.rectTransform.pivot.y;
				}
				else
				{
					float width = pixelAdjustedRect.width;
					pixelAdjustedRect.width = pixelAdjustedRect.height * num3;
					pixelAdjustedRect.x += (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x;
				}
			}
			return new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.x, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.y, pixelAdjustedRect.x + pixelAdjustedRect.width * vector3.z, pixelAdjustedRect.y + pixelAdjustedRect.height * vector3.w);
		}

		public override void SetNativeSize()
		{
			if (overrideSprite != null)
			{
				float x = overrideSprite.rect.width / pixelsPerUnit;
				float y = overrideSprite.rect.height / pixelsPerUnit;
				base.rectTransform.anchorMax = base.rectTransform.anchorMin;
				base.rectTransform.sizeDelta = new Vector2(x, y);
				SetAllDirty();
			}
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (overrideSprite == null)
			{
				base.OnPopulateMesh(toFill);
				return;
			}
			switch (type)
			{
			case Type.Simple:
				GenerateSimpleSprite(toFill, m_PreserveAspect);
				break;
			case Type.Sliced:
				GenerateSlicedSprite(toFill);
				break;
			case Type.Tiled:
				GenerateTiledSprite(toFill);
				break;
			case Type.Filled:
				GenerateFilledSprite(toFill, m_PreserveAspect);
				break;
			}
		}

		private void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
		{
			Vector4 drawingDimensions = GetDrawingDimensions(lPreserveAspect);
			Vector4 vector = ((!(overrideSprite != null)) ? Vector4.zero : DataUtility.GetOuterUV(overrideSprite));
			Color color = this.color;
			vh.Clear();
			vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), color, new Vector2(vector.x, vector.y));
			vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), color, new Vector2(vector.x, vector.w));
			vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), color, new Vector2(vector.z, vector.w));
			vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), color, new Vector2(vector.z, vector.y));
			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}

		private void GenerateSlicedSprite(VertexHelper toFill)
		{
			if (!hasBorder)
			{
				GenerateSimpleSprite(toFill, false);
				return;
			}
			Vector4 vector;
			Vector4 vector2;
			Vector4 vector3;
			Vector4 vector4;
			if (overrideSprite != null)
			{
				vector = DataUtility.GetOuterUV(overrideSprite);
				vector2 = DataUtility.GetInnerUV(overrideSprite);
				vector3 = DataUtility.GetPadding(overrideSprite);
				vector4 = overrideSprite.border;
			}
			else
			{
				vector = Vector4.zero;
				vector2 = Vector4.zero;
				vector3 = Vector4.zero;
				vector4 = Vector4.zero;
			}
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			vector4 = GetAdjustedBorders(vector4 / pixelsPerUnit, pixelAdjustedRect);
			vector3 /= pixelsPerUnit;
			s_VertScratch[0] = new Vector2(vector3.x, vector3.y);
			s_VertScratch[3] = new Vector2(pixelAdjustedRect.width - vector3.z, pixelAdjustedRect.height - vector3.w);
			s_VertScratch[1].x = vector4.x;
			s_VertScratch[1].y = vector4.y;
			s_VertScratch[2].x = pixelAdjustedRect.width - vector4.z;
			s_VertScratch[2].y = pixelAdjustedRect.height - vector4.w;
			Vector2 scale = new Vector2(1f / pixelAdjustedRect.width, 1f / pixelAdjustedRect.height);
			for (int i = 0; i < 4; i++)
			{
				s_UV2Scratch[i] = s_VertScratch[i];
				s_UV2Scratch[i].Scale(scale);
			}
			for (int j = 0; j < 4; j++)
			{
				s_VertScratch[j].x += pixelAdjustedRect.x;
				s_VertScratch[j].y += pixelAdjustedRect.y;
			}
			s_UVScratch[0] = new Vector2(vector.x, vector.y);
			s_UVScratch[1] = new Vector2(vector2.x, vector2.y);
			s_UVScratch[2] = new Vector2(vector2.z, vector2.w);
			s_UVScratch[3] = new Vector2(vector.z, vector.w);
			toFill.Clear();
			for (int k = 0; k < 3; k++)
			{
				int num = k + 1;
				for (int l = 0; l < 3; l++)
				{
					if (m_FillCenter || k != 1 || l != 1)
					{
						int num2 = l + 1;
						AddQuad(toFill, new Vector2(s_VertScratch[k].x, s_VertScratch[l].y), new Vector2(s_VertScratch[num].x, s_VertScratch[num2].y), color, new Vector2(s_UVScratch[k].x, s_UVScratch[l].y), new Vector2(s_UVScratch[num].x, s_UVScratch[num2].y), new Vector2(s_UV2Scratch[k].x, s_UV2Scratch[l].y), new Vector2(s_UV2Scratch[num].x, s_UV2Scratch[num2].y));
					}
				}
			}
		}

		private void GenerateTiledSprite(VertexHelper toFill)
		{
			Vector4 vector;
			Vector4 vector2;
			Vector2 vector4;
			Vector4 vector3;
			if (overrideSprite != null)
			{
				vector = DataUtility.GetOuterUV(overrideSprite);
				vector2 = DataUtility.GetInnerUV(overrideSprite);
				vector3 = overrideSprite.border;
				vector4 = overrideSprite.rect.size;
			}
			else
			{
				vector = Vector4.zero;
				vector2 = Vector4.zero;
				vector3 = Vector4.zero;
				vector4 = Vector2.one * 100f;
			}
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			float num = (vector4.x - vector3.x - vector3.z) / pixelsPerUnit;
			float num2 = (vector4.y - vector3.y - vector3.w) / pixelsPerUnit;
			vector3 = GetAdjustedBorders(vector3 / pixelsPerUnit, pixelAdjustedRect);
			Vector2 uvMin = new Vector2(vector2.x, vector2.y);
			Vector2 vector5 = new Vector2(vector2.z, vector2.w);
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = color;
			float x = vector3.x;
			float num3 = pixelAdjustedRect.width - vector3.z;
			float y = vector3.y;
			float num4 = pixelAdjustedRect.height - vector3.w;
			toFill.Clear();
			Vector2 uvMax = vector5;
			if (num == 0f)
			{
				num = num3 - x;
			}
			if (num2 == 0f)
			{
				num2 = num4 - y;
			}
			if (m_FillCenter)
			{
				for (float num5 = y; num5 < num4; num5 += num2)
				{
					float num6 = num5 + num2;
					if (num6 > num4)
					{
						uvMax.y = uvMin.y + (vector5.y - uvMin.y) * (num4 - num5) / (num6 - num5);
						num6 = num4;
					}
					uvMax.x = vector5.x;
					for (float num7 = x; num7 < num3; num7 += num)
					{
						float num8 = num7 + num;
						if (num8 > num3)
						{
							uvMax.x = uvMin.x + (vector5.x - uvMin.x) * (num3 - num7) / (num8 - num7);
							num8 = num3;
						}
						AddQuad(toFill, new Vector2(num7, num5) + pixelAdjustedRect.position, new Vector2(num8, num6) + pixelAdjustedRect.position, color, uvMin, uvMax);
					}
				}
			}
			if (!hasBorder)
			{
				return;
			}
			uvMax = vector5;
			for (float num9 = y; num9 < num4; num9 += num2)
			{
				float num10 = num9 + num2;
				if (num10 > num4)
				{
					uvMax.y = uvMin.y + (vector5.y - uvMin.y) * (num4 - num9) / (num10 - num9);
					num10 = num4;
				}
				AddQuad(toFill, new Vector2(0f, num9) + pixelAdjustedRect.position, new Vector2(x, num10) + pixelAdjustedRect.position, color, new Vector2(vector.x, uvMin.y), new Vector2(uvMin.x, uvMax.y));
				AddQuad(toFill, new Vector2(num3, num9) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, num10) + pixelAdjustedRect.position, color, new Vector2(vector5.x, uvMin.y), new Vector2(vector.z, uvMax.y));
			}
			uvMax = vector5;
			for (float num11 = x; num11 < num3; num11 += num)
			{
				float num12 = num11 + num;
				if (num12 > num3)
				{
					uvMax.x = uvMin.x + (vector5.x - uvMin.x) * (num3 - num11) / (num12 - num11);
					num12 = num3;
				}
				AddQuad(toFill, new Vector2(num11, 0f) + pixelAdjustedRect.position, new Vector2(num12, y) + pixelAdjustedRect.position, color, new Vector2(uvMin.x, vector.y), new Vector2(uvMax.x, uvMin.y));
				AddQuad(toFill, new Vector2(num11, num4) + pixelAdjustedRect.position, new Vector2(num12, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(uvMin.x, vector5.y), new Vector2(uvMax.x, vector.w));
			}
			AddQuad(toFill, new Vector2(0f, 0f) + pixelAdjustedRect.position, new Vector2(x, y) + pixelAdjustedRect.position, color, new Vector2(vector.x, vector.y), new Vector2(uvMin.x, uvMin.y));
			AddQuad(toFill, new Vector2(num3, 0f) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, y) + pixelAdjustedRect.position, color, new Vector2(vector5.x, vector.y), new Vector2(vector.z, uvMin.y));
			AddQuad(toFill, new Vector2(0f, num4) + pixelAdjustedRect.position, new Vector2(x, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(vector.x, vector5.y), new Vector2(uvMin.x, vector.w));
			AddQuad(toFill, new Vector2(num3, num4) + pixelAdjustedRect.position, new Vector2(pixelAdjustedRect.width, pixelAdjustedRect.height) + pixelAdjustedRect.position, color, new Vector2(vector5.x, vector5.y), new Vector2(vector.z, vector.w));
		}

		private static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
		{
			int currentVertCount = vertexHelper.currentVertCount;
			for (int i = 0; i < 4; i++)
			{
				vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);
			}
			vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}

		private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
		{
			int currentVertCount = vertexHelper.currentVertCount;
			vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
			vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
			vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
			vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
			vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}

		private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax, Vector2 uv2Min, Vector2 uv2Max)
		{
			int currentVertCount = vertexHelper.currentVertCount;
			vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y), new Vector2(uv2Min.x, uv2Min.y), Vector3.zero, Vector4.zero);
			vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y), new Vector2(uv2Min.x, uv2Max.y), Vector3.zero, Vector4.zero);
			vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y), new Vector2(uv2Max.x, uv2Max.y), Vector3.zero, Vector4.zero);
			vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y), new Vector2(uv2Max.x, uv2Min.y), Vector3.zero, Vector4.zero);
			vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}

		private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
		{
			for (int i = 0; i <= 1; i++)
			{
				float num = border[i] + border[i + 2];
				if (rect.size[i] < num && num != 0f)
				{
					float num2 = rect.size[i] / num;
					border[i] *= num2;
					border[i + 2] *= num2;
				}
			}
			return border;
		}

		private void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
		{
			toFill.Clear();
			if (m_FillAmount < 0.001f)
			{
				return;
			}
			Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
			Vector4 vector = ((!(overrideSprite != null)) ? Vector4.zero : DataUtility.GetOuterUV(overrideSprite));
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = color;
			float num = vector.x;
			float num2 = vector.y;
			float num3 = vector.z;
			float num4 = vector.w;
			if (m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical)
			{
				if (fillMethod == FillMethod.Horizontal)
				{
					float num5 = (num3 - num) * m_FillAmount;
					if (m_FillOrigin == 1)
					{
						drawingDimensions.x = drawingDimensions.z - (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
						num = num3 - num5;
					}
					else
					{
						drawingDimensions.z = drawingDimensions.x + (drawingDimensions.z - drawingDimensions.x) * m_FillAmount;
						num3 = num + num5;
					}
				}
				else if (fillMethod == FillMethod.Vertical)
				{
					float num6 = (num4 - num2) * m_FillAmount;
					if (m_FillOrigin == 1)
					{
						drawingDimensions.y = drawingDimensions.w - (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
						num2 = num4 - num6;
					}
					else
					{
						drawingDimensions.w = drawingDimensions.y + (drawingDimensions.w - drawingDimensions.y) * m_FillAmount;
						num4 = num2 + num6;
					}
				}
			}
			s_Xy[0] = new Vector2(drawingDimensions.x, drawingDimensions.y);
			s_Xy[1] = new Vector2(drawingDimensions.x, drawingDimensions.w);
			s_Xy[2] = new Vector2(drawingDimensions.z, drawingDimensions.w);
			s_Xy[3] = new Vector2(drawingDimensions.z, drawingDimensions.y);
			s_Uv[0] = new Vector2(num, num2);
			s_Uv[1] = new Vector2(num, num4);
			s_Uv[2] = new Vector2(num3, num4);
			s_Uv[3] = new Vector2(num3, num2);
			if (m_FillAmount < 1f && m_FillMethod != 0 && m_FillMethod != FillMethod.Vertical)
			{
				if (fillMethod == FillMethod.Radial90)
				{
					if (RadialCut(s_Xy, s_Uv, m_FillAmount, m_FillClockwise, m_FillOrigin))
					{
						AddQuad(toFill, s_Xy, color, s_Uv);
					}
				}
				else if (fillMethod == FillMethod.Radial180)
				{
					for (int i = 0; i < 2; i++)
					{
						int num7 = ((m_FillOrigin > 1) ? 1 : 0);
						float t;
						float t2;
						float t3;
						float t4;
						if (m_FillOrigin == 0 || m_FillOrigin == 2)
						{
							t = 0f;
							t2 = 1f;
							if (i == num7)
							{
								t3 = 0f;
								t4 = 0.5f;
							}
							else
							{
								t3 = 0.5f;
								t4 = 1f;
							}
						}
						else
						{
							t3 = 0f;
							t4 = 1f;
							if (i == num7)
							{
								t = 0.5f;
								t2 = 1f;
							}
							else
							{
								t = 0f;
								t2 = 0.5f;
							}
						}
						s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t3);
						s_Xy[1].x = s_Xy[0].x;
						s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t4);
						s_Xy[3].x = s_Xy[2].x;
						s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t);
						s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t2);
						s_Xy[2].y = s_Xy[1].y;
						s_Xy[3].y = s_Xy[0].y;
						s_Uv[0].x = Mathf.Lerp(num, num3, t3);
						s_Uv[1].x = s_Uv[0].x;
						s_Uv[2].x = Mathf.Lerp(num, num3, t4);
						s_Uv[3].x = s_Uv[2].x;
						s_Uv[0].y = Mathf.Lerp(num2, num4, t);
						s_Uv[1].y = Mathf.Lerp(num2, num4, t2);
						s_Uv[2].y = s_Uv[1].y;
						s_Uv[3].y = s_Uv[0].y;
						float value = ((!m_FillClockwise) ? (m_FillAmount * 2f - (float)(1 - i)) : (fillAmount * 2f - (float)i));
						if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value), m_FillClockwise, (i + m_FillOrigin + 3) % 4))
						{
							AddQuad(toFill, s_Xy, color, s_Uv);
						}
					}
				}
				else
				{
					if (fillMethod != FillMethod.Radial360)
					{
						return;
					}
					for (int j = 0; j < 4; j++)
					{
						float t5;
						float t6;
						if (j < 2)
						{
							t5 = 0f;
							t6 = 0.5f;
						}
						else
						{
							t5 = 0.5f;
							t6 = 1f;
						}
						float t7;
						float t8;
						if (j == 0 || j == 3)
						{
							t7 = 0f;
							t8 = 0.5f;
						}
						else
						{
							t7 = 0.5f;
							t8 = 1f;
						}
						s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t5);
						s_Xy[1].x = s_Xy[0].x;
						s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, t6);
						s_Xy[3].x = s_Xy[2].x;
						s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t7);
						s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, t8);
						s_Xy[2].y = s_Xy[1].y;
						s_Xy[3].y = s_Xy[0].y;
						s_Uv[0].x = Mathf.Lerp(num, num3, t5);
						s_Uv[1].x = s_Uv[0].x;
						s_Uv[2].x = Mathf.Lerp(num, num3, t6);
						s_Uv[3].x = s_Uv[2].x;
						s_Uv[0].y = Mathf.Lerp(num2, num4, t7);
						s_Uv[1].y = Mathf.Lerp(num2, num4, t8);
						s_Uv[2].y = s_Uv[1].y;
						s_Uv[3].y = s_Uv[0].y;
						float value2 = ((!m_FillClockwise) ? (m_FillAmount * 4f - (float)(3 - (j + m_FillOrigin) % 4)) : (m_FillAmount * 4f - (float)((j + m_FillOrigin) % 4)));
						if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(value2), m_FillClockwise, (j + 2) % 4))
						{
							AddQuad(toFill, s_Xy, color, s_Uv);
						}
					}
				}
			}
			else
			{
				AddQuad(toFill, s_Xy, color, s_Uv);
			}
		}

		private static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
		{
			if (fill < 0.001f)
			{
				return false;
			}
			if ((corner & 1) == 1)
			{
				invert = !invert;
			}
			if (!invert && fill > 0.999f)
			{
				return true;
			}
			float num = Mathf.Clamp01(fill);
			if (invert)
			{
				num = 1f - num;
			}
			num *= (float)Math.PI / 2f;
			float cos = Mathf.Cos(num);
			float sin = Mathf.Sin(num);
			RadialCut(xy, cos, sin, invert, corner);
			RadialCut(uv, cos, sin, invert, corner);
			return true;
		}

		private static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
		{
			int num = (corner + 1) % 4;
			int num2 = (corner + 2) % 4;
			int num3 = (corner + 3) % 4;
			if ((corner & 1) == 1)
			{
				if (sin > cos)
				{
					cos /= sin;
					sin = 1f;
					if (invert)
					{
						xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
						xy[num2].x = xy[num].x;
					}
				}
				else if (cos > sin)
				{
					sin /= cos;
					cos = 1f;
					if (!invert)
					{
						xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
						xy[num3].y = xy[num2].y;
					}
				}
				else
				{
					cos = 1f;
					sin = 1f;
				}
				if (!invert)
				{
					xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
				}
				else
				{
					xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
				}
				return;
			}
			if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
					xy[num2].y = xy[num].y;
				}
			}
			else if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
					xy[num3].x = xy[num2].x;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (invert)
			{
				xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
			}
			else
			{
				xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
			}
		}

		public virtual void CalculateLayoutInputHorizontal()
		{
		}

		public virtual void CalculateLayoutInputVertical()
		{
		}

		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
		{
			if (m_EventAlphaThreshold >= 1f)
			{
				return true;
			}
			Sprite sprite = overrideSprite;
			if (sprite == null)
			{
				return true;
			}
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out localPoint);
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			localPoint.x += base.rectTransform.pivot.x * pixelAdjustedRect.width;
			localPoint.y += base.rectTransform.pivot.y * pixelAdjustedRect.height;
			localPoint = MapCoordinate(localPoint, pixelAdjustedRect);
			Rect textureRect = sprite.textureRect;
			Vector2 vector = new Vector2(localPoint.x / textureRect.width, localPoint.y / textureRect.height);
			float u = Mathf.Lerp(textureRect.x, textureRect.xMax, vector.x) / (float)sprite.texture.width;
			float v = Mathf.Lerp(textureRect.y, textureRect.yMax, vector.y) / (float)sprite.texture.height;
			try
			{
				return sprite.texture.GetPixelBilinear(u, v).a >= m_EventAlphaThreshold;
			}
			catch (UnityException ex)
			{
				Debug.LogError("Using clickAlphaThreshold lower than 1 on Image whose sprite texture cannot be read. " + ex.Message + " Also make sure to disable sprite packing for this sprite.", this);
				return true;
			}
		}

		private Vector2 MapCoordinate(Vector2 local, Rect rect)
		{
			Rect rect2 = sprite.rect;
			if (type == Type.Simple || type == Type.Filled)
			{
				return new Vector2(local.x * rect2.width / rect.width, local.y * rect2.height / rect.height);
			}
			Vector4 border = sprite.border;
			Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
			for (int i = 0; i < 2; i++)
			{
				if (!(local[i] <= adjustedBorders[i]))
				{
					if (rect.size[i] - local[i] <= adjustedBorders[i + 2])
					{
						local[i] -= rect.size[i] - rect2.size[i];
					}
					else if (type == Type.Sliced)
					{
						float t = Mathf.InverseLerp(adjustedBorders[i], rect.size[i] - adjustedBorders[i + 2], local[i]);
						local[i] = Mathf.Lerp(border[i], rect2.size[i] - border[i + 2], t);
					}
					else
					{
						local[i] -= adjustedBorders[i];
						local[i] = Mathf.Repeat(local[i], rect2.size[i] - border[i] - border[i + 2]);
						local[i] += border[i];
					}
				}
			}
			return local;
		}
	}
}
