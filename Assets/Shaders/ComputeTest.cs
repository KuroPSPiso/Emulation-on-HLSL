using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    //TOOD: remove TEMP
    public Texture2D DebugROMData;

    public int trueResolutionX = 256, trueResolutionY = 256;
    public int resolutionX = 256, resolutionY = 256;
    public int FPS = 60;
    public int Cycle = 0;
    public int RAMCapacity = 4096;

    public int FixedFPS = 5;
    public bool FPSEnable = false;
    public bool IsDebug = false;

    private uint inputValue;

    struct SystemInfo
    {
        public float data; 
    }

    struct SystemRam
    {
        public uint data;
    }
    struct SystemRegister
    {
        public uint data;
    }

    private SystemInfo[] clocks;
    private SystemRam[] ram;
    private SystemRam[] stack;
    private SystemRegister[] registers;

    //max 30min per frame
    private long SystemTime { get
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) % 1800000;
        }
    }
    private ComputeBuffer clocksBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.renderTexture == null)
        {
            this.renderTexture = new RenderTexture(256, 256, 24);
            this.renderTexture.enableRandomWrite = true;
            this.renderTexture.Create();

            this.Reset();
        }

        this.computeShader.SetFloat("Cycle", this.Cycle);
        this.computeShader.SetTexture(0, "Result", this.renderTexture);
        this.computeShader.SetFloat("ResolutionX", this.resolutionX);
        this.computeShader.SetFloat("ResolutionY", this.resolutionY);
        this.computeShader.SetFloat("SystemTime", (float)this.SystemTime);
        this.computeShader.SetBool("FPSEnable", this.FPSEnable);

        this.computeShader.SetFloat("FixedFPS", this.FixedFPS);

        this.computeShader.Dispatch(0, this.renderTexture.width / 8, this.renderTexture.height, 1);
        
        Graphics.Blit(this.renderTexture, destination);
    }

    [ContextMenu("Reset")]
    private void Reset()
    {
        this.GenerateClocks();

        /*
        this.ram = new SystemRam[this.RAMCapacity];
        this.stack = new SystemRam[256]; //stack w/o stack counter/pointer
        this.registers = new SystemRegister[20]; //0x00-0x0F regular registers, 0x10 = PC, 0x11 = INDEXER, 0x12 = HALT, 0x13 = SPEED
        this.registers[0x10].data = 0x200;
        this.registers[0x13].data = 10;
        this.registers[0x14].data = 1;*/
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
        this.clocksBuffer = new ComputeBuffer(clocks.Length + 5, dataSize);


        this.clocksBuffer.SetData(clocks);
        this.computeShader.SetBuffer(0, "Clocks", this.clocksBuffer);
    }

    private void Update()
    {
        if(this.IsDebug)
        {
            if(this.DebugROMData != null)
            {
                int length = 0;
                Color clrLength = this.DebugROMData.GetPixel(1, 0);
                Color clrLength2 = this.DebugROMData.GetPixel(0, 0);

                //Color32[] clrLength = this.DebugROMData.GetPixels32(1, 0);

                int b1, g1, r1, b2;

                //little-endian size
                length += (b1 = (int)((clrLength.b * 100) * 256)) / 100;
                length += (g1 = (int)((clrLength.g * 100) * 256)) / 100 << 8;
                length += (r1 = (int)((clrLength.r * 100) * 256)) / 100 << 16;
                length += (b2 = (int)((clrLength2.b * 100) * 256)) / 100 << 24;
                Debug.Log($"ROM size:{b1 / 100},{g1 / 100},{r1 / 100},{b2 / 100} ||" + length);
            }
        }

        if (this.clocksBuffer != null && IsDebug)
        {
            this.clocksBuffer.GetData(this.clocks);

            //if(this.IsDebug) Debug.Log($"frames in ms(target):{this.clocks[0].data}, cycle:{this.clocks[1].data}, sys:{(float)this.SystemTime}, oldsys:{(long)this.clocks[2].data}, passes:{clocks[3].data}");
        }

        switch (Input.inputString)
        {
            default: this.inputValue = 0x00; break;
            case "1": this.inputValue = 0x11; break;
            case "2": this.inputValue = 0x12; break;
            case "3": this.inputValue = 0x13; break;
            case "4": this.inputValue = 0x1C; break;
            case "Q": this.inputValue = 0x14; break;
            case "W": this.inputValue = 0x15; break;
            case "E": this.inputValue = 0x16; break;
            case "R": this.inputValue = 0x1D; break;
            case "A": this.inputValue = 0x17; break;
            case "S": this.inputValue = 0x18; break;
            case "D": this.inputValue = 0x19; break;
            case "F": this.inputValue = 0x1E; break;
            case "Z": this.inputValue = 0x1A; break;
            case "X": this.inputValue = 0x10; break;
            case "C": this.inputValue = 0x1B; break;
            case "V": this.inputValue = 0x1F; break;
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
