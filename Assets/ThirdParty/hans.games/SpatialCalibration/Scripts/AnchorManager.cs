using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace hans.games.spatialcalibration
{
    [RequireComponent(typeof(AnchorLocalStorageManager))]
    public class AnchorManager : MonoBehaviour
    {
        [SerializeField] private PositionPlaceholder positionPlaceholder;
        [SerializeField] private GameObject anchorPrefab;
        [SerializeField] private GameObject origin;
        
        [Tooltip("If uuids are found in local storage that are not discovered in current space, should they be deleted?")]
        [SerializeField] private bool deleteUnusedUuids = false;
        
        private AnchorLocalStorageManager anchorStorageManager;
        private List<GameObject> savedAnchors;
        
        private GameObject currentlySelectedAnchor;

        private void Start()
        {
            anchorStorageManager = GetComponent<AnchorLocalStorageManager>();
            savedAnchors = new List<GameObject>();
            
            LoadAnchors();
        }

        public void ConvertPlaceholderToAnchor()
        {
            if (!positionPlaceholder) throw new Exception("No position placeholder assigned");
            if (!positionPlaceholder.IsActive()) return;
            
            positionPlaceholder.Hide();

            StartCoroutine(CreateOvrSpatialAnchor());
        }

        private IEnumerator CreateOvrSpatialAnchor()
        {
            if (!anchorPrefab) throw new Exception("No anchor prefab assigned");
            
            GameObject anchorObject = Instantiate(anchorPrefab, transform);
            anchorObject.transform.SetPositionAndRotation(positionPlaceholder.GetPosition(), positionPlaceholder.GetRotation());
            OVRSpatialAnchor anchor = anchorObject.GetComponent<OVRSpatialAnchor>();
            
            yield return new WaitUntil(() => anchor.Created);
            
            anchorObject.name = $"Anchor {anchor.Uuid}";
            
            TaskCompletionSource<bool> tcs = new ();
            SaveAnchor(anchor, tcs);
            yield return new WaitUntil(() => tcs.Task.IsCompleted);
            
            if (tcs.Task.IsCompletedSuccessfully) savedAnchors.Add(anchorObject);

            UpdateOrigin();
        }

        private async void SaveAnchor(OVRSpatialAnchor _anchor, TaskCompletionSource<bool> _tcs)
        {
            if (!_anchor)
            {
                _tcs.SetResult(false);
                return;
            }

            var result = await _anchor.SaveAnchorAsync();
            
            if (result.Success)
            {
                anchorStorageManager.SaveUuid(_anchor.Uuid.ToString());
            }
            else
            {
                Debug.LogError($"Anchor {_anchor.Uuid} failed to save with error {result.Status}");
                _tcs.SetResult(false);
            }
            
            _tcs.SetResult(true);
        }

        private async void LoadAnchors()
        {
            List<Guid> uuids = anchorStorageManager.GetUuids();
            
            List<OVRSpatialAnchor.UnboundAnchor> unboundAnchors = new();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
            if (!result.Success)
            {
                Debug.LogError($"LoadAnchors failed with error {result.Status}.");
                return;
            }

            CleanupUuids(uuids, unboundAnchors);
            
            List<Task> localizationTasks = new();
            foreach (OVRSpatialAnchor.UnboundAnchor unboundAnchor in unboundAnchors)
            {
                localizationTasks.Add(LocalizeAnchorAsync(unboundAnchor));
            }
            await Task.WhenAll(localizationTasks);
            
            Debug.Log($"LoadAnchors completed, {savedAnchors.Count} anchors were loaded.");
            
            UpdateOrigin();
        }

        private void CleanupUuids(List<Guid> _uuids, List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors)
        {
            if (!deleteUnusedUuids) return;
            
            foreach (Guid uuid in _uuids)
            {
                if (!_unboundAnchors.All(_anchor => _anchor.Uuid != uuid)) continue;
                
                Debug.Log(
                    $"Found uuid in local storage that is not discovered in current space, deleting\n{uuid}");
                
                anchorStorageManager.RemoveUuid(uuid.ToString());
            }
        }

        async Task LocalizeAnchorAsync(OVRSpatialAnchor.UnboundAnchor _unboundAnchor)
        {
            bool success = await _unboundAnchor.LocalizeAsync();

            if (!success)
            {
                Debug.LogError($"[AnchorManager] Localization failed for anchor {_unboundAnchor.Uuid}");
                return;
            }
            
            GameObject anchorObject = Instantiate(anchorPrefab);
            OVRSpatialAnchor spatialAnchor = anchorObject.GetComponent<OVRSpatialAnchor>();
            _unboundAnchor.BindTo(spatialAnchor);
            
            anchorObject.name = $"Anchor {_unboundAnchor.Uuid}";
            
            savedAnchors.Add(anchorObject);
        }

        private void UpdateOrigin()
        {
            if (!origin) throw new Exception("No Origin gameobject assigned");
            if (savedAnchors.Count < 2)
            {
                DeactivateOrigin();
                return;
            }
            
            origin.SetActive(true);
            
            Vector3 average = Vector3.zero;

            foreach (GameObject savedAnchor in savedAnchors)
            {
                average += savedAnchor.transform.position;
            }
            
            average /= savedAnchors.Count;
            
            origin.transform.position = average;
            
        }

        private void DeactivateOrigin()
        {
            origin.SetActive(false);
        }

        public void SelectNextAnchor()
        {
            if (savedAnchors.Count == 0) return;
            
            if (currentlySelectedAnchor == null) SelectAnchor(0);
            int currentIndex = savedAnchors.IndexOf(currentlySelectedAnchor);
            int nextIndex = (currentIndex + 1) % savedAnchors.Count;
            SelectAnchor(nextIndex);
        }

        public void SelectPreviousAnchor()
        {
            if (savedAnchors.Count == 0) return;
            
            if (currentlySelectedAnchor == null) SelectAnchor(0);
            int currentIndex = savedAnchors.IndexOf(currentlySelectedAnchor);
            int previousIndex = (currentIndex - 1 + savedAnchors.Count) % savedAnchors.Count;
            SelectAnchor(previousIndex);
        }

        private void SelectAnchor(int _index)
        {
            if (_index < 0 || savedAnchors.Count <= _index) return;

            if (currentlySelectedAnchor != null)
            {
                currentlySelectedAnchor.GetComponent<AnchorVisual>().Select(false);
            }
            
            currentlySelectedAnchor = savedAnchors[_index];
            currentlySelectedAnchor.GetComponent<AnchorVisual>().Select(true);
        }

        public async void DeleteAnchor()
        {
            if (!currentlySelectedAnchor) return;
            
            OVRSpatialAnchor spatialAnchor = currentlySelectedAnchor.GetComponent<OVRSpatialAnchor>();

            var result = await spatialAnchor.EraseAnchorAsync();
            
            if (!result.Success) 
            {
                Debug.LogError($"Failed to erase anchor {spatialAnchor.Uuid} with result {result.Status}");
                return;
            }
            
            anchorStorageManager.RemoveUuid(spatialAnchor.Uuid.ToString());
            savedAnchors.Remove(currentlySelectedAnchor);
            Destroy(currentlySelectedAnchor);
            currentlySelectedAnchor = null;
            
            UpdateOrigin();
        }
    }
}
