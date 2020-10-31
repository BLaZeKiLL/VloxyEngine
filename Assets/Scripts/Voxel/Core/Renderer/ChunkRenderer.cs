using System.Threading.Tasks;

using CodeBlaze.Voxel.Core.Mesh;

using UnityEngine;

namespace CodeBlaze.Voxel.Core.Renderer {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkRenderer : MonoBehaviour {

        private MeshBuilder _builder;

        private MeshFilter _filter;
        private MeshRenderer _renderer;
        
        private void Awake() {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _builder = new MeshBuilder();
        }

        public void SetMaterial(Material material) {
            _renderer.material = material;
        }

        public async void Render<T>(Chunk<T> chunk) where T : IBlock {
            var mesh = _filter.mesh;
            Clear();
            
            var data = await Task.Run(() => _builder.GenerateMesh(chunk));
            
            Debug.Log("Mesh Building Done");

            mesh.vertices = data.Vertices;
            mesh.triangles = data.Triangles;
            mesh.colors32 = data.Colors;
            mesh.normals = data.Normals;
        }

        public void Clear() {
            _filter.mesh.Clear();
        }

    }

}