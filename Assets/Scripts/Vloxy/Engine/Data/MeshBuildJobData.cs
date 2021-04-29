using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class MeshBuildJobData<B> where B : IBlock {

        private readonly Chunk<B>[] Data;
        private readonly Vector3Int KeySize = 3 * Vector3Int.one;
        private readonly Vector3Int ChunkSize = VoxelProvider<B>.Current.Settings.Chunk.ChunkSize;

        public MeshBuildJobData(Chunk<B>[] data) {
            Data = data;
        }

        public Chunk<B> GetChunk() => Data[13];
        
        // ChunkSize = 2 then anything mod 1 becomes 0 hence it does not work
        // ChunkSize = 1 then divie by zero error occurs
        public B GetBlock(Vector3Int pos) { // -1 to ChunkSize
            var key = Vector3Int.zero;

            for (int index = 0; index < 3; index++) {
                if (pos[index] >= 0 && pos[index] < ChunkSize[index]) continue;

                key[index] += pos[index] % (ChunkSize[index] - 1);
                pos[index] = pos[index] - key[index] * ChunkSize[index];
            }

            var chunk = Data[KeySize.Flatten(key + Vector3Int.one)];

            return chunk?.Data == null ? default : chunk.Data.GetBlock(pos.x, pos.y, pos.z);
        }

    }

}