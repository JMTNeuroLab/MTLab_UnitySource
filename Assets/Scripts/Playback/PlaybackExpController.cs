using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

// Default playback behavior.
// Edit to inherit from your custom experiment controller. 
public class PlaybackExpController : ExperimentController
{
    // custom task info: replace class with custom class. 
    public PlaybackTaskInfo customTaskInfo;

    // DO NOT EDIT. =========================================================
    private new void OnEnable()
    {
        taskInfo = customTaskInfo;
        GenerateIDMap();
        EventsController.OnPlaybackParamUpdate += UpdateTrialParameters;
        EventsController.OnPlaybackDataUpdate += UpdataTrialData;
        EventsController.OnPlaybackStart += StartPlayback;
        EventsController.OnEyeCalibrationUpdate += UpdateEyeCalibration; 
    }

    public Text txt_Targets;

    private bool playTrialData = false;
    private List<double[]> frames = new List<double[]>();
    private GazeProcessing gp;
    private GazeView gv;

    private float pix_per_deg;
    private int XRes, YRes;
    private int lastState = 13; // States.null;
    private int N_Frames = 0;

    public override void PrepareAllTrials() { }

    // Start is called before the first frame update
    void Start()
    {
        // Override base clas start to avoir generating trials and starting the publish 
        // coroutine
        gp = gameObject.AddComponent<GazeProcessing>();
        gv = gameObject.AddComponent<GazeView>();
    }

    private void StartPlayback()
    {
        playTrialData = true;
        EventsController.instance.SendManagePlaybackRecording(true);
        Debug.Log("Starting playback");
    }

    private void UpdataTrialData(PlaybackTrialData data)
    {
        frames.Add(data.data);
    }

    private void UpdateEyeCalibration(EyeCalibrationParameters parameters)
    {
        gp.UpdateCalibration(parameters);
        pix_per_deg = parameters.pix_per_deg;
        XRes = parameters.ml_x_res;
        YRes = parameters.ml_y_res;
    }

    private void UpdateTrialParameters(PlaybackTrialParameters parameters)
    {
        _currentTrial.Trial_Number = parameters.Trial_Number;
        _currentTrial.Start_Position = parameters.Start_Position;
        _currentTrial.Start_Rotation = parameters.Start_Rotation;

        _currentTrial.Cue_Objects = FindInTaskInfo(taskInfo.CueObjects, parameters.Cue_Objects);
        _currentTrial.Target_Objects = FindInTaskInfo(taskInfo.TargetObjects, parameters.Target_Objects);
        _currentTrial.Distractor_Objects = FindInTaskInfo(taskInfo.DistractorObjects, parameters.Distractor_Objects);

        _currentTrial.Target_Positions = parameters.Target_Positions;
        _currentTrial.Distractor_Positions = parameters.Distractor_Positions;

        Condition temp_cnd = FindCurrentCondition(parameters.Cue_Material, parameters.Target_Materials, parameters.Distractor_Materials);
        _currentTrial.Cue_Material = temp_cnd.CueMaterial;
        _currentTrial.Target_Materials = temp_cnd.TargetMaterials;
        _currentTrial.Distractor_Materials = temp_cnd.DistractorMaterials;
        N_Frames = parameters.n_Frames;

        PrepareTrial();
    }

    private Condition FindCurrentCondition(string cue_mat, string[] targ_mat, string[] dist_mat)
    {
        foreach (Condition cnd in taskInfo.Conditions)
        {
            bool cue_match = false;
            bool targ_match = false;
            bool dist_match = false;

            if (cnd.CueMaterial.name == cue_mat)
                cue_match = true;

            int cnt = 0;
            foreach (Material mat in cnd.TargetMaterials)
            {
                if (Array.IndexOf(targ_mat, mat.name) != -1)
                    cnt += 1;
            }
            if (cnt == cnd.TargetMaterials.Length)
                targ_match = true;

            cnt = 0;
            foreach (Material mat in cnd.DistractorMaterials)
            {
                if (Array.IndexOf(dist_mat, mat.name) != -1)
                    cnt += 1;
            }
            if (cnt == cnd.DistractorMaterials.Length)
                dist_match = true;

            if (cue_match && targ_match && dist_match)
                return cnd;
        }
        return new Condition();
    }

