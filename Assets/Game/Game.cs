namespace MyBilliardGame
{
		using UnityEngine;
		using System.Collections;
		using System.Collections.Generic;
		using Leap;
		using UnityEngine.UI;

		public class Game : MonoBehaviour
		{
				public static LeapManager leapManager;
				public CueStickController cueStickController;
				public enum GameState
				{
						CameraAutoAdjust,
						CameraManualAdjust,
						Aiming,
						AfterShot,
						TurnEnd}
				;

				public enum HandState
				{
						NoAction,
						Fisting,
						Pointing}
				;
				private List<GameState> gameStates = new List<GameState> ();
				private List<GameObject> balls = new List<GameObject> ();
				private List<Vector3> ballsLastPosition = new List<Vector3> ();
				private List<Vector3> ballsFirstPosition = new List<Vector3> ();

				public static GameState currentState{ get; set; }

				public static HandState handState{ get; set; }
		
				public enum Player
				{
						One = 1,
						Two =2}
				;
		
				public static Player turn;
				private float noActionDuration = 0.0f, pointingDuration = 0.0f, fistingDuration = 0.0f;
				private float noActionDetectionTime = 0.45f, pointingDetectionTime = 0.15f, fistingDetectionTime = 0.15f; //, detectionRunningTime = 0.0f;
				private float actionDetectionWindow = 1.5f, actionDetectionAccuracy = 0.40f;
				private RawImage viewAdjustmentStateIcon, aimingStateIcon, swipeActionIcon;
				private Text playerTurn;
				private GameObject ballsParent;
				private GameObject cueBall, camera;
				private int pitocksCount = 0;

				void Awake ()
				{	
						handState = HandState.NoAction;
						
						leapManager = (GameObject.Find ("LeapManager") as GameObject).GetComponent (typeof(LeapManager)) as LeapManager;
						cueStickController = (GameObject.Find ("CueStickTip") as GameObject).GetComponent (typeof(CueStickController)) as CueStickController;

						leapManager.leapController.EnableGesture (Gesture.GestureType.TYPE_SWIPE);
						leapManager.leapController.Config.SetFloat ("Gesture.Swipe.MinLength", 200.0f);
						leapManager.leapController.Config.SetFloat ("Gesture.Swipe.MinVelocity", 750f);
						leapManager.leapController.Config.Save ();

						gameStates.Add (GameState.CameraAutoAdjust);
						gameStates.Add (GameState.CameraManualAdjust);
						gameStates.Add (GameState.Aiming);
						gameStates.Add (GameState.AfterShot);
						gameStates.Add (GameState.TurnEnd);

						

						viewAdjustmentStateIcon = (GameObject.Find ("ViewAdjustmentStateIcon") as GameObject).GetComponent (typeof(RawImage)) as RawImage; 
						aimingStateIcon = (GameObject.Find ("AimingStateIcon") as GameObject).GetComponent (typeof(RawImage)) as RawImage; 
						swipeActionIcon = (GameObject.Find ("SwipeActionIcon") as GameObject).GetComponent (typeof(RawImage)) as RawImage;

						playerTurn = (GameObject.Find ("PlayerTurn") as GameObject).GetComponent (typeof(Text)) as Text; 
						cueBall = GameObject.FindGameObjectWithTag ("cueBall");

						cueBall = GameObject.FindGameObjectWithTag ("cueBall");
						camera = GameObject.FindGameObjectWithTag ("PlayerCamera");
						ballsParent = GameObject.FindGameObjectWithTag ("balls");
						foreach (Transform ball in ballsParent.transform) {
								balls.Add (ball.gameObject);				
						}
				}

				void saveBallsPostion (List<Vector3> list)
				{
						for (int i=0; i<balls.Count; i++) {
								list.Add (balls [i].transform.position);
						}
				}

				void loadBallsPostion (List<Vector3> list)
				{
						for (int i=0; i<balls.Count; i++) {
								balls [i].transform.position = list [i];
						}
				}
		
				void Start ()
				{
						cueStickController.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
						camera.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
						camera.transform.rotation = Quaternion.Euler (new Vector3 (90.0f, 0.0f, 0.0f));
						saveBallsPostion (ballsFirstPosition);
						ballsLastPosition = ballsFirstPosition;
						pitocksCount = 0;
						currentState = GameState.CameraAutoAdjust;
						turn = Player.One;
				}

				void Update ()
				{		
						bool actionOccured = false;
						Gesture swipeGesture = null;			
						
						if (Input.GetKeyDown (KeyCode.Space)) {
								loadBallsPostion (ballsFirstPosition);
								Start ();
						} else if (Input.GetKeyDown (KeyCode.Escape)) {	
								Application.Quit ();
						} else {
								if (currentState == GameState.CameraManualAdjust || currentState == GameState.Aiming) {
										Hand hand = leapManager.frontmostHand ();	
						
										if (currentState == GameState.CameraManualAdjust && handState.Equals (HandState.NoAction)) {
												Frame frame = leapManager.currentFrame;	
								
												GestureList gestures = frame.Gestures ();
												foreach (var gesture in gestures) {
														if (gesture.Type == Gesture.GestureType.TYPE_SWIPE && gesture.IsValid) {
																actionOccured = true;
																swipeGesture = gesture;
												
														}
												}
										} 
										if (!actionOccured && hand.IsValid) {
							
								
												HandState mHandState = momentHandState ();
												switch (mHandState) {
												case HandState.Fisting:
														if (currentState == GameState.CameraManualAdjust && handState.Equals (HandState.NoAction)) {
																fistingDuration += Time.deltaTime;
																//StartCoroutine (lerpAlpha (viewAdjustmentStateIcon, fistingDuration / fistingDetectionTime));		
														}
														break;
												case HandState.Pointing:
														if (handState.Equals (HandState.NoAction)) {
																pointingDuration += Time.deltaTime;	
																//StartCoroutine (lerpAlpha (aimingStateIcon, pointingDuration / pointingDetectionTime));
														}
														break;
												case HandState.NoAction:
														if (!handState.Equals (HandState.NoAction)) {
																noActionDuration += Time.deltaTime;
														}
														break;	
												}
										}
						
										if (actionOccured) {			
												//if swiped call swipe delegates and reset
												noActionDuration = 0.0f;
												pointingDuration = 0.0f;
												fistingDuration = 0.0f;
												onSwipeAction (swipeGesture);
												Debug.Log ("Swiped");
										} else { 
												float accumTime = noActionDuration + pointingDuration + fistingDuration;
												if (pointingDuration / accumTime > actionDetectionAccuracy && pointingDuration > pointingDetectionTime) {
														//	if poiting for the most of passed time call noaction delegates and reset
														handState = HandState.Pointing;
														actionOccured = true;					
														onPointAction ();					
										
												} else if (fistingDuration / accumTime > actionDetectionAccuracy && fistingDuration > fistingDetectionTime) {
														//	if fisting for the most of passed time call noaction delegates and reset
														handState = HandState.Fisting;					
														actionOccured = true;					
														onFistAction ();
						
												} else if (noActionDuration / accumTime > actionDetectionAccuracy && noActionDuration > noActionDetectionTime) {
														//	if noaction for the most of passed time call noaction delegates and reset
														handState = HandState.NoAction;					
														actionOccured = true;					
														onNoAction ();					
								
												}
								
												if (accumTime > actionDetectionWindow) {
														float minTime = Mathf.Min (Mathf.Min (noActionDuration, fistingDuration), pointingDuration);
														noActionDuration -= minTime;
														fistingDuration -= minTime;
														pointingDuration -= minTime;
												}
												//Debug.Log ("na: " + noActionDuration + " pt: " + pointingDuration + " ft: " + fistingDuration + " dtr: " + accumTime + " hs: " + handState + " pnt? " + leapManager.pointerAvailible);
								
										}
						
								} else if (currentState == GameState.AfterShot) {
										bool shotEnded = true;
										if (cueBall.rigidbody.velocity.magnitude == 0) {
												for (int i=0; i< balls.Count; i++) {
														if (balls [i].rigidbody.velocity.magnitude > 0) {
																shotEnded = false;		
																break;
														}	
												}
										} else {
												shotEnded = false;	
										}
										if (shotEnded) {
												doTurnEnd ();
										}
				    
								}
						}
				}
		
				public void onPointAction ()
				{	
						setHandStateIcong ();	
						if (currentState.Equals (GameState.CameraManualAdjust)) {
								doAiming ();
						} else if (currentState.Equals (GameState.Aiming)) {
								cueStickController.onAimingStart ();
						}
				
				}

				public void onFistAction ()
				{	
						setHandStateIcong ();
						//		setAlpha (viewAdjustmentStateIcon, 1.0f);
				}

				public void onSwipeAction (Gesture swipeGesture)
				{	
						setAlpha (swipeActionIcon, 1.0f);		
						StartCoroutine (lerpAlpha (swipeActionIcon, 0.0f));
				}

				public void onNoAction ()
				{		
						setHandStateIcong ();
						
						if (currentState.Equals (GameState.Aiming)) {
								cueStickController.onAimingStop ();				
								doCameraManualAdjust ();
						}
						;
						//StartCoroutine (lerpAlpha (swipeActionIcon, 0.0f));
						//StartCoroutine (lerpAlpha (viewAdjustmentStateIcon, 0.0f));
						//StartCoroutine (lerpAlpha (aimingStateIcon, 0.0f));
				}

				public static void doCameraAutoAdjust ()
				{
						Debug.Log ("in doCameraAutoAdjust");
						currentState = GameState.CameraAutoAdjust;
				}

				public static void doCameraManualAdjust ()
				{
						Debug.Log ("in doCameraManualAdjust");
						currentState = GameState.CameraManualAdjust;
				}

				public static void doAiming ()
				{
						Debug.Log ("in doAiming");
						currentState = GameState.Aiming;
				}

				public static void doAfterShot ()
				{
						Debug.Log ("in doAfterShot");
						currentState = GameState.AfterShot;
				}

				public void doTurnEnd ()
				{		
						Debug.Log ("in doTurnEnd");
						currentState = GameState.TurnEnd;

						int turnPitocks = 0;
						
						for (int i=0; i<balls.Count; i++) {
								if (balls [i].transform.position.y < 0) {
										turnPitocks ++;
								}
						}
						
						if (cueBall.transform.position.y <= 0) {
								cueBall.transform.position = new Vector3 (-2.97f, -0.64f, 0.1f);
								loadBallsPostion (ballsLastPosition);
								if (turn.Equals (Player.One)) {
										turn = Player.Two;
								} else {
										turn = Player.One;
								}
						} else if (turnPitocks > pitocksCount) {
								pitocksCount = turnPitocks;
						} else {
								if (turn.Equals (Player.One)) {
										turn = Player.Two;
								} else {
										turn = Player.One;
								}
						}
				
		
						if (turn.Equals (Player.One)) {
								playerTurn.text = "Player One";
						} else {
								playerTurn.text = "Player Two";			
						}

						saveBallsPostion (ballsLastPosition);

						doCameraAutoAdjust ();
				}
		
				public static void onAutoAdjustFinishedAction ()
				{
						Debug.Log ("in onAutoAdjustFinishedAction");
						doCameraManualAdjust ();
				}
		
				IEnumerator lerpAlpha (RawImage image, float a)
				{
						Color first = image.color;
						Color last = new Color (first.r, first.g, first.b, a);
						while (!Mathf.Approximately(image.color.a, last.a)) {
								image.color = Color.Lerp (first, last, Time.deltaTime);
								yield return null;
						}
				}

				void setAlpha (RawImage image, float a)
				{	
				
						Color color = image.color;
						color.a = a;
						image.color = color; 
				}
		
				void setHandStateIcong ()
				{	
						switch (handState) {
						case HandState.Fisting:		
								setAlpha (viewAdjustmentStateIcon, 1.0f);
								setAlpha (aimingStateIcon, 0.0f);


								break;
						case HandState.Pointing:
								setAlpha (viewAdjustmentStateIcon, 0.0f);
								setAlpha (aimingStateIcon, 1.0f);

								break;
						case HandState.NoAction:
								setAlpha (viewAdjustmentStateIcon, 0.0f);
								setAlpha (aimingStateIcon, 0.0f);

								break;
						}
	
				}

				HandState momentHandState ()
				{	
						Hand hand = leapManager.frontmostHand ();	
						ArrayList forwardFingers = LeapManager.forwardFacingFingers (hand);
						if (forwardFingers.Count == 0) {
								return HandState.Fisting;
						} else if (forwardFingers.Count < 3) {
								float minZ = float.MaxValue;
								Finger forwardFinger = Finger.Invalid;

								foreach (Finger finger in forwardFingers) {
										if (finger.TipPosition.z < minZ) {
												minZ = finger.TipPosition.z;
												forwardFinger = finger;
										}
								}
								if (forwardFinger.IsValid && (forwardFinger.Type () == Finger.FingerType.TYPE_INDEX || forwardFinger.Type () == Finger.FingerType.TYPE_MIDDLE)) {
										return HandState.Pointing;
								}
						}
						return HandState.NoAction;
				}
		
		}
}