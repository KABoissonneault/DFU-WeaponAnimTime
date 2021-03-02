using System;
using UnityEngine;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

public class WeaponAnimTimeMod : MonoBehaviour
{
    static Mod mod;

    float ScaleFactor;
    float BaseValue;

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
        FormulaHelper.RegisterOverride<Func<PlayerEntity, WeaponTypes, ItemHands, float>>(mod, "GetMeleeWeaponAnimTime", GetMeleeWeaponAnimTime);

        Debug.Log("Finished mod init: Kab's Scalable Speed");
    }

    float GetMeleeWeaponAnimTime(PlayerEntity player, WeaponTypes ignore, ItemHands ignore2)
    {
        float speed = BaseValue * Mathf.Pow(1 - ScaleFactor, player.Stats.LiveSpeed - 50);
        return speed / FormulaHelper.classicFrameUpdate;
    }
}
