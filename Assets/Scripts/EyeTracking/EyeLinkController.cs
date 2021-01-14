///<summary>
/// This script interfaces with the EyeLink system. The public variables are for configuration
/// and include screen proprotions, EyeLink IP address and such. 
/// 
/// 
/// 
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SREYELINKLib;
using System.Threading;

// Since the Connection attempt takes a long time and significantly decrease frame rate
// we will implement a thread to attempt to connect, that way execution won't be affected 
// when no eyelink is present. There isn't a simple way to test whether the eyelink is 
//available or not, except by trying to connect to it. 

public class EyeLinkChecker
{
    public string IP;
    public bool threadRunning = false;
    public bool elOnline = false;
    private Thread thread;
    
    public void StartThread(string elIP)
    {
        if (thread == null)
        {
            Debug.Log("Eyelink checker thread started");
            IP = elIP;
            // It's possible that this is a re-spawn because a connection was lost
            elOnline = false;
            thread = new Thread(ThreadConnect);
            thread.Start();
        }
    }

    public void StopThread()
    {
        threadRunning = false;
        // This waits until the thread exits,
        // ensuring any cleanup we do after this is safe. 
        if (thread != null)
        {
            thread.Join();
            Debug.Log("Eyelink checker thread stopped.");
            thread = null;
        }
    }
   
    public bool RunCheck()
    {
        return threadRunning;
    }
    public bool CheckELOnline()
    {
        return elOnline;
    }
        
    private void ThreadConnect()
    {
        EyeLink _el = new EyeLink();
        _el.setEyelinkAddress(IP, -1);

        threadRunning = true;

        while (threadRunning)
        {
            if (!elOnline)
            {
                try
                {
                    //Debug.Log("Trying to connect");
                    _el.broadcastOpen();

                }
                catch
                {

                }

                if (_el.isConnected())
                {
                    elOnline = true;
                    threadRunning = false;
                    continue;
                }
            }
            
        }
    }

}

public class EyeLinkController : EyeControllerBase
{

    // Eye Link settings
    private EL_EYE el_Eye = EL_EYE.EL_EYE_NONE;
    private EyeLinkUtil el_Util;
    private EyeLink el;
   
    private EyeLinkChecker checker;
    
    // Start is called before the first frame update
    void Awake()
    {
        Initialize();

        el = new EyeLink();
        el_Util = new EyeLinkUtil();
        checker = new EyeLinkChecker();
    }

    private void OnDisable()
    {
        Disable();

        if (checker != null)
        {
            checker.StopThread();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Sample s;
        //Sample s;
        double lastSampleTime = 0.0;

        // if not connected, has eye calibration and no thread: Initialize thread
        if (!el.isConnected() && !checker.RunCheck() && !checker.CheckELOnline() && eyecal.has_calibration)
        {
            // Configure eye
            switch (eyecal.GetEyeLinkTrackedEye())
            {
                case 0:
                    el_Eye = EL_EYE.EL_LEFT;
                    break;
                case 1:
                    el_Eye = EL_EYE.EL_RIGHT;
                    break;
                default:
                    el_Eye = EL_EYE.EL_EYE_NONE;
                    break;
            }

            // Spawn checker thread to test connection
            checker.StartThread(eyecal.GetEyeLinkIP());
            
            s = null;
        }
        // If checker created
        else if(!el.isConnected() && checker.CheckELOnline() && eyecal.has_calibration)
        {
            // Eyelink Online
            // Connect
            el.setEyelinkAddress(eyecal.GetEyeLinkIP(), -1);
            el.broadcastOpen();
            if (el.isConnected())
            {
                Debug.Log("EyeLink Connected");
                checker.StopThread();

                // 
                el.resetData();
                el.dataSwitch(4 | 8);
            }
            s = null;
        }
        
        // If connected, has calibration and tracker is in record mode: get sample    
        else if (el.isConnected() && eyecal.has_calibration && el.getTrackerMode() == 14)
        {
            try
            {
                s = el.getNewestSample();
            }
            catch
            {
                //Debug.Log(e.ToString());
                s = null;
            }
        }
        else
        {
            s = null;
        }
        
        // Get position on screen in pixels
        if (s != null && s.time != lastSampleTime)
        {
            if (el_Eye != EL_EYE.EL_EYE_NONE)
            {
                if (el_Eye == EL_EYE.EL_BINOCULAR)
                    el_Eye = EL_EYE.EL_LEFT;

                _eyeRaw.x = s.get_px(el_Eye);
                _eyeRaw.y = s.get_py(el_Eye);

                eyecal.EL_RawToPix(_eyeRaw, out _eyeDeg, out _eyePix);
                
                gazeProcess.ProcessGaze(_eyePix, out _gazeTargets, out _gazeCounts, out _gazeHits);
                gazeView.ShowGaze(_gazeHits);

                lastSampleTime = s.time;
            }
            else
            {
                el_Eye = (EL_EYE)el.eyeAvailable();
            }
            // Update values to the experiment controller
            EventsController.instance.SendEyeLateUpdateEvent(_eyeDeg, _gazeTargets, _gazeCounts);
        }
    }
    
    private void OnDestroy()
    {
        el.stopRecording();
        el.closeDataFile();
        el.close();
    }
    
}
