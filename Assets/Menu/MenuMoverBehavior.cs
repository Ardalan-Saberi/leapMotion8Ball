using UnityEngine;
using System.Collections;

public class MenuMoverBehavior : MonoBehaviour {
	public bool _handSweepEnabled;
	public float _fullThrowDistance;
	public AnimationCurve _throwFilter;
	public float _throwSpeed; //speed to move from 0 to 1 on the filter.
	public float _fadeInTime;
	public float _fadeOutTime;
	public AnimationCurve _fadeCurve;
	public float _fadeThrowDistance;
	public Vector2 _layoutOriginalAspectRatio;

	private GameObject[] _menuRoots;
	private GameObject[] _menus;
	private float _throwLocation;
	private float _fadePushPercent;
	private float _fadeStartPercent;
	private GameObject _activeMenu;
	private MoverState _currentState;
	private float _fadeStartTime;
	private Matrix4x4 _guiMatrix;

	private enum MoverState { FADING_IN, FADING_OUT, PASSIVE };
	
	void Start () {
		_menuRoots = GameObject.FindGameObjectsWithTag("MenuRoot");
		_menus = GameObject.FindGameObjectsWithTag("Menu");
		_currentState = MoverState.PASSIVE;

		//Account for differing aspect ratios
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		Vector2 screenDiff = _layoutOriginalAspectRatio -  screenSize;

		if(Mathf.Abs(screenDiff.x) < Mathf.Abs(screenDiff.y)) {
			float amt = _layoutOriginalAspectRatio.x / screenSize.x;
			screenSize *= amt;
		}
		else {
			float amt = _layoutOriginalAspectRatio.y / screenSize.y;
			screenSize *= amt;
		}

		float horizRatio = screenSize.x / (float)_layoutOriginalAspectRatio.x;
		float vertRatio = screenSize.y / (float)_layoutOriginalAspectRatio.y;

		_guiMatrix = Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (horizRatio, vertRatio, 1));

		foreach(GameObject menu in _menus)
		{
			MenuBehavior menuScript = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
			menuScript.baseLocation = _guiMatrix.MultiplyPoint(menuScript.baseLocation);
		}
	}

	void Update () {
		bool menuIsActive = false;
		bool menuIsClosing = false;

		foreach(GameObject menu in _menus)
		{
			MenuBehavior menuBehavior = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
			if(menuBehavior.currentState == MenuBehavior.MenuState.ACTIVE || menuBehavior.currentState == MenuBehavior.MenuState.ACTIVATING)
			{
				menuIsActive = true;
				_activeMenu = menu;
			}
			else if(menuBehavior.currentState == MenuBehavior.MenuState.DEACTIVATION || menuBehavior.currentState == MenuBehavior.MenuState.SELECTION)
			{
				menuIsClosing = true;
			}
		}

		switch(_currentState)
		{
		case MoverState.FADING_IN:
			_fadePushPercent = Mathf.Clamp((Time.time - _fadeStartTime) / (_fadeInTime * _fadeStartPercent), 0.0f, 1.0f);

			foreach(GameObject menuRoot in _menuRoots)
			{
				GameObject menu = null;
				MenuBehavior menuRootBehavior = null;
				foreach(Transform child in menuRoot.transform)
				{
					if(child.name == "Menu")
					{
						menu = child.gameObject;
						menuRootBehavior = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
					}
				}
				
				if(menu != _activeMenu)
				{
					Vector3 awayVector = new Vector3(menuRoot.transform.position.x - _activeMenu.transform.position.x, 
					                                 menuRoot.transform.position.y - _activeMenu.transform.position.y,
					                                 0).normalized * _fadeCurve.Evaluate(1-_fadePushPercent) * _fadeThrowDistance;
					
					if(menuRootBehavior != null)
					{
						menuRoot.transform.position = menuRootBehavior.baseLocation + awayVector;
						menuRootBehavior.setOpacity(_fadePushPercent);
					}
				}
			}

			if(Time.time > _fadeStartTime + (_fadeInTime * _fadeStartPercent)) 
			{ 
				_currentState = MoverState.PASSIVE; 
				
				foreach(GameObject menu in _menus)
				{
					if(menu != _activeMenu)
					{
						MenuBehavior menuBehavior = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
						menuBehavior.currentState = MenuBehavior.MenuState.INACTIVE;
					}
				}
				return;
			}

			break;
		case MoverState.FADING_OUT:
			_fadePushPercent = Mathf.Clamp((Time.time - _fadeStartTime) / _fadeOutTime, 0.0f, 1.0f);

			if(!menuIsActive)
			{
				_fadeStartTime = Time.time;
				_fadeStartPercent = _fadePushPercent;
				_currentState = MoverState.FADING_IN;
				return;
			}

			foreach(GameObject menuRoot in _menuRoots)
			{
				GameObject menu = null;
				MenuBehavior menuRootBehavior = null;
				foreach(Transform child in menuRoot.transform)
				{
					if(child.name == "Menu")
					{
						menu = child.gameObject;
						menuRootBehavior = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
					}
				}

				if(menu != _activeMenu)
				{
					Vector3 awayVector = new Vector3(menuRoot.transform.position.x - _activeMenu.transform.position.x, 
					                                 menuRoot.transform.position.y - _activeMenu.transform.position.y,
					                                 0).normalized * _fadeCurve.Evaluate(_fadePushPercent) * _fadeThrowDistance;

					if(menuRootBehavior != null)
					{
						menuRoot.transform.position = menuRootBehavior.baseLocation + awayVector;
						menuRootBehavior.setOpacity(1.0f-_fadePushPercent);
					}
				}
			}
			break;
		case MoverState.PASSIVE:
			if(menuIsActive && !menuIsClosing)
			{
				_fadeStartTime = Time.time;
				_currentState = MoverState.FADING_OUT;

				foreach(GameObject menu in _menus)
				{
					if(menu != _activeMenu)
					{
						MenuBehavior menuBehavior = menu.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
						menuBehavior.currentState = MenuBehavior.MenuState.DISABLED;
					}
				}

				return;
			}

			if(_handSweepEnabled)
			{
				_throwLocation = Mathf.Clamp(_throwLocation + (ScaleManager._scaleRate * _throwSpeed * Time.deltaTime),0,1.0f);
				
				foreach(GameObject menuRoot in _menuRoots)
				{
					MenuBehavior menuRootBehavior = null;
					foreach(Transform child in menuRoot.transform)
					{
						if(child.name == "Menu")
						{
							menuRootBehavior = child.gameObject.GetComponent(typeof(MenuBehavior)) as MenuBehavior;
						}
					}
					
					Vector3 awayVector = new Vector3(menuRoot.transform.position.x - gameObject.transform.position.x, 
					                                 menuRoot.transform.position.y - gameObject.transform.position.y,
					                                 0).normalized * _throwFilter.Evaluate(_throwLocation) * _fullThrowDistance;

					
					if(menuRootBehavior != null)
					{
						menuRoot.transform.position = menuRootBehavior.baseLocation + awayVector;
					}
				}
			}
			break;
		}
	}
}
