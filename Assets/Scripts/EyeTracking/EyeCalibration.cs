﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EyeCalibration : MonoBehaviour
{
    private EyeCalibrationParameters _eyecal_params;
    
    // Read-only
    private bool _has_calibration = false;
    public bool has_calibration
    {
        set { }
        get { return _has_calibration; }
    }
    

    public void UpdateCalibration(EyeCalibrationParameters parameters)
    {
        _eyecal_params = parameters;
        _has_calibration = true;
    }

    // The calibration is handled in the Tobii Tracker Manager so we are not relying on data
    // received from Monkeylogic. We simply convert the calibrated data in a normalized Active 
    // Display Coordinate System (ADCS) from 0 -> 1 starting at the Top-Left towards Botton-Right.
    // Unity starts at the Bottom-Left towards Top-Right. 
    public Vector2 T_ADCSToPix(Vector2 in_eye)
    {
        // Pixel values are within unity only. Unity origin (0,0) is the bottom-left. 
        // We also scale the value if the Unity resolution differs than the MonkeyLogic one. 
        
        Vector2 eye_pix = new Vector2
        {
            x = in_eye.x * ExperimentConfiguration.ResolutionX,
            y = (1.0f - in_eye.y) * ExperimentConfiguration.ResolutionY
        };

        // prevent values from falling outside of screen
        if (eye_pix.x < 0 || eye_pix.x > ExperimentConfiguration.ResolutionX || float.IsNaN(eye_pix.x))
            eye_pix.x = -1;
        if (eye_pix.y < 0 || eye_pix.y > ExperimentConfiguration.ResolutionY || float.IsNaN(eye_pix.y))
            eye_pix.y = -1;

        return eye_pix;
    }

    // will receive raw Int values from the eyelink and return calibrated pixel 
    // position on screen. 
    public void EL_RawToPix(Vector3 in_eye, out Vector2 eye_deg, out Vector2 eye_pix)
    {
        // From MonkeyLogic, 
        // First step: 
        // Output = (Raw - offset) * gain
        in_eye.x = (in_eye.x - _eyecal_params.el_offsets[0]) * _eyecal_params.el_gains[0];
        in_eye.y = (in_eye.y - _eyecal_params.el_offsets[1]) * _eyecal_params.el_gains[1];
        in_eye.z = 1.0f;

        // Second step:
        //      [X,Y,adjust] = [x_raw, y_raw, 1] * t_transform
        // Convert t_transform list into a 4X4 matrix
        Matrix4x4 t_mat = Matrix4x4.zero;
        t_mat.m00 = _eyecal_params.t_transform[0];
        t_mat.m01 = _eyecal_params.t_transform[1];
        t_mat.m02 = _eyecal_params.t_transform[2];

        t_mat.m10 = _eyecal_params.t_transform[3];
        t_mat.m11 = _eyecal_params.t_transform[4];
        t_mat.m12 = _eyecal_params.t_transform[5];

        t_mat.m20 = _eyecal_params.t_transform[6];
        t_mat.m21 = _eyecal_params.t_transform[7];
        t_mat.m22 = _eyecal_params.t_transform[8];

        // Multiply, but need to transpose first
        // TODO: Figure out why we need the transposition
        in_eye = t_mat.transpose.MultiplyPoint3x4(in_eye);

        // Third step: 
        // normalize out_eye values to set z back equal to 1
        in_eye /= in_eye.z;

        // Fourth step: 
        // get eye position in degrees
        eye_deg = new Vector2
        {
            x = ((in_eye.x - _eyecal_params.t_offset[0]) * _eyecal_params.t_rotation[0]) + ((in_eye.y - _eyecal_params.t_offset[1]) * _eyecal_params.t_rotation[2]),
            y = ((in_eye.x - _eyecal_params.t_offset[0]) * _eyecal_params.t_rotation[1]) + ((in_eye.y - _eyecal_params.t_offset[1]) * _eyecal_params.t_rotation[3])
        };

        // Fifth step: 
        // Assuming that (0,0) in pixels is the BOTTOM LEFT corner of the screen
        // need to make sure the screen resolutions match between ML and Unity
        eye_pix = new Vector2
        {
            x = (eye_deg.x * (_eyecal_params.pix_per_deg * ExperimentConfiguration.ResolutionX / _eyecal_params.ml_x_res)) + (0.5f * ExperimentConfiguration.ResolutionX),
            y = (eye_deg.y * (_eyecal_params.pix_per_deg * ExperimentConfiguration.ResolutionY / _eyecal_params.ml_y_res)) + (0.5f * ExperimentConfiguration.ResolutionY)
        };

        // prevent values from falling outside of screen
        if (eye_pix.x < 0 || eye_pix.x > ExperimentConfiguration.ResolutionX)
            eye_pix.x = -1;
        if (eye_pix.y < 0 || eye_pix.y > ExperimentConfiguration.ResolutionY)
            eye_pix.y = -1;
    }

    public string GetEyeLinkIP()
    {
        return _eyecal_params.el_IP;
    }

    public int GetEyeLinkTrackedEye()
    {
        // 0: Left; 1: Right;
        return _eyecal_params.el_eyeID;
    }
}

