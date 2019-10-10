﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class PlaybackExpController : ExperimentController
{
    public PlaybackController pbCtrl;
    public Text txt_Targets;

    private bool playTrialData = false;
    private List<double[]> frames = new List<double[]>();
    private GazeProcessing gp;
    private GazeView gv;

    private float pix_per_deg;
    private int XRes, YRes;
    private int lastState = 13; // States.null;
    private int N_Frames = 0; 

    public override void PrepareAllTrials() {  }
    
    private void OnEnable()
    {
        EventsController.OnPlaybackParamUpdate += UpdateTrialParameters;
        EventsController.OnPlaybackDataUpdate += UpdataTrialData;
        EventsController.OnPlaybackStart += StartPlayback;
        EventsController.OnEyeCalibrationUpdate += UpdateEyeCalibration; 
    }

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

        _currentTrial.Cue_Objects = FindInTaskInfo(taskInfo.CueObjects, parameters.cue_Objects);
        _currentTrial.Target_Objects = FindInTaskInfo(taskInfo.TargetObjects, parameters.target_Objects);
        _currentTrial.Distractor_Objects = FindInTaskInfo(taskInfo.DistractorObjects, parameters.distractor_Objects);

        _currentTrial.Target_Positions = parameters.Target_Positions;
        _currentTrial.Distractor_Positions = parameters.Distractor_Positions;

        Condition temp_cnd = FindCurrentCondition(parameters.cue_Material, parameters.target_Materials, parameters.distractor_Materials);
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
            Camera.main.transform.position = new Vector3 { x = (float)tmp[0], y = (float)(tmp[1] + 0.8), z = (float)tmp[2] };
            Camera.main.transform.rotation = Quaternion.Euler(0f, (float)tmp[3], 0f);
            // TODO Trialstate and gaze
            float x = (float)tmp[4];
            float y = (float)tmp[5];
            x = (pix_per_deg * x) + (0.5f * XRes);
            y = (pix_per_deg * y) + (0.5f * YRes);

            Vector2 _eyePix = new Vector2 { x = x, y = y };
            
            // manually convert to pixels
            //_eyePix = Mouse.current.position.ReadValue();   
            gp.ProcessGaze(_eyePix, out string[] gazeTargets, out float[] gazeCounts, out Vector3[] hitPoints);
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
                    case 7: //Target
                        ShowTargets();
                        ShowDistractors();
                        break;
                    case 8: // Response
                        break;
                    case 9: //EndOfTrial
                        HideCues();
                        HideTargets();
                        HideDistractors();
                        break;
                    case 10: // Feedback
                        break;
                    case 11: // Pause
                        break;
                    case 12: // Resume
                        break;
                    case 13: // Null
                        break;
                }

                lastState = state;
            }

        }
        else if (playTrialData && frames.Count < 1)
        {
            playTrialData = false;
            pbCtrl.PublishTrial("Done");
        }
        else
        {
            // Display loaded frames
            int i = frames.Count;
            
            txt_Targets.text = i.ToString() + " frames out of " + N_Frames.ToString() + " loaded.";
        }
    }

    private void DisplayTargets(string[] names, float[] counts)
    {
        string txt = "";
        for (int i = 0; i < names.Length; i++)
        {
            txt += names[i] + " [" + counts[i].ToString() + "] \n";
        }
        txt_Targets.text = txt;
    }
}
