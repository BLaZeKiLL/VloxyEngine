using System;
using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Jobs.Page;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        internal ChunkStoreAccessor Accessor { get; }

        private ChunkState _ChunkState;
        private ChunkSettings _ChunkSettings;
        private ChunkPageScheduler _ChunkPageScheduler;

        private ChunkPage _Page;
        private ISet<int3> _Claim;
        private ISet<int3> _Reclaim;

        public ChunkStore(VloxySettings settings, ChunkState chunkState, ChunkPageScheduler chunkPageScheduler) {
            _ChunkPageScheduler = chunkPageScheduler;
            _ChunkSettings = settings.Chunk;
            _ChunkState = chunkState;

            var viewRegionSize = _ChunkSettings.DrawDistance.CubedSize();

            _Page = new ChunkPage(int3.zero, _ChunkSettings.PageSize, _ChunkSettings.ChunkSize);
            
            Accessor = new ChunkStoreAccessor(_Page.Chunks, _ChunkSettings.ChunkSize);

            _Claim = new HashSet<int3>(viewRegionSize);
            _Reclaim = new HashSet<int3>(viewRegionSize);
        }

        internal void GenerateChunks() {
            // Schedule Job
            _ChunkPageScheduler.Schedule(_Page);
            
            // Complete Job
            _ChunkPageScheduler.Complete();

            // Dispose Job
            _ChunkPageScheduler.Dispose();

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
                Update(_Reclaim, focusChunkCoord, -diff, ChunkState.State.ACTIVE);
                Update(_Claim, newFocusChunkCoord, diff, ChunkState.State.INACTIVE);
            } else {
                InitialRegion(newFocusChunkCoord);
            }
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkStore>($"New Focus : {newFocusChunkCoord}, Focus : {focusChunkCoord}");
            VloxyLogger.Info<ChunkStore>($"Claim : {_Claim.Count()}, Reclaim : {_Reclaim.Count}");
#endif

            return (_Claim.ToList(), _Reclaim.ToList());
        }

        internal void Dispose() {
            _Page.Dispose();
        }

        private void InitialRegion(int3 focus) {
            for (int x = -_ChunkSettings.DrawDistance; x <= _ChunkSettings.DrawDistance; x++) {
                for (int z = -_ChunkSettings.DrawDistance; z <= _ChunkSettings.DrawDistance; z++) {
                    for (int y = -_ChunkSettings.DrawDistance; y <= _ChunkSettings.DrawDistance; y++) {
                        Add(_Claim, focus + new int3(x, y, z) * _ChunkSettings.ChunkSize, ChunkState.State.INACTIVE);
                    }
                }
            }
        }

        private void Update(ISet<int3> set, int3 focus, int3 diff, ChunkState.State state) {
            var distance = _ChunkSettings.DrawDistance;
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
            if (!_Page.ContainsChunk(position)) return;
            if (_ChunkState.GetState(position) != state) return;

            set.Add(position);

            switch (state) {
                case ChunkState.State.INACTIVE:
                    _ChunkState.SetState(position, ChunkState.State.SCHEDULED);
                    break;
                case ChunkState.State.SCHEDULED:
                    break;
                case ChunkState.State.ACTIVE:
                    _ChunkState.SetState(position, ChunkState.State.INACTIVE);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

    }

}