    private GameObject[] FindInTaskInfo(GameObject[] arr_go, string[] names)
    {
        List<GameObject> temp_go = new List<GameObject>();
        foreach (GameObject go in arr_go)
        {
            if (Array.IndexOf(names, go.name) != -1)
            {
                temp_go.Add(go);
            }
        }
        return temp_go.ToArray();
    }

    public override void PrepareTrial()
    {
        // Prepare cues and targets. 
        HideCues();
        PrepareCues(); // Empty for this example, cue objects are visible. 
        HideTargets();
        PrepareTargets();
        HideDistractors();
        PrepareDistractors();

        // Sanity checks
        TrialEnded = false;
        Outcome = "aborted";
    }

    private void Update()
    {
        
        if (playTrialData && frames.Count >= 1)
        {
            double[] tmp = frames[0];
            frames.RemoveAt(0);
            
            // Data is: pos , pos y, pos z, rot, gaze X, gaze Y, trial state
            // IMPORTANT there is a 0.8 unit offset between the player controller and the camera, manually added here. 
            Camera.main.transform.position = new Vector3 { x = (float)tmp[0], y = (float)(tmp[1] + 0.8), z = (float)tmp[2] };
            Camera.main.transform.rotation = Quaternion.Euler(0f, (float)tmp[3], 0f);
            // TODO Trialstate and gaze
            Vector2 _eyePix;
            switch (ExperimentConfiguration.Eye_Tracker)
            {
                case ExperimentConfiguration.EyeTrackers.EyeLink:
                    // gaze data is in degrees
                    _eyePix = new Vector2
                    {
                        x = (pix_per_deg * (float)tmp[4]) + (0.5f * XRes),
                        y = (pix_per_deg * (float)tmp[5]) + (0.5f * YRes)
                    };
                    break;

                case ExperimentConfiguration.EyeTrackers.TobiiProFusion:
                    // gaze data is in relative screen position
                    _eyePix = new Vector2
                    {
                        x = (float)tmp[4] * XRes,
                        y = (1f - (float)tmp[5]) * YRes
                    };
                    break;

                default:
                    _eyePix = Vector2.negativeInfinity;
                    break;

            }
            
            // manually convert to pixels
            gp.ProcessGaze(_eyePix, out float[] gazeTargets, out float[] gazeCounts, out Vector3[] hitPoints);
            DisplayTargets(gazeTargets, gazeCounts);
            gv.ShowGaze(hitPoints);

            // Experiment epoch
            int state = (int)tmp[6];
            if (state != lastState)
            {
                switch (state)
                {
                    case 0: // ITI
                        HideCues();
                        PrepareCues(); // Empty for this example, cue objects are visible. 
                        HideTargets();
                        PrepareTargets();
                        HideDistractors();
                        PrepareDistractors();
                        break;
                    case 1: // StartOfTrial

                        break;
                    case 2: // Delay_1

                        break;
                    case 3: // Cue
                        ShowCues();
                        break;
                    case 4: // Delay_2
                        HideCues();
                        break;
                    case 5: // Distractor
                        break;
                    case 6: // Delay_3
                        break;
                    case 7: // force FOV
                        break;
                    case 8: //Target
                        ShowTargets();
                        ShowDistractors();
                        break;
                    case 9: // Response
                        break;
                    case 10: //EndOfTrial
                        HideCues();
                        HideTargets();
                        HideDistractors();
                        break;
                    case 11: // Feedback
                        break;
                    case 12: // Pause
                        break;
                    case 13: // Resume
                        break;
                    case 14: // Null
                        break;
                }

                lastState = state;
            }

        }
        else if (playTrialData && frames.Count < 1)
        {
            playTrialData = false;
            EventsController.instance.SendManagePlaybackRecording(false);
            EventsController.instance.SendPlaybackPublishTrial("Done");
        }
        else
        {
            // Display loaded frames
            int i = frames.Count;
            
            txt_Targets.text = i.ToString() + " frames out of " + N_Frames.ToString() + " loaded.";
        }
    }

    private void DisplayTargets(float[] names, float[] counts)
    {
        string txt = "";
        
        for (int i = 0; i < names.Length; i++)
        {
            txt += IDToName((int)names[i]) + " [" + counts[i].ToString() + "] \n";
        }
        txt_Targets.text = txt;
    }
}
