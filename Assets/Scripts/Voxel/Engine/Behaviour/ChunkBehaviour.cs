using CodeBlaze.Voxel.Engine.Meshing;
using CodeBlaze.Voxel.Engine.Settings;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Voxel.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {
        
        private Mesh _mesh;
        private MeshRenderer _renderer;
        
        private void Awake() {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

        public void SetRenderSettings(ChunkRendererSettings settings) {
            _renderer.material = settings.Material;
            if (!settings.CastShadows) _renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Render(MeshData meshData) => meshData.Apply(_mesh);

    }

}