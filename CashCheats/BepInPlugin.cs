using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace CashCheats
{
    [BepInPlugin("aedenthorn.CashCheats", "Cash Cheats", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<float> cashAddMult;
        public static ConfigEntry<float> cashLossMult;
        public static ConfigEntry<string> cashAddKey;
        public static ConfigEntry<int> cashAddAmount;

        public static void Dbgl(string str = "", LogLevel logLevel = LogLevel.Debug)
        {
            if (isDebug.Value)
                context.Logger.Log(logLevel, str);
        }
        private void Awake()
        {

            context = this;
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");
            cashAddMult = Config.Bind<float>("Options", "CashAddMult", 1f, "Multiply cash received by this amount");
            cashLossMult = Config.Bind<float>("Options", "CashLossMult", 1f, "Multiply cash lost by this amount");
            cashAddKey = Config.Bind<string>("Options", "CashAddKey", "[0]", "Key to add cash when pressed");
            cashAddAmount = Config.Bind<int>("Options", "CashAddAmount", 1000, "Cash amount to add on key press");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);

            Dbgl("Mod Loaded");
        }

        private void Update()
        {
            if (modEnabled.Value && AedenthornUtils.CheckKeyDown(cashAddKey.Value))
            {
                var cs = CareerStatus.Get();
                if (cs != null)
                {
                    Dbgl($"Adding ${cashAddAmount.Value}");
                    cs.AddCash(cashAddAmount.Value);
                }
            }
        }

        [HarmonyPatch(typeof(CareerStatus), nameof(CareerStatus.AddCash))]
        static class CareerStatus_AddCash_Patch
        {
            public static bool Prefix(ref int amount)
            {
                if (modEnabled.Value)
                    amount = (int)Math.Round(amount * cashAddMult.Value);
                return amount > 0;
            }
        }
        [HarmonyPatch(typeof(CareerStatus), nameof(CareerStatus.SpendCash))]
        static class CareerStatus_SpendCash_Patch
        {
            public static bool Prefix(ref int cash)
            {
                if (modEnabled.Value)
                    cash = (int)Math.Round(cash * cashLossMult.Value);
                return cash > 0;
            }
        }
    }
}
