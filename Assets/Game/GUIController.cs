using UnityEngine;
using System.Collections;
using MyBilliardGame;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
		private Text hintText;
		// Use this for initialization
		void Start ()
		{
				
				hintText = (GameObject.Find ("HintText") as GameObject).GetComponent (typeof(Text)) as Text; 
		}
	
		// Update is called once per frame
		void Update ()
		{
				switch (Game.currentState) {
				case Game.GameState.CameraManualAdjust:
			hintText.text = "By fisting your hand and moving it arounf you can adjust the view to achieve the best shot!\nOpen hand to release the camera adjustment.\nTo start aiming point towards the screen, the cue stick will show up then.";
						break;
				case Game.GameState.Aiming:
			hintText.text = "Keep pointing until you find your suitable angle then hit the ball as fast as you can.\nBy Opening your hand and the fisting it you can adjust the camera view again!";
						break;
				default:
			hintText.text = "";
						break;

				}
				
	
		}

}
