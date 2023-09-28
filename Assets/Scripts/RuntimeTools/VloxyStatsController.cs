using System.Text;

using CodeBlaze.Vloxy.Engine.World;

using Tayx.Graphy.Utils.NumString;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UI;

namespace CodeBlaze.Vloxy.Demo.Debug {

    public class VloxyStatsController : MonoBehaviour {

        [SerializeField] private VloxyWorld World;

        [SerializeField] private Text Coords;
        [SerializeField] private Text ChunkCoords;
        [SerializeField] private Text Data;
        [SerializeField] private Text Mesh;
        [SerializeField] private Text Bake;

        private StringBuilder sb;

        private void Start() {
            sb = new StringBuilder();
        }

        private void Update() {
            UpdateCoords(Vector3Int.RoundToInt(World.Focus.position));
            UpdateChunkCoords(World.FocusChunkCoord / 32);
            UpdateData(World.Scheduler.DataQueueCount, World.Scheduler.DataAvgTiming);
            UpdateMesh(World.Scheduler.MeshQueueCount, World.Scheduler.MeshAvgTiming);
            UpdateBake(World.Scheduler.BakeQueueCount, World.Scheduler.BakeAvgTiming);
        }

        private void UpdateCoords(Vector3Int coords) {
            sb.Clear();

            sb.Append("Focus Block Coordinates : ")
              .Append("X = ").Append(coords.x.ToStringNonAlloc())
              .Append(", Y = ").Append(coords.y.ToStringNonAlloc())
              .Append(", Z = ").Append(coords.z.ToStringNonAlloc());

            Coords.text = sb.ToString();
        }
        
        private void UpdateChunkCoords(int3 chunk_coords) {
            sb.Clear();

            sb.Append("Focus Chunk Coordinates : ")
              .Append("X = ").Append(chunk_coords.x.ToStringNonAlloc())
              .Append(", Y = ").Append(chunk_coords.y.ToStringNonAlloc())
              .Append(", Z = ").Append(chunk_coords.z.ToStringNonAlloc());

            ChunkCoords.text = sb.ToString();
        }

        private void UpdateData(int queue_count, float avg) {
            sb.Clear();

            sb.Append("Data Queue : ").Append(queue_count.ToStringNonAlloc())
              .Append(", Average : ").Append(avg.ToString("F3")).Append("ms");

            Data.text = sb.ToString();
        }

        private void UpdateMesh(int queue_count, float avg) {
            sb.Clear();

            sb.Append("Mesh Queue : ").Append(queue_count.ToStringNonAlloc())
              .Append(", Average : ").Append(avg.ToString("F3")).Append("ms");

            Mesh.text = sb.ToString();
        }

        private void UpdateBake(int queue_count, float avg) {
            sb.Clear();

            sb.Append("Bake Queue : ").Append(queue_count.ToStringNonAlloc())
              .Append(", Average : ").Append(avg.ToString("F3")).Append("ms");

            Bake.text = sb.ToString();
        }

    }

}