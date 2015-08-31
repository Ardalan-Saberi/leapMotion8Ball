using UnityEngine;
using System.Collections;

public class ArcMaker : MonoBehaviour {

	public float _startAngle;
	public float _endAngle;
	public float _bottom;
	public float _top;
	public int _quadCount;
	public bool _drawMesh = false;
	public float _contentScaleFactor = 1.0f;

	private GameObject _activeSprite;
	private GameObject _inactiveSprite;
	private GameObject _textLabel;
	private Shader _trasparent = Shader.Find("Transparent/Diffuse");
	private MenuBehavior.MenuType _arcType;
	private bool _isActive = false;
	private bool _inactiveSpriteAvailible = false;
	private float _spriteScalingFactor = 1.0f;


	MeshFilter mf;
	Mesh mesh;
	Vector3[] verticies;
	int[] tris;
	Vector3[] normals;
	Vector2[] uvs;

	public bool isActive { 
		get { return _isActive; }
	}

	// Use this for initialization
	void Start() 
	{
		if(_textLabel == null || _activeSprite == null || _inactiveSprite == null) { findChildren(); }

		gameObject.layer = 8;
		gameObject.renderer.material.shader = _trasparent;

		if(_quadCount > 0)
		{
			mf = GetComponent(typeof(MeshFilter)) as MeshFilter;
			mesh = new Mesh();
			verticies = new Vector3[(_quadCount * 2)+2];
			tris = new int[_quadCount*6];
			normals = new Vector3[(_quadCount * 2)+2];
			uvs = new Vector2[(_quadCount * 2)+2];
			mf.mesh = mesh;
		}
		else
		{
			Debug.LogError("No Quads Created, quad count is <= 0");
		}
	}

	private void findChildren()
	{
		foreach(Transform child in gameObject.transform)
		{
			switch( child.name )
			{
			case "activeSprite":
				_activeSprite = child.gameObject;
				break;
			case "inactiveSprite":
				_inactiveSprite = child.gameObject;
				break;
			case "textLabel":
				_textLabel = child.gameObject;
				break;
			default:
				Debug.LogError("Unknown child name: " + child.name);
				break;
			}
		}
		
		_activeSprite.renderer.enabled = false;
		_inactiveSprite.renderer.enabled = false;
		_textLabel.renderer.enabled = false;
	}

	public void CreateMesh (int quadCount = 50, float startAngle = 0, float endAngle = 360, float bottom = 1, float top = 5) {

		if(startAngle != -1) { _startAngle = startAngle; }
		if(endAngle != -1) { _endAngle = endAngle; }
		if(bottom != -1) { _bottom = bottom; }
		if(top != -1) { _top = top; }
		_quadCount = quadCount;

		_drawMesh = true;
	}

	public void setContent(string text)
	{
		if(_textLabel == null) { findChildren(); }
		_arcType = MenuBehavior.MenuType.TEXT;
		(_textLabel.GetComponent(typeof(TextMesh)) as TextMesh).text = text;
		_textLabel.renderer.enabled = true;
	}

