using System.Collections.Generic;

using UnityEngine;

namespace CodeBlaze.Voxel.Core.Mesh {

    public class MeshBuilder {

        private readonly List<Color32> colors;
        private readonly List<Vector3> normals;
        private readonly List<int> triangles;

        private readonly List<Vector3> vertices;

        private int index;

        public MeshBuilder() {
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color32>();
            normals = new List<Vector3>();
        }

        public MeshData GenerateMesh(Chunk chunk) {
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

                var mask = new Mask[axis1Limit * axis2Limit];
                directionMask[direction] = 1;

                // Check each slice of the chunk one at a time
                for (chunkItr[direction] = -1; chunkItr[direction] < mainAxisLimit;) {
                    var n = 0;

                    // Compute the mask
                    for (chunkItr[axis2] = 0; chunkItr[axis2] < axis2Limit; ++chunkItr[axis2]) {
                        for (chunkItr[axis1] = 0; chunkItr[axis1] < axis1Limit; ++chunkItr[axis1]) {
                            var currentBlock = chunk.GetBlock(
                                chunkItr[0],
                                chunkItr[1],
                                chunkItr[2]
                            );

                            var compareBlock = chunk.GetBlock(
                                chunkItr[0] + directionMask[0],
                                chunkItr[1] + directionMask[1],
                                chunkItr[2] + directionMask[2]
                            );

                            bool blockCurrent =
                                0 <= chunkItr[direction]
                                    ? currentBlock.IsSolid()
                                    : false; // check neighbour in -ve axis
                            bool blockCompare =
                                chunkItr[direction] < mainAxisLimit - 1
                                    ? compareBlock.IsSolid()
                                    : false; // check neighbour in +ve axis

                            if (blockCurrent == blockCompare) {
                                mask[n++] = new Mask(0, Color.magenta);
                            } else if (blockCurrent) {
                                mask[n++] = new Mask(1, currentBlock.Color);
                            } else {
                                mask[n++] = new Mask(-1, compareBlock.Color);
                            }
                        }
                    }

                    ++chunkItr[direction];
                    n = 0;

                    // Generate a mesh from the mask using lexicographic ordering,      
                    // by looping over each block in this slice of the chunk
                    for (j = 0; j < axis2Limit; j++) {
                        for (i = 0; i < axis1Limit;) {
                            if (mask[n].normal != 0) {
                                // Current Stuff
                                var currentMask = mask[n];
                                chunkItr[axis1] = i;
                                chunkItr[axis2] = j;

                                // Compute the width of this quad and store it in w                        
                                // This is done by searching along the current axis until mask[n + w] is false
                                for (width = 1; i + width < axis1Limit && mask[n + width] == currentMask; width++) { }

                                // Compute the height of this quad and store it in h                        
                                // This is done by checking if every block next to this row (range 0 to w) is also part of the mask.
                                // For example, if w is 5 we currently have a quad of dimensions 1 x 5. To reduce triangle count,
                                // greedy meshing will attempt to expand this quad out to CHUNK_SIZE x 5, but will stop if it reaches a hole in the mask

                                bool done = false;

                                for (height = 1; j + height < axis2Limit; height++) {
                                    // Check each block next to this quad
                                    for (k = 0; k < width; ++k) {
                                        // If there's a hole in the mask, exit
                                        if (mask[n + k + height * axis1Limit] != currentMask) {
                                            done = true;

                                            break;
                                        }
                                    }

                                    if (done) break;
                                }

                                var deltaAxis1 = new int[3];
                                deltaAxis1[axis1] = width;

                                var deltaAxis2 = new int[3];
                                deltaAxis2[axis2] = height;

                                // create quad
                                CreateQuad(
                                    currentMask,
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
                                        mask[n + k + l * axis1Limit] = new Mask(0, Color.magenta);

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

            var data = new MeshData(
                vertices.ToArray(),
                triangles.ToArray(),
                colors.ToArray(),
                normals.ToArray()
            );

            // Clear Builder
            Clear();

            return data;
        }

        // v1 -> BL
        // v2 -> TL
        // v3 -> BR
        // v4 -> TR
        private void CreateQuad(Mask mask, IReadOnlyList<int> directionMask, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            if (mask.normal == 1) {
                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 3);
                triangles.Add(index);
                triangles.Add(index + 3);
                triangles.Add(index + 2);
            } else if (mask.normal == -1) {
                triangles.Add(index);
                triangles.Add(index + 3);
                triangles.Add(index + 1);
                triangles.Add(index);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
            }

            index += 4;

            var normal = new Vector3(
                mask.normal * directionMask[0],
                mask.normal * directionMask[1],
                mask.normal * directionMask[2]
            );

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            colors.Add(mask.color);
            colors.Add(mask.color);
            colors.Add(mask.color);
            colors.Add(mask.color);
        }

        private void Clear() {
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            normals.Clear();
            index = 0;
        }
        
        private readonly struct Mask {

            public readonly Color32 color;
            public readonly sbyte normal;

            public Mask(sbyte normal, Color32 color) {
                this.color = color;
                this.normal = normal;
            }

            public static bool operator ==(Mask m1, Mask m2) {
                return
                    m1.normal == m2.normal &&
                    m1.color.r == m2.color.r &&
                    m1.color.g == m2.color.g &&
                    m1.color.b == m2.color.b;
            }

            public static bool operator !=(Mask m1, Mask m2) {
                return !(m1 == m2);
            }

        }

    }

}