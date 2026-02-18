using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using FrogDataLib.DataManagement;
using HarmonyLib;
using Mirror;
using YAPYAP;

namespace SaveMyFit
{
    [BepInDependency(FrogDataLib.FrogDataPlugin.Id)]
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        private static FrogDataContainer<SavedFits> FitContainer = new("com.darmuh.SaveMyFit");
        private static SavedFits FitCollection = new();

        private static int GetFitIndex(Pawn player)
        {
            if (player == null || player.PawnMaterial == null)
                return -1;

            return player.PawnMaterial.CurrentCostumeIndex;
        }

        internal static string GetFitName(int index)
        {
            if (index < 0 || UIManager.Instance == null || UIManager.Instance.costumeLibrary.costumes.Length <= index)
                return string.Empty;

            return UIManager.Instance.costumeLibrary.costumes[index].costumeName;
        }

        private void Awake()
        {
            Log = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin {Name} is loaded with version {Version}!");

            // Subscribe to FrogDataLib events
            FrogDataManager.OnBeginSaving += SaveMyData;
            FrogDataManager.OnLoadCompleted += LoadMyData;
            FrogDataManager.OnSessionEnded += ClearCache;
        }

        private static void SaveMyData()
        {
            Log.LogMessage($"Saving Fits to save!");
            foreach(var item in GameManager.Instance.playersByActorId)
            {
                FitCollection.SetFitFor(item.Value.PlayerId, GetFitIndex(item.Value));
            }

            //save collection
            FitContainer.SaveModData(FitCollection);
            Log.LogDebug($"======== Fit Info [ Indexes ] ========");
            foreach (var indexFo in FitCollection.FitNumbers)
            {
                Log.LogDebug($"{indexFo.Key} - {indexFo.Value}");
            }
            Log.LogDebug($"======================================");
            Log.LogDebug($"======== Fit Info [ Names ] ========");
            foreach (var indexFo in FitCollection.FitNames)
            {
                Log.LogDebug($"{indexFo.Key} - {indexFo.Value}");
            }
            Log.LogDebug($"======================================");
        }

        private static void LoadMyData()
        {
            Log.LogMessage($"Fits loaded from save!");
            FitCollection = FitContainer.GetModData();
            Log.LogDebug($"======== Fit Info [ Indexes ] ========");
            foreach(var indexFo in FitCollection.FitNumbers)
            {
                Log.LogDebug($"{indexFo.Key} - {indexFo.Value}");
            }
            Log.LogDebug($"======================================");
            Log.LogDebug($"======== Fit Info [ Names ] ========");
            foreach (var indexFo in FitCollection.FitNames)
            {
                Log.LogDebug($"{indexFo.Key} - {indexFo.Value}");
            }
            Log.LogDebug($"======================================");
        }

        private static void ClearCache()
        {
            FitCollection = new();
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.ServerSetPlayerId))]
        public class PawnStart
        {
            public static void Postfix(Pawn __instance, string id)
            {
                if (FitCollection.TryGetFitIndex(id, out int index) && FitCollection.TryGetFitName(id, out string name))
                {
                    SetPawnOutfit(__instance.PawnMaterial, index, name);
                }
                else
                    Log.LogMessage($"No saved fit for player with playerID - {id}");
            }
        }

        [HarmonyPatch(typeof(PawnMaterial), nameof(PawnMaterial.ApplyCostumeInternal))]
        public class CatchCostumeChange
        {
            public static void Postfix(PawnMaterial __instance, int costumeIndex)
            {
                //only run this patch on the host client
                if (NetworkClient.active && !NetworkServer.active)
                    return;

                if (!__instance.TryGetComponent<Pawn>(out var pawn))
                    return;

                FitCollection.SetFitFor(pawn.PlayerId, costumeIndex);
                FitContainer.SaveModData(FitCollection);
            }
        }

        private static void SetPawnOutfit(PawnMaterial pawnMaterial, int fitIndex, string fitName)
        {
            if (fitIndex < 0)
            {
                Log.LogWarning("No saved fit to set!");
                return;
            }

            if (pawnMaterial.costumeLibrary.costumes.Length <= fitIndex)
            {
                Log.LogWarning($"Costumes list does not contain the FitNumber [ {fitIndex} ]");
                return;
            } 

            if (pawnMaterial.costumeLibrary.costumes[fitIndex].costumeName != fitName)
            {
                Log.LogWarning($"""
                    Saved fit appears to have a mismatched name!
                    Fit at index {fitIndex}: {pawnMaterial.costumeLibrary.costumes[fitIndex].costumeName}
                    Expected Fit: {fitName}
                    
                    """);
                return;
            }

            pawnMaterial.ApplyCostume(fitIndex);
            Log.LogMessage($"Set costume to saved fit {fitName}");
        }
    }
}
