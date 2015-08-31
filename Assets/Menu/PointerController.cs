using UnityEngine;
using System.Collections;
using Leap;

public class PointerController : MonoBehaviour {
	public GameObject[] _menus;
	
	private LeapManager _leapManager;
	
	// Use this for initialization
	void Start () {
		_menus = GameObject.FindGameObjectsWithTag("Menu");
		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}
	
	// Update is called once per frame
	void Update () {
		if(_leapManager.pointerAvailible/* && !any_menu_active*/)
		{
			gameObject.renderer.enabled = true;
			gameObject.transform.position = _leapManager.pointerPositionWorld;
		}
		else
		{
			gameObject.renderer.enabled = false;
		}
		
	}
}
