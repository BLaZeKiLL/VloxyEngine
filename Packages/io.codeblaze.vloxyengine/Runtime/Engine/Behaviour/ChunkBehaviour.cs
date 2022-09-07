using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {
        
        private Mesh _Mesh;
        private MeshRenderer _Renderer;

        private void Awake() {
            _Mesh = GetComponent<MeshFilter>().mesh;
            _Renderer = GetComponent<MeshRenderer>();
        }

        public void SetRenderSettings(RendererSettings settings) {
            _Renderer.material = settings.Material;
            if (!settings.CastShadows) _Renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public Mesh Mesh() => _Mesh;

    }

}