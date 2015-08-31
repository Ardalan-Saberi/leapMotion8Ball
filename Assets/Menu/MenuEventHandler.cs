using UnityEngine;
using System.Collections;

public class MenuEventHandler : MonoBehaviour
{	
	public Menu _menu;

		void Start ()
		{ 
				
		}

		public void recieveMenuEvent (MenuBehavior.ButtonAction action)
		{
				switch (action) {
				case MenuBehavior.ButtonAction.ABOUT:
						break;
				case MenuBehavior.ButtonAction.EXIT:
					_menu.Hint ("Good Bye!");
						//Application.Quit ();
						break;
				case MenuBehavior.ButtonAction.START:
						break;
				case MenuBehavior.ButtonAction.PLAYGROUND:
						break;
				default:
						break;
				}
		}
		public class ExampleClass : MonoBehaviour
		{
				IEnumerator Start ()
				{
						AsyncOperation async = Application.LoadLevelAdditiveAsync ("MyAddLevel");
						yield return async;

				}
		}
}
