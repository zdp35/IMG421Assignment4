using UnityEngine;
using LanternDrift.Boat;
using LanternDrift.Water;

namespace LanternDrift.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class GatorHazard : MonoBehaviour
    {
        public float detectionRange = 7.2f;
        public float attackRange = 3.1f;
        public float moveSpeed = 1.05f;
        public float lungeTurnSpeed = 2.8f;
        public float dangerPerSecond = 9f;
        public float slowMultiplier = 0.88f;
        public float turnMultiplier = 0.94f;
        public float escapeSpeedThreshold = 1.15f;
        public float homeLeashDistance = 7f;

        private Vector3 homePosition;
        private BoatController boat;
        private BoatStatus boatStatus;

        private void Awake()
        {
            homePosition = transform.position;
            SphereCollider sphere = GetComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = detectionRange;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                boat = GameManager.Instance.playerBoat;
                boatStatus = boat != null ? boat.GetComponent<BoatStatus>() : null;
            }
        }

        private void Update()
        {
            if (boat == null)
            {
                return;
            }

            Vector3 flatBoatPosition = new Vector3(boat.transform.position.x, transform.position.y, boat.transform.position.z);
            float distance = Vector3.Distance(transform.position, flatBoatPosition);

            if (GameManager.Instance != null && !GameManager.Instance.GameRunning)
            {
                ReturnHome();
                return;
            }

            if (distance <= detectionRange)
            {
                Vector3 toBoat = (flatBoatPosition - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, flatBoatPosition, moveSpeed * Time.deltaTime);
                if (toBoat.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(toBoat, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lungeTurnSpeed);
                }

                if (distance <= attackRange && boat.CurrentSpeed <= escapeSpeedThreshold && distance <= attackRange * 0.82f)
                {
                    boatStatus?.ApplyDebuff(slowMultiplier, turnMultiplier, 0.15f);
                    GameManager.Instance.AddSink(dangerPerSecond * Time.deltaTime);
                }
            }
            else
            {
                ReturnHome();
            }

            Vector3 pos = transform.position;
            pos.y = WaterSurfaceUtility.GetWaterHeight(pos) + 0.12f;
            transform.position = pos;
        }

        private void ReturnHome()
        {
            Vector3 target = homePosition;
            if (Vector3.Distance(transform.position, homePosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * 0.55f * Time.deltaTime);
            }
        }
    }
}
