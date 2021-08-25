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

    float ScaleFactor;
    float BaseValue;
    bool RoleplayRealismItemsWeaponBalance;

    [Invoke(DaggerfallWorkshop.Game.StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        new GameObject(mod.Title).AddComponent<WeaponAnimTimeMod>();
    }

    void Awake()
    {
        Debug.Log("Begin mod init: Kab's Scalable Speed");
        
        ModSettings settings = mod.GetSettings();
        ScaleFactor = settings.GetFloat("Core", "ScaleFactor") / 100f;
        BaseValue = settings.GetInt("Core", "BaseValue");
        RoleplayRealismItemsWeaponBalance = RoleplayRealismItemsWeaponBalanceCompatibility(settings);
        if (RoleplayRealismItemsWeaponBalance)
            Debug.Log("Using Roleplay and Realism:Items's Weapon Balance");
        FormulaHelper.RegisterOverride<Func<PlayerEntity, WeaponTypes, ItemHands, float>>(mod, "GetMeleeWeaponAnimTime", GetMeleeWeaponAnimTime);

        Debug.Log("Finished mod init: Kab's Scalable Speed");
    }

    static bool RoleplayRealismItemsWeaponBalanceCompatibility(ModSettings modSettings)
    {
        if (!modSettings.GetBool("Compatibility", "RoleplayRealismItems-WeaponBalance"))
            return false;

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
