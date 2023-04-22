using CodeBlaze.Vloxy.Engine.Data;

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Mesher {

    [BurstCompatible]
    public static class GreedyMesher {

        [BurstCompile]
        private readonly struct Mask {

            public readonly int Block;

            internal readonly byte MeshIndex;
            internal readonly sbyte Normal;
            internal readonly int4 AO;

            public Mask(int block, byte meshIndex, sbyte normal, int4 ao) {
                MeshIndex = meshIndex;
                Block = block;
                Normal = normal;
                AO = ao;
            }

        }

        [BurstCompile]
        internal static MeshBuffer GenerateMesh(
            ChunkAccessor accessor, int3 pos, int3 size
            ) {
            var mesh = new MeshBuffer {
                VertexBuffer = new NativeList<Vertex>(Allocator.Temp),
                IndexBuffer0 = new NativeList<int>(Allocator.Temp),
                IndexBuffer1 = new NativeList<int>(Allocator.Temp)
            };

            int vertex_count = 0;

            for (int direction = 0; direction < 3; direction++) {
                int axis1 = (direction + 1) % 3;
                int axis2 = (direction + 2) % 3;

                int mainAxisLimit = size[direction];
                int axis1Limit = size[axis1];
                int axis2Limit = size[axis2];

                var deltaAxis1 = int3.zero;
                var deltaAxis2 = int3.zero;

                var chunkItr = int3.zero;
                var directionMask = int3.zero;
                directionMask[direction] = 1;

                // Optimize Allocation
                var normalMask = new NativeArray<Mask>(axis1Limit * axis2Limit, Allocator.Temp);

                for (chunkItr[direction] = -1; chunkItr[direction] < mainAxisLimit;) {
                    var n = 0;

                    // Compute the mask
                    for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2]) {
                        for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1]) {
                            var currentBlock = accessor.GetBlockInChunk(pos, chunkItr);
                            var compareBlock = accessor.GetBlockInChunk(pos, chunkItr + directionMask);

                            var currentMeshIndex = GetMeshIndex(currentBlock);
                            var compareMeshIndex = GetMeshIndex(compareBlock);

                            if (currentMeshIndex == compareMeshIndex) {
                                normalMask[n++] = default;
                            } else if (currentMeshIndex < compareMeshIndex) {
                                normalMask[n++] = new Mask(currentBlock, currentMeshIndex, 1, ComputeAOMask(accessor, pos, chunkItr + directionMask, axis1, axis2));
                            } else {
                                normalMask[n++] = new Mask(compareBlock, compareMeshIndex, -1, ComputeAOMask(accessor, pos, chunkItr, axis1, axis2));
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

                                for (width = 1; i + width < axis1Limit && CompareMask(normalMask[n + width] , currentMask); width++) { }

                                // Compute the height of this quad and store it in h                        
                                // This is done by checking if every block next to this row (range 0 to w) is also part of the mask.
                                // For example, if w is 5 we currently have a quad of dimensions 1 x 5. To reduce triangle count,
                                // greedy meshing will attempt to expand this quad out to CHUNK_SIZE x 5, but will stop if it reaches a hole in the mask

                                int height;
                                bool done = false;

                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (int k = 0; k < width; ++k) {
                                        if (CompareMask(normalMask[n + k + height * axis1Limit], currentMask)) continue;

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
                                    mesh, vertex_count, currentMask, directionMask,
                                    width, height,
                                    chunkItr,
                                    chunkItr + deltaAxis1,
                                    chunkItr + deltaAxis2,
                                    chunkItr + deltaAxis1 + deltaAxis2
                                );

                                vertex_count += 4;

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

            return mesh;
        }

        [BurstCompile]
        private static void CreateQuad(
            MeshBuffer mesh, int vertex_count, Mask mask, int3 directionMask, 
            int width, int height, int3 v1, int3 v2, int3 v3, int3 v4
            ) {
            float3 vf1 = v1, vf2 = v2, vf3 = v3, vf4 = v4;
            
            var normal = directionMask * mask.Normal;

            // Main UV
            float3 uv1, uv2, uv3, uv4;
            var uvz = GetUV0Index(mask.Block, normal);

            if (normal.x is 1 or -1) {
                uv1 = new float3(0, 0, uvz);
                uv2 = new float3(0, width, uvz);
                uv3 = new float3(height, 0, uvz);
                uv4 = new float3(height, width, uvz);
            } else {
                uv1 = new float3(0, 0, uvz);
                uv2 = new float3(width, 0, uvz);
                uv3 = new float3(0, height, uvz);
                uv4 = new float3(width, height, uvz);
            }

            if (mask.MeshIndex == 1 && normal.y == 1) {
                vf1.y -= 0.25f;
                vf2.y -= 0.25f;
                vf3.y -= 0.25f;
                vf4.y -= 0.25f;
            }
            
            // 1 Bottom Left
            var vertex1 = new Vertex {
                Position = vf1,
                Normal = normal,
                UV0 = uv1,
                UV1 = new float2(0, 0),
                UV2 = mask.AO
            };

            // 2 Top Left
            var vertex2 = new Vertex {
                Position = vf2,
                Normal = normal,
                UV0 = uv2,
                UV1 = new float2(0, 1),
                UV2 = mask.AO
            };

            // 3 Bottom Right
            var vertex3 = new Vertex {
                Position = vf3,
                Normal = normal,
                UV0 = uv3,
                UV1 = new float2(1, 0),
                UV2 = mask.AO
            };

            // 4 Top Right
            var vertex4 = new Vertex {
                Position = vf4,
                Normal = normal,
                UV0 = uv4,
                UV1 = new float2(1, 1),
                UV2 = mask.AO
            };
            
            mesh.VertexBuffer.Add(vertex1);
            mesh.VertexBuffer.Add(vertex2);
            mesh.VertexBuffer.Add(vertex3);
            mesh.VertexBuffer.Add(vertex4);

            // var indexBuffer = mask.MeshIndex == 0 ? mesh.IndexBuffer0 : mesh.IndexBuffer1;
            var indexBuffer = mesh.IndexBuffer0;

            if (mask.AO[0] + mask.AO[3] > mask.AO[1] + mask.AO[2]) { // + -
                indexBuffer.Add(vertex_count); // 0 0
                indexBuffer.Add(vertex_count + 2 - mask.Normal); // 1 3
                indexBuffer.Add(vertex_count + 2 + mask.Normal); // 3 1
                indexBuffer.Add(vertex_count + 3); // 3 3
                indexBuffer.Add(vertex_count + 1 + mask.Normal); // 2 0
                indexBuffer.Add(vertex_count + 1 - mask.Normal); // 0 2
            } else { // + -
                indexBuffer.Add(vertex_count + 1); // 1 1
                indexBuffer.Add(vertex_count + 1 + mask.Normal); // 2 0
                indexBuffer.Add(vertex_count + 1 - mask.Normal); // 0 2
                indexBuffer.Add(vertex_count + 2); // 2 2
                indexBuffer.Add(vertex_count + 2 - mask.Normal); // 1 3
                indexBuffer.Add(vertex_count + 2 + mask.Normal); // 3 1
            }
        }

        [BurstCompile]
        private static bool CompareMask(Mask m1, Mask m2) {
            return
                m1.MeshIndex == m2.MeshIndex &&
                m1.Block == m2.Block &&
                m1.Normal == m2.Normal &&
                m1.AO[0] == m2.AO[0] &&
                m1.AO[1] == m2.AO[1] &&
                m1.AO[2] == m2.AO[2] &&
                m1.AO[3] == m2.AO[3];
        }

        [BurstCompile]
        private static int4 ComputeAOMask(ChunkAccessor accessor, int3 pos, int3 coord, int axis1, int axis2) {
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

            LBC[axis1] -= 1;
            LBC[axis2] -= 1;
            RBC[axis1] -= 1;
            RBC[axis2] += 1;
            LTC[axis1] += 1;
            LTC[axis2] -= 1;
            RTC[axis1] += 1;
            RTC[axis2] += 1;

            var LO = GetMeshIndex(accessor.GetBlockInChunk(pos, L)) == 0 ? 1 : 0;
            var RO = GetMeshIndex(accessor.GetBlockInChunk(pos, R)) == 0 ? 1 : 0;
            var BO = GetMeshIndex(accessor.GetBlockInChunk(pos, B)) == 0 ? 1 : 0;
            var TO = GetMeshIndex(accessor.GetBlockInChunk(pos, T)) == 0 ? 1 : 0;

            var LBCO = GetMeshIndex(accessor.GetBlockInChunk(pos, LBC)) == 0 ? 1 : 0;
            var RBCO = GetMeshIndex(accessor.GetBlockInChunk(pos, RBC)) == 0 ? 1 : 0;
            var LTCO = GetMeshIndex(accessor.GetBlockInChunk(pos, LTC)) == 0 ? 1 : 0;
            var RTCO = GetMeshIndex(accessor.GetBlockInChunk(pos, RTC)) == 0 ? 1 : 0;

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
        
        [BurstCompile]
        private static int GetUV0Index(
            int block,
            int3 normal
        ) {
            switch (block) {
                case (int) Block.GRASS when normal.y is 1: return 15;
                case (int) Block.GRASS when normal.y is -1: return 52;
                case (int) Block.GRASS: return 43;
                case (int) Block.DIRT: return 52;
                case (int) Block.STONE: return 39;
                case (int) Block.WATER: return 54;
            }

            return 0;
        }

        [BurstCompile]
        private static byte GetMeshIndex(int block) {
            switch (block) {
                case (int) Block.AIR: return 2;
                case (int) Block.WATER: return 1; // Should be 2
                default: return 0;
            }
        }

    }

}