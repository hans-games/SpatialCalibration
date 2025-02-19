using UnityEngine;
using UnityEngine.Events;

namespace hans.games.spatialcalibration
{
    public class PositionPlaceholder : MonoBehaviour
    {
        [SerializeField] private UnityEvent onPlaced;
        
        public void PlaceAtTransform(Transform _transform)
        {
            if (_transform.position == Vector3.zero) return;
            
            gameObject.SetActive(true);
            transform.position = _transform.position;
            
            onPlaced.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }
    }
}
