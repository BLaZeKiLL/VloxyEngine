using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkStore<B> where B : IBlock {

        protected Dictionary<Vector3Int, Chunk<B>> Chunks;

        private Dictionary<Vector3Int, Chunk<B>> ActiveChunks;

        private INoiseProfile<B> _noiseProfile;
        private ChunkSettings _chunkSettings;
        private int _viewRegionSize;
        private int _pageSize;

        public ChunkStore(INoiseProfile<B> noiseProfile, ChunkSettings chunkSettings) {
            _noiseProfile = noiseProfile;
            _chunkSettings = chunkSettings;
            
            _viewRegionSize = 
                (2 *  _chunkSettings.DrawDistance + 1) *
                (2 *  _chunkSettings.DrawDistance + 1) *
                (2 *  _chunkSettings.DrawDistance + 1);
            
            _pageSize = 
                (2 *  _chunkSettings.ChunkPageSize + 1) *
                (2 *  _chunkSettings.ChunkPageSize + 1) *
                (2 *  _chunkSettings.ChunkPageSize + 1);
            
            Chunks = new Dictionary<Vector3Int, Chunk<B>>(_pageSize);
            ActiveChunks = new Dictionary<Vector3Int, Chunk<B>>(_viewRegionSize);
        }

        public bool ContainsChunk(Vector3Int coord) => Chunks.ContainsKey(coord);
        
        public void GenerateChunks() {
            for (int x = -_chunkSettings.ChunkPageSize; x < _chunkSettings.ChunkPageSize; x++) {
                for (int z = -_chunkSettings.ChunkPageSize; z < _chunkSettings.ChunkPageSize; z++) {
                    for (int y = -_chunkSettings.ChunkPageSize; y < _chunkSettings.ChunkPageSize; y++) {
                        var pos = new Vector3Int(x, y, z) * _chunkSettings.ChunkSize;
                        var chunk = VoxelProvider<B>.Current.CreateChunk(pos);
                        chunk.Data = VoxelProvider<B>.Current.ChunkCreationPipeLine.Apply(_noiseProfile.GenerateChunkData(chunk));
                        
                        Chunks.Add(pos, chunk);
                    }
                }
            }

            CBSL.Logging.Logger.Info<ChunkStore<B>>("Chunks Created : " + Chunks.Count);
        }

        public void ActiveChunkUpdate() {
            foreach (var chunk in ActiveChunks.Values) {
                chunk.Update();
            }
        }

        public (List<MeshBuildJobData<B>> Claim, List<Chunk<B>> Reclaim) ViewRegionUpdate(Vector3Int focus) {
            var current = new List<Vector3Int>(_viewRegionSize);

            for (int x = -_chunkSettings.DrawDistance; x <= _chunkSettings.DrawDistance; x++) {
                for (int z = -_chunkSettings.DrawDistance; z <= _chunkSettings.DrawDistance; z++) {
                    for (int y = -_chunkSettings.DrawDistance; y <= _chunkSettings.DrawDistance; y++) {
                        current.Add(focus + new Vector3Int(x, y, z) * _chunkSettings.ChunkSize);
                    }
                }
            }

            var reclaim = ActiveChunks.Keys
                        .Where(x => !current.Contains(x))
                        .Where(ContainsChunk)
                        .Where(x => Chunks[x].State == ChunkState.ACTIVE)
                        .Select(GetChunk)
                        .ToList();

            var claim = current
                        .Where(x => !ActiveChunks.Keys.Contains(x))
                        .Where(ContainsChunk)
                        .Where(x => Chunks[x].State == ChunkState.INACTIVE)
                        .Where(x => Chunks[x].Data != null)
                        .Select(GetChunkJobData)
                        .ToList();

            reclaim.ForEach(chunk => ActiveChunks.Remove(chunk.Position));
            claim.ForEach(jobData => ActiveChunks.Add(jobData.Chunk.Position, jobData.Chunk));
            
            CBSL.Logging.Logger.Info<ChunkStore<B>>($"Claim : {claim.Count}, Reclaim : {reclaim.Count}");

            return (claim, reclaim);
        }
        
        private MeshBuildJobData<B> GetChunkJobData(Vector3Int position) {
            var px = position + Vector3Int.right * _chunkSettings.ChunkSize;
            var py = position + Vector3Int.up * _chunkSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * _chunkSettings.ChunkSize;
            var nx = position + Vector3Int.left * _chunkSettings.ChunkSize;
            var ny = position + Vector3Int.down * _chunkSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * _chunkSettings.ChunkSize;
            var chunk = Chunks[position];

            chunk.State = ChunkState.QUEUED;
            
            return new MeshBuildJobData<B> {
                Chunk = chunk,
                ChunkPX = Chunks.ContainsKey(px) ? Chunks[px] : null,
                ChunkPY = Chunks.ContainsKey(py) ? Chunks[py] : null,
                ChunkPZ = Chunks.ContainsKey(pz) ? Chunks[pz] : null,
                ChunkNX = Chunks.ContainsKey(nx) ? Chunks[nx] : null,
                ChunkNY = Chunks.ContainsKey(ny) ? Chunks[ny] : null,
                ChunkNZ = Chunks.ContainsKey(nz) ? Chunks[nz] : null
            };
        }

        public Chunk<B> GetChunk(Vector3Int coord) => Chunks[coord];

        #region Neighbors
        public Chunk<B> GetNeighborPX(Chunk<B> chunk) {
            var px = chunk.Position + Vector3Int.right * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }
        
        public Chunk<B> GetNeighborPY(Chunk<B> chunk) {
            var py = chunk.Position + Vector3Int.up * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }
        
        public Chunk<B> GetNeighborPZ(Chunk<B> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }
        
        public Chunk<B> GetNeighborNX(Chunk<B> chunk) {
            var nx = chunk.Position + Vector3Int.left * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }
        
        public Chunk<B> GetNeighborNY(Chunk<B> chunk) {
            var ny = chunk.Position + Vector3Int.down * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }
        
        public Chunk<B> GetNeighborNZ(Chunk<B> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * _chunkSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }
        
        #endregion

    }

}