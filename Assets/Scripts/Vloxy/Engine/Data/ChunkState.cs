using System.Collections.Generic;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkState {

        public enum State {

            INACTIVE,
            SCHEDULED,
            ACTIVE

        }

        private IDictionary<int3, State> _Dictionary;

        public ChunkState() {
            _Dictionary = new Dictionary<int3, State>();
        }

        public void Initialize(int3 position, int pageSize, int3 chunkSize) {
            for (int x = -pageSize; x <= pageSize; x++) {
                for (int z = -pageSize; z <= pageSize; z++) {
                    for (int y = -pageSize; y <= pageSize; y++) {
                        // + Page Position
                        _Dictionary.Add((new int3(x, y, z) * chunkSize), State.INACTIVE);
                    }
                }
            }
        }
        
        public State GetState(int3 position) => _Dictionary[position];

        public State SetState(int3 position, State state) => _Dictionary[position] = state;

    }

}