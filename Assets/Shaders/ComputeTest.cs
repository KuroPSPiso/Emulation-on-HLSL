using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    public int trueResolutionX = 256, trueResolutionY = 256;
    public int resolutionX = 256, resolutionY = 256;
    public int FPS = 60;
    public int Cycle = 0;

    public int FixedFPS = 5;
    public bool FPSEnable = false;
    public bool IsDebug = false;

    struct SystemInfo
    {
        public float data; 
    }

    private SystemInfo[] clocks;
    //max 30min per frame
    private long SystemTime { get
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) % 1800000;
        }
    }
    private ComputeBuffer clocksBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(256, 256, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();

            Reset();
        }

        computeShader.SetFloat("Cycle", Cycle);
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("ResolutionX", resolutionX);
        computeShader.SetFloat("ResolutionY", resolutionY);
        computeShader.SetFloat("SystemTime", (float)SystemTime);
        computeShader.SetBool("FPSEnable", FPSEnable);

        computeShader.SetFloat("FixedFPS", FixedFPS);

        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height, 1);
        
        Graphics.Blit(renderTexture, destination);
    }

    [ContextMenu("Reset")]

    private void Reset()
    {
        GenerateClocks();
    }

    private void GenerateClocks()
    {
        clocks = new SystemInfo[]
        {
            new SystemInfo(){data = 1000 / FPS},
            new SystemInfo(){data = 0},
            new SystemInfo(){data = SystemTime},
            new SystemInfo(){data = 0}
        };

        int fDataSize = sizeof(float);
        int dataSize = fDataSize;
        clocksBuffer = new ComputeBuffer(clocks.Length + 5, dataSize);


        clocksBuffer.SetData(clocks);
        computeShader.SetBuffer(0, "Clocks", clocksBuffer);
    }

    private void Update()
    {
        if (clocksBuffer != null && IsDebug)
        {
            clocksBuffer.GetData(clocks);

            Debug.Log($"frames in ms(target):{clocks[0].data}, cycle:{clocks[1].data}, sys:{(float)SystemTime}, oldsys:{(long)clocks[2].data}, passes:{clocks[3].data}");
        }
    }

    private void OnDisable()
    {
        if (clocksBuffer != null) clocksBuffer.Dispose();
    }

    private void OnApplicationQuit()
    {
        if (clocksBuffer != null) clocksBuffer.Dispose();
    }
}
