using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FullScreenView : MonoBehaviour
{
    public bool AutoLaunch; 

    // FullScreen Game Window
    private EditorWindow win;

    public void LaunchView(int ResolutionX, int ResolutionY, int MenuOffset, int XOffset)
    { 
        win = (EditorWindow)ScriptableObject.CreateInstance("UnityEditor.GameView");
        win.name = "FullScreenView";
        win.ShowUtility();

        win.minSize = new Vector2 { x = ResolutionX, y = ResolutionY + MenuOffset };
        win.position = new Rect
        {
            x = XOffset,
            y = -MenuOffset,
            width = ResolutionX,
            height = ResolutionY + MenuOffset
        };
      
    }

    private void OnDisable()
    {
        if (win != null) win.Close();
    }
}
