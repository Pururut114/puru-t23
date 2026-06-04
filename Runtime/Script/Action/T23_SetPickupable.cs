
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Trigger2to3
{
    public class T23_SetPickupable : T23_ActionBase
    {
        public GameObject[] recievers;

        public bool pickupable = true;
        public bool dropIfDisabling = true;

        protected override void OnAction()
        {
            for (int i = 0; i < recievers.Length; i++)
            {
                if (recievers[i] == null) continue;
                var pickup = (VRC_Pickup)recievers[i].GetComponent(typeof(VRC_Pickup));
                if (pickup == null) continue;
                if (!pickupable && dropIfDisabling)
                    pickup.Drop();
                pickup.pickupable = pickupable;
            }
        }
    }
}
