using System;
using  UnityEngine;

public class FPSSetting :MonoBehaviour
{
    public int FPS=60;
    private void Start()
    {
        Application.targetFrameRate = FPS;
    }
}
