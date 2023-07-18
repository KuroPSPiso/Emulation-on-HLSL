struct SystemRam
{
    uint data;
};

float ResolutionX;
float ResolutionY;

RWStructuredBuffer<SystemRam> VRAM; //0x00 - ?

uint SetPixel(uint x, uint y)
{
    uint resX = (uint) round(ResolutionX);
    uint resY = (uint) round(ResolutionY);

    //normalise values
    if (x > resX) x -= resX;
    else if (x < 0) x += resX;
    if (y > resY) y -= resY;
    else if (y < 0) y += resY;

    return !(VRAM[x + (y * resX)].data ^= 1);
}

uint GetPixel(float x, float y, float width)
{
    uint posX = (uint) round(x);
    uint posY = (uint) round(y) * (uint) round(width);
    return VRAM[posX + posY].data;
}