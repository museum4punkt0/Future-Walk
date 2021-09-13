/*
Granular synth developed by Mei-Fang Liau 2020
floatingspectrum@protonmail.com
*/
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static void SaveSettings(SharedGrainInfo info, string path)
    {
        string json = JsonUtility.ToJson(info, true);
        File.WriteAllText(path, json);
    }

    public static SharedGrainInfo LoadSettings(GranularSynth synth, string path)
    {
        return JsonUtility.FromJson<SharedGrainInfo>(File.ReadAllText(path));
    }
}
