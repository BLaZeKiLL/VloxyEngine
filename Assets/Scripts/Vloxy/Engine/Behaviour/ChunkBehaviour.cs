using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Mathematics;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {
        
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private int3 _halfChunkSize;

#if UNITY_EDITOR
        private MeshData _meshData;
#endif
        
        private void Awake() {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + _halfChunkSize.GetVector3Int(), _halfChunkSize.GetVector3Int() * 2);

            var index = 0;
            var style = new GUIStyle {normal = {textColor = Color.magenta}};
            foreach (var vertex in _meshData.Vertices) {
                Handles.Label(transform.position + vertex, $"{_meshData.UV2[index++].x}", style);
            }
        }
#endif
        
        public void SetRenderSettings(RendererSettings settings, int3 halfChunkSize) {
            _halfChunkSize = halfChunkSize;
            _renderer.material = settings.Material;
            if (!settings.CastShadows) _renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Render(MeshData meshData) {
#if UNITY_EDITOR
            _meshData = meshData;
#endif
            meshData.Apply(_mesh, _renderer.material);
        }

    }

}