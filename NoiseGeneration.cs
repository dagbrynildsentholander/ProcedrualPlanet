using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class NoiseGeneration
{
    private static float[,,] noiseLookup;

    public static Vector4 GetHeightNormal(Vector3 position)
    {
        float freq = .0001f;
        float scale = 1000;
        Vector4 noise = new Vector4();
        for (int i = 0; i < 5; i++)
        {
            Vector4 n = Noised(position * freq);
            noise.x += n.x * scale;
            noise.y += n.y * freq * scale;
            noise.z += n.z * freq * scale;
            noise.w += n.w * freq * scale;
            position += new Vector3(noise.x, noise.y, noise.x); // Domain warp
            freq *= 2f;
            scale *= 0.5f;
        }
        return noise;
    }
    public static Vector4 Noised(Vector3 x)
    {
        Vector3 i = new Vector3(Mathf.Floor(x.x), Mathf.Floor(x.y), Mathf.Floor(x.z));
        Vector3 w = new Vector3(x.x - Mathf.Floor(x.x), x.y - Mathf.Floor(x.y), x.z - Mathf.Floor(x.z));

        // cubic interpolation
        // w * w * (3 - 2 * w) = 3x^2 - 2x^3 -> dx = 6x - 6x^2
        Vector3 u = new Vector3(w.x * w.x * (3.0f - 2.0f * w.x), w.y * w.y * (3.0f - 2.0f * w.y), w.z * w.z * (3.0f - 2.0f * w.z));
        Vector3 du = new Vector3(6 * w.x - 6 * w.x* w.x, 6 * w.y - 6 * w.y * w.y, 6 * w.z - 6 * w.z * w.z);

        float a = NoiseSample(i);
        float b = NoiseSample(i + new Vector3(1, 0, 0));
        float c = NoiseSample(i + new Vector3(0, 1, 0));
        float d = NoiseSample(i + new Vector3(1, 1, 0));
        float e = NoiseSample(i + new Vector3(0, 0, 1));
        float f = NoiseSample(i + new Vector3(1, 0, 1));
        float g = NoiseSample(i + new Vector3(0, 1, 1));
        float h = NoiseSample(i + new Vector3(1, 1, 1));

        float k0 = a;
        float k1 = b - a;
        float k2 = c - a;
        float k3 = e - a;
        float k4 = a - b - c + d;
        float k5 = a - c - e + g;
        float k6 = a - b - e + f;
        float k7 = -a + b + c - d + e - f - g + h;

        return new Vector4(k0 + k1 * u.x + k2 * u.y + k3 * u.z + k4 * u.x * u.y + k5 * u.y * u.z + k6 * u.z * u.x + k7 * u.x * u.y * u.z,
                        du.x * (k1 + k4 * u.y + k6 * u.z + k7 * u.y * u.z),
                                du.y * (k2 + k5 * u.z + k4 * u.x + k7 * u.z * u.x),
                                du.z * (k3 + k6 * u.x + k5 * u.y + k7 * u.x * u.y));
    }

    private static float[,,] CreateNoiseMap()
    {
        float[,,] tmp = new float[16, 16, 16];

        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    float random = Random.value*2f-1;
                    tmp[x, y, z] = random;
                }
            }
        }

        return tmp;
    }

    public static void GenerateNoiseLookup()
    {
        noiseLookup = CreateNoiseMap();
    }

    public static float NoiseSample(Vector3 p)
    {
        float x_d = p.x - Mathf.Floor(p.x);
        float y_d = p.y - Mathf.Floor(p.y);
        float z_d = p.z - Mathf.Floor(p.z);

        int x = Mathf.FloorToInt(Mathf.Repeat(p.x, 15f));
        int y = Mathf.FloorToInt(Mathf.Repeat(p.y, 15f));
        int z = Mathf.FloorToInt(Mathf.Repeat(p.z, 15f));

        float x_val;
        float y_val;
        float z_val;

        if(x == 15)
            x_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[0, y, z], x_d);
        else
            x_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[x + 1, y, z], x_d);

        if (y == 15)
            y_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[x, 0, z], y_d);
        else
            y_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[x, y + 1, z], y_d);

        if(z == 15)
            z_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[x, y, 0], z_d);
        else
            z_val = Mathf.Lerp(noiseLookup[x, y, z], noiseLookup[x, y, z + 1], z_d);

        return (x_val + y_val + z_val)/3;
    }
}
