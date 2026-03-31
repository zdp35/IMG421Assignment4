using UnityEngine;

namespace LanternDrift.Water
{
    public class WaterFloatFollower : MonoBehaviour
    {
        public float heightOffset = 0f;
        public bool bob = true;
        public float bobAmplitude = 0.05f;
        public float bobSpeed = 1.5f;

        private Vector3 cachedPosition;

        private void Update()
        {
            cachedPosition = transform.position;
            float bobOffset = bob ? Mathf.Sin(Time.time * bobSpeed + transform.position.x + transform.position.z) * bobAmplitude : 0f;
            cachedPosition.y = WaterSurfaceUtility.GetWaterHeight(transform.position) + heightOffset + bobOffset;
            transform.position = cachedPosition;
        }
    }
}
