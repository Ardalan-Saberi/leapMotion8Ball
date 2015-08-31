using MyBilliardGame;
using UnityEngine;
using System.Collections;
using Leap;

public class PlayerCameraController : MonoBehaviour
{
		public GameObject cueBall;
		private float highX = 0.35f;
		private float LowX = -0.35f;
		private float highY = -0.2f;
		private float LowY = -0.95f;
		private float highZ = 0.3f;
		private float LowZ = -0.6f;
		private float rotStep = 36.0f;
		private float zoomStep = 0.2f;
		private float trStep = 0.05f;
		private float trMax = 10.0f;
		private float trMin = 1.0f;
		private float minDistance = 1.0f;
		private float maxDistance = 10.0f;
		private Vector3 pvaaDest;
		private Vector3 pvaaLookAt;
		private float pvaaTime = 0;
		private bool inLerp = false;

		// Use this for initialization
		void Start ()
		{
	
		}
	
		// Update is called once per frame
		void Update ()
		{			
				Hand hand = Game.leapManager.frontmostHand ();

				if (Game.currentState.Equals (Game.GameState.CameraManualAdjust) && Game.handState.Equals (Game.HandState.Fisting)) {
						
						Vector3 handLocation = hand.PalmPosition.ToUnityTranslated ();
						handLocation.x = Mathf.Clamp (handLocation.x, LeapManager.HandMinX, LeapManager.HandMaxX);
						handLocation.y = Mathf.Clamp (handLocation.y, LeapManager.HandMinY, LeapManager.HandMaxY);
						handLocation.z = Mathf.Clamp (handLocation.z, LeapManager.HandMinZ, LeapManager.HandMaxZ);
						float coefY = 2 * (((handLocation.x - LeapManager.HandMinX) / (LeapManager.HandMaxX - LeapManager.HandMinX)) - 0.5f);
						float coefX = 2 * (((handLocation.y - LeapManager.HandMinY) / (LeapManager.HandMaxY - LeapManager.HandMinY)) - 0.5f);
						float coefZ = 2 * (((handLocation.z - LeapManager.HandMinZ) / (LeapManager.HandMaxZ - LeapManager.HandMinZ)) - 0.5f);
						bool rotY = coefY < LowX || coefY > highX;
						bool rotX = coefX < LowY || coefX > highY;
						bool zoom = coefZ < LowZ || coefZ > highZ;
						if (rotY) {
								gameObject.transform.RotateAround (cueBall.transform.position, new Vector3 (0.0f, (rotY ? Mathf.Sign (coefY) * 1.0f : 0.0f), 0.0f), rotStep * Time.deltaTime);
						}
						if (rotX) {
				gameObject.transform.RotateAround (cueBall.transform.position, new Vector3 (0.0f, 0.0f, (rotX ? Mathf.Sign (coefY) * 1.0f : 0.0f)), rotStep * Time.deltaTime);
						}
			
						if (zoom) {
								gameObject.transform.Translate ((cueBall.transform.position - gameObject.transform.position) * Mathf.Sign (coefZ) * zoomStep * Time.deltaTime, Space.World);
						}
						
				} else if (Game.currentState.Equals (Game.GameState.CameraAutoAdjust)) {
						GameObject ballsParent;
						
						GameObject cueBall;
						Vector3 center = Vector3.zero;

						ballsParent = GameObject.FindGameObjectWithTag ("balls");
						cueBall = GameObject.FindGameObjectWithTag ("cueBall");
						
						foreach (Transform ball in ballsParent.transform) {
								center += ball.transform.position;
							
						}

						center = center / 15;
						Debug.DrawRay (gameObject.transform.position, (center - gameObject.transform.position).normalized);
						Vector3 dir = cueBall.transform.position - center;
						Vector3 point = cueBall.transform.position + dir * 0.5f;
						point.y = 3.5f;
						pvaaDest = point;
						pvaaLookAt = (center + cueBall.transform.position) / 2;
						
						gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, pvaaDest, Time.deltaTime);
						gameObject.transform.LookAt (pvaaLookAt);
						
						if (Vector3.Distance (gameObject.transform.position, pvaaDest) <= 0.1f) {
								Game.onAutoAdjustFinishedAction ();
						}
				}
	
		}

}
