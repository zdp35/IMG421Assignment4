using UnityEngine;

namespace LanternDrift.Water
{
    public class Wave : MonoBehaviour
    {
        [SerializeField] private bool active = true;
        public float maxHeight = 0.6f;
        public float speed = 1f;
        public Vector2 offset = Vector2.zero;
        public Vector2 scale = new Vector2(10f, 10f);
        [Min(0f)] public float radius = 0f;
        [Min(0.01f)] public float falloff = 5f;

        private void OnEnable()
        {
            WaveRegistry.Register(this);
        }

        private void OnDisable()
        {
            WaveRegistry.Unregister(this);
        }

        public float GetHeight(float x, float z)
        {
            if (!active)
            {
                return 0f;
            }

            float bell = GetBellHeight(x, z);
            if (radius <= 0f)
            {
                return bell;
            }

            Vector2 center = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(new Vector2(x, z), center);
            if (distance >= radius)
            {
                return 0f;
            }

            float t = Mathf.Clamp01(1f - (distance / Mathf.Max(0.01f, radius)));
            float smooth = Mathf.Pow(t, falloff * 0.25f);
            return bell * smooth;
        }

        private float GetBellHeight(float x, float z)
        {
            Vector2 sinWave = new Vector2(
                Mathf.Sin(x / Mathf.Max(0.01f, scale.x) + X + Time.time * speed),
                Mathf.Sin(z / Mathf.Max(0.01f, scale.y) + Y + Time.time * speed));

            return maxHeight * 0.5f * (sinWave.x + sinWave.y);
        }

        private float X => -transform.position.x / Mathf.Max(0.01f, scale.x) + offset.x;
        private float Y => -transform.position.z / Mathf.Max(0.01f, scale.y) + offset.y;
    }
}
