
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Trigger2to3
{
    public class T23_PickupHaptic : T23_ActionBase
    {
        public VRC_Pickup[] recievers;

        [Range(0f, 1f)] public float duration  = 0.3f;
        [Range(0f, 1f)] public float amplitude = 0.8f;
        [Range(0f, 1f)] public float frequency = 0.5f;

        protected override void OnAction()
        {
            for (int i = 0; i < recievers.Length; i++)
            {
                if (recievers[i])
                {
                    Execute(recievers[i]);
                }
            }
        }

        private void Execute(VRC_Pickup target)
        {
            VRC_Pickup.PickupHand hand = target.currentHand;
            if (hand == VRC_Pickup.PickupHand.None) return;
            VRCPlayerApi player = Networking.LocalPlayer;
            if (!Utilities.IsValid(player)) return;
            player.PlayHapticEventInHand(hand, duration, amplitude, frequency);
        }
    }
}
