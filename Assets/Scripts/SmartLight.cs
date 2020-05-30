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
        background.gameObject.SetActive(false);
        interactive.gameObject.SetActive(false);
    }

    public void On()
    {
        background.gameObject.SetActive(true);
        interactive.gameObject.SetActive(true);
    }
}
