using UnityEngine;

namespace hans.games.spatialcalibration
{
    public class MenuTransformer : MonoBehaviour
    {
        [SerializeField] private Transform handTransform;
        [SerializeField] private Transform controllerTransform;
        [SerializeField] private float maxMoveSpeed;
        [SerializeField] private float maxRotateSpeed;
        void Update()
        {
            //if (OVRPlugin.GetHandTrackingEnabled())
            if (OVRInput.IsControllerConnected(OVRInput.Controller.Hands))
            {
                transform.position = Vector3.Lerp(transform.position, handTransform.position, Time.deltaTime * maxMoveSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, handTransform.rotation * Quaternion.Euler(0, 270, 0), Time.deltaTime * maxRotateSpeed);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, controllerTransform.position, Time.deltaTime * maxMoveSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, controllerTransform.rotation, Time.deltaTime * maxRotateSpeed);
            }
        }
    }
}
