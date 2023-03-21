using System;

using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Priority_Queue;

using Unity.Collections;

using UnityEngine;

namespace CodeBlaze.Sandbox {

    public class Vector3Node : FastPriorityQueueNode {

        public Vector3Int Value { get; set; }

        public static bool operator ==(Vector3Node a, Vector3Node b) {
            return a.Value.x == b.Value.x && a.Value.y == b.Value.y && a.Value.z == b.Value.z;
        }

        public static bool operator !=(Vector3Node a, Vector3Node b) {
            return !(a == b);
        }

    }
    
    public class Sandbox : MonoBehaviour {

        private FastPriorityQueue<Vector3Node> _Queue = new(16);

        private void Start() {
            var node = new Vector3Node { Value = new Vector3Int(1,2,3) };
            _Queue.Enqueue(node, 1);
            Debug.Log(_Queue.Contains(new Vector3Node { Value = new Vector3Int(1,2,3) }));
        }

    }

}