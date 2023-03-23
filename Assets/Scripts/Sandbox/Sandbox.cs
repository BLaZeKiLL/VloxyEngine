using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Sandbox {

    public class Sandbox : MonoBehaviour {
        
        private void Start() {
            Debug.Log("SANDBOX");

            var set = new HashSet<int3>();

            var a = new int3(32, 32, -224);

            set.Add(a);
            
            Debug.Log(set.Contains(a));
            Debug.Log(set.Contains(new int3(32, 32, -224)));
        }

    }

}