/*
 * Handles the LSL communication for both the Control Inlet and the publication of frame data on the Frame Outlet. 
 * 
 * the FrameOutlet will publish as floats: 
    * Position X, Y, Z 
    * Rotation
    * TODO: Add more
 * 
 * the Trial Outlet will publish at the end of trial: 
    * Current Target
    * TODO: Add more
 *
 * This script is based on the LSLMarkerStream.cs script in LSL/Scripts/. 
 * 
 * Notes: 
 * - We will use delegate functions in the MonkeyLogicController children classes that
 *      will call forward functions to trigger Events in the Events controller class instead of sending the events directly from
 *      the children classes. This is to have a better control on the ins and outs of the LSL streams and the events. 
 * - 
 * 
 * */

using System;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using Misc;

public class MonkeyLogicController : MonoBehaviour
{
    // Outlets
    private MonkeyLogicOutlet outlets;
    private int frameOutlet; // index in the outlets list. 
    private string _frameOutletName = "ML_FrameData";
    private string _frameOutletType = "Unity";
    private string _frameOutletID = "frame1214";

    private int trialOutlet;
    private string _trialOutletName = "ML_TrialData";
    private string _trialOutletType = "Markers";
    private string _trialOutletID = "trial1214";

    // Inlets
    private MonkeyLogicInlet inlet;
    private string _controlInletName = "ML_ControlStream";
    private string _controlInletType = "Markers";
    private string _controlInletID = "control1214";
    private MonkeyLogicResolver _resolver;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("You're running " + SystemInfo.operatingSystem +
                ". Aborting MonkeyLogicController.cs");
            return; // LSL crashes OSX           
        }

        // Create stream descriptors
        outlets = gameObject.AddComponent<MonkeyLogicOutlet>();

        // These need to be MonoBehavior so leave unchanged
        _resolver = gameObject.AddComponent<MonkeyLogicResolver>();
        inlet = gameObject.AddComponent<MonkeyLogicInlet>();

        // Configure
        frameOutlet = outlets.Configure(_frameOutletName,
                                _frameOutletType,
                                22,
                                liblsl.IRREGULAR_RATE,
                                liblsl.channel_format_t.cf_double64,
                                _frameOutletID,
                                GenerateXMLMetaData());

        trialOutlet = outlets.Configure(_trialOutletName,
                                _trialOutletType,
                                1,
                                liblsl.IRREGULAR_RATE,
                                liblsl.channel_format_t.cf_string,
                                _trialOutletID,
                                GenerateXMLMetaData());

        inlet.Configure(_controlInletName, _controlInletType, _controlInletID, _resolver);

        // add listener for Streams delegates
        inlet.OnCalibrationReceived += ForwardEyecalibration;
        inlet.OnCommand += ForwardCommand;

        EventsController.OnPublishFrame += PublishFrame;
        EventsController.OnPublishTrial += PublishTrial;

    }
    private IDictionary<string, IDictionary<string, int>> GenerateXMLMetaData()
    {
        IDictionary<string, IDictionary<string, int>> metadata_dicts_names = new Dictionary<string, IDictionary<string, int>>();

        if (ExperimentController.instance)
        {
            // Get Name - InstanceID dict from Experiment Controller
            IDictionary<string, int> obj_map = ExperimentController.instance.InstanceIDMap;
            IDictionary<string, int> phase_map = new Dictionary<string, int>();
            foreach (var test in Enum.GetValues(typeof(StateNames)))
            {
                phase_map.Add(test.ToString(), (int)test);
            }

            metadata_dicts_names.Add("phase_map", phase_map);
            metadata_dicts_names.Add("obj_map", obj_map);


        }
        return metadata_dicts_names;
    }

    // Forward delegates from children classes to the Events Controller. 
    private void ForwardEyecalibration(EyeCalibrationParameters parameters)
    {
        EventsController.instance.SendEyeCalibrationUpdate(parameters);
    }

    private void ForwardCommand(string command)
    {
        switch (command)
        {
            case "Begin":
                EventsController.instance.SendBegin();
                break;
            case "Pause":
                EventsController.instance.SendPause();
                break;
            case "Resume":
                EventsController.instance.SendResume();
                break;
            case "End":
                EventsController.instance.SendEnd();
                break;
            default:
                break;
        }
    }

    public double GetLSLTime()
    {
        return liblsl.local_clock();
    }

    public void PublishFrame(double[] to_publish)
    {
        outlets.Write(frameOutlet, to_publish);
    }

    public void PublishTrial(string to_publish)
    {
        outlets.Write(trialOutlet, to_publish);
    }

    // To add outlets controlled by other classes
    public int AddExternalOutlet(string name, string type, int chan_count, double rate,
                                 string unique_id,
                                 liblsl.channel_format_t format = liblsl.channel_format_t.cf_double64,
                                 IDictionary<string, IDictionary<string, int>> metadata_dicts_names = null)
    { 
        if (metadata_dicts_names == null)
            metadata_dicts_names = new Dictionary<string, IDictionary<string, int>>();

        int outlet_id = outlets.Configure(name, type, chan_count, rate, format, unique_id, metadata_dicts_names);

        return outlet_id;
    }

    public void PublishExternal(int outlet_id, double[] to_publish)
    {
        outlets.Write(outlet_id, to_publish);
    }
}
