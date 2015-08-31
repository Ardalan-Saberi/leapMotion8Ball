using UnityEngine;
using System.Collections;

public class MenuBehavior : MonoBehaviour {
	/*
	 * Public Configuration Options
	 */

	public MenuType _menu_type; /*
	 * _menu_type: This enumeration defines what kind of content your menu is using. 
 	 * > TEXT will create menus with text lables on each button. 
 	 * > ICON will allow you to specify Sprite icons that appear on each button. text provided will show up in a sub-label if available. 
 	 * > TEXTURE will allow you to specify a square Texture2D for each button that will be stretched across the button area.
 	 * > Each of the menu types can be seen in the example scene.
 	 */
	public MenuEventHandler _eventHandler; //The Game Object containing a MenuEventHandler script. This script will accept the button actions sent from menus on selection. The logic after this is up to you.
	public string[] _text; //An array of strings. The text label associated with each button option.
	public Sprite[] _icons_active; // Used in ICON mode only. An array of Sprites. The icon used when the menu option is  selected. If an inactive icon is not provided, a reduced alpha version of this icon will be displayed when the menu option is inactive. 
	public Sprite[] _icons_inactive; //Used in ICON mode only. An array of Sprites. The icon used when the menu option is inactive.
	public Texture2D[] _textures; //Used in Texture mode only. The Texture2D applied to the button.
	public ButtonAction[] _buttonActions; /*
	 * Array of ButtonAction’s. The length of this array determines how many slices the menu circle 
	 * is split into. If a slice is given an action, that slice is interactable and will have content. 
	 * If the slice is given NONE, it will be an inactive zone. This allows you to create menus that 
	 * only use certain wedges of the total circle.
	 */
	public float _angleOffset; //Often when a menu only uses a certain wedge of the total circle, you will want to orient the menu so the wedge is facing the proper direction. The angle offset rotates the menu’s orientation. Angles are 0-360 counter clockwise. 
	public float _radius; //The radius of the fully activated menu. The radius is the center of the strip.
	public float _thickness; //The thickness of the menu strip.
	public float _captureOffset; // By the menu wedge is captured when the user reaches the center of the wedge. The offset allows you to offset this control point from the radius. A negative number will move the capture point towards the menu’s center.
	public GameObject _button_prefab; //The prefab to use to create each menu wedge. It is not recommended that you modify this value. 
	public float _activation_radius; //The radius from the menu center (in pixels) where the menu will activate.
	public float _selection_radius; //The radius from the menu center where a menu button will be selected.
	public float _deactivate_z; //The world space Z of the Leap’s finger at which the menu will deactivate. 
	public float _deactivationSpeed; //The speed at which the menu will scale down on deactivation.
	public AnimationCurve _activationCurve; //The easing curve for the menu activation action.
	public float _activationTime; //The total time of the activation animation.
	public float _startHighlight; //The distance from the wedge center when the highlight color transition will begin.
	public float _fullHighlight; //The distance from the wedge center when the highlight color transition will complete.
	public Color _baseColor; //The unactivated color of the menu wedges. 
	public Color _selectedColor; //The color of a previously selected menu wedge.
	public Color _highlightColor; //The color of a highlighted menu wedge.
	public float _highlightPercentGrowth; //The scale of ICON and TEXT content when the wedge becomes highlighted.
	public float _scaleDownSpeed; //Similar to Deactivation Speed. The Scale down speed is the speed the other wedges scale down when another menu wedge is selected.
	public float _selectionDelayTime; 
	public float _selectionSnapDistance;
	public float _selectionSnapTime; //When a wedge is selected, it snaps back towards the menu center a small amount. This is how long that snap takes.
	public float _spriteScalingFactor; //When a wedge is selected, it snaps back towards the menu center a small amount. This is how far it snaps back.
	public float _selectionCooldown; //This is how long the menu will wait to select another wedge after a wedge has been selected. 

