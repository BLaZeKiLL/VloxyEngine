﻿using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class Chunk<B> where B : IBlock {

        // TODO : initialize chunk data
        public IChunkData<B> Data { get; }
        
        public Vector3Int Position { get; }
        
        public Chunk(Vector3Int position) {
            Position = position;
        }

        public virtual string Name() {
            return $"Chunk {Position}";
        }

    }

}