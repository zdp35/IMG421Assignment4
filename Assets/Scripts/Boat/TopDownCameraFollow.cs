using UnityEngine;

namespace LanternDrift.Boat
{
    // Kept original class name so existing bootstrap references still compile.
    public class TopDownCameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 5.5f, -9f);
        public Vector3 lookTargetOffset = new Vector3(0f, 1.2f, 5f);
        public float positionSmoothTime = 0.14f;
        public float rotationLerpSpeed = 7f;
        public bool rotateWithBoat = true;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = rotateWithBoat
                ? target.TransformPoint(offset)
                : target.position + offset;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

            Vector3 lookPoint = rotateWithBoat
                ? target.TransformPoint(lookTargetOffset)
                : target.position + lookTargetOffset;

            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }
}
