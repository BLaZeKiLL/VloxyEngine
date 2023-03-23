using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkManager {

        internal ChunkStore Store { get; }
        
        private ChunkSettings _ChunkSettings;
        
        private ISet<int3> _Claim;
        private ISet<int3> _Reclaim;

        public ChunkManager(VloxySettings settings) {
            _ChunkSettings = settings.Chunk;

            Store = new ChunkStore(settings);
            
            var viewRegionSize = _ChunkSettings.DrawDistance.CubedSize();
            
            _Claim = new HashSet<int3>(viewRegionSize);
            _Reclaim = new HashSet<int3>(viewRegionSize);
        }

        internal (List<int3>, List<int3>) ChunkRegionUpdate(int3 newFocusChunkCoord, int3 focusChunkCoord) {
            var initial = focusChunkCoord == new int3(1, 1, 1) * int.MinValue;
            var diff = newFocusChunkCoord - focusChunkCoord;
            
            if (initial.AndReduce()) return (null, null);
            
            _Reclaim.Clear();
            _Claim.Clear();
            
            Update(_Claim, newFocusChunkCoord, diff, _ChunkSettings.LoadDistance);
            Update(_Reclaim, focusChunkCoord, -diff, _ChunkSettings.LoadDistance);

#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkManager>($"Data Claim : {_Claim.Count()}, Data Reclaim : {_Reclaim.Count}");
#endif
            return (_Claim.ToList(), _Reclaim.ToList());
        }

        internal (List<int3>, List<int3>) ViewRegionUpdate(int3 newFocusChunkCoord, int3 focusChunkCoord) {
            var initial = focusChunkCoord == new int3(1, 1, 1) * int.MinValue;
            var diff = newFocusChunkCoord - focusChunkCoord;
            
            _Reclaim.Clear();
            _Claim.Clear();
            
            if (!initial.AndReduce()) {
                Update(_Claim, newFocusChunkCoord, diff, _ChunkSettings.DrawDistance);
                Update(_Reclaim, focusChunkCoord, -diff, _ChunkSettings.DrawDistance);
            } else {
                InitialViewRegion(newFocusChunkCoord);
            }
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkManager>($"View Claim : {_Claim.Count()}, View Reclaim : {_Reclaim.Count}");
#endif

            return (_Claim.ToList(), _Reclaim.ToList());
        }

        internal void Dispose() {
            Store.Dispose();
        }

        internal NativeArray<int3> InitialChunkRegion(Allocator handle) {
            var size = _ChunkSettings.LoadDistance;

            var result = new NativeArray<int3>(size.CubedSize(), handle);
            var index = 0;
             
            for (int x = -size; x <= size; x++) {
                for (int z = -size; z <= size; z++) {
                    for (int y = -size; y <= size; y++) {
                        var position = new int3(x, y, z) * _ChunkSettings.ChunkSize;
                        result[index] = position;
                        index++;
                    }
                }
            }

            return result;
        }

        private void InitialViewRegion(int3 focus) {
            for (int x = -_ChunkSettings.DrawDistance; x <= _ChunkSettings.DrawDistance; x++) {
                for (int z = -_ChunkSettings.DrawDistance; z <= _ChunkSettings.DrawDistance; z++) {
                    for (int y = -_ChunkSettings.DrawDistance; y <= _ChunkSettings.DrawDistance; y++) {
                        _Claim.Add(focus + new int3(x, y, z) * _ChunkSettings.ChunkSize);
                    }
                }
            }
        }

        private void Update(ISet<int3> set, int3 focus, int3 diff, int distance) {
            var size = _ChunkSettings.ChunkSize;
            
            for (int i = -distance; i <= distance; i++) {
                for (int j = -distance; j <= distance; j++) {
                    if (diff.x != 0) {
                        var position = new int3(focus + new int3(diff.x * distance, i * size.y, j * size.z));
                        
                        set.Add(position);
                    }

                    if (diff.y != 0) {
                        var position = new int3(focus + new int3(i * size.x, diff.y * distance, j * size.z));
                        
                        set.Add(position);
                    }

                    if (diff.z != 0) {
                        var position = new int3(focus + new int3(i * size.x, j * size.y, diff.z * distance));
                        
                        set.Add(position);
                    }
                }
            }
        }

    }

}