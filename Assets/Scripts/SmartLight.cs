using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class SmartLight : MonoBehaviour
{
    public Light2D background;
    public Light2D interactive;

    private void Awake()
    {
        Off();
    }

    private void OnEnable()
    {
        On();
    }

    private void OnDisable()
    {
        Off();
    }

    public void Off()
    {
        background.enabled = false;
        interactive.enabled = false;
    }

    public void On()
    {
        background.enabled = true;
        interactive.enabled = true;
    }
}
