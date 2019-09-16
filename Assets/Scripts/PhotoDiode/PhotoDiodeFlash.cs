﻿///<summary>
///     We will flash the screen every 3-10 frames randomly to give time to the 
///     photodiode to plateau instead of flashing every frame. 
///     
///     Canvas objects are rendered after the scene so even if we change it during
///     update, the screen value will only change at the end of the frame. 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class PhotoDiodeFlash : MonoBehaviour
{
    // Framerate control
    //public float Rate = 10.0f;
    //float currentFrameTime;

    private int nFrames;
    private int countFrames = -1;
    private float greyScale;
    private Image square; 
    // Start is called before the first frame update
    void Start()
    {
        square = gameObject.GetComponentInChildren<Image>();

        /*
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        currentFrameTime = Time.realtimeSinceStartup;
        StartCoroutine("WaitForNextFrame");
        */
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(1/Time.deltaTime);
        if (countFrames == -1)
        {
            countFrames = 0;
            nFrames = Random.Range(50, 100);
            greyScale = Random.Range(0.0f, 1.0f);
        }
        else if (countFrames == nFrames)
        {
            // Reset counter, next frame will define range
            countFrames = -1;
        }
        else if (countFrames < nFrames)
        {
            countFrames += 1;
        }

        if (square != null)
        {
            Color rgb = new Color() { r = greyScale, g = greyScale, b = greyScale, a = 1 };
            square.color = rgb;

            // Send data to the experiment controller to be saved on the frame stream
            EventsController.instance.SendPhotoDiodeUpdate(greyScale);

        }
    }
    /*
    IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            currentFrameTime += 1.0f / Rate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = currentFrameTime - t - 0.005f;
            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 100));
            while (t < currentFrameTime)
                t = Time.realtimeSinceStartup;
        }
    }
    */
}