using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchManager : SingletonBase<CameraSwitchManager>
{
    public List<GameObject> Cameras;
    int CurrectCamIndex = 1;


    void Update()
    {
        for (int i = 1; i <= Cameras.Count; i++)
        {
            KeyCode key = KeyCode.Alpha0 + i;
            if (Input.GetKeyDown(key))
            {
                SwitchCamera(i);
            }
        }
    }

    public void SwitchCamera(int CamIndex)
    {
        if (CamIndex != CurrectCamIndex)
        {
            for (int j = 0; j < Cameras.Count; j++)
            {
                if (Cameras[j] != null)
                    Cameras[j].SetActive(j == (CamIndex - 1));
            }
        }
    }
}
