using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FrogDataLib.DataManagement;
using HarmonyLib;
using YAPYAP;

namespace SaveMyFit
{
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        private static FrogDataContainer<SavedFit> FitContainer;
        private static SavedFit MySavedFit = new();

        private static string GetFitName
        {
            get
            {
                if (GetFitIndex < 0)
                    return string.Empty;

                return Pawn.LocalInstance.PawnMaterial.costumeLibrary.costumes[GetFitIndex].costumeName;
            }
        }

        private static int GetFitIndex
        {
            get
            {
                if (Pawn.LocalInstance == null || Pawn.LocalInstance.PawnMaterial == null)
                    return -1;

                return Pawn.LocalInstance.PawnMaterial.CurrentCostumeIndex;
            }  
        }

        private void Awake()
        {
            Log = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin {Name} is loaded!");

            FitContainer = new FrogDataContainer<SavedFit>("com.darmuh.SaveMyFit");

            // Subscribe to FrogDataLib events
            FrogDataManager.OnBeginSaving += SaveMyData;
            FrogDataManager.OnLoadCompleted += LoadMyData;
        }

        private static void SaveMyData()
        {
            Log.LogMessage($"Saving Fit to save!");
            MySavedFit.FitNumber = GetFitIndex;
            MySavedFit.FitName = GetFitName;
            FitContainer.SaveModData(MySavedFit);
            Log.LogMessage($"Name: {MySavedFit.FitName}\nIndex: {MySavedFit.FitNumber}");
        }

        private static void LoadMyData()
        {
            Log.LogMessage($"Fit updated from save!");
        }

        [HarmonyPatch(typeof(PawnMaterial), nameof(PawnMaterial.OnStartClient))]
        public class PawnMaterialStartClient
        {
            public static void Postfix(PawnMaterial __instance)
            {
                SetMyOutfit(__instance);
            }
        }

        private static void SetMyOutfit(PawnMaterial pawnMaterial)
        {
            if (MySavedFit.FitNumber < 0)
            {
                Log.LogWarning("No saved fit to set!");
                return;
            }

            if (pawnMaterial.costumeLibrary.costumes.Length <= MySavedFit.FitNumber)
            {
                Log.LogWarning($"Costumes list does not contain our FitNumber [ {MySavedFit.FitNumber} ]");
                return;
            } 

            if (pawnMaterial.costumeLibrary.costumes[MySavedFit.FitNumber].costumeName != MySavedFit.FitName)
            {
                Log.LogWarning($"""
                    Saved fit appears to have a mismatched name!
                    Fit at index {MySavedFit.FitNumber}: {pawnMaterial.costumeLibrary.costumes[MySavedFit.FitNumber].costumeName}
                    Expected Fit: {MySavedFit.FitName}
                    
                    """);
                return;
            }

            pawnMaterial.ApplyCostume(MySavedFit.FitNumber);
            Log.LogMessage($"Set costume to saved fit {MySavedFit.FitName}");
        }
    }
}
