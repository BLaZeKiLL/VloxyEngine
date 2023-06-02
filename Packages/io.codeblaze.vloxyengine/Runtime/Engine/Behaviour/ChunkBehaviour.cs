using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {

        private MeshRenderer _Renderer;

        public Mesh Mesh { get; private set; }
        public MeshCollider Collider { get; private set; }

        private void Awake() {
            Mesh = GetComponent<MeshFilter>().mesh;
            _Renderer = GetComponent<MeshRenderer>();
        }

        public void Init(RendererSettings settings, MeshCollider m_collider) {
            _Renderer.sharedMaterials = settings.Materials;
            Collider = m_collider;

            if (!settings.CastShadows) _Renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

}