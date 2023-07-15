using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    private RenderTexture ROMData;

    //TOOD: remove TEMP
    public Texture2D ROMDataTex;

    public int trueResolutionX = 256, trueResolutionY = 256;
    public int resolutionX = 256, resolutionY = 256;
    public int FPS = 60;
    public int Cycle = 0;
    public int RAMCapacity = 4096;

    public int FixedFPS = 7;
    public bool FPSEnable = false;
    public bool IsDebug = false;
    public bool IsDebug_ROMLoad = false;
    public bool IsDebug_PC = false;

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
    private SystemRam[] vram;
    private SystemRam[] stack;
    private SystemRegister[] registers;


    //max 30min per frame
    private long SystemTime { get
        {
            return (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond) % 1800000;
        }
    }
    private ComputeBuffer clocksBuffer;
    private ComputeBuffer ramBuffer;
    private ComputeBuffer vramBuffer;
    private ComputeBuffer stackBuffer;
    private ComputeBuffer registersBuffer;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.renderTexture == null)
        {
            this.renderTexture = new RenderTexture(resolutionX, resolutionY, 24);
            this.renderTexture.enableRandomWrite = true;
            this.renderTexture.Create();

            this.Reset();
        }

        if(this.ROMData == null)
        {
            int width = this.ROMDataTex.width;
            int height = this.ROMDataTex.height;
            this.ROMData = new RenderTexture(width, height, 16);
            this.ROMData.enableRandomWrite = true;
            this.ROMData.height = this.ROMDataTex.height;
            this.ROMData.format = RenderTextureFormat.ARGB32;
            //this.ROMData.format = (RenderTextureFormat)this.ROMDataTex.format;
            this.ROMData.Create();
            Graphics.Blit(this.ROMDataTex, this.ROMData);
        }

        this.computeShader.SetTexture(0, "ROM", this.ROMData);
        this.computeShader.SetFloat("Cycle", this.Cycle);
        this.computeShader.SetTexture(0, "Result", this.renderTexture);
        this.computeShader.SetFloat("ResolutionX", this.resolutionX);
        this.computeShader.SetFloat("ResolutionY", this.resolutionY);
        this.computeShader.SetFloat("SystemTime", (float)this.SystemTime);
        this.computeShader.SetBool("FPSEnable", this.FPSEnable);

        this.computeShader.SetFloat("FixedFPS", this.FixedFPS);

        this.computeShader.Dispatch(0, this.renderTexture.width / 8, this.renderTexture.height, 1);

        if(this.IsDebug_ROMLoad) Graphics.Blit(this.ROMData, destination);
        else Graphics.Blit(this.renderTexture, destination);
    }

    [ContextMenu("Reset")]
    private void Reset()
    {
        this.GenerateClocks();

        this.GenerateRam();

        this.GenerateRegisters();
    }

    private void GenerateRam()
    {
        this.ram = new SystemRam[this.RAMCapacity];
        this.stack = new SystemRam[256]; //stack w/o stack counter/pointer
        this.vram = new SystemRam[resolutionX * resolutionY];

        int uiDataSize = sizeof(uint);
        int dataSize = uiDataSize;
        this.ramBuffer = new ComputeBuffer(this.ram.Length, dataSize);
        this.vramBuffer = new ComputeBuffer(this.vram.Length, dataSize);
        this.stackBuffer = new ComputeBuffer(this.ram.Length, dataSize);

        this.ramBuffer.SetData(this.ram);
        this.computeShader.SetBuffer(0, "RAM", this.ramBuffer);

        this.vramBuffer.SetData(this.vram);
        this.computeShader.SetBuffer(0, "VRAM", this.vramBuffer);

        this.stackBuffer.SetData(this.stack);
        this.computeShader.SetBuffer(0, "STACK", this.stackBuffer);
    }

    private void GenerateRegisters()
    {
        this.registers = new SystemRegister[0xFF]; //0x00-0x0F regular registers, 0x10 = PC, 0x11 = INDEXER, 0x12 = HALT, 0x13 = SPEED, 0x14 = RESET, 0x15 = CURR_OPCODE
        this.registers[0x10].data = 0x200; //PC
        this.registers[0x13].data = 10; //speed
        this.registers[0x14].data = 1; //reset
        this.registers[0x15].data = 0x00; //stack pointer

        int uiDataSize = sizeof(uint);
        int dataSize = uiDataSize;
        this.registersBuffer = new ComputeBuffer(this.ram.Length, dataSize);

        this.registersBuffer.SetData(this.registers);
        this.computeShader.SetBuffer(0, "Registers", this.registersBuffer);
    }

    private void GenerateClocks()
    {
        this.clocks = new SystemInfo[]
        {
            new SystemInfo(){data = 1000 / FPS},
            new SystemInfo(){data = 0},
            new SystemInfo(){data = SystemTime},
            new SystemInfo(){data = 0}
        };

        int fDataSize = sizeof(float);
        int dataSize = fDataSize;
        this.clocksBuffer = new ComputeBuffer(this.clocks.Length + 5, dataSize);

        this.clocksBuffer.SetData(this.clocks);
        this.computeShader.SetBuffer(0, "Clocks", this.clocksBuffer);
    }

    uint[] PCWhenNotO = new uint[4];

    private void Update()
    {
        if(this.IsDebug)
        {
            this.ClearLog();
            int length = 0;
            if (this.ROMData != null)
            {
                Texture2D linearTex = new Texture2D(this.ROMData.width, this.ROMData.height, TextureFormat.RGBA32, 0, true);

                Color clrLength = this.ROMDataTex.GetPixel(1, 0);
                Color clrLength2 = this.ROMDataTex.GetPixel(0, 0);
                
                int b1, g1, r1, b2;

                //little-endian size
                length += (b1 = (int)Mathf.Round(clrLength.b * 255.0f)) << 0;
                length += (g1 = (int)Mathf.Round(clrLength.g * 255.0f)) << 8;
                length += (r1 = (int)Mathf.Round(clrLength.r * 255.0f)) << 16;
                length += (b2 = (int)Mathf.Round(clrLength2.b * 255.0f)) << 24;
                Debug.Log($"ROM size:{b1},{g1},{r1},{b2} || {length} || {clrLength.r + clrLength.g + clrLength.b} : {clrLength2.r + clrLength2.g + clrLength2.b}");

                if(IsDebug_ROMLoad)
                {
                    Texture2D rominVrom = new Texture2D(this.ROMData.width, this.ROMData.height, this.ROMDataTex.format, this.ROMDataTex.mipmapCount, true);
                    RenderTexture.active = this.ROMData;
                    rominVrom.ReadPixels(new Rect(0, 0, this.ROMData.width, this.ROMData.height), 0, 0, false);

                    clrLength = rominVrom.GetPixel(1, 0);
                    clrLength2 = rominVrom.GetPixel(0, 0);

                    length = 0;
                    length += (b1 = (int)Mathf.Round(clrLength.b * 255.0f)) << 0;
                    length += (g1 = (int)Mathf.Round(clrLength.g * 255.0f)) << 8;
                    length += (r1 = (int)Mathf.Round(clrLength.r * 255.0f)) << 16;
                    length += (b2 = (int)Mathf.Round(clrLength2.b * 255.0f)) << 24;
                    Debug.Log($"SS ROM size:{b1},{g1},{r1},{b2} || {length} || {clrLength.r + clrLength.g + clrLength.b} : {clrLength2.r + clrLength2.g + clrLength2.b}");
                }
            }
            if (this.vramBuffer != null)
            {
                this.vramBuffer.GetData(vram);
                Debug.Log($"Loaded ROM size:{vram[0].data},{vram[1].data},{vram[2].data},{vram[3].data} || {vram[4].data} ||");


                this.ramBuffer.GetData(this.ram);
                Color clrLength = this.ROMDataTex.GetPixel(2, 0);
                int b, g, r;
                b = (int)Mathf.Round(clrLength.b * 255.0f);
                g = (int)Mathf.Round(clrLength.g * 255.0f);
                r = (int)Mathf.Round(clrLength.r * 255.0f);
                Debug.Log($"first data sect: img {b},{g},{r} : gpu {this.ram[0x200 + 0].data},{this.ram[0x200 + 1].data},{this.ram[0x200 + 2].data}");


                if (length > 2)
                {
                    clrLength = this.ROMDataTex.GetPixel(4, 4);
                    b = (int)Mathf.Round(clrLength.b * 255.0f);
                    g = (int)Mathf.Round(clrLength.g * 255.0f);
                    r = (int)Mathf.Round(clrLength.r * 255.0f);
                    Debug.Log($"last data sect: img {b},{g},{r} : gpu {this.ram[0x200 + 390].data},{this.ram[0x200 + 391].data},{this.ram[0x200 + 392].data}");
                }
            }
            if(IsDebug_PC)
            {
                this.registersBuffer.GetData(registers);
                if(registers[250].data != 0)
                {
                    PCWhenNotO[0] = registers[250].data;
                    PCWhenNotO[1] = registers[251].data;
                    PCWhenNotO[2] = registers[252].data;
                    PCWhenNotO[3] = registers[253].data;
                }

                Debug.Log($"PC: {PCWhenNotO[0]:X}{PCWhenNotO[1]:X}{PCWhenNotO[2]:X}{PCWhenNotO[3]:X}");
            }
        }

        if (this.clocksBuffer != null && IsDebug)
        {
            this.clocksBuffer.GetData(this.clocks);

            if(this.IsDebug) Debug.Log($"frames in ms(target):{this.clocks[0].data}, cycle:{this.clocks[1].data}, sys:{(float)this.SystemTime}, oldsys:{(long)this.clocks[2].data}, passes:{clocks[3].data}");
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
        if (this.clocksBuffer != null) clocksBuffer.Dispose();
        if (this.ramBuffer != null) ramBuffer.Dispose();
        if (this.vramBuffer != null) vramBuffer.Dispose();
        if (this.stackBuffer != null) stackBuffer.Dispose();
        if (this.registersBuffer != null) registersBuffer.Dispose();
    }

    private void OnApplicationQuit()
    {
        if (this.clocksBuffer != null) clocksBuffer.Dispose();
        if (this.ramBuffer != null) ramBuffer.Dispose();
        if (this.vramBuffer != null) vramBuffer.Dispose();
        if (this.stackBuffer != null) stackBuffer.Dispose();
        if (this.registersBuffer != null) registersBuffer.Dispose();
    }
    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}
