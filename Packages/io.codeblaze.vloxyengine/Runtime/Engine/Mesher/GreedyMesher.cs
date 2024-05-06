using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Mesher {

    [GenerateTestsForBurstCompatibility]
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
        private static int GetUV0Index(
            int block,
            int3 normal
        ) {
            return block switch {
                (int) Block.GRASS when normal.y is 1 => 15,
                (int) Block.GRASS when normal.y is -1 => 52,
                (int) Block.GRASS => 43,
                (int) Block.DIRT => 52,
                (int) Block.STONE => 39,
                (int) Block.SAND => 57,
                _ => 0
            };
        }

        [BurstCompile]
        private static byte GetMeshIndex(int block) {
            return block switch {
                (int) Block.AIR => 2,
                (int) Block.WATER => 1,
                _ => 0
            };
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

            var vertex_count = 0;

            for (var direction = 0; direction < 3; direction++) {
                var axis1 = (direction + 1) % 3;
                var axis2 = (direction + 2) % 3;

                var mainAxisLimit = size[direction];
                var axis1Limit = size[axis1];
                var axis2Limit = size[axis2];

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
                                normalMask[n++] = default; // Air with Air or Water with Water or Solid with Solid, no face in this case
                            } else if (currentMeshIndex < compareMeshIndex) {
                                normalMask[n++] = new Mask(currentBlock, currentMeshIndex, 1, ComputeAOMask(accessor, pos, chunkItr + directionMask, axis1, axis2));
                            } else {
                                normalMask[n++] = new Mask(compareBlock, compareMeshIndex, -1, ComputeAOMask(accessor, pos, chunkItr, axis1, axis2));
                            }
                        }
                    }

                    ++chunkItr[direction];
                    n = 0;

                    for (var j = 0; j < axis2Limit; j++) {
                        for (var i = 0; i < axis1Limit;) {
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
                                var done = false;

                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (var k = 0; k < width; ++k) {
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
                                vertex_count += CreateQuad(
                                    mesh, vertex_count, currentMask, directionMask,
                                    width, height,
                                    chunkItr,
                                    chunkItr + deltaAxis1,
                                    chunkItr + deltaAxis2,
                                    chunkItr + deltaAxis1 + deltaAxis2
                                );

                                // reset delta's
                                deltaAxis1 = int3.zero;
                                deltaAxis2 = int3.zero;

                                // Clear this part of the mask, so we don't add duplicate faces
                                for (var l = 0; l < height; ++l)
                                    for (var k = 0; k < width; ++k)
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
        private static int CreateQuad(
            MeshBuffer mesh, int vertex_count, Mask mask, int3 directionMask, 
            int width, int height, int3 v1, int3 v2, int3 v3, int3 v4
        ) {
            return mask.MeshIndex switch {
                0 => CreateQuadMesh0(mesh, vertex_count, mask, directionMask, width, height, v1, v2, v3, v4),
                1 => CreateQuadMesh1(mesh, vertex_count, mask, directionMask, width, height, v1, v2, v3, v4),
                _ => 0
            };
        }
        
        [BurstCompile]
        private static int CreateQuadMesh0(
            MeshBuffer mesh, int vertex_count, Mask mask, int3 directionMask, 
            int width, int height, float3 v1, float3 v2, float3 v3, float3 v4
        ) {
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

            // 1 Bottom Left
            var vertex1 = new Vertex {
                Position = v1,
                Normal = normal,
                UV0 = uv1,
                UV1 = new float2(0, 0),
                UV2 = mask.AO
            };

            // 2 Top Left
            var vertex2 = new Vertex {
                Position = v2,
                Normal = normal,
                UV0 = uv2,
                UV1 = new float2(0, 1),
                UV2 = mask.AO
            };

            // 3 Bottom Right
            var vertex3 = new Vertex {
                Position = v3,
                Normal = normal,
                UV0 = uv3,
                UV1 = new float2(1, 0),
                UV2 = mask.AO
            };

            // 4 Top Right
            var vertex4 = new Vertex {
                Position = v4,
                Normal = normal,
                UV0 = uv4,
                UV1 = new float2(1, 1),
                UV2 = mask.AO
            };
            
            mesh.VertexBuffer.Add(vertex1);
            mesh.VertexBuffer.Add(vertex2);
            mesh.VertexBuffer.Add(vertex3);
            mesh.VertexBuffer.Add(vertex4);

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

            return 4;
        }
        
        [BurstCompile]
        private static int CreateQuadMesh1(
            MeshBuffer mesh, int vertex_count, Mask mask, int3 directionMask, 
            int width, int height, float3 v1, float3 v2, float3 v3, float3 v4
        ) {
            var normal = directionMask * mask.Normal;

            // Main UV
            float3 uv1, uv2, uv3, uv4;

            if (normal.x is 1 or -1) {
                uv1 = new float3(0, 0, 0);
                uv2 = new float3(0, width, 0);
                uv3 = new float3(height, 0, 0);
                uv4 = new float3(height, width, 0);
            } else {
                uv1 = new float3(0, 0, 0);
                uv2 = new float3(width, 0, 0);
                uv3 = new float3(0, height, 0);
                uv4 = new float3(width, height, 0);
            }

            if (normal.y == 1) {
                v1.y -= 0.25f;
                v2.y -= 0.25f;
                v3.y -= 0.25f;
                v4.y -= 0.25f;
            }
            
            // 1 Bottom Left
            var vertex1 = new Vertex {
                Position = v1,
                Normal = normal,
                UV0 = uv1,
                UV1 = new float2(0, 0),
                UV2 = mask.AO
            };

            // 2 Top Left
            var vertex2 = new Vertex {
                Position = v2,
                Normal = normal,
                UV0 = uv2,
                UV1 = new float2(0, 1),
                UV2 = mask.AO
            };

            // 3 Bottom Right
            var vertex3 = new Vertex {
                Position = v3,
                Normal = normal,
                UV0 = uv3,
                UV1 = new float2(1, 0),
                UV2 = mask.AO
            };

            // 4 Top Right
            var vertex4 = new Vertex {
                Position = v4,
                Normal = normal,
                UV0 = uv4,
                UV1 = new float2(1, 1),
                UV2 = mask.AO
            };
            
            mesh.VertexBuffer.Add(vertex1);
            mesh.VertexBuffer.Add(vertex2);
            mesh.VertexBuffer.Add(vertex3);
            mesh.VertexBuffer.Add(vertex4);

            var indexBuffer = mesh.IndexBuffer1;

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
            
            if ((normal != new int3(0, 1, 0)).AndReduce()) return 4;

            normal *= -1;
            
            // 1 Bottom Left
            var vertex5 = new Vertex {
                Position = v1,
                Normal = normal,
                UV0 = uv1,
                UV1 = new float2(0, 0),
                UV2 = mask.AO
            };

            // 2 Top Left
            var vertex6 = new Vertex {
                Position = v2,
                Normal = normal,
                UV0 = uv2,
                UV1 = new float2(0, 1),
                UV2 = mask.AO
            };

            // 3 Bottom Right
            var vertex7 = new Vertex {
                Position = v3,
                Normal = normal,
                UV0 = uv3,
                UV1 = new float2(1, 0),
                UV2 = mask.AO
            };

            // 4 Top Right
            var vertex8 = new Vertex {
                Position = v4,
                Normal = normal,
                UV0 = uv4,
                UV1 = new float2(1, 1),
                UV2 = mask.AO
            };
            
            mesh.VertexBuffer.Add(vertex5);
            mesh.VertexBuffer.Add(vertex6);
            mesh.VertexBuffer.Add(vertex7);
            mesh.VertexBuffer.Add(vertex8);

            vertex_count += 4;
            
            if (mask.AO[0] + mask.AO[3] > mask.AO[1] + mask.AO[2]) { // + -
                indexBuffer.Add(vertex_count + 2 + mask.Normal); // 3 1
                indexBuffer.Add(vertex_count + 2 - mask.Normal); // 1 3
                indexBuffer.Add(vertex_count); // 0 0
                
                indexBuffer.Add(vertex_count + 1 - mask.Normal); // 0 2
                indexBuffer.Add(vertex_count + 1 + mask.Normal); // 2 0
                indexBuffer.Add(vertex_count + 3); // 3 3
            } else { // + -
                indexBuffer.Add(vertex_count + 1 - mask.Normal); // 0 2
                indexBuffer.Add(vertex_count + 1 + mask.Normal); // 2 0
                indexBuffer.Add(vertex_count + 1); // 1 1
                
                indexBuffer.Add(vertex_count + 2 + mask.Normal); // 3 1
                indexBuffer.Add(vertex_count + 2 - mask.Normal); // 1 3
                indexBuffer.Add(vertex_count + 2); // 2 2
            }

            return 8;
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

    }

}