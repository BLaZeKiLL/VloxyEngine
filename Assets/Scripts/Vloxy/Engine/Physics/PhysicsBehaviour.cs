using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Physics {

    public class PhysicsBehaviour : MonoBehaviour {

        private Transform _transform;
        
        public void Attach(Transform trans) {
            _transform = trans;
            WorldPhysicsManager.OnPhysicsUpdate += OnPhysicsUpdate;
        }

        private void OnPhysicsUpdate(object sender, WorldPhysicsManager.PhysicsUpdateArgs e) {
            transform.position += Vector3.down * (e.Gravity * Time.fixedDeltaTime);
        }

        public Transform DeAttach() {
            WorldPhysicsManager.OnPhysicsUpdate -= OnPhysicsUpdate;

            return _transform;
        }

    }

}