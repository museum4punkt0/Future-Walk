/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/
using UnityEngine;
using System;

public class Sampler
{
    private double position = 0;
    private float[] samples;

    private double[] hanningWindow;
    private double[] sineWindow;
    private const int windowSize = 1024;
    GranularSynth synth;
    private int clipChannelCount;

    public Sampler(GranularSynth gSynth)
    {
        synth = gSynth;

        hanningWindow = new double[windowSize];

        for (int i = 0; i < windowSize; i++)
        {
            hanningWindow[i] = hanning(i);
        }

        sineWindow = new double[windowSize];

        for (int i = 0; i < windowSize; i++)
        {
            sineWindow[i] = Mathf.Sin(Mathf.PI * i / windowSize);
        }
    }

    public double getNextSampleIndex()
    {
        double current = position;
        position += (synth.sharedInfo.playbackSpeed * synth.sharedInfo.speedFactor);

        if (position >= synth.sharedInfo.sampleLength)
        {
            position = 0;
        }
        else if (position < 0)
        {
            position = synth.sharedInfo.sampleLength - 1;
        }

        return current;
    }
    public void setAudioClip(AudioClip newClip, string path)
    {
        synth.sharedInfo.filePath = path;
        synth.sharedInfo.speedFactor = (float)newClip.frequency / AudioSettings.outputSampleRate;
        synth.sharedInfo.sampleLength = newClip.samples;

        // load data into array
        samples = new float[newClip.samples * newClip.channels];
        newClip.GetData(samples, 0);

        clipChannelCount = newClip.channels;

        // reset the sample playback position
        position = 0;
    }

    public int getSampleChannelCount()
    {
        return clipChannelCount;
    }

    double hanning(int n)
    {
        return (double)(0.5 - 0.5 * Mathf.Cos(2* Mathf.PI * n / (windowSize - 1)));
    }

    public double getInterpolatedGrainValue(SampleLocationInfo locationInfo, bool isLeftChannel)
    {
        double smoothingVal = sineWindow[(int)(windowSize * locationInfo.completionRatio)];
        return getInterpolatedSampleValue(locationInfo.position, isLeftChannel) * smoothingVal;
    }

    public double getInterpolatedSampleValue(double position, bool isLeftChannel)
    {
        int previousIndex = (int)Math.Floor(position);
        double t = position - previousIndex;

        if (isLeftChannel)
        {
            double x1 = (previousIndex < 1) ? samples[previousIndex * clipChannelCount] :
                                                samples[(previousIndex - 1) * clipChannelCount];
            double x2 = samples[previousIndex * clipChannelCount];

            double x3 = (previousIndex >= synth.sharedInfo.sampleLength - 1) ? samples[(synth.sharedInfo.sampleLength - 1) * clipChannelCount] : samples[(previousIndex + 1) * clipChannelCount];
            double x4 = (previousIndex >= synth.sharedInfo.sampleLength - 2) ? samples[(synth.sharedInfo.sampleLength - 1) * clipChannelCount] : samples[(previousIndex + 2) * clipChannelCount];

            // return optimal4p3o(x1, x2, x3, x4, t);
            return InterpolateHermite4pt3oX(x1, x2, x3, x4, t);
        }
        else
        {
            double x5 = (previousIndex < 1) ? samples[previousIndex * clipChannelCount + 1] : samples[(previousIndex - 1) * clipChannelCount + 1];
            double x6 = samples[previousIndex * 2 + 1];

            double x7 = (previousIndex >= synth.sharedInfo.sampleLength - 1) ? samples[(synth.sharedInfo.sampleLength - 1) * clipChannelCount + 1] : samples[(previousIndex + 1) * clipChannelCount + 1];
            double x8 = (previousIndex >= synth.sharedInfo.sampleLength - 2) ? samples[(synth.sharedInfo.sampleLength - 1) * clipChannelCount + 1] : samples[(previousIndex + 2) * clipChannelCount + 1];

            // return optimal4p3o(x5, x6, x7, x8, t);
            return InterpolateHermite4pt3oX(x5, x6, x7, x8, t);
        }
    }

    public static double optimal4p3o(double y0, double y1, double y2, double y3, double x)
    {
        double z = x - 0.5;
        double even1 = y2+y1, odd1 = y2-y1;
        double even2 = y3+y0, odd2 = y3-y0;
        double c0 = even1*0.45645918406487612 + even2*0.04354173901996461;
        double c1 = odd1*0.47236675362442071 + odd2*0.17686613581136501;
        double c2 = even1*-0.253674794204558521 + even2*0.25371918651882464;
        double c3 = odd1*-0.37917091811631082 + odd2*0.11952965967158000;
        double c4 = even1*0.04252164479749607 + even2*-0.04289144034653719;
        return (((c4*z+c3)*z+c2)*z+c1)*z+c0;
    }

    // not used currently, need to see which one performs better
    public static double InterpolateHermite4pt3oX(double x0, double x1, double x2, double x3, double t)
    {
        double c0 = x1;
        double c1 = 0.5 * (x2 - x0);
        double c2 = x0 - (2.5 * x1) + (2 * x2) - (0.5 * x3);
        double c3 = (.5 * (x3 - x0)) + (1.5 * (x1 - x2));
        return (((((c3 * t) + c2) * t) + c1) * t) + c0;
    }
}
