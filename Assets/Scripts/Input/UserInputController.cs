using UnityEngine;
using UnityEngine.InputSystem;

public class UserInputController : MonoBehaviour
{

    public static Vector2 ReadAxes()
    {
        Vector2 axes;

        switch(ExperimentConfiguration.InputDevice)
        {
            case ExperimentConfiguration.UserInputDevice.GamePad:
                if (Gamepad.current != null)
                {
                    axes = Gamepad.current.leftStick.ReadValue();
                }
                else
                {
                    axes = Vector2.zero;
                }
                break;
            case ExperimentConfiguration.UserInputDevice.Joystick:
                if (Joystick.current != null)
                {
                    axes = Joystick.current.stick.ReadValue();
                }
                else
                    axes = Vector2.zero;
                break;
            case ExperimentConfiguration.UserInputDevice.Keyboard:
                axes = new Vector2
                {
                    x = Keyboard.current.rightArrowKey.ReadValue() - Keyboard.current.leftArrowKey.ReadValue(),
                    y = Keyboard.current.upArrowKey.ReadValue() - Keyboard.current.downArrowKey.ReadValue()
                };
                break;
            case ExperimentConfiguration.UserInputDevice.Mouse:
                axes = Mouse.current.delta.ReadValue();
                break;
            case ExperimentConfiguration.UserInputDevice.Null:
                axes = Vector2.zero;
                break;
            default:
                axes = Vector2.zero;
                break;
        }
        axes.x *= ExperimentConfiguration.Turn_Sensitivity;
        axes.y *= ExperimentConfiguration.Move_Sensitivity;
        return axes;
    }
}
