/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/
using UnityEngine;

public class Waves
{
    float[] sinewave;
    float[] triangle;

    float[] up;
    float[] down;

    static public int tableSize = 4096;

    public Waves()
    {
        sinewave = new float[tableSize];
        triangle = new float[tableSize];

        for (int i = 0; i < tableSize; ++i)
        {   float factor = i / (float)tableSize;
            int center = tableSize / 2;
            if (i < center)
            {
                triangle[i] = i / (float)center;
            }
            else
            {
                triangle[i] = triangle[tableSize - i - 1];
            }

            sinewave[i] = Mathf.Sin(2.0f * Mathf.PI * factor);
        }
    }

    public float getSine(int position)
    {
        return sinewave[position];
    }

    public float getRect(int position)
    {
        return (position < tableSize * 0.5) ? 1 : 0;
    }

    public float getSawUp(int position)
    {
        return position / (float)tableSize;
    }

    public float getSawDown(int position)
    {
        return 1.0f - getSawUp(position);
    }

    public float getTriangle(int position)
    {
        return triangle[position];
    }

    public float getRandom()
    {
        return GranularSynth.RandomNumber(0, 1000) * 0.001f;
    }
}

public class LFO
{
    Waves waveforms;

    // Hz - number of cycles per second
    // samples to move in the table per sample
    float speed = 0;
    WaveType type = WaveType.Sine;

    // current position in the wave table
    float currentPosition = 0;

    float sampleRateFactor = 1F;

    public LFO(Waves waves)
    {
        sampleRateFactor = 48000F / (float)AudioSettings.outputSampleRate;
        waveforms = waves;
    }

    public void setWaveType(WaveType newType)
    {
        type = newType;
    }

    public void setRate(float newRateinHZ)
    {
        speed = newRateinHZ * Waves.tableSize / (float)AudioSettings.outputSampleRate;
    }

    public void update(int samplesOffset)
    {
        currentPosition += sampleRateFactor * speed * samplesOffset;
        if (currentPosition >= Waves.tableSize)
            currentPosition = 0;
    }

    public float getControlData()
    {
        int roundedPos = (int)currentPosition;
        switch (type)
        {
            case WaveType.Sine:
            return waveforms.getSine(roundedPos);

            case WaveType.SawUp:
            return waveforms.getSawUp(roundedPos);

            case WaveType.SawDown:
            return waveforms.getSawDown(roundedPos);

            case WaveType.Rect:
            return waveforms.getRect(roundedPos);

            case WaveType.Triangle:
            return waveforms.getTriangle(roundedPos);

            case WaveType.Random:
            return waveforms.getRandom();

            default:
            return 0;
        }
    }
}
