using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Jobs.Chunk;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        public ChunkStoreAccessor Accessor { get; }
        
        private ChunkSettings _ChunkSettings;
        private ChunkPageScheduler _chunkPageScheduler;

        private ChunkPage _Page;
        private HashSet<int3> _Claim;
        private List<int3> _Reclaim;
        
        public ChunkStore(ChunkPageScheduler chunkPageScheduler, ChunkSettings chunkSettings) {
            _chunkPageScheduler = chunkPageScheduler;
            _ChunkSettings = chunkSettings;

            var viewRegionSize = _ChunkSettings.DrawDistance.CubedSize();

            _Page = new ChunkPage(int3.zero, _ChunkSettings.ChunkPageSize, _ChunkSettings.ChunkSize);
            
            Accessor = new ChunkStoreAccessor(_Page.Chunks, _ChunkSettings.ChunkSize);

            _Claim = new HashSet<int3>(viewRegionSize);
            _Reclaim = new List<int3>(viewRegionSize);
        }

        internal void GenerateChunks() {
            // Schedule Job
            _chunkPageScheduler.Schedule(_Page);
            
            // Complete Job
            _chunkPageScheduler.Complete();

            // Dispose Job
            _chunkPageScheduler.Dispose();

#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkStore>($"Chunk Page : {_Page.Position},Chunks Created : {_Page.ChunkCount()}");
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
            _Page.Dispose();
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