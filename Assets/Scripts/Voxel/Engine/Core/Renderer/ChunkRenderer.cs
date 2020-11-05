using System.Threading.Tasks;

using CodeBlaze.Voxel.Engine.Core.Mesher;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Voxel.Engine.Core.Renderer {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkRenderer : MonoBehaviour {
        
        private Mesh _mesh;
        private MeshRenderer _renderer;
        
        private void Awake() {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

        public void SetRenderSettings(Material material, bool shadows) {
            _renderer.material = material;
            if (!shadows) _renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Render(MeshData meshData) => meshData.Apply(_mesh);

    }

}