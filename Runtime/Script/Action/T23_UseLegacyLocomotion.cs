
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Trigger2to3
{
    public class T23_UseLegacyLocomotion : T23_ActionBase
    {
        protected override void OnAction()
        {
            // UseLegacyLocomotion() убран из VRChat SDK — этот компонент ничего не делает
            Debug.LogWarning("[T23] UseLegacyLocomotion: API удалён из VRChat SDK, действие пропущено.");
        }
    }
}