	public void setContent(Sprite activeSprite, Sprite inactiveSprite = null, float scalingFactor = 1.0f)
	{
		_spriteScalingFactor = scalingFactor;
		if(_activeSprite == null || _inactiveSprite == null) { findChildren(); }
		_arcType = MenuBehavior.MenuType.ICON;
		(_activeSprite.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = activeSprite;
		if(inactiveSprite != null) 
		{
			_inactiveSpriteAvailible = true;
			(_inactiveSprite.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer).sprite = inactiveSprite; 
			_inactiveSprite.renderer.enabled = true;
		}
		else
		{
			_activeSprite.renderer.enabled = true;
		}
	}

	public void setContent(Texture2D tex)
	{
		gameObject.renderer.material.mainTexture = tex;
	}

	public void makeActive()
	{
		switch(_arcType)
		{
		case MenuBehavior.MenuType.ICON:
			if(_inactiveSpriteAvailible)
			{
				_activeSprite.renderer.enabled = true;
				_inactiveSprite.renderer.enabled = false;
			}
			else
			{
				_activeSprite.renderer.material.color = new Color(1.0f,1.0f,1.0f, 1.0f);
			}
			break;
		case MenuBehavior.MenuType.TEXT:
			break;
		case MenuBehavior.MenuType.TEXTURE:
			break;
		}
	}

	public void makeInactive()
	{
		switch(_arcType)
		{
		case MenuBehavior.MenuType.ICON:
			if(_inactiveSpriteAvailible)
			{
				_activeSprite.renderer.enabled = false;
				_inactiveSprite.renderer.enabled = true;
			}
			else
			{
				_activeSprite.renderer.material.color = new Color(1.0f,1.0f,1.0f, 0.6f);
			}
			break;
		case MenuBehavior.MenuType.TEXT:
			break;
		case MenuBehavior.MenuType.TEXTURE:
			break;
		}
	}

	// Currently redraws the mesh each frame. Better would be to create the verts once and deform after that.
	void Update () {
		if(_drawMesh && _quadCount > 0)
		{
			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(0, 0);



			for(int i=0; i<verticies.Length; i++)
			{
				float angle = _endAngle + ((_startAngle - _endAngle) * (float)((Mathf.Floor(i/2.0f) / (float)Mathf.Floor((verticies.Length-1)/2.0f))));
				angle = angle * Mathf.Deg2Rad;
				Vector2 point = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				point = point.normalized * (i%2==0 ? _bottom : _top);
				verticies[i] = new Vector3(point.x, point.y, 0);

				if(point.x < min.x) { min.x = point.x; }
				if(point.x > max.x) { max.x = point.x; }
				if(point.y < min.y) { min.y = point.y; }
				if(point.y > max.y) { max.y = point.y; }
			}

			if(max.x - min.x > max.y - min.y) { 
				max.y = min.y + (max.x - min.x);

			}
			else { 
				max.x = min.x + (max.y - min.y); 
			}

			for(int i=0; i<_quadCount; i++)
			{
				tris[(i*6)+0] = (i*2)+0;
				tris[(i*6)+1] = (i*2)+1;
				tris[(i*6)+2] = (i*2)+2;
				
				tris[(i*6)+3] = (i*2)+1;
				tris[(i*6)+4] = (i*2)+3;
				tris[(i*6)+5] = (i*2)+2;
			}
			
			for(int i=0; i < verticies.Length; i++) { normals[i] = -Vector3.forward; }
			for(int i=0; i < verticies.Length; i++) 
			{ 
				Vector2 normLocation = new Vector2(Mathf.Clamp((verticies[i].x - min.x) / (max.x - min.x), 0.0f, 1.0f), Mathf.Clamp((verticies[i].y - min.y) / (max.y - min.y), 0.0f, 1.0f));
				uvs[i] = normLocation;
			}
			
			mesh.vertices = verticies;
			mesh.triangles = tris;
			mesh.normals = normals;
			mesh.uv = uvs;

			float contentAngle = _startAngle + ((_endAngle-_startAngle)/2.0f);
			contentAngle = contentAngle * Mathf.Deg2Rad;
			float contentDist = _bottom + ((_top-_bottom) * 0.5f);
			Vector2 contentPoint = new Vector2(Mathf.Cos (contentAngle), Mathf.Sin (contentAngle));
			contentPoint = contentPoint.normalized * contentDist;

			switch(_arcType)
			{
			case MenuBehavior.MenuType.ICON:
				_activeSprite.transform.localPosition = new Vector3(contentPoint.x, contentPoint.y, gameObject.transform.position.z - 1);
				_inactiveSprite.transform.localPosition = new Vector3(contentPoint.x, contentPoint.y, gameObject.transform.position.z - 1);
				_activeSprite.transform.localScale = new Vector3(1.0f * _contentScaleFactor * _spriteScalingFactor, 
				                                              1.0f * _contentScaleFactor * _spriteScalingFactor, 
				                                              1.0f);
				_inactiveSprite.transform.localScale = new Vector3(1.0f * _contentScaleFactor * _spriteScalingFactor, 
				                                              1.0f * _contentScaleFactor * _spriteScalingFactor, 
				                                              1.0f);
				break;
			case MenuBehavior.MenuType.TEXT:
				_textLabel.transform.localPosition = new Vector3(contentPoint.x, contentPoint.y, gameObject.transform.position.z - 1);
				_textLabel.transform.localScale = new Vector3(1.0f * _contentScaleFactor, 
				                                              1.0f * _contentScaleFactor, 
				                                              1.0f);
				break;
			}
		}
	}
}
