using UnityEngine;
using Tobii.Research.Unity;
using LSL;

// Notes: 
// Tobii calibration will be done in the EyeTracker Manager? 
// Active display coordinate system: origin (0,0) is Top-Left, (1,1) is Bottom-Right
// Unity viewport coordinates: origin is Bottom-Left, (1,1) is Top-Right

public class TobiiController : EyeControllerBase
{

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    // Unity fixed update is by default @ 20 ms or 50 Hz. Since the tracker operates at either 60, 120 or 250 Hz,
    // we would have at most 5 samples to process, which should not affect frame rates. Timing is somewhat reliable.
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
    private void FixedUpdate()
    {
        if (TobiiEyeTracker.Instance.Connected)
        {
            int n_samples = TobiiEyeTracker.Instance.GazeDataCount;
            if (n_samples > 0)
            {
                // 
                double[,] to_publish = new double[n_samples, 10];
                for (int i = 0; i < n_samples; i++)
                {
                    IGazeData tmp = TobiiEyeTracker.Instance.NextData;

                    to_publish[i, 0] = tmp.Left.GazePointOnDisplayArea.x;
                    to_publish[i, 1] = tmp.Left.GazePointOnDisplayArea.y;
                    to_publish[i, 2] = tmp.Left.PupilDiameter;
                    to_publish[i, 3] = tmp.Left.GazePointValid ? 1.0 : 0.0;
                    to_publish[i, 4] = tmp.Right.GazePointOnDisplayArea.x;
                    to_publish[i, 5] = tmp.Right.GazePointOnDisplayArea.y;
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
                EventsController.instance.SendPublishTobii(to_publish);
            }
        }
    }

    private void Update()
    {
        if (TobiiEyeTracker.Instance.Connected)
        {
            IGazeData gd = TobiiEyeTracker.Instance.LatestGazeData;

            // Based on GazeData.cs lines 17-30
            if (gd.Left.GazePointValid && gd.Right.GazePointValid)
            {
                Vector2 combinedPoint = (gd.Left.GazePointOnDisplayArea + gd.Right.GazePointOnDisplayArea) / 2f;
                // Based on screen values received from monkey logic. Creates a ~2 DVA radius foveation circle based on
                // the screen size and approximate distance entered in MonkeyLogic. 
                _eyePix = eyecal.T_ADCSToPix(combinedPoint);

                gazeProcess.ProcessGaze(_eyePix, out _gazeTargets, out _gazeCounts, out _gazeHits);
                gazeView.ShowGaze(_gazeHits);

                EventsController.instance.SendEyeLateUpdateEvent(combinedPoint, _gazeTargets, _gazeCounts);
            }

        }

    }
}
