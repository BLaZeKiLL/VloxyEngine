using System;
using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Jobs.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkManager {

        internal ChunkAccessor Accessor { get; }
        internal ChunkStore Store { get; }
        
        private ChunkState _ChunkState;
        private ChunkSettings _ChunkSettings;
        
        private ISet<int3> _Claim;
        private ISet<int3> _Reclaim;

        public ChunkManager(VloxySettings settings, ChunkState chunkState) {
            _ChunkSettings = settings.Chunk;
            
            _ChunkState = chunkState;
            
            Store = new ChunkStore(
                int3.zero, 
                _ChunkSettings.ChunkSize,
                _ChunkSettings.PageSize,
                settings.Noise.Height
            );

            Accessor = new ChunkAccessor(Store.Chunks, _ChunkSettings.ChunkSize);

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
            
            Update(_Claim, newFocusChunkCoord, diff, _ChunkSettings.PageSize, ChunkState.State.STREAMING);
            Update(_Reclaim, focusChunkCoord, -diff, _ChunkSettings.PageSize, ChunkState.State.UNLOAD);

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
                Update(_Claim, newFocusChunkCoord, diff, _ChunkSettings.DrawDistance, ChunkState.State.MESHING);
                Update(_Reclaim, focusChunkCoord, -diff, _ChunkSettings.DrawDistance, ChunkState.State.LOADED);
            } else {
                InitialRegion(newFocusChunkCoord);
            }
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkManager>($"View Claim : {_Claim.Count()}, View Reclaim : {_Reclaim.Count}");
#endif

            return (_Claim.ToList(), _Reclaim.ToList());
        }

        internal void Dispose() {
            Store.Dispose();
        }

        private void InitialRegion(int3 focus) {
            for (int x = -_ChunkSettings.DrawDistance; x <= _ChunkSettings.DrawDistance; x++) {
                for (int z = -_ChunkSettings.DrawDistance; z <= _ChunkSettings.DrawDistance; z++) {
                    for (int y = -_ChunkSettings.DrawDistance; y <= _ChunkSettings.DrawDistance; y++) {
                        Add(_Claim, focus + new int3(x, y, z) * _ChunkSettings.ChunkSize, ChunkState.State.MESHING);
                    }
                }
            }
        }

        private void Update(ISet<int3> set, int3 focus, int3 diff, int distance, ChunkState.State state) {
            var size = _ChunkSettings.ChunkSize;
            
            for (int i = -distance; i <= distance; i++) {
                for (int j = -distance; j <= distance; j++) {
                    if (diff.x != 0) {
                        Add(set, new int3(focus + new int3(diff.x * distance, i * size.y, j * size.z)), state);
                    }

                    if (diff.y != 0) {
                        Add(set, new int3(focus + new int3(i * size.x, diff.y * distance, j * size.z)), state);
                    }

                    if (diff.z != 0) {
                        Add(set, new int3(focus + new int3(i * size.x, j * size.y, diff.z * distance)), state);
                    }
                }
            }
        }

        private void Add(ISet<int3> set, int3 position, ChunkState.State state) {
            if (!Store.ContainsChunk(position)) {
                if (state != ChunkState.State.STREAMING) {
#if VLOXY_LOGGING
                    VloxyLogger.Warn<ChunkManager>($"Invalid Claim/Reclaim : Position {position}, State {state}");
#endif
                    
                    return;
                }

                set.Add(position);
                _ChunkState.AddState(position, state);

                return;
            }

            set.Add(position);
            _ChunkState.SetState(position, state);
        }

    }

}