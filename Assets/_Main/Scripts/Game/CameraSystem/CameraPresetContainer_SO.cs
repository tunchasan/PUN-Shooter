using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.MyCompany.MyGame.Camera
{
    [CreateAssetMenu]
    public class CameraPresetContainer_SO : ScriptableObject
    {
        #region Private Serialized Fields

        [Tooltip("Stores Character Cameras Presets")] 
        [SerializeField] private List<CameraPreset> presets = new List<CameraPreset>();

        #endregion

        #region Public Methods

        public CameraPreset Find(Enums.PlayerStates state)
        {
            var isExist = presets.Any(preset => preset.GetState == state);
            
            return isExist ? presets.
                FirstOrDefault((elem) => elem.GetState == state) : null;
        }

        #endregion
    }
}