using System.IO;
using UnityEngine;

public class TakeScreenshot : MonoBehaviour
{
    private Camera cam;
    public static string path = "Screenshots";
    public KeyCode screenshotKey = KeyCode.Z;
    public bool take4KImage = true;


    private const int fourKWidth = 3840;
    private const int fourKHeight = 2160;


    private void Awake()
    {
        cam = GetComponent<Camera>();
    }



    public static string ScreenShotName(int width, int height)
    {
        return string.Format("{0}/" + path + "/screen_{1}x{2}_{3}.png",
                             Application.dataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }


    public void Screenshot(int width, int height)
    {
        // https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(width, height);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));

    }


    void LateUpdate()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            int w = Display.main.renderingWidth, h = Display.main.renderingHeight;
            if(take4KImage)
            {
                w = fourKWidth;
                h = fourKHeight;
            }
            Screenshot(w, h);
        }

    }


}
