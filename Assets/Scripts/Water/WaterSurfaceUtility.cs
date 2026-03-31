using UnityEngine;

namespace LanternDrift.Water
{
    public static class WaterSurfaceUtility
    {
        public static float GetWaterHeight(Vector3 worldPosition)
        {
            float height = 0f;
            var waves = WaveRegistry.Waves;
            for (int i = 0; i < waves.Count; i++)
            {
                if (waves[i] != null)
                {
                    height += waves[i].GetHeight(worldPosition.x, worldPosition.z);
                }
            }

            return height;
        }
    }
}
