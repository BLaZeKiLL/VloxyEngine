using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {
        
        private Mesh _mesh;
        private MeshRenderer _renderer;

        private void Awake() {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

        public void SetRenderSettings(RendererSettings settings) {
            _renderer.material = settings.Material;
            if (!settings.CastShadows) _renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public Mesh Mesh() => _mesh;

    }

}