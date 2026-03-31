using UnityEngine;

namespace LanternDrift.Boat
{
    public class BoatWakeEffects : MonoBehaviour
    {
        public BoatController boat;
        public ParticleSystem bubbleParticles;
        public TrailRenderer wakeTrail;
        public float minimumSpeedForEmission = 0.6f;
        public float maxEmissionRate = 40f;
        public float maxTrailTime = 0.8f;
        public float maxTrailWidth = 1.6f;

        private void Awake()
        {
            if (boat == null)
            {
                boat = GetComponentInParent<BoatController>();
            }
        }

        private void Update()
        {
            if (boat == null)
            {
                return;
            }

            float speed01 = Mathf.Clamp01(boat.CurrentSpeed / Mathf.Max(0.01f, boat.maxForwardSpeed));
            bool emit = boat.canControl && boat.CurrentSpeed > minimumSpeedForEmission;

            if (bubbleParticles != null)
            {
                var emission = bubbleParticles.emission;
                emission.rateOverTime = emit ? speed01 * maxEmissionRate : 0f;
            }

            if (wakeTrail != null)
            {
                wakeTrail.time = emit ? Mathf.Lerp(0.08f, maxTrailTime, speed01) : 0f;
                wakeTrail.widthMultiplier = emit ? Mathf.Lerp(0.2f, maxTrailWidth, speed01) : 0f;
            }
        }
    }
}
