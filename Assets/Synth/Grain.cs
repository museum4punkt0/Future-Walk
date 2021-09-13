/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/

using UnityEngine;
using System;
public class Grain
{
    // inital position in sample
    public int mStartPos;

    public int mGrainSize;
    private double mCurrentIndex = 0;
    private bool mIsValid = false;
    private double mPlaybackSpeed;
    private GranularSynth synth;
    private SharedGrainInfo mGrainInfo;

    public Grain(SharedGrainInfo grainInfo, GranularSynth gSynth)
    {
        mGrainInfo = grainInfo;
        synth = gSynth;
    }

    double getHarmonyFactor(HarmonyType type)
    {
        switch (type)
        {
            case HarmonyType.Major3rd:
            return 1.25992f;
            case HarmonyType.Minor3rd:
            return 1.1892f;
            case HarmonyType.Perfect4th:
            return 1.33484f;
            case HarmonyType.Perfect5th:
            return 1.4983f;
            case HarmonyType.Major6th:
            return 1.68179f;
            case HarmonyType.Minor6th:
            return 1.5874f;
            case HarmonyType.Major7th:
            return 1.88774f;
            case HarmonyType.Minor7th:
            return 1.78179f;
            case HarmonyType.Octave:
            return 2;
        }
        return 1;
    }
    public void updateIfNeeded(double detuneAmt)
    {
        if (!mIsValid)
        {
            int randomHarmoRate = (int)(100 * (1.0f - synth.sharedInfo.addHarmony)) - 1;
            bool shouldAddHarmo = GranularSynth.RandomNumber(0, 100) > randomHarmoRate;
            double harmony = getHarmonyFactor(synth.sharedInfo.harmonyType);


            double playbackSpeed = (mGrainInfo.playbackSpeed * (shouldAddHarmo ? harmony : 1.0f) + detuneAmt) * mGrainInfo.speedFactor;
            if (playbackSpeed < 0.1 && playbackSpeed > -0.1)
            {
                playbackSpeed = (playbackSpeed > 0) ? 0.1f : -0.1f;
            }
            mPlaybackSpeed = playbackSpeed;

            double grainSizeLFOAmount = synth.controlData.getLFO(mGrainInfo.grainSizeLFO);

            mGrainSize = Mathf.Min(Mathf.Max((int)(mGrainInfo.grainSize * (1 + grainSizeLFOAmount * mGrainInfo.grainSizeLFOAmount)), 100),
                                   mGrainInfo.sampleLength);

            int sampleLength = mGrainInfo.sampleLength;
            double startPosLFOAmt = synth.controlData.getLFO(mGrainInfo.startPosLFO) * mGrainInfo.startPosLFOAmount * sampleLength;

            mStartPos = Mathf.Min(Mathf.Max((int)(mGrainInfo.startPos * sampleLength
                                    + GranularSynth.RandomNumber(0, mGrainInfo.randomStartPos)
                                    + startPosLFOAmt
                                    + (synth.sharedInfo.scanOn ? synth.controlData.scanPosition : 0)),
                                    0),
                                   sampleLength - 1
                                );

            if (mPlaybackSpeed > 0)
            {
                if (mStartPos > sampleLength - mGrainSize)
                {
                    mStartPos = sampleLength - mGrainSize;
                }
            }
            else
            {
                if (mStartPos - mGrainSize < 0)
                {
                    mStartPos = mGrainSize - 1;
                }
            }

            mCurrentIndex = 0;
            mIsValid = true;
        }
    }

    public bool isValid()
    {
        return mIsValid;
    }

    public void invalidate()
    {
        mIsValid = false;
    }
    public void getLocationInfo(SampleLocationInfo output)
    {
        output.position = mStartPos + mCurrentIndex;
        output.completionRatio = Math.Abs(mCurrentIndex) / (double)mGrainSize;

        mCurrentIndex += mPlaybackSpeed;

        if (Math.Abs(mCurrentIndex) >= mGrainSize - 1 /*index is zero-based but grainsize isn't*/)
        {
            mIsValid = false;
        }
    }
}