	private LeapManager _leapManager;
	private int _buttonCount;
	private GameObject[] _buttons;
	private MenuState _currentState;
	private int _currentSelection = -1;
	private Camera _mainCam;
	private Camera _uiCam;
	private int _closest = -1;
	private int _lastClosest = -1;
	private float _activationStartTime;
	private float _closestDistance = float.MaxValue;
	private float _scalingFactor = 1.0f;
	private float _selectionEndTime;
	private float _currentSelectionOffset;
	private bool _selectionMade = false;
	private TextMesh _subLabel;
	private bool _hasSubLabel;
	private float _selectionCooldownTime = 0;
	private Vector3 _baseLocation;

	public enum MenuType { ICON, TEXT, TEXTURE };
	public enum MenuState { INACTIVE, ACTIVATING, ACTIVE, SELECTION, DEACTIVATION, DISABLED };
	public enum ButtonAction { 
		NONE, 
		START, PLAYGROUND, EXIT, ABOUT};

	public MenuState currentState
	{
		get { return _currentState; }
		set { _currentState = value; }
	}

	public Vector3 baseLocation
	{
		get { return _baseLocation; }
		set { 
			_baseLocation = value; 
			gameObject.transform.parent.position = value;
		}
	}

	// Use this for initialization
	void Start () {
		//Get references to the main scene and UI cameras.
		_mainCam = (GameObject.Find("MainCam") as GameObject).GetComponent(typeof(Camera)) as Camera;
		_uiCam = (GameObject.Find("UI Cam") as GameObject).GetComponent(typeof(Camera)) as Camera;
		_baseLocation = gameObject.transform.parent.position;
		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
		_leapManager._mainCam = _mainCam;

		//Get a reference to the subLabel
		foreach(Transform child in gameObject.transform.parent)
		{
			if(child.name == "menuSub")
			{
				_subLabel = child.gameObject.GetComponent(typeof(TextMesh)) as TextMesh;
				_hasSubLabel = true;
			}
		}

		float segmentSweep; //how large is each button segment

		_buttonCount = _buttonActions.Length;

		_buttons = new GameObject[_buttonCount];
		segmentSweep = 360.0f / (float)_buttonCount;

		//Create the buttons, fill in their content, etc.
		for(int i=0; i<_buttonCount; i++)
		{
			_buttons[i] = Instantiate(_button_prefab, gameObject.transform.position, Quaternion.identity) as GameObject;
			_buttons[i].transform.parent = gameObject.transform;
			ArcMaker buttonScript = _buttons[i].GetComponent(typeof(ArcMaker)) as ArcMaker;
			buttonScript.CreateMesh(50, (i*segmentSweep)+_angleOffset, (i*segmentSweep)+segmentSweep+_angleOffset, _radius - (_thickness/2.0f), _radius + (_thickness/2.0f));

			//Setup the button content
			if(_buttonActions[i] != ButtonAction.NONE)
			{
				switch(_menu_type)
				{
				case MenuType.ICON:
					if(i < _icons_active.Length && _icons_active[i] != null)
					{
						if(i < _icons_inactive.Length && _icons_inactive[i] != null) { 	
							buttonScript.setContent(_icons_active[i], _icons_inactive[i], _spriteScalingFactor); 
						}
						else { 
							buttonScript.setContent(_icons_active[i], null, _spriteScalingFactor); 
						}
					}
					else
					{
						Debug.LogError("Active icon missing for: " + i);
					}
					break;
				case MenuType.TEXT:
					if(i < _text.Length && _text[i] != null)
					{
						buttonScript.setContent(_text[i]);
					}
					else
					{
						Debug.LogError("Text missing for: " + i);
					}
					break;
				case  MenuType.TEXTURE:
					if(i < _textures.Length && _textures[i] != null)
					{
						buttonScript.setContent(_textures[i]);
					}
					else
					{
						Debug.LogError("Texture missing for: " + i);
					}
					break;
				}
			}
		}

		gameObject.transform.localScale = new Vector3(0,0,1);
		_currentState = MenuState.INACTIVE;
	}

