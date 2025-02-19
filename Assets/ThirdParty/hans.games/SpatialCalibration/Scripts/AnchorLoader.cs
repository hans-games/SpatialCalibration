using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace hans.games.spatialcalibration
{
    [RequireComponent(typeof(SpatialCalibration),typeof(AnchorLocalStorageManager))]
    public class AnchorLoader : MonoBehaviour
    {
        [Tooltip("If uuids are found in local storage that are not discovered in current space, should they be deleted?")]
        [SerializeField] private bool deleteUnusedUuids = false;
        
        private SpatialCalibration spatialCalibration;        
        private AnchorLocalStorageManager anchorStorageManager;
        
        private void Start()
        {
            spatialCalibration = GetComponent<SpatialCalibration>();            
            anchorStorageManager = GetComponent<AnchorLocalStorageManager>();
            
            LoadAnchors();
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
            
            Debug.Log($"LoadAnchors completed, {spatialCalibration.GetAnchorCount()} anchors were loaded.");
            
            spatialCalibration.UpdateOrigin();
        }
        
        private void CleanupUuids(List<Guid> _uuids, List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors)
        {
            if (!deleteUnusedUuids) return;
            
            foreach (Guid uuid in _uuids)
            {
                if (_unboundAnchors.Any(_anchor => _anchor.Uuid == uuid)) continue;
                
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

            GameObject anchorObject = spatialCalibration.CreateAnchor();
            OVRSpatialAnchor spatialAnchor = anchorObject.GetComponent<OVRSpatialAnchor>();
            _unboundAnchor.BindTo(spatialAnchor);
            
            anchorObject.name = $"Anchor {_unboundAnchor.Uuid}";
            
            //savedAnchors.Add(anchorObject);
        }        
    }
    
    
}
