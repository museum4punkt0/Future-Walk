/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com'
*/

public enum WaveType {Sine, SawUp, SawDown, Rect, Triangle, Random}
public enum LFOType {None = -1, LFO_1, LFO_2};
public enum HarmonyType {Major3rd, Minor3rd, Perfect4th, Perfect5th, Major6th, Minor6th, Major7th, Minor7th, Octave}
public class ControlData
{
    public float scanPosition = 0;
    private int scanSamples;
    private GranularSynth synth;

    Waves waveforms = new Waves();
    LFO[] lfos;
    int lfoCount = 2;
    public ControlData(GranularSynth gSynth)
    {
        synth = gSynth;

        lfos = new LFO[lfoCount];
        for (int i = 0; i < lfoCount; ++i)
        {
            lfos[i] = new LFO(waveforms);
        }

        updateLFOSettings();
    }

    public void update(int sampleSize)
    {
        scanPosition += sampleSize * 100 / synth.sharedInfo.scanTime;

        // check with b: stick to the end of loop the interval?
        if (scanPosition >= scanSamples)
            scanPosition -= scanSamples;

        foreach (LFO l in lfos)
        {
            l.update(sampleSize);
        }
    }
    public void updateLFOSettings()
    {
        scanSamples = (int)(synth.sharedInfo.sampleLength * synth.sharedInfo.scanDistance);
        lfos[0].setRate(synth.sharedInfo.LFO1Rate);
        lfos[0].setWaveType(synth.sharedInfo.LFO1Type);

        lfos[1].setRate(synth.sharedInfo.LFO2Rate);
        lfos[1].setWaveType(synth.sharedInfo.LFO2Type);
    }

    public float getLFO(LFOType index)
    {
        if (index == LFOType.None)
            return 0;
        else
            return lfos[(int)index].getControlData();
    }

    public void setLFOWaveType(int index, WaveType type)
    {
        lfos[index].setWaveType(type);
    }

    public void setLFORate(int index, float rate)
    {
        lfos[index].setRate(rate);
    }
}
