using FrogDataLib.DataManagement;
using System;

namespace SaveMyFit;

[Serializable]
public class SavedFit : FrogDataModel
{
    public int FitNumber = -1;
    public string FitName = string.Empty;
}
