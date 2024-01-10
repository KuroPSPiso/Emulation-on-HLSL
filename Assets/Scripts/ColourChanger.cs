#pragma warning disable CS0108

using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ColourChanger : MonoBehaviour
{
    public Material mat;
    public int colourIndex = 0;
    public Color Darkest, Dark, Light, Lightest;

    public Color[] DarkestPalette;
    public Color[] DarkPalette;
    public Color[] LightPalette;
    public Color[] LightestPalette;

    private MeshRenderer renderer;

    private void Start()
    {
        renderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (colourIndex >= DarkestPalette.Length) colourIndex = DarkestPalette.Length - 1;

        renderer.material.SetColor("_Darkest", DarkestPalette[colourIndex]);
        renderer.material.SetColor("_Dark", DarkestPalette[colourIndex]);
        renderer.material.SetColor("_Light", DarkestPalette[colourIndex]);
        renderer.material.SetColor("_Lightest", DarkestPalette[colourIndex]);
    }
}