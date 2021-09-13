/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/
using UnityEngine;


[System.Serializable]
public class SharedGrainInfo
{
    [Range(0.0f, 5)]
    public float gain = 1.0f;

    [Range(-1.0f, 2.0f)]
    public float playbackSpeed = 1;

    [Range(0.0f, 1.0f)]
    public float samplerWet = 1.0f;

    [Range(0.0f, 40.0f)]
    // Hz
    public float LFO1Rate = 1;
    public WaveType LFO1Type = WaveType.Sine;

    [Range(0.0f, 40.0f)]
    public float LFO2Rate = 1;
    public WaveType LFO2Type = WaveType.Sine;

    [Range(0.0f, 10.0f)]
    public float spread;

    [Range(100f, 96000f)]
    public int grainSize = 5000;
    public LFOType grainSizeLFO = LFOType.None;

    // percentage
    [Range(0.0f, 1.0f)]
    public float grainSizeLFOAmount = 0;

    [Range(0.0f, 1.0f)]
    public float startPos = 0.3f;
    public LFOType startPosLFO = LFOType.None;

    // percentage
    [Range(0.0f, 1.0f)]
    public float startPosLFOAmount = 0;

    [Range(0, 10000)]
    public int randomStartPos = 200;

    [Range(0, 5000)]
    public int randomizeMorphAmt = 0;
    // the amount of crossfade between grains
    [Range(-0.5f, 1.0f)]
    public float morphAmount = 0.5f;

    [Range(0.0f, 1.0f)]
    public float addHarmony = 0.0f;
    public HarmonyType harmonyType = HarmonyType.Perfect5th;

    [SerializeField]
    public string filePath;

    public bool scanOn = false;

    [Range(0.01f, 1.0f)]
    public float scanDistance = 0.2f;

    [Range(25, 5000)]
    public float scanTime = 1500.0f;

    [System.NonSerialized]
    public float speedFactor;

    [System.NonSerialized] public int sampleLength;
}

public class SampleLocationInfo
{
    public double position {get;set;}

    public double completionRatio {get;set;}
}