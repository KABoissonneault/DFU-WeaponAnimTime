using System;
using UnityEngine;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

public class WeaponAnimTimeMod : MonoBehaviour
{
    static Mod mod;
    static WeaponAnimTimeMod instance;

    float ScaleFactor;
    float BaseValue;
    bool RoleplayRealismItemsWeaponBalance;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        instance = new GameObject(mod.Title).AddComponent<WeaponAnimTimeMod>();
        mod.LoadSettingsCallback = LoadSettings;

        mod.IsReady = true;
    }

    void Start()
    {
        Debug.Log("Begin mod init: Kab's Scalable Speed");

        mod.LoadSettings();

        if (RoleplayRealismItemsWeaponBalance)
            Debug.Log("Using Roleplay and Realism:Items's Weapon Balance");

        FormulaHelper.RegisterOverride<Func<PlayerEntity, WeaponTypes, ItemHands, float>>(mod, "GetMeleeWeaponAnimTime", GetMeleeWeaponAnimTime);

        Debug.Log("Finished mod init: Kab's Scalable Speed");
    }

    static void LoadSettings(ModSettings settings, ModSettingsChange change)
    {
        instance.ScaleFactor = settings.GetFloat("Core", "ScaleFactor") / 100f;
        instance.BaseValue = settings.GetInt("Core", "BaseValue");
        instance.RoleplayRealismItemsWeaponBalance = RoleplayRealismItemsWeaponBalanceCompatibility();
    }

    static bool RoleplayRealismItemsWeaponBalanceCompatibility()
    {
        Mod roleplayRealismItems = ModManager.Instance.GetMod("RoleplayRealism-Items");
        if (roleplayRealismItems == null)
            return false;

        ModSettings rrModSettings = roleplayRealismItems.GetSettings();
        return rrModSettings.GetBool("Modules", "weaponBalance");
    }

    float GetMeleeWeaponAnimTime(PlayerEntity player, WeaponTypes weaponType, ItemHands ignore2)
    {
        int playerSpeed = player.Stats.LiveSpeed;

        if(RoleplayRealismItemsWeaponBalance)
        {
            playerSpeed = GetRoleplayRealismItemsWeaponBalanceAdjustedSpeed(playerSpeed, player, weaponType);
        }

        float tickTime = BaseValue * Mathf.Pow(1 - ScaleFactor, playerSpeed - 50);
        return tickTime / FormulaHelper.classicFrameUpdate;
    }

    static int GetRoleplayRealismItemsWeaponBalanceAdjustedSpeed(int baseSpeed, PlayerEntity player, WeaponTypes weaponType)
    {
        EquipSlots weaponSlot = GameManager.Instance.WeaponManager.UsingRightHand ? EquipSlots.RightHand : EquipSlots.LeftHand;
        DaggerfallUnityItem weapon = player.ItemEquipTable.GetItem(weaponSlot);

        // Hand-to-hand keeps its whole speed
        if (weaponType == WeaponTypes.Melee || weapon == null)
            return baseSpeed;

        float weaponWeight = weapon.ItemTemplate.baseWeight; // Seems like the Feather Weight weapon enchant won't affect this formula
        int strWeightPerc = 150 - player.Stats.LiveStrength;
        float adjustedWeight = strWeightPerc * weaponWeight / 100f;
        float speedReductionPerc = adjustedWeight * 3.4f / 90f; // Magic numbers taken from R&R:I
        return (int)(baseSpeed * (1 - speedReductionPerc));
    }
}
