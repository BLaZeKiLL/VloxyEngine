using System;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Physics {

    public class WorldPhysicsManager : MonoBehaviour {

        public static event EventHandler<PhysicsUpdateArgs> OnPhysicsUpdate;

        private void FixedUpdate() {
            OnPhysicsUpdate?.Invoke(this, new PhysicsUpdateArgs {
                Gravity = -9.81f
            });
        }

        public class PhysicsUpdateArgs : EventArgs {

            public float Gravity { get; set; }

        }

    }

}