namespace SAIN.SAINComponent.Classes.EnemyClasses;

/// <summary>
/// Pure timing math for the vision pipeline. No EFT/Unity types so the regression test project can link it.
/// </summary>
public static class VisionTiming
{
    /// <summary>Max positive multiplier Enemy.LookUpdateInterval applies to the base interval.</summary>
    public const float MaxIntervalJitter = 1.25f;

    /// <summary>Longest base interval (inactive enemy, 0.5 s) * jitter is 0.625 s; clamp sits just above it.</summary>
    public const float MaxLookDeltaTime = 0.75f;

    /// <summary>Original fixed raycast-success window; never shrink below it.</summary>
    public const float MinSuccessWindow = 0.25f;

    // Raycast job tick is 1/30 s plus one frame before results are consumed.
    private const float SuccessWindowLatencyPad = 0.1f;

    /// <summary>
    /// Time elapsed since this enemy's previous look check, as vanilla LookSensor passes to CheckLookEnemy.
    /// Returns 0 on the first check (lastLookTime &lt; 0) and clamps stale gaps so a long gap can't saturate visibility in one step.
    /// </summary>
    public static float LookDeltaTime(float now, float lastLookTime)
    {
        if (lastLookTime < 0f)
        {
            return 0f;
        }
        float dt = now - lastLookTime;
        if (!(dt > 0f)) // also catches NaN
        {
            return 0f;
        }
        return dt > MaxLookDeltaTime ? MaxLookDeltaTime : dt;
    }

    /// <summary>
    /// How long a successful raycast stays valid. Must outlast the worst-case gap between samples
    /// (base interval * max jitter + job latency) or LoS/CanBeSeen flicker false between samples.
    /// </summary>
    public static float SuccessWindow(float baseInterval)
    {
        float window = baseInterval * MaxIntervalJitter + SuccessWindowLatencyPad;
        return window > MinSuccessWindow ? window : MinSuccessWindow;
    }
}
