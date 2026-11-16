using SAIN.SAINComponent.Classes.Sense;

partial class Program
{
    static void DazzleMathTests()
    {
        Test("dazzle is 3x at point blank and 1x at max range", () =>
        {
            Check(Near(DazzleMath.Modifier(0f, 40f, false), 3f), "point blank");
            Check(Near(DazzleMath.Modifier(40f, 40f, false), 1f), "max range");
        });
        Test("dazzle falls linearly with distance (old InverseLerp gave 0 for every dist < max)", () =>
        {
            Check(Near(DazzleMath.Modifier(20f, 40f, false), 2f), "halfway");
            Check(DazzleMath.Modifier(10f, 40f, false) > DazzleMath.Modifier(30f, 40f, false), "closer must dazzle more");
            Check(DazzleMath.Modifier(39f, 40f, false) > 1f, "must not collapse to the clamp floor under max range");
        });
        Test("dazzle outside range or with bad range stays 1x", () =>
        {
            Check(Near(DazzleMath.Modifier(80f, 40f, false), 1f), "beyond max");
            Check(Near(DazzleMath.Modifier(-5f, 40f, false), 3f), "negative distance clamps to point blank");
            Check(Near(DazzleMath.Modifier(10f, 0f, false), 1f), "zero range");
        });
        Test("dazzle NVG bonus is x1.5", () =>
        {
            Check(Near(DazzleMath.Modifier(0f, 40f, true), 4.5f), "nvg point blank");
            Check(Near(DazzleMath.Modifier(40f, 40f, true), 1.5f), "nvg max range");
        });
    }
}
