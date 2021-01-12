using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research;
using LSL;

// Notes: 
// Tobii calibration will be done in the EyeTracker Manager? 
// Active display coordinate system: origin (0,0) is Top-Left, (1,1) is Bottom-Right
// Unity viewport coordinates: origin is Bottom-Left, (1,1) is Top-Right

public class TobiiController : MonoBehaviour
{
    public enum TrackedEye
    {
        Left, 
        Right
    }

    public enum SamplingRates
    {
        _60 = 60,
        _120 = 120,
        _250 = 250
    };

    public SamplingRates SR = SamplingRates._60;
    public TrackedEye TrackEye = TrackedEye.Left;

    // to keep the latest recorded sample in pixels
    // ProcessGaze scripts work with pixels
    private Vector2 _eyePix = new Vector2();
    private Vector2 _eyeADCS = new Vector2();

    // We need the monkeylogic controller here to forward gaze data as soon as we have it
    // for MonkeyLogic to record it. 
    public MonkeyLogicController MLController;
    protected int mlOutletID = -1;

    // Start is called before the first frame update
    protected IEyeTracker tobiiTracker;
    protected GazeOutputFrequencyCollection allRates;

    // Calibration script
    private EyeCalibration eyecal;

    // Gaze 
    private GazeProcessing gazeProcess;
    private GazeView gazeView;

    void Start()
    {
        eyecal = gameObject.AddComponent<EyeCalibration>();
        gazeProcess = gameObject.AddComponent<GazeProcessing>();
        gazeView = gameObject.AddComponent<GazeView>();

        // Add Listeners
        EventsController.OnEyeCalibrationUpdate += gazeProcess.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate += eyecal.UpdateCalibration;

        if (FindTracker())
            ConfigureOulet();
    }

    private void OnDisable()
    {
        tobiiTracker.GazeDataReceived -= EyeTracker_GazeDataReceived;

        EventsController.OnEyeCalibrationUpdate -= gazeProcess.UpdateCalibration;
        EventsController.OnEyeCalibrationUpdate -= eyecal.UpdateCalibration;
    }

    private bool FindTracker()
    {
        // TODO: find by address? 
        EyeTrackerCollection tmpTrackers = EyeTrackingOperations.FindAllEyeTrackers();
        foreach (IEyeTracker item in tmpTrackers)
        {
            if (item.DeviceName == "Tobii Pro Fusion")
            {
                tobiiTracker = item;
                if (tobiiTracker.GetAllGazeOutputFrequencies().Contains((float)SR))
                {
                    tobiiTracker.SetGazeOutputFrequency((float)SR);
                }
                else
                {
                    tobiiTracker.SetGazeOutputFrequency(60.0f);
                    SR = SamplingRates._60;
                }
                tobiiTracker.GazeDataReceived += EyeTracker_GazeDataReceived;
                return true; 
            }
        }
        return false;
    }

    private void ConfigureOulet()
    {
        // Data sent to stream: 
        // Left eye: 
        //    X in Active Display Coordinate System (normalized 0-1)
        //    Y in Active Display Coordinate System (normalized 0-1)
        //    Pupil Size
        //    Validity
        // Right eye: 
        //    X in Active Display Coordinate System (normalized 0-1)
        //    Y in Active Display Coordinate System (normalized 0-1)
        //    Pupil Size
        //    Validity
        // System Time: Computer clock time in useconds
        // LSL time: computer clock time in seconds
        mlOutletID = MLController.AddExternalOutlet("TobiiGazeData", "TobiiGazeData", 10, liblsl.IRREGULAR_RATE, "tobii1214");
    }

    private void EyeTracker_GazeDataReceived(object sender, GazeDataEventArgs e)
    {
        if (TrackEye == TrackedEye.Left)
        {
            _eyeADCS = new Vector2
            {
                x = e.LeftEye.GazePoint.PositionOnDisplayArea.X,
                y = e.LeftEye.GazePoint.PositionOnDisplayArea.Y
            };
        }
        else
        {
            _eyeADCS = new Vector2
            {
                x = e.RightEye.GazePoint.PositionOnDisplayArea.X,
                y = e.RightEye.GazePoint.PositionOnDisplayArea.Y
            };
        }
        _eyePix = eyecal.T_ADCSToPix(_eyeADCS); 

        if (mlOutletID != -1 && MLController)
        {
            double[] to_publish = new double[10];
            to_publish[0] = e.LeftEye.GazePoint.PositionOnDisplayArea.X;
            to_publish[1] = e.LeftEye.GazePoint.PositionOnDisplayArea.Y;
            to_publish[2] = e.LeftEye.Pupil.PupilDiameter;
            to_publish[3] = (double)e.LeftEye.GazePoint.Validity;
            to_publish[4] = e.RightEye.GazePoint.PositionOnDisplayArea.X;
            to_publish[5] = e.RightEye.GazePoint.PositionOnDisplayArea.Y;
            to_publish[6] = e.RightEye.Pupil.PupilDiameter;
            to_publish[7] = (double)e.RightEye.GazePoint.Validity;
            //to_publish[8] = e.DeviceTimeStamp;
            // SystemTimeStamp and local_clock use the same clock but at different units
            // Tobii system time is in microseconds
            // lsl system time is in seconds
            // there seem to be significant jitter in the lsl timestamps compared to the tobii
            // might be due to unity? 
            to_publish[8] = e.SystemTimeStamp;
            to_publish[9] = liblsl.local_clock();

            MLController.PublishExternal(mlOutletID, to_publish);
        }
    }

    private void Update()
    {
        if (tobiiTracker != null)
        {
            if (mlOutletID != -1)
            {
                gazeProcess.ProcessGaze(_eyePix, out float[] gazeTargets, out float[] gazeCounts, out Vector3[] hitPoints);
                gazeView.ShowGaze(hitPoints);
                EventsController.instance.SendEyeLateUpdateEvent(_eyeADCS, gazeTargets, gazeCounts);
            }
            else
            {
                ConfigureOulet();
            }
        }
        else
        {
            FindTracker();
        }
    }
}
