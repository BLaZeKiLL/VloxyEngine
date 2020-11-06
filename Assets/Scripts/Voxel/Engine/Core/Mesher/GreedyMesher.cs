using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Core.Mesher {

    public abstract class GreedyMesher<T> : IMesher<T> where T : IBlock {

        protected readonly MeshData MeshData;

        private int index;

        public GreedyMesher() {
            MeshData = new MeshData();
        }

        protected abstract T EmptyBlock();

        protected abstract T NullBlock();
        
        public MeshData GenerateMesh(Chunk<T> chunk)  {
            // Sweep over each axis (X, Y and Z)
            for (int direction = 0; direction < 3; direction++) {
                int i, // loop var
                    j, // loop var
                    k, // loop var
                    l, // loop var
                    width, // quad width
                    height; // quad height

                // 2 Perpendicular axis
                int axis1 = (direction + 1) % 3;
                int axis2 = (direction + 2) % 3;

                int mainAxisLimit = chunk.Size[direction];
                int axis1Limit = chunk.Size[axis1];
                int axis2Limit = chunk.Size[axis2];

                var chunkItr = new int[3];
                var directionMask = new int[3];

                var normalMask = new Mask[axis1Limit * axis2Limit];
                directionMask[direction] = 1;

                // Check each slice of the chunk one at a time
                for (chunkItr[direction] = -1; chunkItr[direction] < mainAxisLimit;) {
                    var n = 0;

                    // Compute the mask
                    for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2]) {
                        for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1]) {
                            T currentBlock, compareBlock;

                            if (chunkItr[direction] >= 0) {
                                currentBlock = chunk.GetBlock(
                                    chunkItr[0],
                                    chunkItr[1],
                                    chunkItr[2]
                                );
                            } else { // check neighbour in -ve axis
                                currentBlock = EmptyBlock();
                            }
                            
                            if (chunkItr[direction] < mainAxisLimit - 1) {
                                compareBlock = chunk.GetBlock(
                                    chunkItr[0] + directionMask[0],
                                    chunkItr[1] + directionMask[1],
                                    chunkItr[2] + directionMask[2]
                                );
                            } else { // check neighbour in +ve axis
                                compareBlock = EmptyBlock();
                            }
                            
                            var blockCurrent = currentBlock.IsOpaque();
                            var blockCompare = compareBlock.IsOpaque(); 

                            if (blockCurrent == blockCompare) {
                                normalMask[n++] = new Mask(NullBlock(), 0);
                            } else if (blockCurrent) {
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
                                for (width = 1; i + width < axis1Limit && normalMask[n + width].Normal == currentMask.Normal; width++) { }

                                // Compute the height of this quad and store it in h                        
                                // This is done by checking if every block next to this row (range 0 to w) is also part of the mask.
                                // For example, if w is 5 we currently have a quad of dimensions 1 x 5. To reduce triangle count,
                                // greedy meshing will attempt to expand this quad out to CHUNK_SIZE x 5, but will stop if it reaches a hole in the mask

                                bool done = false;

                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (k = 0; k < width; ++k) {
                                        if (normalMask[n + k + height * axis1Limit].Normal == currentMask.Normal) continue;

                                        done = true;
                                        break; // If there's a hole in the mask, exit
                                    }

                                    if (done) break;
                                }

                                var deltaAxis1 = new int[3];
                                deltaAxis1[axis1] = width;

                                var deltaAxis2 = new int[3];
                                deltaAxis2[axis2] = height;

                                // create quad
                                CreateQuad(
                                    currentMask.Normal,
                                    currentMask.Block,
                                    directionMask,
                                    new Vector3(chunkItr[0], chunkItr[1], chunkItr[2]),
                                    new Vector3(chunkItr[0] + deltaAxis1[0], chunkItr[1] + deltaAxis1[1],
                                        chunkItr[2] + deltaAxis1[2]),
                                    new Vector3(chunkItr[0] + deltaAxis2[0], chunkItr[1] + deltaAxis2[1],
                                        chunkItr[2] + deltaAxis2[2]),
                                    new Vector3(chunkItr[0] + deltaAxis1[0] + deltaAxis2[0],
                                        chunkItr[1] + deltaAxis1[1] + deltaAxis2[1],
                                        chunkItr[2] + deltaAxis1[2] + deltaAxis2[2])
                                );

                                // Clear this part of the mask, so we don't add duplicate faces
                                for (l = 0; l < height; ++l)
                                    for (k = 0; k < width; ++k)
                                        normalMask[n + k + l * axis1Limit] = new Mask(NullBlock(), 0);

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
            MeshData.Clear();
            index = 0;
        }
        
        protected virtual void CreateQuad(T block, Vector3Int normal) { }
        
        // v1 -> BL
        // v2 -> TL
        // v3 -> BR
        // v4 -> TR
        private void CreateQuad(sbyte normalMask, T block, IReadOnlyList<int> directionMask, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            MeshData.Vertices.Add(v1);
            MeshData.Vertices.Add(v2);
            MeshData.Vertices.Add(v3);
            MeshData.Vertices.Add(v4);

            if (normalMask == 1) {
                MeshData.Triangles.Add(index);
                MeshData.Triangles.Add(index + 1);
                MeshData.Triangles.Add(index + 3);
                MeshData.Triangles.Add(index);
                MeshData.Triangles.Add(index + 3);
                MeshData.Triangles.Add(index + 2);
            } else if (normalMask == -1) {
                MeshData.Triangles.Add(index);
                MeshData.Triangles.Add(index + 3);
                MeshData.Triangles.Add(index + 1);
                MeshData.Triangles.Add(index);
                MeshData.Triangles.Add(index + 2);
                MeshData.Triangles.Add(index + 3);
            }

            index += 4;

            var normal = new Vector3Int(
                normalMask * directionMask[0],
                normalMask * directionMask[1],
                normalMask * directionMask[2]
            );

            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);
            MeshData.Normals.Add(normal);

            CreateQuad(block, normal);
        }

        private readonly struct Mask {

            public readonly T Block;
            public readonly sbyte Normal;

            public Mask(T block, sbyte normal) {
                Block = block;
                Normal = normal;
            }

        }
        
    }

}