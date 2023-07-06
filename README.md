# Emulation on HLSL
 Unity


## Debug

FixedFPS value (debug slider):
Default = 7 for FPS counter
Any other value with debug ticked:

```c
if (FixedFPS >= 10 && FixedFPS <= 10 + 0x15) //register data (10-26 registers, 27 pc, 28-31 custom sys)
else if (FixedFPS > 6) //fps
else if (FixedFPS == 6) //elapsed calculated
else if (FixedFPS == 5) //elapsed raw
else if (FixedFPS == 4) //systime in ms
else //target ms, cycle, oldsys ms, frame counter
```