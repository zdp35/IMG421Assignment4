using UnityEngine;

namespace LanternDrift.Boat
{
    public class BoatStatus : MonoBehaviour
    {
        public float speedMultiplier { get; private set; } = 1f;
        public float turnMultiplier { get; private set; } = 1f;

        private float speedDebuffTimer;
        private float turnDebuffTimer;
        private float requestedSpeedMultiplier = 1f;
        private float requestedTurnMultiplier = 1f;

        public void ApplyDebuff(float slowMultiplier, float turningMultiplier, float duration)
        {
            requestedSpeedMultiplier = Mathf.Min(requestedSpeedMultiplier, Mathf.Clamp(slowMultiplier, 0.05f, 1f));
            requestedTurnMultiplier = Mathf.Min(requestedTurnMultiplier, Mathf.Clamp(turningMultiplier, 0.05f, 1f));
            speedDebuffTimer = Mathf.Max(speedDebuffTimer, duration);
            turnDebuffTimer = Mathf.Max(turnDebuffTimer, duration);
        }

        private void Update()
        {
            if (speedDebuffTimer > 0f)
            {
                speedDebuffTimer -= Time.deltaTime;
                speedMultiplier = Mathf.MoveTowards(speedMultiplier, requestedSpeedMultiplier, Time.deltaTime * 8f);
            }
            else
            {
                speedMultiplier = Mathf.MoveTowards(speedMultiplier, 1f, Time.deltaTime * 2f);
            }

            if (turnDebuffTimer > 0f)
            {
                turnDebuffTimer -= Time.deltaTime;
                turnMultiplier = Mathf.MoveTowards(turnMultiplier, requestedTurnMultiplier, Time.deltaTime * 8f);
            }
            else
            {
                turnMultiplier = Mathf.MoveTowards(turnMultiplier, 1f, Time.deltaTime * 2f);
            }

            requestedSpeedMultiplier = 1f;
            requestedTurnMultiplier = 1f;
        }
    }
}
