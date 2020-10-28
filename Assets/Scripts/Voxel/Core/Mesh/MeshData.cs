using UnityEngine;

namespace CodeBlaze.Voxel.Core.Mesh {

    public class MeshData {

        public MeshData(Vector3[] vertices, int[] triangles, Color32[] colors, Vector3[] normals) {
            Vertices = vertices;
            Triangles = triangles;
            Colors = colors;
            Normals = normals;
        }

        public Vector3[] Vertices { get; }
        public int[] Triangles { get; }
        public Color32[] Colors { get; }
        public Vector3[] Normals { get; }

    }

}