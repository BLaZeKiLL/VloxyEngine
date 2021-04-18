using System;
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

        private INoiseProfile<B> _NoiseProfile;
        private ChunkSettings _ChunkSettings;

        private int _ViewRegionSize;

        public ChunkStore(INoiseProfile<B> noiseProfile) {
            _NoiseProfile = noiseProfile;
            _ChunkSettings = VoxelProvider<B>.Current.Settings.Chunk;

            _ViewRegionSize =
                (2 * _ChunkSettings.DrawDistance + 1) *
                (2 * _ChunkSettings.DrawDistance + 1) *
                (2 * _ChunkSettings.DrawDistance + 1);

            var pageSize =
                (2 * _ChunkSettings.ChunkPageSize + 1) *
                (2 * _ChunkSettings.ChunkPageSize + 1) *
                (2 * _ChunkSettings.ChunkPageSize + 1);

            Chunks = new Dictionary<Vector3Int, Chunk<B>>(pageSize);
            ActiveChunks = new Dictionary<Vector3Int, Chunk<B>>(_ViewRegionSize);
        }

        public bool ContainsChunk(Vector3Int coord) => Chunks.ContainsKey(coord);

        public void GenerateChunks() {
            for (int x = -_ChunkSettings.ChunkPageSize; x < _ChunkSettings.ChunkPageSize; x++) {
                for (int z = -_ChunkSettings.ChunkPageSize; z < _ChunkSettings.ChunkPageSize; z++) {
                    for (int y = -_ChunkSettings.ChunkPageSize; y < _ChunkSettings.ChunkPageSize; y++) {
                        var pos = new Vector3Int(x, y, z) * _ChunkSettings.ChunkSize;
                        var chunk = VoxelProvider<B>.Current.CreateChunk(pos);
                        chunk.Data =
                            VoxelProvider<B>.Current.ChunkCreationPipeLine.Apply(
                                _NoiseProfile.GenerateChunkData(chunk));

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

        public (List<MeshBuildJobData<B>> Claim, List<Chunk<B>> Reclaim) ViewRegionUpdate(Vector3Int newFocusCoords, Vector3Int focusCoords) {
            var faces = newFocusCoords - focusCoords;
            
            var reclaim = GetVectorList(focusCoords, -faces)
                          .Where(ContainsChunk)
                          .Where(x => Chunks[x].State != ChunkState.INACTIVE)
                          .Select(GetChunk)
                          .ToList();

            var claim = GetVectorList(newFocusCoords, faces)
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
        
        public List<MeshBuildJobData<B>> InitialViewRegion(Vector3Int focus) {
            var current = new List<MeshBuildJobData<B>>(_ViewRegionSize);

            for (int x = -_ChunkSettings.DrawDistance; x <= _ChunkSettings.DrawDistance; x++) {
                for (int z = -_ChunkSettings.DrawDistance; z <= _ChunkSettings.DrawDistance; z++) {
                    for (int y = -_ChunkSettings.DrawDistance; y <= _ChunkSettings.DrawDistance; y++) {
                        var position = focus + new Vector3Int(x, y, z) * _ChunkSettings.ChunkSize;
                        var chunk = GetChunk(position);

                        if (chunk.Data == null) continue;

                        ActiveChunks.Add(position, chunk);
                        current.Add(GetChunkJobData(position));
                    }
                }
            }

            return current;
        }

        private IEnumerable<Vector3Int> GetVectorList(Vector3Int center, Vector3Int faces) {
            var list = new HashSet<Vector3Int>();
            var distance = _ChunkSettings.DrawDistance;
            var size = _ChunkSettings.ChunkSize;

            var actions = new List<Action<int, int>>(3);
            
            if (faces.x != 0) actions.Add((i, j) => list.Add(center + new Vector3Int(faces.x * distance, i * size.y, j * size.z)));
            if (faces.y != 0) actions.Add((i, j) => list.Add(center + new Vector3Int(i * size.x, faces.y * distance, j * size.z)));
            if (faces.z != 0) actions.Add((i, j) => list.Add(center + new Vector3Int(i * size.x, j * size.y, faces.z * distance)));
            
            for (int i = -distance; i <= distance; i++) {
                for (int j = -distance; j <= distance; j++) {
                    actions.ForEach(action => action(i,j));
                }
            }
            
            return list;
        }

        private MeshBuildJobData<B> GetChunkJobData(Vector3Int position) {
            var px = position + Vector3Int.right * _ChunkSettings.ChunkSize;
            var py = position + Vector3Int.up * _ChunkSettings.ChunkSize;
            var pz = position + new Vector3Int(0, 0, 1) * _ChunkSettings.ChunkSize;
            var nx = position + Vector3Int.left * _ChunkSettings.ChunkSize;
            var ny = position + Vector3Int.down * _ChunkSettings.ChunkSize;
            var nz = position + new Vector3Int(0, 0, -1) * _ChunkSettings.ChunkSize;
            var chunk = Chunks[position];

            chunk.State = ChunkState.PROCESSING;

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
            var px = chunk.Position + Vector3Int.right * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(px) ? Chunks[px] : null;
        }

        public Chunk<B> GetNeighborPY(Chunk<B> chunk) {
            var py = chunk.Position + Vector3Int.up * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(py) ? Chunks[py] : null;
        }

        public Chunk<B> GetNeighborPZ(Chunk<B> chunk) {
            var pz = chunk.Position + new Vector3Int(0, 0, 1) * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(pz) ? Chunks[pz] : null;
        }

        public Chunk<B> GetNeighborNX(Chunk<B> chunk) {
            var nx = chunk.Position + Vector3Int.left * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(nx) ? Chunks[nx] : null;
        }

        public Chunk<B> GetNeighborNY(Chunk<B> chunk) {
            var ny = chunk.Position + Vector3Int.down * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(ny) ? Chunks[ny] : null;
        }

        public Chunk<B> GetNeighborNZ(Chunk<B> chunk) {
            var nz = chunk.Position + new Vector3Int(0, 0, -1) * _ChunkSettings.ChunkSize;

            return Chunks.ContainsKey(nz) ? Chunks[nz] : null;
        }

        #endregion

    }

}