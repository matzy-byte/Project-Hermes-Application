namespace Helper;

public static class MathHelper
{
    public static float linearInterpolation(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
