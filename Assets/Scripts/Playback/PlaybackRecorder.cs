using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine.InputSystem;
using UnityEditor;

public class PlaybackRecorder : MonoBehaviour
{
    private RecorderControllerSettings recctrl_sett;
    private RecorderController rec_ctrl;
    private RecorderSettings rec_sett;
    private Recorder rec;
    private MovieRecorderSettings mov_sett;
    private int trial_number = 0;

    private void OnEnable()
    {
        recctrl_sett = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        rec_ctrl = new RecorderController(recctrl_sett);

        mov_sett = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        mov_sett.name = "My Video Recorder";
        mov_sett.Enabled = true;
        mov_sett.VideoBitRateMode = VideoBitrateMode.High;
        mov_sett.FrameRatePlayback = FrameRatePlayback.Variable;
        mov_sett.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = FullScreenView.ResolutionX,
            OutputHeight = FullScreenView.ResolutionY
        };
        mov_sett.OutputFormat = 0;
        mov_sett.AudioInputSettings.PreserveAudio = true;
        mov_sett.OutputFile = "RecordedTrial_<Trial>";
        mov_sett.FileNameGenerator.AddWildcard("<Trial>", IncrementTrial);
        recctrl_sett.AddRecorderSettings(mov_sett);
    }

    string IncrementTrial(RecordingSession sess)
    {
        return trial_number.ToString();
    }

    public void SetTrialNumber(int nbr)
    {
        trial_number = nbr;
    }

    public void SetFrameInterval(int frame)
    {
        recctrl_sett.SetRecordModeToFrameInterval(0, frame);
    }

    public void SetOutputFile(string name)
    {
        mov_sett.OutputFile = name + "_<Trial>";
    }

    public void StartRecording()
    {
        Debug.Log("Starting recording");
        rec_ctrl.PrepareRecording();
        rec_ctrl.StartRecording();
    }

    public void StopRecording()
    {
        rec_ctrl.StopRecording();
        Debug.Log("Stopping recording");
    }

    // Update is called once per frame
    void Update()
    {
        // Manual On Black
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            StartRecording();
        }
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            StopRecording();
        }
    }
}
