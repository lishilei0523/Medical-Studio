using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Builders
{
    ///<summary>
    ///Mesh工厂
    ///</summary>
    public static class MeshFactory
    {
        ///<summary>
        ///创建点
        ///</summary>
        ///<param name="position">位置</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreatePoint(Vector3 position, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            Vertex vertex = new()
            {
                Position = position,
                Color = color,
                TextureCoord = Vector2.Zero,
                Normal = Vector3.UnitY
            };

            List<Vertex> vertices = [vertex];
            List<uint> indices = [0];

            return new MeshGeometry(PrimitiveType.Points, vertices, indices);
        }

        ///<summary>
        ///创建点集
        ///</summary>
        ///<param name="positions">位置列表</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreatePoints(ICollection<Vector3> positions, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];

            uint index = 0;
            foreach (Vector3 position in positions)
            {
                vertices.Add(new Vertex
                {
                    Position = position,
                    Color = color,
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitY
                });
                indices.Add(index++);
            }

            return new MeshGeometry(PrimitiveType.Points, vertices, indices);
        }

        ///<summary>
        ///创建线
        ///</summary>
        ///<param name="start">起点</param>
        ///<param name="end">终点</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateLine(Vector3 start, Vector3 end, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            Vector3 direction = Vector3.Normalize(end - start);
            List<Vertex> vertices =
            [
                new()
                {
                    Position = start,
                    Color = color,
                    TextureCoord = new Vector2(0, 0),
                    Normal = direction
                },
                new()
                {
                    Position = end,
                    Color = color,
                    TextureCoord = new Vector2(1, 0),
                    Normal = direction
                }
            ];
            List<uint> indices = [0, 1];

            return new MeshGeometry(PrimitiveType.Lines, vertices, indices);
        }

        ///<summary>
        ///创建折线
        ///</summary>
        ///<param name="positions">位置列表</param>
        ///<param name="closed">是否闭合</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreatePolyline(ICollection<Vector3> positions, bool closed = false, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            List<Vertex> vertices = [];
            List<Vector3> posList = positions.ToList();

            for (int i = 0; i < posList.Count; i++)
            {
                Vector3 normal = i < posList.Count - 1
                    ? Vector3.Normalize(posList[i + 1] - posList[i])
                    : Vector3.Normalize(posList[i] - posList[i - 1]);

                vertices.Add(new Vertex
                {
                    Position = posList[i],
                    Color = color,
                    TextureCoord = new Vector2(i / (float)posList.Count, 0),
                    Normal = normal
                });
            }

            List<uint> indices = [];
            for (int i = 0; i < posList.Count - 1; i++)
            {
                indices.Add((uint)i);
                indices.Add((uint)(i + 1));
            }

            if (closed && posList.Count > 2)
            {
                indices.Add((uint)(posList.Count - 1));
                indices.Add(0);
            }

            return new MeshGeometry(PrimitiveType.Lines, vertices, indices);
        }

        ///<summary>
        ///创建三角形
        ///</summary>
        ///<param name="pointA">顶点A</param>
        ///<param name="pointB">顶点B</param>
        ///<param name="pointC">顶点C</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC - pointA));
            List<Vertex> vertices =
            [
                new()
                {
                    Position = pointA,
                    Color = color,
                    TextureCoord = new Vector2(0, 0),
                    Normal = normal
                },
                new()
                {
                    Position = pointB,
                    Color = color,
                    TextureCoord = new Vector2(0.5f, 1),
                    Normal = normal
                },
                new()
                {
                    Position = pointC,
                    Color = color,
                    TextureCoord = new Vector2(1, 0),
                    Normal = normal
                }
            ];
            List<uint> indices = [0, 1, 2];

            return new MeshGeometry(primitiveType, vertices, indices);
        }

        ///<summary>
        ///创建四边形
        ///</summary>
        ///<param name="pointA">顶点A</param>
        ///<param name="pointB">顶点B</param>
        ///<param name="pointC">顶点C</param>
        ///<param name="pointD">顶点D</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateQuadrangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointD - pointA));
            List<Vertex> vertices =
            [
                new()
                {
                    Position = pointA,
                    Color = color,
                    TextureCoord = new Vector2(0, 1),
                    Normal = normal
                },
                new()
                {
                    Position = pointB,
                    Color = color,
                    TextureCoord = new Vector2(1, 1),
                    Normal = normal
                },
                new()
                {
                    Position = pointC,
                    Color = color,
                    TextureCoord = new Vector2(1, 0),
                    Normal = normal
                },
                new()
                {
                    Position = pointD,
                    Color = color,
                    TextureCoord = new Vector2(0, 0),
                    Normal = normal
                }
            ];
            List<uint> indices = [0, 1, 2, 0, 2, 3];

            return new MeshGeometry(primitiveType, vertices, indices);
        }

        ///<summary>
        ///创建立方体
        ///</summary>
        ///<param name="width">宽度</param>
        ///<param name="height">高度</param>
        ///<param name="depth">深度</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateBox(float width = 1.0f, float height = 1.0f, float depth = 1.0f, PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float halfD = depth * 0.5f;

            //8个顶点
            Vector3[] vertices =
            [
                new(-halfW, -halfH, halfD),     //0
                new(halfW, -halfH, halfD),      //1
                new(halfW, halfH, halfD),       //2
                new(-halfW, halfH, halfD),      //3
                new(-halfW, -halfH, -halfD),    //4
                new(-halfW, halfH, -halfD),     //5
                new(halfW, halfH, -halfD),      //6
                new(halfW, -halfH, -halfD)      //7
            ];

            //每个面的顶点索引
            uint[][] faceIndices = new uint[][]
            {
                [0, 1, 2, 0, 2, 3], //前
                [4, 5, 6, 4, 6, 7], //后
                [3, 2, 6, 3, 6, 5], //上
                [0, 4, 7, 0, 7, 1], //下
                [0, 3, 5, 0, 5, 4], //左
                [1, 7, 6, 1, 6, 2]  //右
            };

            //每个面的法向量
            Vector3[] normals =
            [
                new(0, 0, 1),   //前
                new(0, 0, -1),  //后
                new(0, 1, 0),   //上
                new(0, -1, 0),  //下
                new(-1, 0, 0),  //左
                new(1, 0, 0)    //右
            ];

            //纹理坐标
            Vector2[][] texCoords = new Vector2[][]
            {
                [new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)], //前
                [new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)], //后
                [new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)], //上
                [new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)], //下
                [new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)], //左
                [new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)]  //右
            };

            List<Vertex> finalVertices = [];
            List<uint> finalIndices = [];

            uint vertexCounter = 0;
            for (int face = 0; face < 6; face++)
            {
                for (int i = 0; i < 6; i++)
                {
                    uint vertexIdx = faceIndices[face][i];
                    uint texIdx = (uint)(i < 4 ? i : i - 4); //纹理坐标索引映射

                    finalVertices.Add(new Vertex
                    {
                        Position = vertices[vertexIdx],
                        Color = color,
                        TextureCoord = texCoords[face][texIdx],
                        Normal = normals[face]
                    });
                    finalIndices.Add(vertexCounter++);
                }
            }

            return new MeshGeometry(primitiveType, finalVertices, finalIndices);
        }

        ///<summary>
        ///创建边界立方体
        ///</summary>
        ///<param name="width">宽度</param>
        ///<param name="height">高度</param>
        ///<param name="depth">深度</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateBoundingBox(float width = 1.0f, float height = 1.0f, float depth = 1.0f, PrimitiveType primitiveType = PrimitiveType.Lines, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float halfD = depth * 0.5f;

            Vector3[] vertices =
            [
                new(-halfW, -halfH, halfD),     //0
                new(halfW, -halfH, halfD),      //1
                new(halfW, halfH, halfD),       //2
                new(-halfW, halfH, halfD),      //3
                new(-halfW, -halfH, -halfD),    //4
                new(-halfW, halfH, -halfD),     //5
                new(halfW, halfH, -halfD),      //6
                new(halfW, -halfH, -halfD)      //7
            ];

            uint[][] edges = new uint[][]
            {
                [0, 1], [1, 2], [2, 3], [3, 0], //前
                [4, 5], [5, 6], [6, 7], [7, 4], //后
                [0, 4], [1, 7], [2, 6], [3, 5]  //连接
            };

            List<Vertex> finalVertices = [];
            for (int i = 0; i < 8; i++)
            {
                finalVertices.Add(new Vertex
                {
                    Position = vertices[i],
                    Color = color,
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.Zero
                });
            }

            List<uint> indices = [];
            for (int i = 0; i < 12; i++)
            {
                indices.Add(edges[i][0]);
                indices.Add(edges[i][1]);
            }

            return new MeshGeometry(primitiveType, finalVertices, indices);
        }

        ///<summary>
        ///创建球体
        ///</summary>
        ///<param name="radius">半径</param>
        ///<param name="segments">经线数量</param>
        ///<param name="rings">纬线数量</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateSphere(float radius = 1.0f, int segments = 32, int rings = 16, PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];

            for (int i = 0; i <= rings; i++)
            {
                float v = i / (float)rings;
                float phi = v * MathHelper.Pi;

                for (int j = 0; j <= segments; j++)
                {
                    float u = j / (float)segments;
                    float theta = u * 2.0f * MathHelper.Pi;

                    float x = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    float y = radius * (float)Math.Cos(phi);
                    float z = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    Vector3 position = new(x, y, z);
                    Vector3 normal = Vector3.Normalize(position);
                    Vector2 texCoord = new(u, v);

                    vertices.Add(new Vertex
                    {
                        Position = position,
                        Color = color,
                        TextureCoord = texCoord,
                        Normal = normal
                    });
                }
            }

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int first = i * (segments + 1) + j;
                    int second = first + segments + 1;

                    indices.Add((uint)first);
                    indices.Add((uint)second);
                    indices.Add((uint)(first + 1));

                    indices.Add((uint)(first + 1));
                    indices.Add((uint)second);
                    indices.Add((uint)(second + 1));
                }
            }

            return new MeshGeometry(primitiveType, vertices, indices);
        }

        ///<summary>
        ///创建平面
        ///</summary>
        ///<param name="width">宽度</param>
        ///<param name="height">高度</param>
        ///<param name="widthSegments">宽度细分数量</param>
        ///<param name="heightSegments">高度细分数量</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreatePlane(float width = 1.0f, float height = 1.0f,
            int widthSegments = 1, int heightSegments = 1,
            PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            List<Vertex> vertices = [];
            List<uint> indices = [];

            for (int y = 0; y <= heightSegments; y++)
            {
                for (int x = 0; x <= widthSegments; x++)
                {
                    float u = x / (float)widthSegments;
                    float v = y / (float)heightSegments;

                    float px = u * width - halfW;
                    float pz = v * height - halfH;

                    vertices.Add(new Vertex
                    {
                        Position = new Vector3(px, 0, pz),
                        Color = color,
                        TextureCoord = new Vector2(u, 1.0f - v),
                        Normal = Vector3.UnitY
                    });
                }
            }

            for (int y = 0; y < heightSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    int topLeft = y * (widthSegments + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (y + 1) * (widthSegments + 1) + x;
                    int bottomRight = bottomLeft + 1;

                    indices.Add((uint)topLeft);
                    indices.Add((uint)bottomLeft);
                    indices.Add((uint)topRight);

                    indices.Add((uint)topRight);
                    indices.Add((uint)bottomLeft);
                    indices.Add((uint)bottomRight);
                }
            }

            return new MeshGeometry(primitiveType, vertices, indices);
        }

        ///<summary>
        ///创建圆柱体
        ///</summary>
        ///<param name="radius">半径</param>
        ///<param name="height">高度</param>
        ///<param name="segments">细分数量</param>
        ///<param name="withCaps">是否封闭</param>
        ///<param name="primitiveType">图元类型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateCylinder(float radius = 0.5f, float height = 1.0f, int segments = 32, bool withCaps = true, PrimitiveType primitiveType = PrimitiveType.Triangles, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(1.0f);
            }

            float halfH = height * 0.5f;
            List<Vertex> vertices = [];
            List<uint> indices = [];

            //侧面
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2.0f * MathHelper.Pi * i / segments;
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;

                vertices.Add(new Vertex
                {
                    Position = new Vector3(x, halfH, z),
                    Color = color,
                    TextureCoord = new Vector2(i / (float)segments, 0),
                    Normal = new Vector3(x, 0, z)
                });

                vertices.Add(new Vertex
                {
                    Position = new Vector3(x, -halfH, z),
                    Color = color,
                    TextureCoord = new Vector2(i / (float)segments, 1),
                    Normal = new Vector3(x, 0, z)
                });
            }

            //侧面索引
            for (int i = 0; i < segments; i++)
            {
                int baseIdx = i * 2;
                indices.Add((uint)baseIdx);
                indices.Add((uint)(baseIdx + 1));
                indices.Add((uint)(baseIdx + 2));

                indices.Add((uint)(baseIdx + 1));
                indices.Add((uint)(baseIdx + 3));
                indices.Add((uint)(baseIdx + 2));
            }

            if (withCaps)
            {
                uint topCenter = (uint)vertices.Count;
                vertices.Add(new Vertex
                {
                    Position = new Vector3(0, halfH, 0),
                    Color = color,
                    TextureCoord = new Vector2(0.5f, 0.5f),
                    Normal = Vector3.UnitY
                });

                uint bottomCenter = (uint)vertices.Count;
                vertices.Add(new Vertex
                {
                    Position = new Vector3(0, -halfH, 0),
                    Color = color,
                    TextureCoord = new Vector2(0.5f, 0.5f),
                    Normal = -Vector3.UnitY
                });

                for (int i = 0; i < segments; i++)
                {
                    int idx1 = i * 2;
                    int idx2 = (i + 1) % segments * 2;

                    indices.Add(topCenter);
                    indices.Add((uint)idx1);
                    indices.Add((uint)idx2);

                    indices.Add(bottomCenter);
                    indices.Add((uint)(idx2 + 1));
                    indices.Add((uint)(idx1 + 1));
                }
            }

            MeshGeometry meshGeometry = new(primitiveType, vertices, indices);
            CalculateNormals(meshGeometry);

            return meshGeometry;
        }

        ///<summary>
        ///创建坐标轴
        ///</summary>
        ///<param name="length">长度</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateAxes(float length = 1.0f)
        {
            List<Vertex> vertices =
            [
                new()
                {
                    Position = Vector3.Zero,
                    Color = new Vector4(1, 0, 0, 1),
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitX
                },
                new()
                {
                    Position = new Vector3(length, 0, 0),
                    Color = new Vector4(1, 0, 0, 1),
                    TextureCoord = Vector2.UnitX,
                    Normal = Vector3.UnitX
                },
                new()
                {
                    Position = Vector3.Zero,
                    Color = new Vector4(0, 1, 0, 1),
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitY
                },
                new()
                {
                    Position = new Vector3(0, length, 0),
                    Color = new Vector4(0, 1, 0, 1),
                    TextureCoord = Vector2.UnitX,
                    Normal = Vector3.UnitY
                },
                new()
                {
                    Position = Vector3.Zero,
                    Color = new Vector4(0, 0, 1, 1),
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitZ
                },
                new()
                {
                    Position = new Vector3(0, 0, length),
                    Color = new Vector4(0, 0, 1, 1),
                    TextureCoord = Vector2.UnitX,
                    Normal = Vector3.UnitZ
                }
            ];

            List<uint> indices = [0, 1, 2, 3, 4, 5];

            return new MeshGeometry(PrimitiveType.Lines, vertices, indices);
        }

        ///<summary>
        ///创建网格
        ///</summary>
        ///<param name="size">尺寸</param>
        ///<param name="divisions">分隔数量</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateGrid(float size = 10.0f, int divisions = 10, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(0.5f);
            }

            float halfSize = size * 0.5f;
            float step = size / divisions;

            List<Vertex> vertices = [];
            List<uint> indices = [];

            for (int i = 0; i <= divisions; i++)
            {
                float pos = -halfSize + i * step;

                //竖线
                vertices.Add(new Vertex
                {
                    Position = new Vector3(pos, 0, -halfSize),
                    Color = color,
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitY
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector3(pos, 0, halfSize),
                    Color = color,
                    TextureCoord = Vector2.UnitX,
                    Normal = Vector3.UnitY
                });

                //横线
                vertices.Add(new Vertex
                {
                    Position = new Vector3(-halfSize, 0, pos),
                    Color = color,
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitY
                });
                vertices.Add(new Vertex
                {
                    Position = new Vector3(halfSize, 0, pos),
                    Color = color,
                    TextureCoord = Vector2.UnitX,
                    Normal = Vector3.UnitY
                });
            }

            int lines = divisions + 1;
            for (int i = 0; i < lines * 2; i++)
            {
                indices.Add((uint)(i * 2));
                indices.Add((uint)(i * 2 + 1));
            }

            return new MeshGeometry(PrimitiveType.Lines, vertices, indices);
        }

        ///<summary>
        ///创建线框
        ///</summary>
        ///<param name="meshGeometry">网格模型</param>
        ///<param name="color">颜色</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry CreateWireframe(MeshGeometry meshGeometry, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(0, 0, 0, 1);
            }

            if (meshGeometry.PrimitiveType != PrimitiveType.Triangles)
            {
                return meshGeometry;
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];
            HashSet<(uint, uint)> edges = [];

            for (int i = 0; i < meshGeometry.Indices.Length; i += 3)
            {
                uint i0 = meshGeometry.Indices[i];
                uint i1 = meshGeometry.Indices[i + 1];
                uint i2 = meshGeometry.Indices[i + 2];

                (uint, uint)[] edgePairs =
                [
                    (Math.Min(i0, i1), Math.Max(i0, i1)),
                    (Math.Min(i1, i2), Math.Max(i1, i2)),
                    (Math.Min(i2, i0), Math.Max(i2, i0))
                ];

                foreach ((uint, uint) edge in edgePairs)
                {
                    if (edges.Add(edge))
                    {
                        Vertex v1 = meshGeometry.Vertices[(int)edge.Item1];
                        Vertex v2 = meshGeometry.Vertices[(int)edge.Item2];

                        vertices.Add(new Vertex
                        {
                            Position = v1.Position,
                            Color = color,
                            TextureCoord = v1.TextureCoord,
                            Normal = v1.Normal
                        });
                        vertices.Add(new Vertex
                        {
                            Position = v2.Position,
                            Color = color,
                            TextureCoord = v2.TextureCoord,
                            Normal = v2.Normal
                        });

                        uint baseIdx = (uint)(vertices.Count - 2);
                        indices.Add(baseIdx);
                        indices.Add(baseIdx + 1);
                    }
                }
            }

            return new MeshGeometry(PrimitiveType.Lines, vertices, indices);
        }

        ///<summary>
        ///计算法向量
        ///</summary>
        ///<param name="meshGeometry">网格模型</param>
        public static void CalculateNormals(MeshGeometry meshGeometry)
        {
            if (meshGeometry.PrimitiveType != PrimitiveType.Triangles)
            {
                return;
            }

            Vertex[] vertices = meshGeometry.Vertices.ToArray();

            //重置法向量
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = Vector3.Zero;
            }

            //累加每个面的法向量
            for (int i = 0; i < meshGeometry.Indices.Length; i += 3)
            {
                uint i0 = meshGeometry.Indices[i];
                uint i1 = meshGeometry.Indices[i + 1];
                uint i2 = meshGeometry.Indices[i + 2];

                Vector3 v0 = vertices[i0].Position;
                Vector3 v1 = vertices[i1].Position;
                Vector3 v2 = vertices[i2].Position;

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

                vertices[i0].Normal += normal;
                vertices[i1].Normal += normal;
                vertices[i2].Normal += normal;
            }

            //归一化
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Normal.LengthSquared > 0)
                {
                    vertices[i].Normal = Vector3.Normalize(vertices[i].Normal);
                }
            }

            //更新回MeshGeometry
            for (int i = 0; i < vertices.Length; i++)
            {
                meshGeometry.Vertices[i] = vertices[i];
            }
        }

        ///<summary>
        ///合并网格模型
        ///</summary>
        ///<param name="meshGeometry1">网格模型1</param>
        ///<param name="meshGeometry2">网格模型2</param>
        ///<returns>网格模型</returns>
        public static MeshGeometry Combine(MeshGeometry meshGeometry1, MeshGeometry meshGeometry2)
        {
            if (meshGeometry1.PrimitiveType != meshGeometry2.PrimitiveType)
            {
                return meshGeometry1;
            }

            List<Vertex> vertices = new(meshGeometry1.Vertices);
            List<uint> indices = new(meshGeometry1.Indices);

            uint vertexOffset = (uint)vertices.Count;
            vertices.AddRange(meshGeometry2.Vertices);

            foreach (uint index in meshGeometry2.Indices)
            {
                indices.Add(index + vertexOffset);
            }

            return new MeshGeometry(meshGeometry1.PrimitiveType, vertices, indices);
        }

        ///<summary>
        ///变换网格模型
        ///</summary>
        ///<param name="meshGeometry">网格模型</param>
        ///<param name="transform">变换矩阵</param>
        public static void Transform(MeshGeometry meshGeometry, Matrix4 transform)
        {
            Vertex[] vertices = meshGeometry.Vertices.ToArray();

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = Vector3.TransformPosition(vertices[i].Position, transform);
                vertices[i].Normal = Vector3.Normalize(Vector3.TransformNormal(vertices[i].Normal, transform));
            }

            //更新回MeshGeometry
            for (int i = 0; i < vertices.Length; i++)
            {
                meshGeometry.Vertices[i] = vertices[i];
            }
        }
    }
}
