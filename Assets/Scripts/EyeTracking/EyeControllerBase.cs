using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeControllerBase : MonoBehaviour
{
    // Calibration script
    protected EyeCalibration eyecal;

    // Gaze 
    protected GazeProcessing gazeProcess;
    protected GazeView gazeView;

    protected Vector2 _eyeRaw = new Vector2();
    protected Vector2 _eyeDeg = new Vector2();
    protected Vector2 _eyePix = new Vector2();
    protected float[] _gazeTargets;
    protected float[] _gazeCounts;
    protected Vector3[] _gazeHits;


    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    protected void Initialize()
    {
        eyecal = gameObject.AddComponent<EyeCalibration>();
        gazeProcess = gameObject.AddComponent<GazeProcessing>();
        gazeView = gameObject.AddComponent<GazeView>();

        EventsController.OnEyeCalibrationUpdate += gazeProcess.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate += eyecal.UpdateCalibration;
    }

    protected void Disable()
    {
        EventsController.OnEyeCalibrationUpdate -= gazeProcess.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate -= eyecal.UpdateCalibration;
    }

    void OnDisable()
    {
        Disable();
    }
    
    public void SetGazeWindow(float windowSize)
    {
        gazeProcess.SetGazeWindow(windowSize);
    }

}
