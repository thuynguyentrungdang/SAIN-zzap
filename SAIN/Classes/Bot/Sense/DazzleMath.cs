namespace SAIN.SAINComponent.Classes.Sense;

/// <summary>
/// Flashlight/laser dazzle strength by distance. Unity-free so the regression tests can link it.
/// </summary>
internal static class DazzleMath
{
    public const float POINT_BLANK_MULTI = 3f;
    public const float NVG_MULTI = 1.5f;

    /// <summary>
    /// Original design (Solarint 2023): 3x at point blank, linear falloff to 1x at maxRange, x1.5 under NVGs with a visible beam.
    /// The 2024 vision cleanup (ebb86095) replaced it with an InverseLerp that evaluates to 0 for every distance under maxRange,
    /// which ApplyDazzle's clamp then turned into a flat 1x.
    /// </summary>
    public static float Modifier(float distance, float maxRange, bool nvgsAndVisibleBeam)
    {
        if (maxRange <= 0f)
        {
            return 1f;
        }
        float t = distance / maxRange;
        float closeness = 1f - (t < 0f ? 0f : (t > 1f ? 1f : t));
        float result = 1f + ((POINT_BLANK_MULTI - 1f) * closeness);
        if (nvgsAndVisibleBeam)
        {
            result *= NVG_MULTI;
        }
        return result;
    }
}
