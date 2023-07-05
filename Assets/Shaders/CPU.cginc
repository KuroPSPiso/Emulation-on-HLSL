
#define PC_REG 0x10
#define HALT_REG 0x12
#define RESET_REG 0x14
#define OPCODE_REG 0x15

typedef void opcode;

struct Register
{
    uint data;
};

struct SystemRam
{
    uint data;
};

RWStructuredBuffer<Register> Registers; //0x00-0x0F regular registers, 0x10 = PC, 0x11 = INDEXER, 0x12 = HALT, 0x13 = SPEED, 0x14 = RESET, 0x15 = CURR_OPCODE
RWStructuredBuffer<SystemRam> RAM; //0x00 - 0xFFF
RWStructuredBuffer<SystemRam> VRAM; //0x00 - ?
RWStructuredBuffer<SystemRam> STACK; //could be part of RAM above 4096 '\_(-_- )_/`

opcode NOP() {

};

opcode CLR() {

};

opcode Handle0()
{
    //load OPCODE again for remainder bits

};

static opcode opCode0Table[0x01] = {
    NOP()
};

static opcode opCodeTable[0xF] = { //0x0000 - 0xF000
    Handle0(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP(),
    NOP(), NOP(), NOP(), NOP()
};
