using UnityEngine;
using System.Collections;
using Leap;
using MyBilliardGame;

public class CueStickController : MonoBehaviour
{
		Vector3 cueStickTipPosition = Vector3.zero;
		Vector3 cueStickVelocity = Vector3.zero;
		Vector3 cueStickPosition = Vector3.zero;
		Quaternion cueStickRotation = Quaternion.identity;
		GameObject camera, cueStick, cueBall;
		// Use this for initialization
		void Start ()
		{
				camera = GameObject.FindGameObjectWithTag ("PlayerCamera");
				cueStick = GameObject.FindGameObjectWithTag ("CueStick");
				cueBall = GameObject.FindGameObjectWithTag ("cueBall");
		}

		void FixedUpdate ()
		{
				if (Game.currentState.Equals (Game.GameState.Aiming)) {
						gameObject.rigidbody.velocity = cueStickVelocity;
				} else {
						gameObject.rigidbody.velocity = Vector3.zero;
				}
		}
		// Update is called once per frame
		void Update ()
		{
				if (Game.currentState.Equals (Game.GameState.Aiming)) {
						gameObject.renderer.enabled = true;
						cueStick.renderer.enabled = true;
						Hand hand = Game.leapManager.frontmostHand ();
						Finger pointingFinger = LeapManager.pointingFigner (hand);
						Vector3 tip = pointingFinger.TipPosition.ToUnityTranslated ();			
						tip = new Vector3 (tip.z, tip.y, -1 * tip.x);
						Vector3 mid = hand.PalmPosition.ToUnityTranslated ();	
						mid = new Vector3 (mid.z, mid.y, -1 * mid.x);

						float globalResizeCoef = (2 * Vector3.Distance (camera.transform.position, cueBall.transform.position)) / (LeapManager.HandMaxZ - LeapManager.HandMinZ);
						Vector3 tmp = camera.transform.position + (globalResizeCoef * new Vector3 (tip.x, tip.y, tip.z));
						
						

						if (hand.IsValid && pointingFinger.IsValid) {
								cueStickTipPosition = new Vector3 (tmp.x, 0.25f, tmp.z);		
								cueStickVelocity = pointingFinger.TipVelocity.ToUnityScaled ();
						} else {
								cueStickVelocity = Vector3.zero;
						}
						
						
						//Debug.Log ("tip:" + mid.x + 
						//     ", " + mid.y + ", " + mid.z);
						//Vector3 vca = camera.transform.rotation.eulerAngles;
						//Vector3 dir = tip - mid;
						//float dax = Vector3.Angle (dir, Vector3.right);
						//float daz = Vector3.Angle (dir, Vector3.up);
						//float day = Vector3.Angle (dir, Vector3.forward);
						//cueStickRotation = Quaternion.Euler (new Vector3 (dax, day + vca.y, daz));
						//Vector3 cueStickPosition = cueStickTipPosition;	
						//Vector3.MoveTowards (cueStickPosition, dir.normalized, 2.0f);
						//cueStickRotation = Quaternion.Euler (new Vector3 (90.0f, (day - 90) + vca.y, 0.0f));
						//Debug.Log ("vca:" + vca.x + ", " + vca.y + ", " + vca.z + " dax:" + dax + ", " + day + ", " + daz);
						
				} else {						
						gameObject.renderer.enabled = true;
						cueStick.renderer.enabled = true;
				}
		}
	
		IEnumerator moveCueStick ()
		{	
				while (true) {
						gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, cueStickTipPosition, Time.deltaTime);
						//cueStick.transform.position = Vector3.Lerp (cueStick.transform.position, cueStickPosition, Time.deltaTime);
						//cueStick.transform.rotation = Quaternion.Slerp (cueStick.transform.rotation, cueStickRotation, Time.deltaTime);
						//gameObject.transform.LookAt (pvaaLookAt);
						yield return null;
				}
		
		}

		public void onAimingStart ()
		{
				StartCoroutine ("moveCueStick");
		}

		public void onAimingStop ()
		{
				StartCoroutine ("moveCueStick");
		}

		/*void OnCollisionEnter (Collision other)
		{
				if (other.transform.tag.ToLower ().IndexOf ("ball") >= 0) {
						if (Game.currentState.Equals (Game.GameState.Aiming)) {
								Game.doAfterShot ();
						}
				}
		}*/
		void OnCollisionExit (Collision other)
		{
				if (other.transform.tag.ToLower ().IndexOf ("ball") >= 0) {
						if (Game.currentState.Equals (Game.GameState.Aiming)) {
								Game.doAfterShot ();
						}
				}
		}
	
}
