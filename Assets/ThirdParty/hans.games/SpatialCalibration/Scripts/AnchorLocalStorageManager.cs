using System;
using System.Collections.Generic;
using UnityEngine;

namespace hans.games.spatialcalibration
{
    /// <summary>
    /// Manages access to spatial anchors in playerprefs
    /// </summary>
    public class AnchorLocalStorageManager : MonoBehaviour
    {
        private const string Prefix = "spatialUuid_";
        private const string CountUuidsPlayerPref = Prefix + "Count";
        private List<Guid> uuids = new();

        private void Start()
        {
            Debug.Log($"UuidCount: {PlayerPrefs.GetInt(CountUuidsPlayerPref, 0)}");
        }

        public void SaveUuid(string _uuid)
        {
            int UuidCount = PlayerPrefs.GetInt(CountUuidsPlayerPref, 0);
            PlayerPrefs.SetString(Prefix + UuidCount, _uuid);
            PlayerPrefs.SetInt(CountUuidsPlayerPref, ++UuidCount);
            Debug.Log($"Saved Uuid: {UuidCount - 1}");
        }

        public void RemoveUuid(string _uuid)
        {
            int uuidCount = PlayerPrefs.GetInt(CountUuidsPlayerPref, 0);
            
            for (int i = 0; i < uuidCount; i++)
            {
                string key = Prefix + i;
                string value = PlayerPrefs.GetString(key, "");
                if (value.Equals(_uuid))
                {
                    string lastKey = Prefix + (uuidCount - 1);
                    string lastValue = PlayerPrefs.GetString(lastKey);
                    PlayerPrefs.SetString(key, lastValue);
                    PlayerPrefs.DeleteKey(lastKey);

                    uuidCount--;
                    if (uuidCount < 0) uuidCount = 0;
                    PlayerPrefs.SetInt(CountUuidsPlayerPref, uuidCount);
                    break;
                }
            }
        }

        public List<Guid> GetUuids()
        {
            if (!PlayerPrefs.HasKey(CountUuidsPlayerPref))
            {
                PlayerPrefs.SetInt(CountUuidsPlayerPref, 0);
                return null;
            }

            uuids.Clear();

            int UuidCount = PlayerPrefs.GetInt(CountUuidsPlayerPref);

            for (int i = 0; i < UuidCount; i++)
            {
                string key = Prefix + i;

                if (!PlayerPrefs.HasKey(key))
                    continue;

                string value = PlayerPrefs.GetString(key);

                uuids.Add(new Guid(value));
            }

            return uuids;
        }
        
        
    }
}
