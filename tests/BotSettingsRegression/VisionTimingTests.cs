using SAIN.SAINComponent.Classes.EnemyClasses;

partial class Program
{
    static bool Near(float a, float b) => Math.Abs(a - b) < 1e-5f;

    static void VisionTimingTests()
    {
        Test("look delta is elapsed time since previous check, not lateness", () =>
        {
            // Previous check at t=10.00; next was due 10.083 but ran at 10.10. Vanilla passes 0.10, old code passed 0.017.
            Check(Near(VisionTiming.LookDeltaTime(10.10f, 10.00f), 0.10f), "elapsed time not used");
        });
        Test("first look check yields zero delta, never raid time", () =>
        {
            Check(VisionTiming.LookDeltaTime(300f, -1f) == 0f, "first check spiked");
        });
        Test("stale gap is clamped so one step cannot saturate visibility", () =>
        {
            Check(Near(VisionTiming.LookDeltaTime(300f, 5f), VisionTiming.MaxLookDeltaTime), "stale gap not clamped");
            // clamp must sit above the longest legitimate gap: 0.5 s base * max jitter
            Check(VisionTiming.MaxLookDeltaTime >= 0.5f * VisionTiming.MaxIntervalJitter, "clamp cuts legitimate gaps");
        });
        Test("look delta rejects zero, negative and NaN gaps", () =>
        {
            Check(VisionTiming.LookDeltaTime(5f, 5f) == 0f, "zero gap");
            Check(VisionTiming.LookDeltaTime(4f, 5f) == 0f, "negative gap");
            Check(VisionTiming.LookDeltaTime(float.NaN, 5f) == 0f, "NaN now");
            Check(VisionTiming.LookDeltaTime(5f, float.NaN) == 0f, "NaN last");
        });
        Test("success window keeps 0.25 s for fast cadence enemies", () =>
        {
            Check(Near(VisionTiming.SuccessWindow(1f / 30f), VisionTiming.MinSuccessWindow), "current player enemy changed");
            Check(Near(VisionTiming.SuccessWindow(1f / 12f), VisionTiming.MinSuccessWindow), "near enemy changed");
        });
        Test("success window outlasts worst-case gap for every cadence tier", () =>
        {
            float[] baseIntervals = { 1f / 30f, 1f / 15f, 1f / 16f, 1f / 12f, 1f / 8f, 1f / 6f, 1f / 4f, 0.5f };
            foreach (float b in baseIntervals)
            {
                float worstGap = b * VisionTiming.MaxIntervalJitter;
                Check(VisionTiming.SuccessWindow(b) > worstGap, "window " + VisionTiming.SuccessWindow(b) + " <= gap " + worstGap + " at base " + b);
            }
            // The bug: far AI (1/4) and inactive (0.5) exceeded the old fixed 0.25 s window.
            Check(0.25f * VisionTiming.MaxIntervalJitter > 0.25f, "fixture lost: far AI gap should exceed legacy window");
        });
    }
}
