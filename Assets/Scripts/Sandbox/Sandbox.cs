using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;

using UnityEngine;

namespace CodeBlaze.Sandbox {

    public struct Test {

        public int Member;

    }
    
    public class Sandbox : MonoBehaviour {

        private NativeHashMap<int, Test> Map;

        private void Awake() {
            Map = new NativeHashMap<int, Test>(10, Allocator.Persistent);
        }

        private void Start() {
            var x = new Test {
                Member = 5
            };
            
            Map.Add(0, x);

            // We Get A Copy Here
            var y = Map[0];

            // Update the copy
            y.Member = 10;

            // Update the map
            Map[0] = y;

            var z = Map[0];
            
            VloxyLogger.Info<Sandbox>($"X = {x.Member}");
            VloxyLogger.Info<Sandbox>($"Y = {y.Member}");
            VloxyLogger.Info<Sandbox>($"Z = {z.Member}");
        }

        private void OnDestroy() {
            Map.Dispose();
        }

    }

}