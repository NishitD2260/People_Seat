using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Drag the Text UI here in Inspector

    float pollingTime = 1f;
    float time;
    int frameCount;

    void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsText.text = frameRate + " Fps";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}
