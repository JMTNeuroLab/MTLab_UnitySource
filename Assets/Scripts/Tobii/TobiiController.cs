using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Research;
using Tobii.Research.Unity;
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

    // We need the monkeylogic controller here to forward gaze data as soon as we have it
    // for MonkeyLogic to record it. 
    public MonkeyLogicController MLController;
    protected int mlOutletID = -1;

    // Calibration script
    void Start()
    {
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

    // Unity fixed update is by default @ 20 ms or 50 Hz. Since the tracker operates at either 60, 120 or 250 Hz,
    // we would have at most 5 samples to process, which should not affect frame rates. Timing is somewhat reliable.
    private void FixedUpdate()
    {
        if (EyeTracker.Instance.Connected && mlOutletID != -1)
        {
            int n_samples = EyeTracker.Instance.GazeDataCount;
            if (n_samples > 0)
            {
                // 
                double[,] to_publish = new double[n_samples, 10];
                for (int i = 0; i < n_samples; i++)
                {
                    IGazeData tmp = EyeTracker.Instance.NextData;

                    to_publish[i, 0] = tmp.Left.GazePointOnDisplayArea.x;
                    to_publish[i, 1] = tmp.Left.GazePointOnDisplayArea.x;
                    to_publish[i, 2] = tmp.Left.PupilDiameter;
                    to_publish[i, 3] = tmp.Left.GazePointValid ? 1.0 : 0.0;
                    to_publish[i, 4] = tmp.Right.GazePointOnDisplayArea.x;
                    to_publish[i, 5] = tmp.Right.GazePointOnDisplayArea.x;
                    to_publish[i, 6] = tmp.Right.PupilDiameter;
                    to_publish[i, 7] = tmp.Left.GazePointValid ? 1.0 : 0.0;
                    // SystemTimeStamp and local_clock use the same clock but at different units
                    // Tobii system time is in microseconds
                    // lsl system time is in seconds
                    // there seem to be significant jitter in the lsl timestamps compared to the tobii
                    // might be due to unity? 
                    to_publish[i, 8] = tmp.TimeStamp;
                    to_publish[i, 9] = liblsl.local_clock();
                }
                MLController.PublishExternal(mlOutletID, to_publish);
            }
        }
    }

    private void Update()
    {
        if (EyeTracker.Instance.Connected)
        {
            if (mlOutletID == -1)
                ConfigureOulet();

            Ray gazeRay = EyeTracker.Instance.LatestGazeData.CombinedGazeRayScreen;

        }
        
    }
}
