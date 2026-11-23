using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class Recoil(BotComponent sain) : BotBase(sain)
{
    public float ArmInjuryModifier
    {
        get { return calcModFromInjury(Bot.Medical.HitReaction.LeftArmInjury) * calcModFromInjury(Bot.Medical.HitReaction.RightArmInjury); }
    }

    private static bool _debugRecoilLogs
    {
        get { return SAINPlugin.DebugSettings.Logs.DebugRecoilCalculations; }
    }

    private static float _recoilDecayCoef
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.Shoot.BOT_RECOIL_DECAY_COEF; }
    }

    private bool _recoilFinished;
    private bool _armsInjured
    {
        get { return Bot.Medical.HitReaction.ArmsInjured; }
    }

    private float RecoilMultiplier
    {
        get { return Mathf.Round(Bot.Info.FileSettings.Shoot.RecoilMultiplier * GlobalSettings.Shoot.BOT_RECOIL_COEF * 100f) / 100f; }
    }

    public override void Init()
    {
        PlayerComponent.OnShoot += WeaponShot;
        base.Init();
    }

    public override void Dispose()
    {
        PlayerComponent.OnShoot -= WeaponShot;
        base.Dispose();
    }

    public void CalcRecoilDecay()
    {
        float now = Time.time;
        if (!_recoilFinished)
        {
            // COMET port (liquidwarp RecoilStep + SignedRecoil): the old step applied one fixed-step decay per CALL, so the
            // decay speed followed the frame rate; and "<= 0.001" on signed angles ended recoil early for negative
            // (left / down) angles. Now: same per-fixed-step factor, raised to elapsed/fixedDeltaTime, and |angle| checks.
            // Real-time decay: the rate is defined per SECOND (reference step 0.02 s = EFT's physics step, 0.3 coef -> ~2.3 s
            // half-life, matching the "~2 s half-life" design note below). No dependence on frame rate or Time.fixedDeltaTime.
            const float ReferenceStep = 0.02f;
            float elapsed = _lastDecayTime < 0f ? 0f : Mathf.Clamp(now - _lastDecayTime, 0f, 0.25f);
            float retention = Mathf.Clamp01(1f - ReferenceStep * _recoilDecayCoef * SettleMultiplier());
            float factor = elapsed <= 0f ? 1f : Mathf.Pow(retention, elapsed / ReferenceStep);
            _currentRecoilHorizAngle *= factor;
            _currentRecoilVertAngle *= factor;
            if (Mathf.Abs(_currentRecoilHorizAngle) <= 0.001f && Mathf.Abs(_currentRecoilVertAngle) <= 0.001f)
            {
                _recoilFinished = true;
                _currentRecoilHorizAngle = 0f;
                _currentRecoilVertAngle = 0f;
            }
        }
        _lastDecayTime = now;
    }

    private float _lastDecayTime = -1f;

    // zzap (2026-10-13, user: "DMR / sniper bots hit less at long range than bots with assault rifles"): a shot's recoil is an
    // aim offset (angle) that decays with a ~2s half-life, and it is scaled by the weapon's recoil times its "shootability"
    // (sniper ~1.5x, DMR ~1.3x, AR 1.0x). Long-range fire waits a long time between shots, but not long enough for that - so
    // the next deliberate shot still carried most of a big-recoil offset, and an angle error grows with distance. A shooter
    // with a DMR / sniper re-settles on the target between single shots: their offset decays 3x faster (close-range rapid
    // fire barely changes - 0.3s between shots keeps ~70% instead of ~90%).
    private const float SETTLE_MULTI_MARKSMAN = 3f;

    private float SettleMultiplier()
    {
        var weaponClass = Bot.Info.WeaponInfo?.EWeaponClass;
        return weaponClass == SAIN.Preset.Shared.Enums.EWeaponClass.sniperRifle || weaponClass == SAIN.Preset.Shared.Enums.EWeaponClass.marksmanRifle
            ? SETTLE_MULTI_MARKSMAN
            : 1f;
    }

    public void WeaponShot(WeaponInfo WeaponInfo, Vector3 force)
    {
        if (Bot.IsCheater)
        {
            //Logger.LogDebug("cheato");
            return;
        }
        calculateRecoil(WeaponInfo.Weapon);
    }

    private float _currentRecoilHorizAngle;
    private float _currentRecoilVertAngle;

    public Vector3 ApplyRecoil(Vector3 lookdirection)
    {
        return Vector.Rotate(lookdirection, _currentRecoilHorizAngle, _currentRecoilVertAngle, 0f);
    }

    private void calculateRecoil(Weapon weapon)
    {
        if (weapon == null)
        {
#if DEBUG
            Logger.LogError("Weapon Null!");
#endif
            return;
        }

        _recoilFinished = false;
        float addRecoil = SAINPlugin.LoadedPreset.GlobalSettings.Shoot.BOT_RECOIL_ADD;
        float recoilMod = calcRecoilMod();
        float recoilTotal = weapon.RecoilTotal;
        float recoilNum = calcRecoilNum(recoilTotal) + addRecoil;
        float calcdRecoil = recoilNum * recoilMod;

        _currentRecoilVertAngle = Random.Range(calcdRecoil / 3f, calcdRecoil) * randomSign();
        _currentRecoilHorizAngle = Random.Range(calcdRecoil / 3f, calcdRecoil) * randomSign();

#if DEBUG
        if (_debugRecoilLogs)
        {
            Logger.LogDebug(
                $"Recoil! New Recoil: [{_currentRecoilVertAngle}:{_currentRecoilHorizAngle}] "
                    + $"recoilNum: [{recoilNum}] calcdRecoil: [{calcdRecoil}] : "
                    + $"Modifiers [ Add: [{addRecoil}] Multi: [{recoilMod}] Weapon RecoilTotal [{recoilTotal}]] Shoot Modifier: [{Bot.Info.WeaponInfo.FinalModifier}]"
            );
        }
#endif
    }

    private float randomSign()
    {
        return EFTMath.RandomBool() ? -1 : 1;
    }

    private float calcModFromInjury(EInjurySeverity severity)
    {
        switch (severity)
        {
            default:
                return 1f;

            case EInjurySeverity.Injury:
                return 1.15f;

            case EInjurySeverity.HeavyInjury:
                return 1.35f;

            case EInjurySeverity.Destroyed:
                return 1.65f;
        }
    }

    private float calcRecoilMod()
    {
        float recoilMod = 1f * RecoilMultiplier;

        if (Player.IsInPronePose)
        {
            recoilMod *= 0.7f;
        }
        else if (Player.Pose == EPlayerPose.Duck)
        {
            recoilMod *= 0.9f;
        }

        if (BotOwner.WeaponManager?.ShootController?.IsAiming == true)
        {
            recoilMod *= 0.9f;
        }
        if (Bot.Transform.VelocityData.VelocityMagnitudeNormal < 0.1f)
        {
            recoilMod *= 0.85f;
        }
        if (_armsInjured)
        {
            recoilMod *= Mathf.Sqrt(ArmInjuryModifier);
        }

        return recoilMod;
    }

    private float calcRecoilNum(float recoilVal)
    {
        float result = recoilVal / _shootSettings.BOT_RECOIL_BASELINE;
        if (ModDetection.RealismLoaded)
        {
            result = recoilVal / _shootSettings.BOT_RECOIL_BASELINE_REALISM;
        }
        result *= shootModClamped();
        result *= UnityEngine.Random.Range(0.8f, 1.2f);
        return result;
    }

    private float shootModClamped()
    {
        return Mathf.Clamp(_shootModifier, 0.5f, 2f);
    }

    private float _shootModifier
    {
        get { return Bot.Info.WeaponInfo.FinalModifier; }
    }

    private static ShootSettings _shootSettings
    {
        get { return GlobalSettingsClass.Instance.Shoot; }
    }
}
