using System;
using UnityEngine;
using UnityEngine.Events;

namespace hans.games.spatialcalibration
{
    public class ControllerInput : MonoBehaviour
    {
        public UnityEvent OnButtonA;
        public UnityEvent OnButtonB;
        public UnityEvent OnButtonX;
        public UnityEvent OnButtonY;
        public UnityEvent OnLeftThumbstick;

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A)) OnButtonA?.Invoke();
            if (OVRInput.GetDown(OVRInput.RawButton.B)) OnButtonB?.Invoke();
            if (OVRInput.GetDown(OVRInput.RawButton.X)) OnButtonX?.Invoke();
            if (OVRInput.GetDown(OVRInput.RawButton.Y)) OnButtonY?.Invoke();
            if (OVRInput.GetDown(OVRInput.RawButton.LThumbstick)) OnLeftThumbstick?.Invoke();
            
        }
    }
}
