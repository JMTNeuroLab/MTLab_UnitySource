using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class ExperimentConfiguration : MonoBehaviour
{
    public enum UserInputDevice
    {
        Null,
        Joystick,
        Keyboard,
        Mouse, // here mouse is for navigation (e.g. trackball); TODO: mouse for gaze?
        GamePad,
        // NIJoystick,
        // TODO: Touchpad
    }
 
    public enum EyeTrackers
    {
        EyeLink,
        TobiiProFusion,
        None
    }
 
    #region Public Variables
    [Header("User Input")]
    public UserInputDevice SetInputDevice = UserInputDevice.Keyboard;
    public float SetMoveSensitivity = 2.0f;
    public float SetTurnSensitivity = 2.0f;
    // public string NIJoystickX = "Dev1/ai1";
    // public string NIJoystickY = "Dev1/ai0";
 
    [Header("Eye Tracking")]
    // This is necessary because we can't configure the Tobii tracker from MonkeyLogic, only the EyeLink.
    // Keep in mind that the EyeLink system still needs to be calibrated in MonkeyLogic and that the Tobii
    // system needs calibration from it's proprietary software.
    public EyeTrackers SetEyeTracker = EyeTrackers.None;
    // The DVA calculations are based on the screen size and distance values entered in MonkeyLogic
    // For headfree subjects this is only an approximation as the head can move. 
    [Tooltip("Radius size in DVA of gaze window.")] public float SetGazeRadius = 2.0f;

    [Header("Playback Mode")]
    public bool SetPlaybackMode = false;
    public string SetTaskName = "XMaze";
    public string SetMonkeyName = "Monkey";
    public string SetDate = "00-00";
    public bool SetRecordToFile = true;
 
    [Header("Display Settings")]
    public int SetScreenWidth = 1920;
    public int SetScreenHeight = 1080;
    public int SetScreenOffset = 0;
    public int SetMenuBarHeight = 21;
    public bool LaunchFullScreen = true;
    public float SetCameraFOV = 90;
 
 
#endregion Public Variables
 
#region Static Variables
public static UserInputDevice InputDevice;
    public static float Move_Sensitivity;
    public static float Turn_Sensitivity;
    public static int ResolutionX;
    public static int ResolutionY;
    public static int XOffset;
    // The menu bar is exactly 21 pixels in height
    public static int MenuOffset;
    public static EyeTrackers Eye_Tracker; 
    #endregion Static Variables
 
    #region Private Variables
    private GameObject _eyeTracker;
 
    #endregion Private Variables
 
    // Start is called before the first frame update
    void Start()
    {
        // Add the required components
        // Events Controller
        GenerateNestedGameObject("EventsController", new Type[] { typeof(EventsController) });
 
        if (SetPlaybackMode == false)
        {
            // MonkeyLogic
            GenerateNestedGameObject("MonkeyLogicController", new Type[] { typeof(MonkeyLogicController) });
 
            // User input
            GameObject ic = GenerateNestedGameObject("InputController", new Type[] { typeof(UserInputController) });
 
            /* // Removed because of NI board limitations of a single software connection 
            if (SetInputDevice == UserInputDevice.NIJoystick)
            {
                GameObject nidaq = GenerateNestedGameObject("NIJoystick", new Type[] { typeof(NIDAQmxJoystick) });
                NIDAQmxJoystick ni = nidaq.GetComponent<NIDAQmxJoystick>();
                //ni.Channels = new String[] { NIJoystickX, NIJoystickY };
 
                ic.GetComponent<UserInputController>().SetNIDaqJoystick(ni);

            }
            */

            // EyeTrackers
            switch (SetEyeTracker)
            {
                case EyeTrackers.EyeLink:
                    _eyeTracker = GenerateNestedGameObject("EyeLinkController", new Type[] { typeof(EyeLinkController) });
                    break;
 
                case EyeTrackers.TobiiProFusion:
                    _eyeTracker = GenerateNestedGameObject("TobiiController", new Type[] { typeof(TobiiController), typeof(TobiiEyeTracker) });
                    _eyeTracker.GetComponent<TobiiEyeTracker>()._connectToFirst = true;
                    _eyeTracker.GetComponent<TobiiEyeTracker>().SubscribeToGazeData = true;
                    break;
                default:
                    _eyeTracker = null;
                    break;
            }
            if (_eyeTracker)
                _eyeTracker.GetComponent<EyeControllerBase>().SetGazeWindow(SetGazeRadius);
        }
        else
        {
            GameObject pbc = GenerateNestedGameObject("PlaybackController", new Type[] { typeof(PlaybackController) });
            pbc.GetComponent<PlaybackController>().Configure(SetTaskName, SetMonkeyName, SetDate, ResolutionX, ResolutionY, SetRecordToFile);
        }
 
        // FullScreen window
        GameObject fsw = GenerateNestedGameObject("FullScreenView", new Type[] { typeof(FullScreenView) });
        if (LaunchFullScreen)
            fsw.GetComponent<FullScreenView>().LaunchView(ResolutionX, ResolutionY, MenuOffset, XOffset);
    }
 
    private void OnValidate()
    {
        InputDevice = SetInputDevice;
        Move_Sensitivity = SetMoveSensitivity;
        Turn_Sensitivity = SetTurnSensitivity;
 
        ResolutionX = SetScreenWidth;
        ResolutionY = SetScreenHeight;
        XOffset = SetScreenOffset;
        MenuOffset = SetMenuBarHeight;
 
        Eye_Tracker = SetEyeTracker;
 
        // The camera FOV value is for the Vertical FOV, convert to Horizontal
        if (Camera.main != null)
            Camera.main.fieldOfView = 2 * Mathf.Atan(Mathf.Tan(SetCameraFOV * Mathf.Deg2Rad * 0.5f) / Camera.main.aspect) * Mathf.Rad2Deg;
 
        if (_eyeTracker)
            _eyeTracker.GetComponent<EyeControllerBase>().SetGazeWindow(SetGazeRadius);
 
    }
 
    private GameObject GenerateNestedGameObject(string name, Type[] components)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = gameObject.transform;
 
        foreach (Type c in components)
        {
            go.AddComponent(c);
        }
        return go;
    }
}
 