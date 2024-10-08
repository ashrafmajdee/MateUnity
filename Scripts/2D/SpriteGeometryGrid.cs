using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace M8 {
	[ExecuteInEditMode]
	[AddComponentMenu("M8/Sprite/Geometry Grid")]
	public class SpriteGeometryGrid : MonoBehaviour {
		public enum ColorGradientMode {
			None,
			Solid,
			Horizontal,
			Vertical,
			Both,
			Circular
		}

		[SerializeField]
		Sprite _sprite;

		[SerializeField]
		Material _material;

		[SerializeField]
		int _columnCount = 1;

		[SerializeField]
		int _rowCount = 1;

		[SerializeField]
		bool _overridePixelPerUnit;
		[SerializeField]
		float _pixelPerUnit = 32;

		[SerializeField]
		bool _flipX;
		[SerializeField]
		bool _flipY;

		[SerializeField]
		ColorGradientMode _gradientMode = ColorGradientMode.None;

		[SerializeField]
		Gradient[] _gradients = new Gradient[0];

		[SerializeField]
		Vector2 _gradientOffset;

		[SerializeField]
		bool _colorApply = true;

		[SerializeField]
		Color _color = Color.white;
				
		[SerializeField]
		[SortingLayer]
		int _sortingLayer = 0;

		[SerializeField]
		int _sortingOrder = 0;

		[SerializeField]
		MeshFilter _meshFilter;

		[SerializeField]
		MeshRenderer _meshRenderer;

		public Sprite sprite {
			get { return _sprite; }
			set {
				if(_sprite != value) {
					_sprite = value;

					if(_sprite)
						Init();
					else
						ClearMesh();
				}
			}
		}

		public Material sharedMaterial {
			get { return _material; }
			set {
				if(_material != value) {
					_material = value;

					if(isInit)
						_meshRenderer.sharedMaterial = _material;
				}
			}
		}

		public Color color {
			get { return _color; }
			set {
				if(_colorApply && _color != value) {
					_color = value;

					if(isInit) {
						if(mMatPropBlock == null)
							RefreshMaterialProperty();
						else {
							_meshRenderer.GetPropertyBlock(mMatPropBlock);

							mMatPropBlock.SetColor(mColorPropID, _color);

							_meshRenderer.SetPropertyBlock(mMatPropBlock);
						}
					}
				}
			}
		}

		public bool flipX {
			get { return _flipX; }
			set {
				if(_flipX != value) {
					_flipX = value;

					if(isInit) {
						if(mMatPropBlock == null)
							RefreshMaterialProperty();
						else {
							_meshRenderer.GetPropertyBlock(mMatPropBlock);

							mMatPropBlock.SetVector(mFlipPropID, new Vector4(_flipX ? -1f : 1f, _flipY ? -1f : 1f));

							_meshRenderer.SetPropertyBlock(mMatPropBlock);
						}
					}
				}
			}
		}

		public bool flipY {
			get { return _flipY; }
			set {
				if(_flipY != value) {
					_flipY = value;

					if(isInit) {
						if(mMatPropBlock == null)
							RefreshMaterialProperty();
						else {
							_meshRenderer.GetPropertyBlock(mMatPropBlock);

							mMatPropBlock.SetVector(mFlipPropID, new Vector4(_flipX ? -1f : 1f, _flipY ? -1f : 1f));

							_meshRenderer.SetPropertyBlock(mMatPropBlock);
						}
					}
				}
			}
		}

		public int sortingLayerID {
			get { return _sortingLayer; }
			set {
				if(_sortingLayer != value) {
					_sortingLayer = value;

					if(isInit)
						_meshRenderer.sortingLayerID = _sortingLayer;
				}
			}
		}

		public string sortingLayerName {
			get { return SortingLayer.IDToName(sortingLayerID); }
			set {
				sortingLayerID = SortingLayer.NameToID(value);
			}
		}

		public int sortingOrder {
			get { return _sortingOrder; }
			set {
				if(_sortingOrder != value) {
					_sortingOrder = value;

					if(isInit)
						_meshRenderer.sortingOrder = _sortingOrder;
				}
			}
		}

		public MeshRenderer meshRenderer { get { return _meshRenderer; } }

		public bool isInit { get; private set; }

		private Mesh mMesh;

		private static int mTexPropID = Shader.PropertyToID("_MainTex");
		private static int mFlipPropID = Shader.PropertyToID("_Flip");
		private static int mColorPropID = Shader.PropertyToID("_RendererColor");

		private MaterialPropertyBlock mMatPropBlock;

		public void Init() {
			if(!_sprite) return;

			GenerateGeometry();

			RefreshMeshRenderer();

			RefreshMaterialProperty();

			isInit = true;
		}

		public void ClearMesh() {
			if(mMesh) {
				if(Application.isPlaying)
					Destroy(mMesh);
				else
					DestroyImmediate(mMesh);

				mMesh = null;
			}

			if(_meshFilter)
				_meshFilter.sharedMesh = null;

			isInit = false;
		}

		public void RefreshMeshRenderer() {
			_meshRenderer.sharedMaterial = _material;
			_meshRenderer.sortingLayerID = _sortingLayer;
			_meshRenderer.sortingOrder = _sortingOrder;
		}

		public void RefreshMaterialProperty() {
			if(mMatPropBlock == null)
				mMatPropBlock = new MaterialPropertyBlock();

			_meshRenderer.GetPropertyBlock(mMatPropBlock);

			mMatPropBlock.SetTexture(mTexPropID, _sprite.texture);

			if(_colorApply)
				mMatPropBlock.SetColor(mColorPropID, _color);

			mMatPropBlock.SetVector(mFlipPropID, new Vector4(_flipX ? -1f : 1f, _flipY ? -1f : 1f));

			_meshRenderer.SetPropertyBlock(mMatPropBlock);
		}

		public void RefreshVertexColors() {
			if(_gradientMode == ColorGradientMode.None) {
				mMesh.colors32 = null;
			}
			else {
				var clrs = mMesh.colors32;

				var xCount = _columnCount + 1;
				var yCount = _rowCount + 1;
				var vtxCount = xCount * yCount;

				if(clrs == null || clrs.Length != vtxCount)
					clrs = new Color32[vtxCount];

				for(int r = 0; r < yCount; r++) {
					var yt = (float)r / _rowCount;

					for(int c = 0; c < xCount; c++) {
						var xt = (float)c / _columnCount;

						clrs[Util.CellToIndex(r, c, xCount)] = GetColorRange(yt, xt);
					}
				}

				mMesh.colors32 = clrs;
			}
		}

		void OnDidApplyAnimationProperties() {
			RefreshMeshRenderer();
			RefreshMaterialProperty();
		}

		void OnDestroy() {
			ClearMesh();
		}

		void Awake() {
			if(!isInit)
				Init();
		}
				
		private Color32 GetColorRange(float yt, float xt) {
			yt = Mathf.Clamp01(yt + _gradientOffset.y);
			xt = Mathf.Clamp01(xt + _gradientOffset.x);

			switch(_gradientMode) {
				case ColorGradientMode.Solid:
					return _gradients != null && _gradients.Length > 0 ? _gradients[0].Evaluate(0f) : Color.white;

				case ColorGradientMode.Horizontal:
					return _gradients != null && _gradients.Length > 0 ? _gradients[0].Evaluate(xt) : Color.white;

				case ColorGradientMode.Vertical:
					return _gradients != null && _gradients.Length > 0 ? _gradients[0].Evaluate(yt) : Color.white;

				case ColorGradientMode.Both:
					if(_gradients == null || _gradients.Length < 2) return Color.white;

					return Color.Lerp(_gradients[0].Evaluate(yt), _gradients[1].Evaluate(yt), xt);

				case ColorGradientMode.Circular:
					if(_gradients == null || _gradients.Length == 0) return Color.white;

					float xu = Mathf.Lerp(-1f, 1f, xt), yu = Mathf.Lerp(-1f, 1f, yt);

					var len = Mathf.Sqrt(xu * xu + yu * yu);

					return _gradients[0].Evaluate(Mathf.Clamp01(len));

				default:
					return new Color32(255, 255, 255, 255);
			}
		}
				
		private void GenerateGeometry() {
			if(!_sprite)
				return;

			if(!_meshFilter) {
				_meshFilter = GetComponent<MeshFilter>();
				if(!_meshFilter)
					_meshFilter = gameObject.AddComponent<MeshFilter>();
			}

			if(!mMesh) {
				mMesh = new Mesh();
				mMesh.name = name + "_mesh";
				//mMesh.MarkDynamic();
				
				_meshFilter.sharedMesh = mMesh;
			}

			if(!_meshRenderer) {
				_meshRenderer = GetComponent<MeshRenderer>();
				if(!_meshRenderer) {
					_meshRenderer = gameObject.AddComponent<MeshRenderer>();

					_meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					_meshRenderer.receiveShadows = false;
					_meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
					_meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
					_meshRenderer.allowOcclusionWhenDynamic = false;
				}
			}

			var ppu = _overridePixelPerUnit ? _pixelPerUnit : _sprite.pixelsPerUnit;
			var pivot = _sprite.pivot;

			var boundRect = _sprite.rect;

			Vector2 uvMin = Vector2.one, uvMax = Vector2.zero;

			var sprUvs = _sprite.uv;
			for(int i = 0; i < sprUvs.Length; i++) {
				var uv = sprUvs[i];

				if(uv.x < uvMin.x) uvMin.x = uv.x;
				if(uv.y < uvMin.y) uvMin.y = uv.y;
				if(uv.x > uvMax.x) uvMax.x = uv.x;
				if(uv.y > uvMax.y) uvMax.y = uv.y;
			}

			var xCount = _columnCount + 1;
			var yCount = _rowCount + 1;

			var vtxCount = xCount * yCount;
			var trisCount = _columnCount * _rowCount * 6;

			var vtx = new Vector3[vtxCount];
			var uvs = new Vector2[vtxCount];
			var tris = new int[trisCount];

			Color32[] clrs = _gradientMode != ColorGradientMode.None ? new Color32[vtxCount] : null;

			for(int r = 0; r < yCount; r++) {
				var yt = (float)r / _rowCount;

				var y = Mathf.Lerp(boundRect.min.y, boundRect.max.y, yt);

				y = (y - pivot.y) / ppu;

				var v = Mathf.Lerp(uvMin.y, uvMax.y, yt);

				for(int c = 0; c < xCount; c++) {
					var xt = (float)c / _columnCount;

					var x = Mathf.Lerp(boundRect.min.x, boundRect.max.x, xt);

					x = (x - pivot.x) / ppu;

					vtx[Util.CellToIndex(r, c, xCount)] = new Vector3(x, y);

					var u = Mathf.Lerp(uvMin.x, uvMax.x, xt);

					uvs[Util.CellToIndex(r, c, xCount)] = new Vector2(u, v);

					if(clrs != null)
						clrs[Util.CellToIndex(r, c, xCount)] = GetColorRange(yt, xt);
				}
			}

			var trisInd = 0;

			for(int r = 0; r < _rowCount; r++) {
				for(int c = 0; c < _columnCount; c++) {
					tris[trisInd] = Util.CellToIndex(r, c, xCount);
					tris[trisInd + 1] = Util.CellToIndex(r + 1, c, xCount);
					tris[trisInd + 2] = Util.CellToIndex(r + 1, c + 1, xCount);

					trisInd += 3;

					tris[trisInd] = Util.CellToIndex(r + 1, c + 1, xCount);
					tris[trisInd + 1] = Util.CellToIndex(r, c + 1, xCount);
					tris[trisInd + 2] = Util.CellToIndex(r, c, xCount);

					trisInd += 3;
				}
			}

			mMesh.Clear();

			mMesh.vertices = vtx;
			mMesh.uv = uvs;
			mMesh.triangles = tris;
			mMesh.colors32 = clrs;

			mMesh.RecalculateBounds();
		}
	}
}