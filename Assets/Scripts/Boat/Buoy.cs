using UnityEngine;
using LanternDrift.Water;

namespace LanternDrift.Boat
{
    public class Buoy : MonoBehaviour
    {
        public Rigidbody targetRigidbody;
        public float buoyancy = 1.2f;
        public float waterDrag = 0.25f;
        public float waterAngularDrag = 0.15f;

        private void Awake()
        {
            if (targetRigidbody == null)
            {
                targetRigidbody = GetComponentInParent<Rigidbody>();
            }
        }

        private void FixedUpdate()
        {
            if (targetRigidbody == null)
            {
                return;
            }

            float waterHeight = WaterSurfaceUtility.GetWaterHeight(transform.position);
            if (transform.position.y < waterHeight)
            {
                float submersion = waterHeight - transform.position.y;
                float force = targetRigidbody.mass * Physics.gravity.magnitude * buoyancy * submersion;
                targetRigidbody.AddForceAtPosition(Vector3.up * force, transform.position, ForceMode.Force);
                targetRigidbody.AddForce(-targetRigidbody.velocity * waterDrag, ForceMode.Acceleration);
                targetRigidbody.AddTorque(-targetRigidbody.angularVelocity * waterAngularDrag, ForceMode.Acceleration);
            }
        }
    }
}
