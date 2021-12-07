using CodeBlaze.Vloxy.Engine.Components;

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Mesher {

    [BurstCompatible]
    public static class GreedyMesherJob {

        private static readonly float4 AO_CURVE = new(0.75f, 0.825f, 0.9f, 1.0f);
        
        [BurstCompile]
        private readonly struct Mask {

            public readonly int Block;
            
            internal readonly sbyte Normal;
            internal readonly int4 AO;

            public Mask(int block, sbyte normal, int4 ao) {
                Block = block;
                Normal = normal;
                AO = ao;
            }
            
            [BurstCompile] // TODO : Figure out generic compare mask (Burst Function Pointers)
            public static bool Compare(Mask m1, Mask m2) {
                return
                    m1.Block == m2.Block &&
                    m1.Normal == m2.Normal &&
                    m1.AO[0] == m2.AO[0] &&
                    m1.AO[1] == m2.AO[1] &&
                    m1.AO[2] == m2.AO[2] &&
                    m1.AO[3] == m2.AO[3];
            }

        }

        [BurstCompile]
        public static void GenerateMesh(NativeChunkStoreAccessor accessor, int3 pos, Mesh.MeshData mesh, int3 size) {
            for (int direction = 0; direction < 3; direction++) {
                int axis1 = (direction + 1) % 3;
                int axis2 = (direction + 2) % 3;

                int mainAxisLimit = size[direction];
                int axis1Limit = size[axis1];
                int axis2Limit = size[axis2];

                int vindex = 0;
                int tindex = 0;
                
                var deltaAxis1 = int3.zero;
                var deltaAxis2 = int3.zero;

                var chunkItr = int3.zero;
                var directionMask = int3.zero;
                directionMask[direction] = 1;

                // Optimize Allocation
                var normalMask = new NativeArray<Mask>(axis1Limit * axis2Limit, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

                for (chunkItr[direction] = -1; chunkItr[direction] < mainAxisLimit;) {
                    var n = 0;
                    
                    // Compute the mask
                    for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2]) {
                        for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1]) {
                            var currentBlock = accessor.GetBlockInChunk(pos, chunkItr);
                            var compareBlock = accessor.GetBlockInChunk(pos, chunkItr + directionMask);

                            var currentBlockOpaque = currentBlock == 0;
                            var compareBlockOpaque = compareBlock == 0; 

                            if (currentBlockOpaque == compareBlockOpaque) {
                                normalMask[n++] = default;
                            } else if (currentBlockOpaque) {
                                normalMask[n++] = new Mask(currentBlock, 1, ComputeAOMask(accessor, pos, chunkItr + directionMask, axis1, axis2));
                            } else {
                                normalMask[n++] = new Mask(compareBlock, -1, ComputeAOMask(accessor, pos, chunkItr, axis1, axis2));
                            }
                        }
                    }
                    
                    ++chunkItr[direction];
                    n = 0;

                    for (int j = 0; j < axis2Limit; j++) {
                        for (int i = 0; i < axis1Limit;) {
                            if (normalMask[n].Normal != 0) { // Create Quad
                                var currentMask = normalMask[n];
                                chunkItr[axis1] = i;
                                chunkItr[axis2] = j;

                                // Compute the width of this quad and store it in w                        
                                // This is done by searching along the current axis until mask[n + w] is false
                                int width;

                                for (width = 1; i + width < axis1Limit && Mask.Compare(normalMask[n + width] , currentMask); width++) { }

                                // Compute the height of this quad and store it in h                        
                                // This is done by checking if every block next to this row (range 0 to w) is also part of the mask.
                                // For example, if w is 5 we currently have a quad of dimensions 1 x 5. To reduce triangle count,
                                // greedy meshing will attempt to expand this quad out to CHUNK_SIZE x 5, but will stop if it reaches a hole in the mask

                                int height;
                                bool done = false;
                                
                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (int k = 0; k < width; ++k) {
                                        if (Mask.Compare(normalMask[n + k + height * axis1Limit] , currentMask)) continue;

                                        done = true;
                                        break; // If there's a hole in the mask, exit
                                    }

                                    if (done) break;
                                }
                                
                                // set delta's
                                deltaAxis1[axis1] = width;
                                deltaAxis2[axis2] = height;

                                // create quad
                                CreateQuad(
                                    mesh, vindex, tindex, currentMask, directionMask,
                                    chunkItr,
                                    chunkItr + deltaAxis1,
                                    chunkItr + deltaAxis2,
                                    chunkItr + deltaAxis1 + deltaAxis2
                                );

                                // update indexes
                                vindex += 4;
                                tindex += 6;
                                
                                // reset delta's
                                deltaAxis1 = int3.zero;
                                deltaAxis2 = int3.zero;
                                
                                // Clear this part of the mask, so we don't add duplicate faces
                                for (int l = 0; l < height; ++l)
                                    for (int k = 0; k < width; ++k)
                                        normalMask[n + k + l * axis1Limit] = default;

                                // update loop vars
                                i += width;
                                n += width;
                            } else { // nothing to do
                                i++;
                                n++;
                            }
                        }
                    }
                }
                
                normalMask.Dispose();
            }
        }
        
        [BurstCompile]
        private static void CreateQuad(Mesh.MeshData mesh, int vindex, int tindex, Mask mask, int3 directionMask, int3 v1, int3 v2, int3 v3, int3 v4) {
            var normal = directionMask * mask.Normal;
            var ao = new float4(AO_CURVE[mask.AO[0]], AO_CURVE[mask.AO[1]], AO_CURVE[mask.AO[2]], AO_CURVE[mask.AO[3]]);

            var vertices = mesh.GetVertexData<int3>();
            var normals = mesh.GetVertexData<int3>(1);
            var colors = mesh.GetVertexData<float3>(2);
            var uv0 = mesh.GetVertexData<int2>(3);
            var uv1 = mesh.GetVertexData<float4>(4);

            var triangles = mesh.GetIndexData<int>();
            
            vertices[vindex]     = v1;                              // 0 Bottom Left
            vertices[vindex + 1] = v2;                              // 1 Top Left
            vertices[vindex + 2] = v3;                              // 2 Bottom Right
            vertices[vindex + 3] = v4;                              // 3 Top Right

            normals[vindex]     = normal;
            normals[vindex + 1] = normal;
            normals[vindex + 2] = normal;
            normals[vindex + 3] = normal;
            
            colors[vindex]     = new float3(0.8f, 0.8f, 0.8f);
            colors[vindex + 1] = new float3(0.8f, 0.8f, 0.8f);
            colors[vindex + 2] = new float3(0.8f, 0.8f, 0.8f);
            colors[vindex + 3] = new float3(0.8f, 0.8f, 0.8f);

            uv0[vindex]     = new int2(0, 0);
            uv0[vindex + 1] = new int2(0, 1);
            uv0[vindex + 2] = new int2(1, 0);
            uv0[vindex + 3] = new int2(1, 1);
            
            uv1[vindex]     = ao;
            uv1[vindex + 1] = ao;
            uv1[vindex + 2] = ao;
            uv1[vindex + 3] = ao;

            if (mask.AO[0] + mask.AO[3] > mask.AO[1] + mask.AO[2]) {    // + -
                triangles[tindex]     = vindex;                         // 0 0
                triangles[tindex + 1] = vindex + 2 - mask.Normal;       // 1 3
                triangles[tindex + 2] = vindex + 2 + mask.Normal;       // 3 1
                triangles[tindex + 3] = vindex + 3;                     // 3 3
                triangles[tindex + 4] = vindex + 1 + mask.Normal;       // 2 0
                triangles[tindex + 5] = vindex + 1 - mask.Normal;       // 0 2
            } else {                                                    // + -
                triangles[tindex]     = vindex + 1;                     // 1 1
                triangles[tindex + 1] = vindex + 1 + mask.Normal;       // 2 0
                triangles[tindex + 2] = vindex + 1 - mask.Normal;       // 0 2
                triangles[tindex + 3] = vindex + 2;                     // 2 2
                triangles[tindex + 4] = vindex + 2 - mask.Normal;       // 1 3
                triangles[tindex + 5] = vindex + 2 + mask.Normal;       // 3 1
            }
        }

        [BurstCompile]
        private static int4 ComputeAOMask(NativeChunkStoreAccessor accessor, int3 pos, int3 coord, int axis1, int axis2) {
            var L = coord;
            var R = coord;
            var B = coord;
            var T = coord;

            var LBC = coord;
            var RBC = coord;
            var LTC = coord;
            var RTC = coord;
            
            L[axis2] -= 1;
            R[axis2] += 1;
            B[axis1] -= 1;
            T[axis1] += 1;

            LBC[axis1] -= 1; LBC[axis2] -= 1;
            RBC[axis1] -= 1; RBC[axis2] += 1;
            LTC[axis1] += 1; LTC[axis2] -= 1;
            RTC[axis1] += 1; RTC[axis2] += 1;
            
            var LO = accessor.GetBlockInChunk(pos, L) == 0 ? 1 : 0;
            var RO = accessor.GetBlockInChunk(pos, R) == 0 ? 1 : 0;
            var BO = accessor.GetBlockInChunk(pos, B) == 0 ? 1 : 0;
            var TO = accessor.GetBlockInChunk(pos, T) == 0 ? 1 : 0;

            var LBCO = accessor.GetBlockInChunk(pos, LBC) == 0 ? 1 : 0;
            var RBCO = accessor.GetBlockInChunk(pos, RBC) == 0 ? 1 : 0;
            var LTCO = accessor.GetBlockInChunk(pos, LTC) == 0 ? 1 : 0;
            var RTCO = accessor.GetBlockInChunk(pos, RTC) == 0 ? 1 : 0;
            
            return new int4(
                ComputeAO(LO, BO, LBCO), 
                ComputeAO(LO, TO, LTCO), 
                ComputeAO(RO, BO, RBCO), 
                ComputeAO(RO, TO, RTCO)
            );
        }
        
        [BurstCompile]
        private static int ComputeAO(int s1, int s2, int c) {
            if (s1 == 1 && s2 == 1) {
                return 0;
            }

            return 3 - (s1 + s2 + c);
        }

    }

}