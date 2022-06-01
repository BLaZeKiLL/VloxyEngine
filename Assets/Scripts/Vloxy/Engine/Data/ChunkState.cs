using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkState {

        public enum State {

            INACTIVE,
            SCHEDULED,
            ACTIVE

        }

        private IDictionary<int3, State> _Dictionary;

        private int3 _ChunkSize;
        private int _YPageSize;
        private int _PageSize;
        
        public ChunkState(VloxySettings settings) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _PageSize = settings.Chunk.PageSize;

            _YPageSize = settings.Noise.Height / _ChunkSize.y / 2;
                
            _Dictionary = new Dictionary<int3, State>(_PageSize.YCubedSize(_YPageSize));
        }

        public void Initialize(int3 position) {
            for (int x = -_PageSize; x <= _PageSize; x++) {
                for (int z = -_PageSize; z <= _PageSize; z++) {
                    for (int y = -_YPageSize; y < _YPageSize; y++) {
                        // + Page Position
                        _Dictionary.Add((new int3(x, y, z) * _ChunkSize), State.INACTIVE);
                    }
                }
            }
        }
        
        public State GetState(int3 position) => _Dictionary[position];

        public State SetState(int3 position, State state) => _Dictionary[position] = state;

    }

}