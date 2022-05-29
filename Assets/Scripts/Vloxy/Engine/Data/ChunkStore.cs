using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Chunk;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        public ChunkStoreAccessor Accessor { get; }
        
        private ChunkSettings _ChunkSettings;
        private ChunkDataScheduler _ChunkDataScheduler;

        private NativeHashMap<int3, Chunk> _Chunks;
        private HashSet<int3> _Claim;
        private List<int3> _Reclaim;

        public ChunkStore(ChunkDataScheduler chunkDataScheduler, ChunkSettings chunkSettings) {
            _ChunkDataScheduler = chunkDataScheduler;
            _ChunkSettings = chunkSettings;

            var viewRegionSize = _ChunkSettings.DrawDistance.CubedSize();
            _Chunks = new NativeHashMap<int3, Chunk>(_ChunkSettings.ChunkPageSize.CubedSize(), Allocator.Persistent);
            
            Accessor = new ChunkStoreAccessor(_Chunks, _ChunkSettings.ChunkSize);

            _Claim = new HashSet<int3>(viewRegionSize);
            _Reclaim = new List<int3>(viewRegionSize);
        }

        internal void GenerateChunks() {
            var jobs = new NativeList<int3>(_ChunkSettings.ChunkPageSize.CubedSize(), Allocator.TempJob);
            var data = new NativeList<ChunkData>(_ChunkSettings.ChunkPageSize.CubedSize(), Allocator.TempJob);
            
            // Prepare Job
            for (int x = -_ChunkSettings.ChunkPageSize; x <= _ChunkSettings.ChunkPageSize; x++) {
                for (int z = -_ChunkSettings.ChunkPageSize; z <= _ChunkSettings.ChunkPageSize; z++) {
                    for (int y = -_ChunkSettings.ChunkPageSize; y <= _ChunkSettings.ChunkPageSize; y++) {
                        var position = new int3(x, y, z) * _ChunkSettings.ChunkSize;
                        jobs.Add(position);
                        data.Add(VloxyProvider.Current.CreateChunkData());
                    }
                }
            }
            
            // Schedule Job
            _ChunkDataScheduler.Schedule(jobs, data);
            
            // Complete Job
            var result = _ChunkDataScheduler.Complete();

            for (int i = 0; i < jobs.Length; i++) {
                var position = jobs[i];
                _Chunks.Add(position, VloxyProvider.Current.CreateChunk(position, result[position]));
            }

            // Dispose Job
            _ChunkDataScheduler.Dispose();
            jobs.Dispose();
            data.Dispose();

#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkStore>("Chunks Created : " + _Chunks.Count());
#endif
        }
        
        internal (List<int3>, List<int3>) ViewRegionUpdate(int3 newFocusChunkCoord, int3 focusChunkCoord) {
            var initial = focusChunkCoord == new int3(1, 1, 1) * int.MinValue;
            var diff = newFocusChunkCoord - focusChunkCoord;
            
            _Reclaim.Clear();
            _Claim.Clear();
            
            if (!initial.AndReduce()) {
                UpdateReclaim(focusChunkCoord, -diff);
                UpdateClaim(newFocusChunkCoord, diff);
            } else {
                InitialRegion(newFocusChunkCoord);
            }
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkStore>($"Claim : {_Claim.Count()}, Reclaim : {_Reclaim.Count}");
#endif

            return (_Claim.ToList(), _Reclaim);
        }

        internal void Dispose() {
            foreach (var pair in _Chunks) {
                pair.Value.Data.Dispose();
            }
            
            _Chunks.Dispose();
        }

        private void InitialRegion(int3 focus) {
            for (int x = -_ChunkSettings.DrawDistance; x <= _ChunkSettings.DrawDistance; x++) {
                for (int z = -_ChunkSettings.DrawDistance; z <= _ChunkSettings.DrawDistance; z++) {
                    for (int y = -_ChunkSettings.DrawDistance; y <= _ChunkSettings.DrawDistance; y++) {
                        _Claim.Add(focus + new int3(x, y, z) * _ChunkSettings.ChunkSize);
                    }
                }
            }
        }

        private void UpdateClaim(int3 focus, int3 diff) {
            var distance = _ChunkSettings.DrawDistance;
            var size = _ChunkSettings.ChunkSize;
            
            for (int i = -distance; i <= distance; i++) {
                for (int j = -distance; j <= distance; j++) {
                    if (diff.x != 0) {
                        _Claim.Add(new int3(focus + new int3(diff.x * distance, i * size.y, j * size.z)));
                    }

                    if (diff.y != 0) {
                        _Claim.Add(new int3(focus + new int3(i * size.x, diff.y * distance, j * size.z)));
                    }

                    if (diff.z != 0) {
                        _Claim.Add(new int3(focus + new int3(i * size.x, j * size.y, diff.z * distance)));
                    }
                }
            }
        }
        
        private void UpdateReclaim(int3 focus, int3 diff) {
            var distance = _ChunkSettings.DrawDistance;
            var size = _ChunkSettings.ChunkSize;
            
            for (int i = -distance; i <= distance; i++) {
                for (int j = -distance; j <= distance; j++) {
                    if (diff.x != 0) {
                        _Reclaim.Add(new int3(focus + new int3(diff.x * distance, i * size.y, j * size.z)));
                    }

                    if (diff.y != 0) {
                        _Reclaim.Add(new int3(focus + new int3(i * size.x, diff.y * distance, j * size.z)));
                    }

                    if (diff.z != 0) {
                        _Reclaim.Add(new int3(focus + new int3(i * size.x, j * size.y, diff.z * distance)));
                    }
                }
            }
        }

    }

}