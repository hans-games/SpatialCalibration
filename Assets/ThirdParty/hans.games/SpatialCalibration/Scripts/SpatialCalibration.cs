using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace hans.games.spatialcalibration
{
   
    public class SpatialCalibration : MonoBehaviour
    {
        [SerializeField] private bool shouldVisualiseOrigin = false;
        [SerializeField] private GameObject originVisual = null;
        [SerializeField] private bool shouldVisualiseAnchors = false;
        [SerializeField] private GameObject anchorPrefab = null;
        
        private List<GameObject> anchors;

        private void Start()
        {
            anchors = new List<GameObject>();
        }

        public GameObject CreateAnchor(Vector3? _position = null, Quaternion? _rotation = null)
        {
            if (!anchorPrefab) throw new Exception("No anchor prefab assigned. Even though anchors may not be visible, the prefab is still required.");
            
            GameObject anchorObject = Instantiate(anchorPrefab, transform);
            
            anchorObject.transform.position = _position ?? Vector3.zero;
            anchorObject.transform.rotation = _rotation ?? Quaternion.identity;

            if (shouldVisualiseAnchors)
            {
                anchorObject.GetComponent<AnchorVisual>().SetVisible(true);
            }

            anchors.Add(anchorObject);            
            
            return anchorObject;
        }

        public int GetAnchorCount()
        {
            return anchors.Count;
        }

        public GameObject GetAnchor(int _index)
        {
            if (_index < 0 || anchors.Count <= _index) return null;    
            
            return anchors[_index];
        }

        public void RemoveAnchor(GameObject _anchor)
        {
            anchors.Remove(_anchor);
            Destroy(_anchor);
        }

        public void UpdateOrigin()
        {
            if (originVisual) originVisual.SetActive(shouldVisualiseOrigin);

            if (anchors.Count < 2) return;

            Vector3 average = Vector3.zero;
            
            foreach (GameObject savedAnchor in anchors)
            {
                average += savedAnchor.transform.position;
            }
            
            average /= anchors.Count;
            
            transform.position = average;
        }
    }
}
