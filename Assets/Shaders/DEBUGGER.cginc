#pragma once
#define DEBUGGER 1
#define LOGGERSIZE 10

#if DEBUGGER == 1

struct DebugData
{
    uint sysTick;
    uint i;
    uint tick;
    uint pc;
    uint opCode;
    uint newPC;
    uint func;
};

RWStructuredBuffer<DebugData> LOGGER; //0x00 - ?

void LogStatus(uint pc, uint opCode, uint lastFunc, uint newPC)
{
    //current tick
    LOGGER[0].sysTick++;
    //up/reset set value cycler
    LOGGER[0].i++;
    if (LOGGER[0].i >= LOGGERSIZE) LOGGER[0].i = 0;
    
    ////set values
    LOGGER[LOGGER[0].i].tick = LOGGER[0].tick++;
    LOGGER[LOGGER[0].i].pc = pc;
    LOGGER[LOGGER[0].i].opCode = opCode;
    LOGGER[LOGGER[0].i].newPC = newPC;
    LOGGER[LOGGER[0].i].func = lastFunc;

}

#endif