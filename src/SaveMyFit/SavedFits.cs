using System;
using System.Collections.Generic;
using FrogDataLib.DataManagement;

namespace SaveMyFit;

[Serializable]
public class SavedFits : FrogDataModel
{
    public Dictionary<string, int> FitNumbers = [];
    public Dictionary<string, string> FitNames = [];


    public bool TryGetFitIndex(string playerID, out int index)
    {
        index = -1;

        if (string.IsNullOrEmpty(playerID)) return false;

        return FitNumbers.TryGetValue(playerID, out index);
    }

    public bool TryGetFitName(string playerID, out string name)
    {
        name = string.Empty;

        if (string.IsNullOrEmpty(playerID)) return false;

        return FitNames.TryGetValue(playerID, out name);
    }

    public void SetFitFor(string playerID, int index)
    {
        //this is unfortunately something that needs checking
        if (string.IsNullOrEmpty(playerID))
            return;

        if (!FitNumbers.TryAdd(playerID, index))
            FitNumbers[playerID] = index;

        string fitName = Plugin.GetFitName(index);

        if (!FitNames.TryAdd(playerID, fitName))
            FitNames[playerID] = fitName;

        Plugin.Log.LogDebug($"Fit updated for {playerID}");
        Plugin.Log.LogDebug($"{index} - {fitName}");
    }
}
