using UnityEngine;

namespace LanternDrift.Boat
{
    [RequireComponent(typeof(Rigidbody), typeof(BoatStatus))]
    public class BoatController : MonoBehaviour
    {
        public Rigidbody rb;
        public BoatStatus status;

        [Header("Movement")]
        public float forwardForce = 22f;
        public float reverseForce = 11f;
        public float turnTorque = 15f;
        public float pivotTurnTorque = 30f;
        public float maxForwardSpeed = 5.6f;
        public float maxReverseSpeed = 2.4f;
        public float idleWaterDrag = 1.55f;
        public float moveWaterDrag = 1.0f;
        public float angularDrag = 3.2f;
        public float throttleResponse = 5.0f;
        public float throttleDirectionChangeResponse = 10.0f;
        public float steeringResponse = 4.8f;
        public float steeringDirectionChangeResponse = 8.5f;
        public float steeringReleaseResponse = 7.0f;
        public float inputDeadzone = 0.08f;
        public float minTurnAuthority = 0.45f;
        public float maxTurnAuthority = 1.75f;
        public float lowSpeedTurnAuthority = 1.25f;
        public float reverseTurnAuthority = 1.15f;
        public float sideSlipDamping = 4.2f;
        public float reverseBrakeAssist = 7.0f;
        public bool canControl = false;

        [Header("Visuals")]
        public Light boatLamp;
        public float lampFlickerStrength = 0.2f;
        public float lampBaseIntensity = 5.2f;

        public float CurrentSpeed
        {
            get
            {
                if (rb == null) return 0f;
                Vector3 planar = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
                return planar.magnitude;
            }
        }

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }

            if (status == null)
            {
                status = GetComponent<BoatStatus>();
            }

            rb.useGravity = true;
            rb.drag = idleWaterDrag;
            rb.angularDrag = angularDrag;
            rb.centerOfMass = new Vector3(0f, -0.25f, 0f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private float smoothedVertical;
        private float smoothedHorizontal;

        private void FixedUpdate()
        {
            rb.drag = canControl ? moveWaterDrag : idleWaterDrag;

            if (!canControl)
            {
                smoothedVertical = Mathf.MoveTowards(smoothedVertical, 0f, throttleResponse * Time.fixedDeltaTime);
                smoothedHorizontal = Mathf.MoveTowards(smoothedHorizontal, 0f, steeringReleaseResponse * Time.fixedDeltaTime);
                return;
            }

            float targetVertical = GetVerticalInput();
            float targetHorizontal = GetHorizontalInput();

            float throttleStep = Mathf.Sign(targetVertical) != Mathf.Sign(smoothedVertical) && Mathf.Abs(targetVertical) > inputDeadzone && Mathf.Abs(smoothedVertical) > inputDeadzone
                ? throttleDirectionChangeResponse
                : throttleResponse;
            smoothedVertical = Mathf.MoveTowards(smoothedVertical, targetVertical, throttleStep * Time.fixedDeltaTime);

            float steeringStep = steeringReleaseResponse;
            if (Mathf.Abs(targetHorizontal) > inputDeadzone)
            {
                steeringStep = Mathf.Sign(targetHorizontal) != Mathf.Sign(smoothedHorizontal) && Mathf.Abs(smoothedHorizontal) > inputDeadzone
                    ? steeringDirectionChangeResponse
                    : steeringResponse;
            }
            smoothedHorizontal = Mathf.MoveTowards(smoothedHorizontal, targetHorizontal, steeringStep * Time.fixedDeltaTime);

            float vertical = Mathf.Abs(smoothedVertical) < inputDeadzone ? 0f : smoothedVertical;
            float horizontal = Mathf.Abs(smoothedHorizontal) < inputDeadzone ? 0f : smoothedHorizontal;
            float curvedHorizontal = Mathf.Sign(horizontal) * horizontal * horizontal;

            float speedMult = status != null ? status.speedMultiplier : 1f;
            float turnMult = status != null ? status.turnMultiplier : 1f;

            Vector3 planarVelocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
            Vector3 localVelocity = transform.InverseTransformDirection(planarVelocity);

            if (vertical > 0f && localVelocity.z < maxForwardSpeed * speedMult)
            {
                rb.AddForce(transform.forward * vertical * forwardForce * speedMult, ForceMode.Acceleration);
            }
            else if (vertical < 0f && localVelocity.z > -maxReverseSpeed * speedMult)
            {
                rb.AddForce(transform.forward * vertical * reverseForce * speedMult, ForceMode.Acceleration);
            }

            float directionalSpeedFactor = Mathf.Clamp01(Mathf.Abs(localVelocity.z) / Mathf.Max(0.01f, maxForwardSpeed));
            float forwardTurnAuthority = Mathf.Lerp(lowSpeedTurnAuthority, maxTurnAuthority, directionalSpeedFactor);
            float baseTurnAuthority = Mathf.Max(minTurnAuthority, forwardTurnAuthority);
            if (localVelocity.z < -0.1f)
            {
                baseTurnAuthority *= reverseTurnAuthority;
            }

            float throttleTurnBoost = Mathf.Lerp(1f, 1.18f, Mathf.Abs(vertical));
            float appliedTurn = curvedHorizontal * turnTorque * baseTurnAuthority * throttleTurnBoost * turnMult;

            if (Mathf.Abs(localVelocity.z) < 1.15f)
            {
                float pivotStrength = Mathf.Lerp(1f, 0.35f, directionalSpeedFactor);
                appliedTurn += curvedHorizontal * pivotTurnTorque * pivotStrength * turnMult;
            }

            rb.AddTorque(Vector3.up * appliedTurn, ForceMode.Acceleration);

            Vector3 lateralWorld = transform.right * localVelocity.x;
            rb.AddForce(-lateralWorld * sideSlipDamping, ForceMode.Acceleration);

            if (Mathf.Abs(vertical) > inputDeadzone && Mathf.Sign(vertical) != Mathf.Sign(localVelocity.z) && Mathf.Abs(localVelocity.z) > 0.2f)
            {
                rb.AddForce(-transform.forward * localVelocity.z * reverseBrakeAssist, ForceMode.Acceleration);
            }

            float maxPlanarSpeed = (vertical >= 0f ? maxForwardSpeed : maxReverseSpeed) * speedMult;
            if (planarVelocity.magnitude > maxPlanarSpeed && planarVelocity.sqrMagnitude > 0.0001f)
            {
                Vector3 clampedPlanar = planarVelocity.normalized * maxPlanarSpeed;
                rb.velocity = new Vector3(clampedPlanar.x, rb.velocity.y, clampedPlanar.z);
            }
        }


        private float GetVerticalInput()
        {
            float axis = 0f;
            try
            {
                axis = Input.GetAxisRaw("Vertical");
            }
            catch
            {
                axis = 0f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                axis = Mathf.Max(axis, 1f);
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                axis = Mathf.Min(axis, -1f);
            }

            return axis;
        }

        private float GetHorizontalInput()
        {
            float axis = 0f;
            try
            {
                axis = Input.GetAxisRaw("Horizontal");
            }
            catch
            {
                axis = 0f;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                axis = Mathf.Min(axis, -1f);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                axis = Mathf.Max(axis, 1f);
            }

            return axis;
        }

        private void Update()
        {
            if (boatLamp != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * 8f, 0.37f);
                boatLamp.intensity = lampBaseIntensity + (noise - 0.5f) * lampFlickerStrength;
            }
        }
    }
}
