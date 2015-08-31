using UnityEngine;
using System.Collections;
using Leap;

public class ScaleManager : MonoBehaviour {
	public static ScaleState _scaleState;
	public static float _scaleRate;
	
	public float _scaleMax;
	public AnimationCurve _scaleFilter;
	public int _scaleWindow;
	public float _scaleBound;

	public enum ScaleState { IN, OUT, NONE };

	private LeapManager _leapManager;
	
	// Use this for initialization
	void Start () {
		_leapManager = (GameObject.Find("LeapManager") as GameObject).GetComponent(typeof(LeapManager)) as LeapManager;
	}
	
	// Update is called once per frame
	void Update () {
		checkScaling(_leapManager.currentFrame, _leapManager.leapController.Frame(_scaleWindow));
	}
	
	void checkScaling(Frame frame, Frame startFrame)
	{
		float logScale = Mathf.Log(frame.ScaleFactor(startFrame)) * 100;
		
		if(frame.Hands.Count == 2)
		{
			int sign = 1;
			
			if(logScale < 0) sign = -1;
			
			float norm = Mathf.Clamp((Mathf.Abs(logScale) - _scaleBound) / (_scaleMax - _scaleBound), 0.0f, 1.0f);
			float postFilter = _scaleFilter.Evaluate(norm) * sign;
			
			_scaleRate = postFilter;
		}
		else
		{
			_scaleRate = 0;
		}
	}
}
