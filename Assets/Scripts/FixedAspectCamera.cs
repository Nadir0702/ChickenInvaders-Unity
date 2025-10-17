using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    public float targetAspect = 4f / 3f;
    Camera cam;
    int sw, sh;

    void Awake(){ cam = GetComponent<Camera>(); Apply(); }
    void Update(){ if (Screen.width != sw || Screen.height != sh) Apply(); }

    void Apply()
    {
        sw = Screen.width; sh = Screen.height;
        float windowAspect = (float)sw / sh;
        float scale = windowAspect / targetAspect;

        if (scale < 1f) { // letterbox top/bottom
            cam.rect = new Rect(0f, (1f - scale) * 0.5f, 1f, scale);
        } else {          // pillarbox sides
            float inv = 1f / scale;
            cam.rect = new Rect((1f - inv) * 0.5f, 0f, inv, 1f);
        }
    }
}