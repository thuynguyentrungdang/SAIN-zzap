using EFT;
using SAIN.Helpers;
using SAIN.Preset.Shared.Enums;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public readonly struct SoundEvent(
    SAINSoundType InSoundType,
    Vector3 InPosition,
    PlayerComponent InPlayerComponent,
    float InRange,
    float InVolume,
    float InSoundSpeed,
    EPhraseTrigger InPhrase = EPhraseTrigger.None,
    ETagStatus InTagStatus = ETagStatus.Unaware
)
{
    public readonly SAINSoundType SoundType = InSoundType;
    public readonly EPhraseTrigger Phrase = InPhrase;
    public readonly ETagStatus TagStatus = InTagStatus;
    public readonly Vector3 Position = InPosition;
    public readonly float SoundSpeed = InSoundSpeed;
    public readonly float Range = InRange;
    public readonly float Volume = InVolume;
    public readonly float BaseRangeWithVolume = InRange * InVolume;
    public readonly PlayerComponent PlayerComponent = InPlayerComponent;
    public readonly float TimeCreated = Time.time;
    public readonly bool IsGunShot = InSoundType.IsGunShot();
    public readonly string ProfileId = InPlayerComponent.ProfileId;
    public readonly bool IsAI = InPlayerComponent.IsAI;
    public readonly int EnvironmentId = InPlayerComponent.Player.AIData.EnvironmentId;

    // zzap: a sound was already legitimately emitted - the source dying/deactivating before a listener gets
    // to react to it must not retroactively invalidate that emission. Only the component itself being gone
    // (destroyed) makes the event truly unprocessable.
    public readonly bool IsValid()
    {
        return PlayerComponent != null;
    }

    public readonly Player GetPlayer()
    {
        return PlayerComponent?.Player;
    }
}
