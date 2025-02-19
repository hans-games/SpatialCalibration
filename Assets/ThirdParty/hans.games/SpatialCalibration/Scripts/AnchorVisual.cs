using UnityEngine;

namespace hans.games.spatialcalibration
{
    public class AnchorVisual : MonoBehaviour
    {
        [SerializeField] private GameObject visual;
        [SerializeField] private GameObject selectedPointer;

        public void SetVisible(bool _visible)
        {
            visual.SetActive(_visible);
        }
        
        public void Select(bool _isSelected)
        {
            selectedPointer.SetActive(_isSelected);
        }
    }
}
