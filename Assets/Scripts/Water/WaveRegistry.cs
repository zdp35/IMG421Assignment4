using System.Collections.Generic;

namespace LanternDrift.Water
{
    public static class WaveRegistry
    {
        private static readonly List<Wave> ActiveWaves = new List<Wave>();

        public static IReadOnlyList<Wave> Waves => ActiveWaves;

        public static void Register(Wave wave)
        {
            if (wave != null && !ActiveWaves.Contains(wave))
            {
                ActiveWaves.Add(wave);
            }
        }

        public static void Unregister(Wave wave)
        {
            if (wave != null)
            {
                ActiveWaves.Remove(wave);
            }
        }
    }
}
