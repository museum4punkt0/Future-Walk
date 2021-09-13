/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Channel
{
    private int currentSamples = 0;
    private int nextGrainIndex = 0;
    private int nextTriggerIntervalInSamples = 0;
    private GranularSynth synth;

    private Grain[] grains;
    SampleLocationInfo mLocationInfo = new SampleLocationInfo();

    const int maxGrainCount = 2;
    private bool isLeftChannel = true;

    public Channel(bool isLeft, GranularSynth gSynth)
    {
        isLeftChannel = isLeft;
        synth = gSynth;
        grains = new Grain[maxGrainCount];
        for (int i = 0; i < grains.Length; ++i)
        {
            grains[i] = new Grain(synth.sharedInfo, synth);
        }
    }

    public void reset()
    {
        foreach (Grain g in grains)
        {
            g.invalidate();
        }
    }

    public double getNextSample()
    {
        if (currentSamples >= nextTriggerIntervalInSamples)
        {
            Grain g = grains[nextGrainIndex];
            if (!g.isValid())
            {
                currentSamples = 0;

                // todo: avoid extra calculation here by observing the spread change and only calculate it when changed
                double detuneAmt = (isLeftChannel ? synth.sharedInfo.spread : -synth.sharedInfo.spread) * 0.001;
                g.updateIfNeeded(detuneAmt);
                nextGrainIndex = (++nextGrainIndex) % maxGrainCount;
                nextTriggerIntervalInSamples = (int)(g.mGrainSize * (1.0 - synth.sharedInfo.morphAmount)
                                                    + GranularSynth.RandomNumber(-synth.sharedInfo.randomizeMorphAmt, synth.sharedInfo.randomizeMorphAmt));
            }
        }
        else
        {
            ++currentSamples;
        }

        double sum = 0;
        foreach (Grain g in grains)
        {
            if (g.isValid())
            {
                g.getLocationInfo(mLocationInfo);
                sum += synth.getInterpolatedGrainValue(mLocationInfo, isLeftChannel);
            }
        }

        return sum;
    }
}
