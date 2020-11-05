using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Core.Mesher {

    public class MeshData {
        
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<int> Triangles = new List<int>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<Vector2> UV = new List<Vector2>();
        public readonly List<Vector2> UV2 = new List<Vector2>();
        public readonly List<Vector2> UV3 = new List<Vector2>();
        public readonly List<Vector2> UV4 = new List<Vector2>();
        public readonly List<Color32> Colors = new List<Color32>();

        public void Apply(Mesh mesh) {
            mesh.Clear();
            
            mesh.SetVertices(Vertices);
            mesh.SetTriangles(Triangles, 0);
            
            mesh.SetUVs(0, UV);
            mesh.SetUVs(1, UV2);
            mesh.SetUVs(2, UV3);
            mesh.SetUVs(3, UV4);
            
            mesh.SetColors(Colors);
            
            if (Normals.Count > 0)
                mesh.SetNormals(Normals);
            else
                mesh.RecalculateNormals();
        }

        public void Clear() {
            Vertices.Clear();
            Triangles.Clear();
            Normals.Clear();
            
            UV.Clear();
            UV2.Clear();
            UV3.Clear();
            UV4.Clear();
            
            Colors.Clear();
        }
        
    }

}