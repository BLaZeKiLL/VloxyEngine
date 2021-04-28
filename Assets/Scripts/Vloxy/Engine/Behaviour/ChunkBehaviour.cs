using System;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Behaviour {

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ChunkBehaviour : MonoBehaviour {
        
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Vector3Int _halfChunkSize;

#if UNITY_EDITOR
        private MeshData _meshData;
#endif
        
        private void Awake() {
            _mesh = GetComponent<MeshFilter>().mesh;
            _renderer = GetComponent<MeshRenderer>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            var index = 0;
            var style = new GUIStyle {normal = {textColor = Color.magenta}};
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + _halfChunkSize, _halfChunkSize * 2);

            foreach (var vertex in _meshData.Vertices) {
                Handles.Label(transform.position + vertex, $"{_meshData.AO[index++]}", style);
            }
        }
#endif
        
        public void SetRenderSettings(RendererSettings settings, Vector3Int halfChunkSize) {
            _halfChunkSize = halfChunkSize;
            _renderer.material = settings.Material;
            if (!settings.CastShadows) _renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void Render(MeshData meshData) {
#if UNITY_EDITOR
            _meshData = meshData;
#endif
            meshData.Apply(_mesh);
        }

    }

}