#include "GPU.cginc"

#define PC_REG 0x10
#define I_REG 0x11
#define HALT_REG 0x12
#define RESET_REG 0x14
#define SP_REG 0x15
#define OP_REG 0xFF
#define X(opcode) (opcode & 0x0F00) >> 8
#define Y(opcode) (opcode & 0x00F0) >> 4
#define REG_LIMIT 0xFF
#define PC_LIMIT 0xFFF

//typedef void opcode;

struct Register
{
    uint data;
};

struct SystemInfo
{
    float data;
};

RWStructuredBuffer<SystemInfo> Clocks; //TARGETFPS, CYCLE, OLDSYSTIME, FRAMECOUNTER
RWStructuredBuffer<Register> Registers; //0x00-0x0F regular registers, 0x10 = PC, 0x11 = INDEXER, 0x12 = HALT, 0x13 = RAND, 0x14 = RESET, 0x15 = SP
RWStructuredBuffer<SystemRam> RAM; //0x00 - 0xFFF
RWStructuredBuffer<SystemRam> STACK; //could be part of RAM above 4096 '\_(-_- )_/`

//no instructions
int NOP() {
    return 0;
};

//clear display
int CLR() {
    return 0;
};

//pop stack and PC = value of pop
int RET() {
    Registers[PC_REG].data = STACK[Registers[SP_REG].data].data;
    Registers[SP_REG].data -= 1;

    return 0;
}

int FuncSystem(uint opCode) //0
{
    [call] switch (opCode & 0x00FF) {
    case 0xE0: 
        return CLR();
    case 0xEE:
        return RET();
    };
    return 0;
};

int FuncJP(uint opCode) //1
{
    Registers[PC_REG].data = opCode & 0x0FFF;
    return 0;
};

int FuncCALL(uint opCode) //2
{
    Registers[SP_REG].data += 1; //up STACK Counter
    STACK[Registers[SP_REG].data].data = Registers[PC_REG].data; //store PC in STACK
    Registers[PC_REG].data = opCode & 0x0FFF; //set NNN in PC
    return 0;
};

//skip if equal Reg[X] == NN
int FuncSE_VX_NN(uint opCode) //3
{
    if(Registers[X(opCode)].data == opCode & 0x00FF) return 2; //up PC
    return 0;
};

//skip if not equal Reg[X] == NN
int FuncSNE_VX_NN(uint opCode) //4
{
    if (Registers[X(opCode)].data != opCode & 0x00FF) return 2; //up PC
    return 0;
};

//skip if equal Reg[X] == Reg[Y]
int FuncSE_VX_VY(uint opCode) //5
{
    if (Registers[X(opCode)].data == Registers[Y(opCode)].data) return 2; //up PC
    return 0;
};

int FuncLD_VX_NN(uint opCode) //6
{
    Registers[X(opCode)].data = opCode & 0x00FF;
    return 0;
};

int FuncADD_VX_NN(uint opCode) //7
{
    Registers[X(opCode)].data = Registers[X(opCode)].data + (opCode & 0x00FF);
    Registers[X(opCode)].data &= REG_LIMIT; //rollover limiter (not present)
    return 0;
};

int FuncALU_VX_VY(uint opCode) //8
{
    //0x000N ALU functions
    [branch] switch (opCode & 0x000F) {
    //LD Reg[X], Reg[Y]
    case 0x0:
        Registers[X(opCode)].data = Registers[Y(opCode)].data;
        return 0;
    //OR Reg[X], Reg[Y]
    case 0x1:
        Registers[X(opCode)].data |= Registers[Y(opCode)].data;
        return 0;
    //AND Reg[X], Reg[Y]
    case 0x2:
        Registers[X(opCode)].data &= Registers[Y(opCode)].data;
        return 0;
    //XOR Reg[X], Reg[Y]
    case 0x3:
        Registers[X(opCode)].data ^= Registers[Y(opCode)].data;
        return 0;
    //ADD Reg[X], Reg[Y]
    case 0x4:
        Registers[X(opCode)].data += Registers[Y(opCode)].data;
        //is carry?
        Registers[0xF].data = 0;
        if (Registers[X(opCode)].data > REG_LIMIT) {
            Registers[0xF].data = 1; //set carry flag
            Registers[X(opCode)].data &= REG_LIMIT; //rollover limiter (not present)
        }
        return 0;
    //SUB Reg[X], Reg[Y]
    case 0x5:
        //is borrow?
        Registers[0xF].data = 0;
        if (Registers[X(opCode)].data > Registers[Y(opCode)].data)
        {
            Registers[0xF].data = 1; //set carry flag
        }

        Registers[X(opCode)].data -= Registers[Y(opCode)].data;
        Registers[X(opCode)].data &= REG_LIMIT;
        return 0;
    //SHR Reg[X], shift right and store carry
    case 0x6:
        Registers[0xF].data = Registers[X(opCode)].data & 0x01; //set carry flag
        Registers[X(opCode)].data >>= 1;
        return 0;
    //SUBN Reg[X], Reg[Y]
    case 0x7:
        //is borrow?
        Registers[0xF].data = 0;
        if (Registers[Y(opCode)].data > Registers[X(opCode)].data)
        {
            Registers[0xF].data = 1; //set carry flag
        }

        Registers[X(opCode)].data = Registers[Y(opCode)].data - Registers[X(opCode)].data;
        return 0;
    //SHL Reg[X], shift left and store carry
    case 0xE:
        Registers[0xF].data = (Registers[X(opCode)].data >> 7) & 0x01; //set carry flag
        Registers[X(opCode)].data <<= 1;
        return 0;
    }
    return 0;
};

