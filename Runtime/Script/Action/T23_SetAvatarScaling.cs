
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Trigger2to3
{
    public class T23_SetAvatarScaling : T23_ActionBase
    {
        public bool setSpecificHeight = true;

        [Range(0.1f, 100f)]
        public float eyeHeight = 1.8f;

        public bool allowManualScaling = true;

        [Range(0.2f, 5f)]
        public float minHeight = 0.2f;

        [Range(0.2f, 5f)]
        public float maxHeight = 5f;

        protected override void OnAction()
        {
            var lp = Networking.LocalPlayer;
            if (lp == null) return;

            if (setSpecificHeight)
            {
                // SetManualAvatarScalingAllowed(false) must come BEFORE SetAvatarEyeHeightByMeters —
                // otherwise VRChat resets the height when manual scaling is later disabled (SDK bug)
                lp.SetManualAvatarScalingAllowed(false);
                lp.SetAvatarEyeHeightByMeters(Mathf.Clamp(eyeHeight, 0.1f, 100f));
            }
            else
            {
                lp.SetManualAvatarScalingAllowed(allowManualScaling);
                lp.SetAvatarEyeHeightMinimumByMeters(Mathf.Clamp(minHeight, 0.2f, 5f));
                lp.SetAvatarEyeHeightMaximumByMeters(Mathf.Clamp(maxHeight, 0.2f, 5f));
            }
        }
    }
}
