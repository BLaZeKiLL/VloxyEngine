using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Meshing.Builder {

    public class GreedyMeshBuilder<B> : IMeshBuilder<B> where B : IBlock {

        protected readonly MeshData MeshData;

        protected ChunkJobData<B> JobData;
        
        private int _index;
        private Vector3Int _size;

        public GreedyMeshBuilder(Vector3Int size) {
            MeshData = new MeshData();
            _size = size;
        }

        protected virtual B EmptyBlock() => default;
        
        protected virtual void CreateQuad(B block, Vector3Int normal) { }

        protected virtual bool CompareBlock(B block1, B block2) => block1.Equals(block2);
        
        public MeshData GenerateMesh(ChunkJobData<B> data) {
            JobData = data;
            
            // Sweep over each axis (X, Y and Z)
            for (int direction = 0; direction < 3; direction++) {
                int i, j, k, l, width, height;

                // 2 Perpendicular axis
                int axis1 = (direction + 1) % 3;
                int axis2 = (direction + 2) % 3;

                int mainAxisLimit = _size[direction];
                int axis1Limit = _size[axis1];
                int axis2Limit = _size[axis2];
                
                var deltaAxis1 = Vector3Int.zero;
                var deltaAxis2 = Vector3Int.zero;

                var chunkItr = Vector3Int.zero;
                var directionMask = Vector3Int.zero;
                directionMask[direction] = 1;

                var normalMask = new Mask[axis1Limit * axis2Limit];

                // Check each slice of the chunk one at a time
                for (chunkItr[direction] = -1; chunkItr[direction] < mainAxisLimit;) {
                    var n = 0;

                    // Compute the mask
                    for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2]) {
                        for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1]) {
                            var currentBlock = GetBlock(chunkItr, mainAxisLimit);
                            var compareBlock = GetBlock(chunkItr + directionMask, mainAxisLimit);

                            var currentBlockOpaque = currentBlock.IsOpaque();
                            var compareBlockOpaque = compareBlock.IsOpaque(); 

                            if (currentBlockOpaque == compareBlockOpaque) {
                                normalMask[n++] = default;
                            } else if (currentBlockOpaque) {
                                normalMask[n++] = new Mask(currentBlock, 1);
                            } else {
                                normalMask[n++] = new Mask(compareBlock, -1);
                            }
                        }
                    }

                    ++chunkItr[direction];
                    n = 0;

                    // Generate a mesh from the mask using lexicographic ordering,      
                    // by looping over each block in this slice of the chunk
                    for (j = 0; j < axis2Limit; j++) {
                        for (i = 0; i < axis1Limit;) {
                            if (normalMask[n].Normal != 0) {
                                // Current Stuff
                                var currentMask = normalMask[n];
                                chunkItr[axis1] = i;
                                chunkItr[axis2] = j;

                                // Compute the width of this quad and store it in w                        
                                // This is done by searching along the current axis until mask[n + w] is false
                                for (width = 1; i + width < axis1Limit && CompareMask(normalMask[n + width] , currentMask); width++) { }

                                // Compute the height of this quad and store it in h                        
                                // This is done by checking if every block next to this row (range 0 to w) is also part of the mask.
                                // For example, if w is 5 we currently have a quad of dimensions 1 x 5. To reduce triangle count,
                                // greedy meshing will attempt to expand this quad out to CHUNK_SIZE x 5, but will stop if it reaches a hole in the mask

                                bool done = false;

                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (k = 0; k < width; ++k) {
                                        if (CompareMask(normalMask[n + k + height * axis1Limit] , currentMask)) continue;

                                        done = true;
                                        break; // If there's a hole in the mask, exit
                                    }

                                    if (done) break;
                                }
                                
                                deltaAxis1[axis1] = width;
                                deltaAxis2[axis2] = height;

                                // create quad
                                CreateQuad(
                                    currentMask.Normal, currentMask.Block, directionMask,
                                    chunkItr,
                                    chunkItr + deltaAxis1,
                                    chunkItr + deltaAxis2,
                                    chunkItr + deltaAxis1 + deltaAxis2
                                );

                                deltaAxis1 = Vector3Int.zero;
                                deltaAxis2 = Vector3Int.zero;
                                
                                // Clear this part of the mask, so we don't add duplicate faces
                                for (l = 0; l < height; ++l)
                                    for (k = 0; k < width; ++k)
                                        normalMask[n + k + l * axis1Limit] = default;

                                i += width;
                                n += width;
                            } else {
                                i++;
                                n++;
                            }
                        }
                    }
                }
            }

            return MeshData;
        }

        public void Clear() {
            JobData = null;
            MeshData.Clear();
            _index = 0;
        }

        protected B GetBlock(Vector3Int pos, int limit) {
            int x = pos.x, y = pos.y, z = pos.z;
            
            if (x < 0) return JobData.ChunkNX == null ? EmptyBlock() : JobData.ChunkNX.Data.GetBlock(x + limit, y, z);
            if (x >= limit) return JobData.ChunkPX == null ? EmptyBlock() : JobData.ChunkPX.Data.GetBlock(x - limit,y,z);
            
            if (y < 0) return JobData.ChunkNY == null ? EmptyBlock() : JobData.ChunkNY.Data.GetBlock(x, y + limit, z);
            if (y >= limit) return JobData.ChunkPY == null ? EmptyBlock() : JobData.ChunkPY.Data.GetBlock(x,y - limit,z);
            
            if (z < 0) return JobData.ChunkNZ == null ? EmptyBlock() : JobData.ChunkNZ.Data.GetBlock(x, y, z + limit);
            if (z >= limit) return JobData.ChunkPZ == null ? EmptyBlock() : JobData.ChunkPZ.Data.GetBlock(x,y,z - limit);

            return JobData.Chunk.Data.GetBlock(x, y, z);
        }

        // v1 -> BL
        // v2 -> TL
        // v3 -> BR
        // v4 -> TR
        private void CreateQuad(sbyte normalMask, B block, Vector3Int directionMask, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            MeshData.Vertices.Add(v1);
            MeshData.Vertices.Add(v2);
            MeshData.Vertices.Add(v3);
            MeshData.Vertices.Add(v4);

            switch (normalMask) {
                case 1:
                    MeshData.Triangles.Add(_index);
                    MeshData.Triangles.Add(_index + 1);
                    MeshData.Triangles.Add(_index + 3);
                    MeshData.Triangles.Add(_index);
                    MeshData.Triangles.Add(_index + 3);
                    MeshData.Triangles.Add(_index + 2);

                    break;
                case -1:
                    MeshData.Triangles.Add(_index);
                    MeshData.Triangles.Add(_index + 3);
                    MeshData.Triangles.Add(_index + 1);
                    MeshData.Triangles.Add(_index);
                    MeshData.Triangles.Add(_index + 2);
                    MeshData.Triangles.Add(_index + 3);

                    break;
            }

            _index += 4;

            var normal = directionMask * normalMask;

            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);

            CreateQuad(block, normal);
        }

        private bool CompareMask(Mask m1, Mask m2) => m1.Normal == m2.Normal && CompareBlock(m1.Block, m2.Block);

        private readonly struct Mask {

            public readonly B Block;
            public readonly sbyte Normal;

            public Mask(B block, sbyte normal) {
                Block = block;
                Normal = normal;
            }

        }
        
    }

}