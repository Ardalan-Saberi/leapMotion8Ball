using UnityEngine;
using System.Collections;
using Leap;

public class Menu : MonoBehaviour
{
		public MenuBehavior _menuBehaviour;
		private static bool _gameStarted;
		private GameObject _menu;
		private GameObject _hintBox;
		
		// Use this for initialization
		void Start ()
		{
				_gameStarted = true;
				//_menu = GameObject.FindGameObjectWithTag ("Menu");
				_hintBox = GameObject.FindGameObjectWithTag ("HintBox");
				//_menuScript = GameObject.GetComponent (typeof(MenuBehavior)) as MenuBehavior;
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (_menuBehaviour.currentState == MenuBehavior.MenuState.INACTIVE) {
						Hint ("Hove over \"Start\" to see the menu!");

				} else {
						Hint ("");
				}

				/*if (_gameStarted) {
						_menuScript._text [0] = "New Game";
				} else {
						_menuScript._text [0] = "Return To Game";
				}*/
	
		}

		public void Hint (string message)
		{
				TextMesh hint = _hintBox.GetComponent (typeof(TextMesh)) as TextMesh;
				hint.text = message;
		}
}