int FuncSNE_VX_VY(uint opCode) //9
{
    if (Registers[X(opCode)].data != Registers[Y(opCode)].data) return 2;
    return 0;
};

int FuncIREG(uint opCode) //A
{
    Registers[I_REG].data = opCode & 0x0FFF;
    return 0;
};

int FuncJP_PC(uint opCode) //B
{
    Registers[PC_REG].data = Registers[0].data + (opCode & 0x0FFF);
    Registers[PC_REG].data &= PC_LIMIT; //rollover limiter (not present)
    return 0;
};

int FuncRAND(uint opCode) //C
{
    Registers[X(opCode)].data = (uint)Clocks[0x04].data & (opCode & 0x00FF);
    Registers[X(opCode)].data &= REG_LIMIT;  //rollover limiter (not present)
    return 0;
};

int FuncDISPLAY(uint opCode) //D
{
    //store n from ADDY in I
    uint pixelsX = 8;
    uint pixelsY = opCode & 0xF; //0x000N nibble
    Registers[0xF].data = 0; //reset collision flag

    for (uint row = 0; row < pixelsY; row++)
    {
        uint sprite = RAM[Registers[I_REG].data + row].data;

        for (uint column = 0; column < pixelsX; column++)
        {
            if ((sprite & 0x80) > 0x00)
            {
                Registers[0xF].data = SetPixel(Registers[X(opCode)].data + column, Registers[X(opCode)].data + row); //set flag if pixel is erased
            }

            sprite <<= 1;
        }
    }

    return 0;
};

int FuncKEY(uint opCode) //E
{
    return 0;
};

int FuncTIMERS(uint opCode) //F
{
    return 0;
};

int ExecuteInstruction(uint opCode)
{
    //TODO: log operations to a looping array for debugging purposes

    Registers[OP_REG].data = opCode;

    Registers[PC_REG].data += 2;

    //opcode (0x[0-F]??? > 0x[0-F])
    [call] switch (opCode >> 12) {
    case 0x0:
        return FuncSystem(opCode);
    case 0x1:
        return FuncJP(opCode);
    case 0x2:
        return FuncCALL(opCode);
    case 0x3:
        return FuncSE_VX_NN(opCode);
    case 0x4:
        return FuncSNE_VX_NN(opCode);
    case 0x5:
        return FuncSE_VX_VY(opCode);
    case 0x6:
        return FuncLD_VX_NN(opCode);
    case 0x7:
        return FuncADD_VX_NN(opCode);
    case 0x8:
        return FuncALU_VX_VY(opCode);
    case 0x9:
        return FuncSNE_VX_VY(opCode);
    case 0xA:
        return FuncIREG(opCode);
    case 0xB:
        return FuncJP_PC(opCode);
    case 0xC:
        return FuncRAND(opCode);
    case 0xD:
        return FuncDISPLAY(opCode);
    case 0xE:
        return FuncKEY(opCode);
    case 0xF:
        return FuncTIMERS(opCode);
    }

    return NOP();
};


/*
static opcode opCode0Table[0x01] = {
    NOP()
};

static opcode opCodePrimaryTable[0xF] = { //0x0000 - 0xF000
    Handle0(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP()
};*/
