using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkState {

        public enum State {
            
            // DEFAULT,
            UNLOADED,
            STREAMING,
            LOADED,
            MESHING,
            ACTIVE,

        }

        private IDictionary<int3, State> _Dictionary;
        
        public ChunkState(VloxySettings settings) {
            _Dictionary = new Dictionary<int3, State>(settings.Chunk.LoadDistance.CubedSize());
        }

        public void RemoveState(int3 position) => _Dictionary.Remove(position);
        
        public State GetState(int3 position) => _Dictionary.TryGetValue(position, out var state) ? state : State.UNLOADED;

        public void SetState(int3 position, State state) {
            if (_Dictionary.ContainsKey(position)) _Dictionary[position] = state; 
            else _Dictionary.Add(position, state);
        }

    }

}