	// Update is called once per frame
	void Update () 
	{
		Vector2 leapScreen = new Vector2(_leapManager.pointerPositionScreen.x,
		                                 _leapManager.pointerPositionScreen.y);

		Vector2 parentScreen = new Vector2(_uiCam.WorldToScreenPoint(gameObject.transform.parent.position).x,
		                                 _uiCam.WorldToScreenPoint(gameObject.transform.parent.position).y);

		Vector2 menuScreen = new Vector2(_uiCam.WorldToScreenPoint(gameObject.transform.position).x,
		                                   _uiCam.WorldToScreenPoint(gameObject.transform.position).y);

		Vector2 parentToFinger = leapScreen - parentScreen;
		Vector2 toFrontFinger = leapScreen - menuScreen;

		//Menu Wide Updates per state
		switch(_currentState)
		{
		case MenuState.INACTIVE:

			if(parentToFinger.magnitude < _activation_radius && _leapManager.pointerPositionWorld.z > _deactivate_z) 
			{
				_activationStartTime = Time.time;
				_currentState = MenuState.ACTIVATING; 
			}

			if(_hasSubLabel && _currentSelection != -1 && _currentSelection < _text.Length && _text[_currentSelection] != null)
			{
				_subLabel.text = _text[_currentSelection];
			}
			break;
		case MenuState.ACTIVATING:
			_selectionMade = false;
			if(Time.time <= _activationStartTime + _activationTime)
			{
				float currentScale = _activationCurve.Evaluate((Time.time - _activationStartTime) / (_activationTime));
				gameObject.transform.localScale = new Vector3(currentScale,
				                                              currentScale,
				                                              1);
			}
			else
			{
				gameObject.transform.localScale = new Vector3(1,
				                                              1,
				                                              1);
				_currentState = MenuState.ACTIVE;
				return;
			}
			break;
		case MenuState.ACTIVE:
			if(_leapManager.pointerPositionWorld.z < _deactivate_z) {
				_selectionMade = false;
				_scalingFactor = 1.0f;
				_currentState = MenuState.DEACTIVATION; 
				return;
			}

			if(Time.time >= _selectionCooldownTime)
			{
				_closest = -1;
				_closestDistance = float.MaxValue;
			}
			break;
		case MenuState.SELECTION:

			_scalingFactor = Mathf.Clamp(_scalingFactor - (_scaleDownSpeed * Time.deltaTime), 0.0f, 1.0f);
			_currentSelectionOffset = Mathf.Clamp((float)(Time.time - _selectionEndTime + _selectionDelayTime) / (float)(_selectionSnapTime), 0.0f, 1.0f) * _selectionSnapDistance;
			if(Time.time >= _selectionEndTime)
			{
				_selectionMade = true;
				_currentState = MenuState.DEACTIVATION;
				return;
			}
			break;
		case  MenuState.DEACTIVATION:
			if(gameObject.transform.localScale.x > 0)
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x - (_deactivationSpeed * Time.deltaTime),
				                                              gameObject.transform.localScale.y - (_deactivationSpeed * Time.deltaTime),
				                                             1);
			}
			else
			{
				gameObject.transform.localScale = new Vector3(0,0,1);
				_currentState = MenuState.INACTIVE;
				return;
			}
			break;
		}

		//Per Button Updates per state
		for(int i=0; i<_buttonCount;i++)
		{
			ArcMaker current = _buttons[i].GetComponent(typeof(ArcMaker)) as ArcMaker;
			current._bottom = _radius - (_thickness / 2.0f);
			current._top = _radius + (_thickness / 2.0f);

			if(i != _closest) 
			{
				current.makeInactive();
				if(i == _currentSelection) { _buttons[i].renderer.material.color = Color.Lerp(_buttons[i].renderer.material.color, _selectedColor, 0.25f); }
				else { _buttons[i].renderer.material.color = Color.Lerp(_buttons[i].renderer.material.color, _baseColor, 0.25f); }
			}

			if(i == _currentSelection) { current.makeActive(); }

			switch(_currentState)
			{
			case MenuState.INACTIVE:
				_buttons[i].SetActive(false);
				break;
			case MenuState.ACTIVATING:
				_buttons[i].SetActive(true);
				current._contentScaleFactor = 1.0f;
				break;
			case MenuState.ACTIVE:
				Vector2 buttonCenter = _uiCam.WorldToScreenPoint(_buttons[i].renderer.bounds.center);
				Vector2 toButton = (Vector2)leapScreen - (Vector2)buttonCenter;

				if(Time.time >= _selectionCooldownTime && toButton.magnitude < _closestDistance)
				{
					_closestDistance = toButton.magnitude;
					_closest = i;
				}

				current._contentScaleFactor = 1.0f;
				break;
			case MenuState.SELECTION:
				if(i != _closest)
				{
					current._bottom *= _scalingFactor;
					current._top *= _scalingFactor;
					current._contentScaleFactor = _scalingFactor;
				}
				else
				{
					current._bottom = _selection_radius + _currentSelectionOffset - (_thickness / 2.0f);
					current._top = _selection_radius + _currentSelectionOffset + (_thickness / 2.0f);
				}
				break;
			case MenuState.DEACTIVATION:
				if(i != _closest || !_selectionMade)
				{
					current._bottom *= _scalingFactor;
					current._top *= _scalingFactor;
				}
				else if(_selectionMade)
				{
					current._bottom = _selection_radius - (_thickness / 2.0f);
					current._top = _selection_radius + (_thickness / 2.0f);
				}
				break;
			}
		}

		//Behavior for selected item
		if(_currentState == MenuState.ACTIVE)
		{
			if(_closest != _lastClosest)
			{
				_lastClosest = _closest;
				_selectionCooldownTime = Time.time + _selectionCooldown;
			}

			//do things with the closest menu
			if(_closest != -1)
			{
				ArcMaker selected = _buttons[_closest].GetComponent(typeof(ArcMaker)) as ArcMaker;
				
				float pixelDistance = (menuScreen - leapScreen).magnitude;
				
				//convert world distance from pixels to world units.
				float worldDistance = pixelDistance * ((_uiCam.orthographicSize*2.0f) / (float)_uiCam.pixelHeight);

				if(_buttonActions[_closest] == ButtonAction.NONE)
				{
					if(worldDistance > _radius + (_thickness/2.0f)) 
					{
						_selectionMade = false;
						_scalingFactor = 1.0f;
						_currentState = MenuState.DEACTIVATION; 
						return; 
					}
				}
				else
				{
					selected.makeActive();

					if(_hasSubLabel && _closest != -1 && _closest < _text.Length && _text[_closest] != null)
					{
						_subLabel.text = _text[_closest];
					}

					//pull out wedge                                           
					if(worldDistance - _captureOffset > _radius)
					{
						selected._bottom = worldDistance - _captureOffset - (_thickness / 2.0f);
						selected._top = worldDistance - _captureOffset + (_thickness / 2.0f);

						if(worldDistance - _captureOffset > _selection_radius)
						{
							_selectionEndTime = Time.time + _selectionDelayTime;
							_currentSelection = _closest;
							_scalingFactor = 1.0f;

							if(_eventHandler != null && _closest < _buttonActions.Length)
							{
								_eventHandler.recieveMenuEvent(_buttonActions[_closest]);
							}

							_currentState = MenuState.SELECTION;
						}
					}

					float highlightPercent = Mathf.Clamp((worldDistance - _fullHighlight) / (_startHighlight - _fullHighlight), 0.0f, 1.0f);
					if(_closest == _currentSelection) { _buttons[_closest].renderer.material.color = Color.Lerp(_selectedColor, _highlightColor, highlightPercent); }
					else { _buttons[_closest].renderer.material.color = Color.Lerp(_baseColor, _highlightColor, highlightPercent); }
					selected._contentScaleFactor = 1.0f + (highlightPercent * _highlightPercentGrowth);
				}
			}
		}
	}

	public void setOpacity(float opacity)
	{
		opacity = Mathf.Clamp(opacity, 0.0f, 1.0f);

		Color current = gameObject.transform.parent.renderer.material.color;
		gameObject.transform.parent.renderer.material.color = new Color(current.r, current.g, current.b, opacity);

		if(_hasSubLabel)
		{
			current = _subLabel.renderer.material.color;
			_subLabel.renderer.material.color = new Color(current.r, current.g, current.b, opacity);
		}
	}
}
