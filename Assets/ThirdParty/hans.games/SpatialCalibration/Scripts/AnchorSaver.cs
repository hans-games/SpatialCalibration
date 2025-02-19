using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace hans.games.spatialcalibration
{
    [RequireComponent(typeof(SpatialCalibration),typeof(AnchorLocalStorageManager))]
    public class AnchorSaver : MonoBehaviour
    {
        [SerializeField] private PositionPlaceholder positionPlaceholder;
        [SerializeField] private UnityEvent onConvert;
        [SerializeField] private UnityEvent onSelect;
        [SerializeField] private UnityEvent onDelete;
    
        private SpatialCalibration spatialCalibration;
        private AnchorLocalStorageManager anchorStorageManager;
        private int currentSelectedIndex = -1;

        private void Start()
        {
            spatialCalibration = GetComponent<SpatialCalibration>();
            anchorStorageManager = GetComponent<AnchorLocalStorageManager>();
        }

        public void ConvertPlaceholderToAnchor()
        {
            if (!positionPlaceholder) throw new Exception("No position placeholder assigned");
            if (!positionPlaceholder.IsActive()) return;
            
            positionPlaceholder.Hide();

            StartCoroutine(CreateOvrSpatialAnchor());
            
            onConvert.Invoke();
        }

        private IEnumerator CreateOvrSpatialAnchor()
        {
            GameObject anchorObject = spatialCalibration.CreateAnchor(positionPlaceholder.GetPosition(), positionPlaceholder.GetRotation());
            OVRSpatialAnchor anchor = anchorObject.GetComponent<OVRSpatialAnchor>();
            
            yield return new WaitUntil(() => anchor.Created);
            
            anchorObject.name = $"Anchor {anchor.Uuid}";
            
            TaskCompletionSource<bool> tcs = new ();
            SaveAnchor(anchor, tcs);
            yield return new WaitUntil(() => tcs.Task.IsCompleted);
            
            //if (tcs.Task.IsCompletedSuccessfully) spatialCalibration.AddAnchor(anchorObject);

            spatialCalibration.UpdateOrigin();
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
        
       public void SelectNextAnchor()
        {
            SelectAnchor(1);
        }

        public void SelectPreviousAnchor()
        {
            SelectAnchor(-1);
        }

        private void SelectAnchor(int _offset)
        {
            if (spatialCalibration.GetAnchorCount() == 0) return;

            spatialCalibration.GetAnchor(currentSelectedIndex)?.GetComponent<AnchorVisual>().Select(false);
            
            currentSelectedIndex += _offset;
            if (currentSelectedIndex >= spatialCalibration.GetAnchorCount()) currentSelectedIndex = 0;
            if (currentSelectedIndex < 0) currentSelectedIndex = spatialCalibration.GetAnchorCount() - 1;
            
            spatialCalibration.GetAnchor(currentSelectedIndex)?.GetComponent<AnchorVisual>().Select(true);
            
            onSelect.Invoke();
        }

        public async void DeleteAnchor()
        {
            if (currentSelectedIndex < 0) return;
            
            GameObject anchorObject = spatialCalibration.GetAnchor(currentSelectedIndex);
            if (anchorObject == null) return;
            
            OVRSpatialAnchor spatialAnchor = anchorObject.GetComponent<OVRSpatialAnchor>();

            var result = await spatialAnchor.EraseAnchorAsync();
            
            if (!result.Success) 
            {
                Debug.LogError($"Failed to erase anchor {spatialAnchor.Uuid} with result {result.Status}");
                return;
            }
            
            anchorStorageManager.RemoveUuid(spatialAnchor.Uuid.ToString());
            spatialCalibration.RemoveAnchor(anchorObject);

            currentSelectedIndex = -1;
            
            spatialCalibration.UpdateOrigin();
            
            onDelete.Invoke();
        }        
    }
}
