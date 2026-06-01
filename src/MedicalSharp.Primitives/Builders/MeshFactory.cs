using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using MIConvexHull;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Builders
{
    /// <summary>
    /// Mesh工厂
    /// </summary>
    public static class MeshFactory
    {
        #region # 创建点 —— static MeshGeometry CreatePoint(Vector3 position)
        /// <summary>
        /// 创建点
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreatePoint(Vector3 position)
        {
            Vertex vertex = new Vertex
            {
                Position = position,
                TextureCoord = Vector2.Zero,
                Normal = Vector3.UnitY
            };

            List<Vertex> vertices = [vertex];
            List<uint> indices = [0];

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建点云 —— static MeshGeometry CreatePointCloud(IReadOnlyList<Vector3> positions)
        /// <summary>
        /// 创建点云
        /// </summary>
        /// <param name="positions">位置列表</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreatePointCloud(IReadOnlyList<Vector3> positions)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];
            for (int index = 0; index < positions.Count; index++)
            {
                Vector3 position = positions[index];
                vertices.Add(new Vertex
                {
                    Position = position,
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.UnitY
                });
                indices.Add((uint)index);
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建线段 —— static MeshGeometry CreateLineSegment(Vector3 start, Vector3 end)
        /// <summary>
        /// 创建线段
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateLineSegment(Vector3 start, Vector3 end)
        {
            Vector3 direction = Vector3.Normalize(end - start);
            List<Vertex> vertices =
            [
                new Vertex
                {
                    Position = start,
                    TextureCoord = new Vector2(0, 0),
                    Normal = direction
                },
                new Vertex
                {
                    Position = end,
                    TextureCoord = new Vector2(1, 0),
                    Normal = direction
                }
            ];
            List<uint> indices = [0, 1];

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建折线 —— static MeshGeometry CreatePolyline(IReadOnlyList<Vector3> positions...
        /// <summary>
        /// 创建折线
        /// </summary>
        /// <param name="positions">位置列表</param>
        /// <param name="closed">是否闭合</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreatePolyline(IReadOnlyList<Vector3> positions, bool closed = false)
        {
            #region # 验证

            if (positions == null || positions.Count == 0)
            {
                return new MeshGeometry([]);  //空网格
            }
            if (positions.Count == 1)
            {
                //单个点：创建一个零长度的线段（点）
                Vertex vertex = new Vertex
                {
                    Position = positions[0],
                    TextureCoord = Vector2.Zero,
                    Normal = Vector3.Zero
                };

                return new MeshGeometry([vertex]);
            }

            #endregion

            List<Vertex> vertices = [];
            for (int index = 0; index < positions.Count; index++)
            {
                //计算法向量（方向）
                Vector3 normal;
                if (index == 0)
                {
                    //第一个点：使用到下一个点的方向
                    normal = Vector3.Normalize(positions[1] - positions[0]);
                }
                else if (index == positions.Count - 1)
                {
                    //最后一个点：使用到前一个点的方向
                    normal = Vector3.Normalize(positions[index] - positions[index - 1]);
                }
                else
                {
                    //中间点：使用前后向量的平均方向
                    Vector3 dir1 = positions[index] - positions[index - 1];
                    Vector3 dir2 = positions[index + 1] - positions[index];
                    normal = Vector3.Normalize(dir1 + dir2);
                }

                vertices.Add(new Vertex
                {
                    Position = positions[index],
                    TextureCoord = new Vector2(index / (float)(positions.Count - 1), 0),
                    Normal = normal
                });
            }

            List<uint> indices = [];
            for (int index = 0; index < positions.Count - 1; index++)
            {
                indices.Add((uint)index);
                indices.Add((uint)(index + 1));
            }
            if (closed && positions.Count > 2)
            {
                indices.Add((uint)(positions.Count - 1));
                indices.Add(0);
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建多边形 —— static MeshGeometry CreatePolygon(IReadOnlyList<Vector3> positions)
        /// <summary>
        /// 创建多边形
        /// </summary>
        /// <param name="positions">位置列表</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreatePolygon(IReadOnlyList<Vector3> positions)
        {
            #region # 验证

            if (positions == null || positions.Count == 0)
            {
                throw new ArgumentNullException(nameof(positions), "位置列表不可为空！");
            }
            if (positions.Count == 1)
            {
                return CreatePoint(positions[0]);
            }
            if (positions.Count == 2)
            {
                return CreateLineSegment(positions[0], positions[1]);
            }

            #endregion

            //计算中心点
            Vector3 center = Vector3.Zero;
            foreach (Vector3 pos in positions)
            {
                center += pos;
            }
            center /= positions.Count;

            //计算法向量（Newell's method，适用于非平面多边形）
            Vector3 normal = Vector3.Zero;
            for (int index = 0; index < positions.Count; index++)
            {
                Vector3 current = positions[index];
                Vector3 next = positions[(index + 1) % positions.Count];
                normal.X += (current.Y - next.Y) * (current.Z + next.Z);
                normal.Y += (current.Z - next.Z) * (current.X + next.X);
                normal.Z += (current.X - next.X) * (current.Y + next.Y);
            }
            normal = Vector3.Normalize(normal);

            //构建顶点列表
            List<Vertex> vertices = [];

            //添加中心顶点
            vertices.Add(new Vertex
            {
                Position = center,
                TextureCoord = new Vector2(0.5f, 0.5f),
                Normal = normal,
                Color = Vector4.One
            });

            //添加边界顶点
            for (int i = 0; i < positions.Count; i++)
            {
                float angle = (float)i / positions.Count * MathHelper.TwoPi;
                vertices.Add(new Vertex
                {
                    Position = positions[i],
                    TextureCoord = new Vector2(
                        0.5f + 0.5f * MathF.Cos(angle),
                        0.5f + 0.5f * MathF.Sin(angle)),
                    Normal = normal,
                    Color = Vector4.One
                });
            }

            //构建索引列表（三角形扇）
            List<uint> indices = [];
            for (int i = 0; i < positions.Count; i++)
            {
                indices.Add(0);                                          //中心点
                indices.Add((uint)(i + 1));                              //当前边界点
                indices.Add((uint)((i + 1) % positions.Count + 1));      //下一个边界点
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建三角形 —— static MeshGeometry CreateTriangle(Vector3 pointA, Vector3 pointB...
        /// <summary>
        /// 创建三角形
        /// </summary>
        /// <param name="pointA">顶点A</param>
        /// <param name="pointB">顶点B</param>
        /// <param name="pointC">顶点C</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //==========线框模式：三条边==========
                vertices.Add(new Vertex { Position = pointA, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointB, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointB, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointC, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointC, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointA, Normal = Vector3.Zero });

                indices.Add(0); indices.Add(1);
                indices.Add(2); indices.Add(3);
                indices.Add(4); indices.Add(5);
            }
            else
            {
                //==========填充模式：一个三角形面==========
                Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointC - pointA));

                vertices.Add(new Vertex { Position = pointA, TextureCoord = new Vector2(0, 0), Normal = normal });
                vertices.Add(new Vertex { Position = pointB, TextureCoord = new Vector2(1, 0), Normal = normal });
                vertices.Add(new Vertex { Position = pointC, TextureCoord = new Vector2(0.5f, 1), Normal = normal });

                indices.Add(0); indices.Add(1); indices.Add(2);
            }

            MeshGeometry meshGeometry = new(vertices, indices);

            if (primitiveType != GraphicPrimitiveType.Lines)
            {
                CalculateNormals(meshGeometry);
            }

            return meshGeometry;
        }
        #endregion

        #region # 创建矩形 —— static MeshGeometry CreateRectangle(Vector3 center, float width...
        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="center">中心点位置</param>
        /// <param name="width">宽度（X轴方向）</param>
        /// <param name="height">高度（Y轴方向）</param>
        /// <param name="normal">法向量（控制矩形朝向）</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateRectangle(Vector3 center, float width, float height, Vector3 normal = default, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Lines)
        {
            if (normal == default)
            {
                //默认朝上（Z轴）
                normal = Vector3.UnitZ;
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            //计算局部坐标系
            Vector3 right, up;

            //根据法向量计算基向量
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.999f)
            {
                //法向量平行于Z轴
                right = Vector3.UnitX;
                up = Vector3.UnitY;
            }
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.999f)
            {
                //法向量平行于Y轴
                right = Vector3.UnitX;
                up = Vector3.UnitZ;
            }
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.999f)
            {
                //法向量平行于X轴
                right = Vector3.UnitY;
                up = Vector3.UnitZ;
            }
            else
            {
                //一般情况：使用叉积构建正交基
                right = Vector3.Normalize(Vector3.Cross(normal, Vector3.UnitZ));
                up = Vector3.Normalize(Vector3.Cross(right, normal));
            }

            //计算四个角点（相对于中心点）
            Vector3[] corners =
            [
                center - right * halfW - up * halfH, //0: 左下
                center + right * halfW - up * halfH, //1: 右下
                center + right * halfW + up * halfH, //2: 右上
                center - right * halfW + up * halfH  //3: 左上
            ];

            List<Vertex> vertices = [];
            List<uint> indices = [];

            //线框模式 - 4个顶点，4条边（8个索引）
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //创建顶点
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new Vertex
                    {
                        Position = corners[i],
                        TextureCoord = Vector2.Zero,
                        Normal = Vector3.Zero  //线框不需要法线
                    });
                }

                //创建索引 - 4条边
                uint[][] edges = new uint[][]
                {
                    [0, 1], //下边
                    [1, 2], //右边
                    [2, 3], //上边
                    [3, 0]  //左边
                };

                for (int i = 0; i < 4; i++)
                {
                    indices.Add(edges[i][0]);
                    indices.Add(edges[i][1]);
                }
            }
            //填充模式 - 4个顶点，2个三角形（6个索引）
            else
            {
                //创建顶点
                Vector2[] texCoords =
                [
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                ];

                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new Vertex
                    {
                        Position = corners[i],
                        TextureCoord = texCoords[i],
                        Normal = normal
                    });
                }

                //创建索引 - 2个三角形
                indices.AddRange([0, 1, 2]); //第一个三角形
                indices.AddRange([0, 2, 3]); //第二个三角形
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建椭圆 —— static MeshGeometry CreateEllipse(Vector3 center, float width...
        /// <summary>
        /// 创建椭圆
        /// </summary>
        /// <param name="center">中心点位置</param>
        /// <param name="width">宽度（X轴方向的直径）</param>
        /// <param name="height">高度（Y轴方向的直径）</param>
        /// <param name="normal">法向量（控制椭圆朝向）</param>
        /// <param name="segments">细分数量（边数）</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateEllipse(Vector3 center, float width, float height, Vector3 normal = default, int segments = 64, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles)
        {
            if (normal == default)
            {
                //默认朝上（Z轴）
                normal = Vector3.UnitZ;
            }

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            //计算局部坐标系
            Vector3 right, up;

            //根据法向量计算基向量
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.999f)
            {
                //法向量平行于Z轴
                right = Vector3.UnitX;
                up = Vector3.UnitY;
            }
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.999f)
            {
                //法向量平行于Y轴
                right = Vector3.UnitX;
                up = Vector3.UnitZ;
            }
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.999f)
            {
                //法向量平行于X轴
                right = Vector3.UnitY;
                up = Vector3.UnitZ;
            }
            else
            {
                //一般情况：使用叉积构建正交基
                right = Vector3.Normalize(Vector3.Cross(normal, Vector3.UnitZ));
                up = Vector3.Normalize(Vector3.Cross(right, normal));
            }

            //确保segments至少为3
            segments = Math.Max(3, segments);

            List<Vertex> vertices = [];
            List<uint> indices = [];

            //线框模式 - 只绘制边界
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //创建边界顶点（不包含中心点）
                for (int i = 0; i < segments; i++)
                {
                    float angle = 2.0f * MathHelper.Pi * i / segments;
                    float x = halfW * (float)Math.Cos(angle);
                    float y = halfH * (float)Math.Sin(angle);

                    Vector3 position = center + right * x + up * y;

                    vertices.Add(new Vertex
                    {
                        Position = position,
                        TextureCoord = Vector2.Zero,
                        Normal = Vector3.Zero  //线框不需要法向量
                    });
                }

                //创建边界线的索引（闭合）
                for (int i = 0; i < segments; i++)
                {
                    indices.Add((uint)i);
                    indices.Add((uint)((i + 1) % segments));
                }
            }
            //填充模式 - 使用三角形扇形（中心点 + 边界顶点）
            else
            {
                //添加中心点
                vertices.Add(new Vertex
                {
                    Position = center,
                    TextureCoord = new Vector2(0.5f, 0.5f),
                    Normal = normal
                });

                //添加边界顶点
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 2.0f * MathHelper.Pi * i / segments;
                    float x = halfW * (float)Math.Cos(angle);
                    float y = halfH * (float)Math.Sin(angle);

                    Vector3 position = center + right * x + up * y;

                    //计算纹理坐标（将椭圆映射到单位圆）
                    float u = (float)Math.Cos(angle) * 0.5f + 0.5f;
                    float v = (float)Math.Sin(angle) * 0.5f + 0.5f;

                    vertices.Add(new Vertex
                    {
                        Position = position,
                        TextureCoord = new Vector2(u, v),
                        Normal = normal
                    });
                }

                //创建三角形索引（中心点 + 相邻的两个边界点）
                for (int i = 0; i < segments; i++)
                {
                    indices.Add(0);                          //中心点
                    indices.Add((uint)(i + 1));              //当前边界点
                    indices.Add((uint)(i + 2));              //下一个边界点
                }
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建四边形 —— static MeshGeometry CreateQuadrangle(Vector3 pointA, Vector3 pointB...
        /// <summary>
        /// 创建四边形
        /// </summary>
        /// <param name="pointA">顶点A</param>
        /// <param name="pointB">顶点B</param>
        /// <param name="pointC">顶点C</param>
        /// <param name="pointD">顶点D</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateQuadrangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles)
        {
            List<Vertex> vertices = [];
            List<uint> indices = [];
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //==========线框模式：四条边==========
                vertices.Add(new Vertex { Position = pointA, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointB, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointB, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointC, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointC, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointD, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointD, Normal = Vector3.Zero });
                vertices.Add(new Vertex { Position = pointA, Normal = Vector3.Zero });

                indices.Add(0); indices.Add(1);
                indices.Add(2); indices.Add(3);
                indices.Add(4); indices.Add(5);
                indices.Add(6); indices.Add(7);
            }
            else
            {
                //==========填充模式：两个三角形==========
                Vector3 normal = Vector3.Normalize(Vector3.Cross(pointB - pointA, pointD - pointA));

                vertices.Add(new Vertex { Position = pointA, TextureCoord = new Vector2(0, 1), Normal = normal });
                vertices.Add(new Vertex { Position = pointB, TextureCoord = new Vector2(1, 1), Normal = normal });
                vertices.Add(new Vertex { Position = pointC, TextureCoord = new Vector2(1, 0), Normal = normal });
                vertices.Add(new Vertex { Position = pointD, TextureCoord = new Vector2(0, 0), Normal = normal });

                //三角形 A-B-C 和 A-C-D
                indices.Add(0); indices.Add(1); indices.Add(2);
                indices.Add(0); indices.Add(2); indices.Add(3);
            }

            MeshGeometry meshGeometry = new(vertices, indices);

            if (primitiveType != GraphicPrimitiveType.Lines)
            {
                CalculateNormals(meshGeometry);
            }

            return meshGeometry;
        }
        #endregion

        #region # 创建立方体 —— static MeshGeometry CreateBox(float width = 1.0f, float height = 1.0f...
        /// <summary>
        /// 创建立方体
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="center">中心点位置</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateBox(float width = 1.0f, float height = 1.0f, float depth = 1.0f, Vector3 center = default)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float halfD = depth * 0.5f;

            //8个顶点（相对于中心点）
            Vector3[] vertices =
            [
                new(center.X - halfW, center.Y + halfD, center.Z - halfH), //0: 左前下
                new(center.X + halfW, center.Y + halfD, center.Z - halfH), //1: 右前下
                new(center.X + halfW, center.Y + halfD, center.Z + halfH), //2: 右前上
                new(center.X - halfW, center.Y + halfD, center.Z + halfH), //3: 左前上
                new(center.X - halfW, center.Y - halfD, center.Z - halfH), //4: 左后下
                new(center.X - halfW, center.Y - halfD, center.Z + halfH), //5: 左后上
                new(center.X + halfW, center.Y - halfD, center.Z + halfH), //6: 右后上
                new(center.X + halfW, center.Y - halfD, center.Z - halfH)  //7: 右后下
            ];

            //每个面的顶点索引
            uint[][] faceIndices = new uint[][]
            {
                [0, 1, 2, 0, 2, 3], //前（Y正方向）
                [4, 5, 6, 4, 6, 7], //后（Y负方向）
                [3, 2, 6, 3, 6, 5], //上（Z正方向）
                [0, 4, 7, 0, 7, 1], //下（Z负方向）
                [0, 3, 5, 0, 5, 4], //左（X负方向）
                [1, 7, 6, 1, 6, 2]  //右（X正方向）
            };

            //每个面的法向量
            Vector3[] normals =
            [
                new(0, 1, 0),   //前（Y正）
                new(0, -1, 0),  //后（Y负）
                new(0, 0, 1),   //上（Z正）
                new(0, 0, -1),  //下（Z负）
                new(-1, 0, 0),  //左（X负）
                new(1, 0, 0)    //右（X正）
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
                        TextureCoord = texCoords[face][texIdx],
                        Normal = normals[face]
                    });
                    finalIndices.Add(vertexCounter++);
                }
            }

            return new MeshGeometry(finalVertices, finalIndices);
        }
        #endregion

        #region # 创建边界立方体 —— static MeshGeometry CreateBoundingBox(float width = 1.0f, float height = 1.0f...
        /// <summary>
        /// 创建边界立方体
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="center">中心点位置</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateBoundingBox(float width = 1.0f, float height = 1.0f, float depth = 1.0f, Vector3 center = default, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Lines)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;  //对应Z轴高度
            float halfD = depth * 0.5f;   //对应Y轴深度

            List<Vertex> vertices = [];
            List<uint> indices = [];

            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //线框模式 - 8个顶点，12条边（24个索引）
                Vector3[] cornerVertices =
                [
                    //前平面（Y为正方向）
                    new Vector3(center.X - halfW, center.Y + halfD, center.Z - halfH), //0: 左下前
                    new Vector3(center.X + halfW, center.Y + halfD, center.Z - halfH), //1: 右下前
                    new Vector3(center.X + halfW, center.Y + halfD, center.Z + halfH), //2: 右上前
                    new Vector3(center.X - halfW, center.Y + halfD, center.Z + halfH), //3: 左上前

                    //后平面（Y为负方向）
                    new Vector3(center.X - halfW, center.Y - halfD, center.Z - halfH), //4: 左下后
                    new Vector3(center.X - halfW, center.Y - halfD, center.Z + halfH), //5: 左上后
                    new Vector3(center.X + halfW, center.Y - halfD, center.Z + halfH), //6: 右上后
                    new Vector3(center.X + halfW, center.Y - halfD, center.Z - halfH)  //7: 右下后
                ];

                //12条边（每边2个点）
                uint[][] edges = new uint[][]
                {
                    //前平面
                    [0, 1], //下边
                    [1, 2], //右边
                    [2, 3], //上边
                    [3, 0], //左边

                    //后平面
                    [4, 5], //左边
                    [5, 6], //上边
                    [6, 7], //右边
                    [7, 4], //下边

                    //连接前后面
                    [0, 4], //左下
                    [1, 7], //右下
                    [2, 6], //右上
                    [3, 5]  //左上
                };

                //创建顶点（法线设为0，因为线框不需要光照计算）
                for (int i = 0; i < 8; i++)
                {
                    vertices.Add(new Vertex
                    {
                        Position = cornerVertices[i],
                        TextureCoord = Vector2.Zero,
                        Normal = Vector3.Zero
                    });
                }

                //创建索引
                for (int i = 0; i < 12; i++)
                {
                    indices.Add(edges[i][0]);
                    indices.Add(edges[i][1]);
                }
            }
            else
            {
                //Triangles模式 - 24个顶点（每个面4个独立顶点），36个索引
                //定义6个面的24个顶点（相对于中心点）
                Vector3[][] faceVertices = new Vector3[][]
                {
            //前面 (Y = center.Y + halfD)
            [
                new Vector3(center.X - halfW, center.Y + halfD, center.Z - halfH), //0
                new Vector3(center.X + halfW, center.Y + halfD, center.Z - halfH), //1
                new Vector3(center.X + halfW, center.Y + halfD, center.Z + halfH), //2
                new Vector3(center.X - halfW, center.Y + halfD, center.Z + halfH)  //3
            ],
            //后面 (Y = center.Y - halfD)
            [
                new Vector3(center.X + halfW, center.Y - halfD, center.Z - halfH), //4
                new Vector3(center.X - halfW, center.Y - halfD, center.Z - halfH), //5
                new Vector3(center.X - halfW, center.Y - halfD, center.Z + halfH), //6
                new Vector3(center.X + halfW, center.Y - halfD, center.Z + halfH)  //7
            ],
            //右面 (X = center.X + halfW)
            [
                new Vector3(center.X + halfW, center.Y + halfD, center.Z - halfH), //8
                new Vector3(center.X + halfW, center.Y - halfD, center.Z - halfH), //9
                new Vector3(center.X + halfW, center.Y - halfD, center.Z + halfH), //10
                new Vector3(center.X + halfW, center.Y + halfD, center.Z + halfH)  //11
            ],
            //左面 (X = center.X - halfW)
            [
                new Vector3(center.X - halfW, center.Y - halfD, center.Z - halfH), //12
                new Vector3(center.X - halfW, center.Y + halfD, center.Z - halfH), //13
                new Vector3(center.X - halfW, center.Y + halfD, center.Z + halfH), //14
                new Vector3(center.X - halfW, center.Y - halfD, center.Z + halfH)  //15
            ],
            //上面 (Z = center.Z + halfH)
            [
                new Vector3(center.X - halfW, center.Y + halfD, center.Z + halfH), //16
                new Vector3(center.X + halfW, center.Y + halfD, center.Z + halfH), //17
                new Vector3(center.X + halfW, center.Y - halfD, center.Z + halfH), //18
                new Vector3(center.X - halfW, center.Y - halfD, center.Z + halfH)  //19
            ],
            //下面 (Z = center.Z - halfH)
            [
                new Vector3(center.X - halfW, center.Y - halfD, center.Z - halfH), //20
                new Vector3(center.X + halfW, center.Y - halfD, center.Z - halfH), //21
                new Vector3(center.X + halfW, center.Y + halfD, center.Z - halfH), //22
                new Vector3(center.X - halfW, center.Y + halfD, center.Z - halfH)  //23
            ]
                };

                //每个面的法线
                Vector3[] faceNormals =
                [
                    new Vector3(0, 1, 0),  //前面（Y正方向）
                    new Vector3(0, -1, 0), //后面（Y负方向）
                    new Vector3(1, 0, 0),  //右面（X正方向）
                    new Vector3(-1, 0, 0), //左面（X负方向）
                    new Vector3(0, 0, 1),  //上面（Z正方向）
                    new Vector3(0, 0, -1)  //下面（Z负方向）
                ];

                //创建24个顶点（每个面4个）
                for (int face = 0; face < 6; face++)
                {
                    for (int vert = 0; vert < 4; vert++)
                    {
                        vertices.Add(new Vertex
                        {
                            Position = faceVertices[face][vert],
                            TextureCoord = Vector2.Zero,
                            Normal = faceNormals[face]
                        });
                    }
                }

                //创建36个索引（每个面2个三角形，6个面 × 6个索引 = 36）
                for (int face = 0; face < 6; face++)
                {
                    uint baseIndex = (uint)(face * 4);

                    //第一个三角形: 0-1-2
                    indices.Add(baseIndex + 0);
                    indices.Add(baseIndex + 1);
                    indices.Add(baseIndex + 2);

                    //第二个三角形: 0-2-3
                    indices.Add(baseIndex + 0);
                    indices.Add(baseIndex + 2);
                    indices.Add(baseIndex + 3);
                }
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建球体 —— static MeshGeometry CreateSphere(float radius = 1.0f, int segments = 32...
        /// <summary>
        /// 创建球体
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="center">中心点位置</param>
        /// <param name="segments">经线数量</param>
        /// <param name="rings">纬线数量</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateSphere(float radius = 1.0f, Vector3 center = default, int segments = 32, int rings = 16)
        {
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

                    //计算相对于原点的位置
                    float x = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    float y = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    float z = radius * (float)Math.Cos(phi);

                    //应用中心点偏移
                    Vector3 position = new(center.X + x, center.Y + y, center.Z + z);

                    //法向量方向保持不变（从球心指向表面）
                    Vector3 normal = Vector3.Normalize(new Vector3(x, y, z));
                    Vector2 texCoord = new(u, v);

                    vertices.Add(new Vertex
                    {
                        Position = position,
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

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建圆柱体 —— static MeshGeometry CreateCylinder(float radius = 0.5f, float height = 1.0f...
        /// <summary>
        /// 创建圆柱体
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="height">高度（Z轴方向）</param>
        /// <param name="center">中心点位置</param>
        /// <param name="segments">细分数量</param>
        /// <param name="primitiveType">图元类型</param>
        /// <param name="withCaps">是否封闭（仅填充模式有效）</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateCylinder(float radius = 0.5f, float height = 1.0f, Vector3 center = default, int segments = 32, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles, bool withCaps = true)
        {
            float halfH = height * 0.5f;  //Z轴方向的一半
            List<Vertex> vertices = [];
            List<uint> indices = [];
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //==========线框模式==========
                //上下两个圆的线段
                for (int index = 0; index < segments; index++)
                {
                    float angle1 = 2.0f * MathHelper.Pi * index / segments;
                    float angle2 = 2.0f * MathHelper.Pi * (index + 1) / segments;
                    float x1 = (float)Math.Cos(angle1) * radius;
                    float y1 = (float)Math.Sin(angle1) * radius;
                    float x2 = (float)Math.Cos(angle2) * radius;
                    float y2 = (float)Math.Sin(angle2) * radius;

                    //上圆（Z = +halfH）
                    vertices.Add(new Vertex { Position = center + new Vector3(x1, y1, halfH), Normal = Vector3.Zero });
                    vertices.Add(new Vertex { Position = center + new Vector3(x2, y2, halfH), Normal = Vector3.Zero });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));

                    //下圆（Z = -halfH）
                    vertices.Add(new Vertex { Position = center + new Vector3(x1, y1, -halfH), Normal = Vector3.Zero });
                    vertices.Add(new Vertex { Position = center + new Vector3(x2, y2, -halfH), Normal = Vector3.Zero });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));
                }

                //垂直方向的线段（沿Z轴）
                for (int index = 0; index < segments; index++)
                {
                    float angle = 2.0f * MathHelper.Pi * index / segments;
                    float x = (float)Math.Cos(angle) * radius;
                    float y = (float)Math.Sin(angle) * radius;

                    Vector3 top = center + new Vector3(x, y, halfH);
                    Vector3 bottom = center + new Vector3(x, y, -halfH);

                    vertices.Add(new Vertex { Position = top, Normal = Vector3.Zero });
                    vertices.Add(new Vertex { Position = bottom, Normal = Vector3.Zero });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));
                }
            }
            else
            {
                //==========填充模式==========
                //侧面
                for (int index = 0; index <= segments; index++)
                {
                    float angle = 2.0f * MathHelper.Pi * index / segments;
                    float x = (float)Math.Cos(angle) * radius;
                    float y = (float)Math.Sin(angle) * radius;
                    Vector3 normal = new Vector3(x, y, 0).Normalized();

                    vertices.Add(new Vertex
                    {
                        Position = center + new Vector3(x, y, halfH),
                        TextureCoord = new Vector2(index / (float)segments, 0),
                        Normal = normal
                    });
                    vertices.Add(new Vertex
                    {
                        Position = center + new Vector3(x, y, -halfH),
                        TextureCoord = new Vector2(index / (float)segments, 1),
                        Normal = normal
                    });
                }

                //侧面索引
                for (int index = 0; index < segments; index++)
                {
                    int baseIdx = index * 2;
                    indices.Add((uint)baseIdx);
                    indices.Add((uint)(baseIdx + 1));
                    indices.Add((uint)(baseIdx + 2));

                    indices.Add((uint)(baseIdx + 1));
                    indices.Add((uint)(baseIdx + 3));
                    indices.Add((uint)(baseIdx + 2));
                }

                //顶盖和底盖
                if (withCaps)
                {
                    uint topCenter = (uint)vertices.Count;
                    vertices.Add(new Vertex
                    {
                        Position = center + new Vector3(0, 0, halfH),
                        TextureCoord = new Vector2(0.5f, 0.5f),
                        Normal = Vector3.UnitZ
                    });

                    uint bottomCenter = (uint)vertices.Count;
                    vertices.Add(new Vertex
                    {
                        Position = center + new Vector3(0, 0, -halfH),
                        TextureCoord = new Vector2(0.5f, 0.5f),
                        Normal = -Vector3.UnitZ
                    });

                    //顶盖三角形
                    for (int index = 0; index < segments; index++)
                    {
                        int idx1 = index * 2;
                        int idx2 = (index + 1) % segments * 2;

                        indices.Add(topCenter);
                        indices.Add((uint)idx1);
                        indices.Add((uint)idx2);

                        indices.Add(bottomCenter);
                        indices.Add((uint)(idx2 + 1));
                        indices.Add((uint)(idx1 + 1));
                    }
                }
            }

            MeshGeometry meshGeometry = new(vertices, indices);
            if (primitiveType != GraphicPrimitiveType.Lines)
            {
                CalculateNormals(meshGeometry);
            }

            return meshGeometry;
        }
        #endregion

        #region # 创建圆锥体 —— static MeshGeometry CreateCone(float radius = 0.5f, float height = 1.0f...
        /// <summary>
        /// 创建圆锥体
        /// </summary>
        /// <param name="radius">底面半径</param>
        /// <param name="height">高度（从底面到尖端的Z轴方向距离）</param>
        /// <param name="center">底面中心点位置</param>
        /// <param name="segments">细分数量</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateCone(float radius = 0.5f, float height = 1.0f, Vector3 center = default, int segments = 32, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles)
        {
            Vector3 tip = center + new Vector3(0, 0, height);   //尖端Z坐标（相对于底面中心）
            Vector3 bottomCenter = center;
            List<Vertex> vertices = [];
            List<uint> indices = [];
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //==========线框模式==========
                //底面圆
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = 2.0f * MathHelper.Pi * i / segments;
                    float angle2 = 2.0f * MathHelper.Pi * (i + 1) / segments;
                    float x1 = (float)Math.Cos(angle1) * radius;
                    float y1 = (float)Math.Sin(angle1) * radius;
                    float x2 = (float)Math.Cos(angle2) * radius;
                    float y2 = (float)Math.Sin(angle2) * radius;

                    //底面圆边
                    vertices.Add(new Vertex { Position = bottomCenter + new Vector3(x1, y1, 0), Normal = Vector3.Zero });
                    vertices.Add(new Vertex { Position = bottomCenter + new Vector3(x2, y2, 0), Normal = Vector3.Zero });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));

                    //侧面线条（从底面边缘到尖端）
                    vertices.Add(new Vertex { Position = bottomCenter + new Vector3(x1, y1, 0), Normal = Vector3.Zero });
                    vertices.Add(new Vertex { Position = tip, Normal = Vector3.Zero });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));
                }
            }
            else
            {
                //==========填充模式==========
                //计算侧面法向量（每个顶点的法向量需要根据位置计算）
                for (int i = 0; i <= segments; i++)
                {
                    float angle = 2.0f * MathHelper.Pi * i / segments;
                    float x = (float)Math.Cos(angle) * radius;
                    float y = (float)Math.Sin(angle) * radius;

                    //底面边缘点
                    Vector3 bottomEdge = bottomCenter + new Vector3(x, y, 0);

                    //计算侧面法向量（从底面边缘指向外的方向与向上方向的组合）
                    Vector3 radialDir = new Vector3(x, y, 0).Normalized();
                    Vector3 sideNormal = (radialDir + Vector3.UnitZ * (radius / height)).Normalized();

                    vertices.Add(new Vertex
                    {
                        Position = bottomEdge,
                        TextureCoord = new Vector2(i / (float)segments, 1),
                        Normal = sideNormal
                    });
                }

                //尖端
                uint tipIndex = (uint)vertices.Count;
                vertices.Add(new Vertex
                {
                    Position = tip,
                    TextureCoord = new Vector2(0.5f, 0),
                    Normal = Vector3.UnitZ
                });

                //侧面三角形
                for (int i = 0; i < segments; i++)
                {
                    int current = i;
                    int next = (i + 1) % segments;

                    indices.Add((uint)current);
                    indices.Add(tipIndex);
                    indices.Add((uint)next);
                }

                //底面（如果需要封闭）
                uint bottomCenterIndex = (uint)vertices.Count;
                vertices.Add(new Vertex
                {
                    Position = bottomCenter,
                    TextureCoord = new Vector2(0.5f, 0.5f),
                    Normal = -Vector3.UnitZ
                });

                for (int index = 0; index < segments; index++)
                {
                    int current = index;
                    int next = (index + 1) % segments;
                    indices.Add(bottomCenterIndex);
                    indices.Add((uint)next);
                    indices.Add((uint)current);
                }
            }

            MeshGeometry meshGeometry = new(vertices, indices);
            if (primitiveType != GraphicPrimitiveType.Lines)
            {
                CalculateNormals(meshGeometry);
            }

            return meshGeometry;
        }
        #endregion

        #region # 创建凸多面体 —— static MeshGeometry CreateConvexPolyhedron(IReadOnlyList<Vector3> positions...
        /// <summary>
        /// 创建凸多面体
        /// </summary>
        /// <param name="positions">位置列表</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateConvexPolyhedron(IReadOnlyList<Vector3> positions, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Triangles)
        {
            #region # 验证

            if (positions == null || !positions.Any())
            {
                throw new ArgumentException("位置列表不可为空！");
            }
            if (positions.Count == 1)
            {
                return CreatePoint(positions[0]);
            }
            if (positions.Count == 2)
            {
                return CreateLineSegment(positions[0], positions[1]);
            }
            if (positions.Count == 3)
            {
                return CreateTriangle(positions[0], positions[1], positions[2], primitiveType);
            }
            if (positions.Count == 4)
            {
                return CreateQuadrangle(positions[0], positions[1], positions[2], positions[3], primitiveType);
            }

            #endregion

            //需要先建立顶点到索引的映射
            //MIConvexHull 会重新排列顶点顺序，所以不能直接使用原始索引
            //方案：直接用 MIConvexHull 的结果，它返回的Points就是去重后的顶点列表
            DefaultVertex[] verticesInput = positions.Select(p => new DefaultVertex { Position = [p.X, p.Y, p.Z] }).ToArray();
            ConvexHullCreationResult<DefaultVertex, DefaultConvexFace<DefaultVertex>> convexHull = ConvexHull.Create(verticesInput);

            //共面或其他状况返回多边形
            if (convexHull == null || convexHull.Result == null)
            {
                return CreatePolyline(positions, true);
            }

            //提取凸包顶点（已经去重）
            Vector3[] hullVertices = convexHull.Result.Points.Select(v => new Vector3(
                (float)v.Position[0],
                (float)v.Position[1],
                (float)v.Position[2])).ToArray();

            //构建顶点到索引的映射（因为Points列表就是索引顺序）
            IDictionary<DefaultVertex, int> vertexToIndex = new Dictionary<DefaultVertex, int>();
            for (int i = 0; i < convexHull.Result.Points.Count(); i++)
            {
                vertexToIndex[convexHull.Result.Points.ElementAt(i)] = i;
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                //==========线框模式：提取所有边==========
                HashSet<(int, int)> edges = [];
                foreach (DefaultConvexFace<DefaultVertex> face in convexHull.Result.Faces)
                {
                    int i0 = vertexToIndex[face.Vertices[0]];
                    int i1 = vertexToIndex[face.Vertices[1]];
                    int i2 = vertexToIndex[face.Vertices[2]];

                    edges.Add(i0 < i1 ? (i0, i1) : (i1, i0));
                    edges.Add(i1 < i2 ? (i1, i2) : (i2, i1));
                    edges.Add(i2 < i0 ? (i2, i0) : (i0, i2));
                }

                //添加顶点
                foreach (Vector3 v in hullVertices)
                {
                    vertices.Add(new Vertex { Position = v, Normal = Vector3.Zero });
                }

                //添加边索引
                foreach ((int i1, int i2) in edges)
                {
                    indices.Add((uint)i1);
                    indices.Add((uint)i2);
                }
            }
            else
            {
                //==========填充模式：生成三角形==========
                foreach (DefaultConvexFace<DefaultVertex> face in convexHull.Result.Faces)
                {
                    int i0 = vertexToIndex[face.Vertices[0]];
                    int i1 = vertexToIndex[face.Vertices[1]];
                    int i2 = vertexToIndex[face.Vertices[2]];

                    Vector3 v0 = hullVertices[i0];
                    Vector3 v1 = hullVertices[i1];
                    Vector3 v2 = hullVertices[i2];
                    Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

                    vertices.Add(new Vertex { Position = v0, Normal = normal });
                    vertices.Add(new Vertex { Position = v1, Normal = normal });
                    vertices.Add(new Vertex { Position = v2, Normal = normal });

                    uint baseIdx = (uint)(vertices.Count - 3);
                    indices.Add(baseIdx);
                    indices.Add(baseIdx + 1);
                    indices.Add(baseIdx + 2);
                }
            }

            MeshGeometry meshGeometry = new(vertices, indices);

            if (primitiveType != GraphicPrimitiveType.Lines)
            {
                CalculateNormals(meshGeometry);
            }

            return meshGeometry;
        }
        #endregion

        #region # 创建网格线 —— static MeshGeometry CreateGridLines(float size = 10.0f, int divisions = 10...
        /// <summary>
        /// 创建网格线
        /// </summary>
        /// <param name="size">尺寸</param>
        /// <param name="divisions">分隔数量</param>
        /// <param name="normal">法向量（控制网格朝向）</param>
        /// <param name="primitiveType">图元类型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateGridLines(float size = 10.0f, int divisions = 10, Vector3 normal = default, GraphicPrimitiveType primitiveType = GraphicPrimitiveType.Lines)
        {
            //默认朝上(Z轴)
            if (normal == default)
            {
                normal = Vector3.UnitZ;
            }

            float halfSize = size * 0.5f;
            float step = size / divisions;

            //计算旋转矩阵，将Y轴旋转到指定的法向量方向
            Matrix4 rotationMatrix;
            if (Vector3.Dot(normal, Vector3.UnitY) > 0.999f)
            {
                rotationMatrix = Matrix4.Identity;
            }
            else if (Vector3.Dot(normal, Vector3.UnitY) < -0.999f)
            {
                rotationMatrix = Matrix4.CreateRotationX(MathF.PI);
            }
            else
            {
                Vector3 rotationAxis = Vector3.Cross(Vector3.UnitY, normal);
                rotationAxis = Vector3.Normalize(rotationAxis);
                float angle = (float)Math.Acos(Vector3.Dot(Vector3.UnitY, normal));
                rotationMatrix = Matrix4.CreateFromAxisAngle(rotationAxis, angle);
            }

            List<Vertex> vertices = [];
            List<uint> indices = [];

            //线框模式
            if (primitiveType == GraphicPrimitiveType.Lines)
            {
                for (int index = 0; index <= divisions; index++)
                {
                    float pos = -halfSize + index * step;

                    //竖线
                    Vector3 startPointV = Vector3.TransformPosition(new Vector3(pos, 0, -halfSize), rotationMatrix);
                    Vector3 endPointV = Vector3.TransformPosition(new Vector3(pos, 0, halfSize), rotationMatrix);
                    vertices.Add(new Vertex
                    {
                        Position = startPointV
                    });
                    vertices.Add(new Vertex
                    {
                        Position = endPointV
                    });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));

                    //横线
                    Vector3 startPointH = Vector3.TransformPosition(new Vector3(-halfSize, 0, pos), rotationMatrix);
                    Vector3 endPointH = Vector3.TransformPosition(new Vector3(halfSize, 0, pos), rotationMatrix);
                    vertices.Add(new Vertex
                    {
                        Position = startPointH
                    });
                    vertices.Add(new Vertex
                    {
                        Position = endPointH
                    });
                    indices.Add((uint)(vertices.Count - 2));
                    indices.Add((uint)(vertices.Count - 1));
                }
            }
            //填充模式
            else
            {
                //生成三角形网格，每个小格子分成两个三角形
                for (int i = 0; i < divisions; i++)
                {
                    for (int j = 0; j < divisions; j++)
                    {
                        float x1 = -halfSize + i * step;
                        float x2 = x1 + step;
                        float z1 = -halfSize + j * step;
                        float z2 = z1 + step;

                        //四个角点
                        Vector3 p1 = new Vector3(x1, 0, z1);
                        Vector3 p2 = new Vector3(x2, 0, z1);
                        Vector3 p3 = new Vector3(x2, 0, z2);
                        Vector3 p4 = new Vector3(x1, 0, z2);

                        //变换到目标朝向
                        p1 = Vector3.TransformPosition(p1, rotationMatrix);
                        p2 = Vector3.TransformPosition(p2, rotationMatrix);
                        p3 = Vector3.TransformPosition(p3, rotationMatrix);
                        p4 = Vector3.TransformPosition(p4, rotationMatrix);

                        //添加顶点（带纹理坐标）
                        int baseIndex = vertices.Count;
                        vertices.Add(new Vertex { Position = p1, TextureCoord = new Vector2(0, 0), Normal = normal });
                        vertices.Add(new Vertex { Position = p2, TextureCoord = new Vector2(1, 0), Normal = normal });
                        vertices.Add(new Vertex { Position = p3, TextureCoord = new Vector2(1, 1), Normal = normal });
                        vertices.Add(new Vertex { Position = p4, TextureCoord = new Vector2(0, 1), Normal = normal });

                        //两个三角形组成一个格子
                        indices.Add((uint)(baseIndex));
                        indices.Add((uint)(baseIndex + 1));
                        indices.Add((uint)(baseIndex + 2));
                        indices.Add((uint)(baseIndex));
                        indices.Add((uint)(baseIndex + 2));
                        indices.Add((uint)(baseIndex + 3));
                    }
                }
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建2D容器 —— static MeshGeometry CreateContainer2D(float width, float height, Vector3 normal)
        /// <summary>
        /// 创建2D容器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="normal">法向量（决定平面朝向）</param>
        /// <returns>网格模型</returns>
        /// <remarks>
        /// 纹理坐标左下角为(0,0)，右上角为(1,1)，平面中心在原点
        /// 支持任意法向量方向的平面创建
        /// </remarks>
        public static MeshGeometry CreateContainer2D(float width, float height, Vector3 normal)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            //根据法向量计算局部坐标系
            Vector3 right, up;

            //法向量平行于Z轴
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.999f)
            {
                right = Vector3.UnitX;
                up = -Vector3.UnitY;
            }
            //法向量平行于Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.999f)
            {
                right = Vector3.UnitX;
                up = Vector3.UnitZ;
            }
            //法向量平行于X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.999f)
            {
                right = Vector3.UnitY;
                up = Vector3.UnitZ;
            }
            //一般情况：使用叉积构建正交基
            else
            {
                right = Vector3.Normalize(Vector3.Cross(normal, Vector3.UnitZ));
                up = Vector3.Normalize(Vector3.Cross(right, normal));
            }

            //四个顶点（相对于中心点）
            Vector3[] positions =
            [
                -right * halfW - up * halfH, // 0: 左下
                 right * halfW - up * halfH, // 1: 右下
                 right * halfW + up * halfH, // 2: 右上
                -right * halfW + up * halfH  // 3: 左上
            ];

            //纹理坐标
            Vector2[] texCoords =
            [
                new Vector2(0, 1),  // 左下 -> 纹理左上
                new Vector2(1, 1),  // 右下 -> 纹理右上
                new Vector2(1, 0),  // 右上 -> 纹理右下
                new Vector2(0, 0)   // 左上 -> 纹理左下
            ];

            //构建顶点列表
            List<Vertex> vertices = [];
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(new Vertex
                {
                    Position = positions[i],
                    TextureCoord = texCoords[i],
                    Normal = normal
                });
            }

            //构建索引（两个三角形）
            List<uint> indices = [0, 1, 2, 0, 2, 3];

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建Overlay容器 —— static MeshGeometry CreateOverlayContainer(float width, float height)
        /// <summary>
        /// 创建Overlay容器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>网格模型</returns>
        /// <remarks>
        /// 用于2D屏幕空间渲染
        /// 纹理坐标左下角为(0,0)，右上角为(1,1)
        /// 平面以左上角为基准点（原点），便于屏幕空间定位
        /// </remarks>
        public static MeshGeometry CreateOverlayContainer(float width, float height)
        {
            Vector3[] positions =
            [
                new Vector3(0, 0, 0),           //0: 左上
                new Vector3(width, 0, 0),       //1: 右上
                new Vector3(width, height, 0),  //2: 右下
                new Vector3(0, height, 0)       //3: 左下
            ];

            Vector2[] texCoords =
            [
                new Vector2(0, 0),  //左上 -> 纹理左上（Y=0）
                new Vector2(1, 0),  //右上 -> 纹理右上（Y=0）
                new Vector2(1, 1),  //右下 -> 纹理右下（Y=1）
                new Vector2(0, 1)   //左下 -> 纹理左下（Y=1）
            ];

            //法向量朝Z（面向相机）
            Vector3 normal = Vector3.UnitZ;

            //构建顶点列表
            List<Vertex> vertices = [];
            for (int index = 0; index < 4; index++)
            {
                vertices.Add(new Vertex
                {
                    Position = positions[index],
                    TextureCoord = texCoords[index],
                    Normal = normal
                });
            }

            //构建索引（两个三角形）
            List<uint> indices = [0, 1, 2, 0, 2, 3];

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建ViewBox —— static MeshGeometry CreateViewBox(float width = 1.0f, float height = 1.0f...
        /// <summary>
        /// 创建ViewBox
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="center">中心点位置</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateViewBox(float width = 1.0f, float height = 1.0f, float depth = 1.0f, Vector3 center = default)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float halfD = depth * 0.5f;

            //8个顶点（相对于中心点）
            Vector3[] vertices =
            [
                new Vector3(center.X - halfW, center.Y + halfD, center.Z - halfH), //0: 左前下
                new Vector3(center.X + halfW, center.Y + halfD, center.Z - halfH), //1: 右前下
                new Vector3(center.X + halfW, center.Y + halfD, center.Z + halfH), //2: 右前上
                new Vector3(center.X - halfW, center.Y + halfD, center.Z + halfH), //3: 左前上
                new Vector3(center.X - halfW, center.Y - halfD, center.Z - halfH), //4: 左后下
                new Vector3(center.X - halfW, center.Y - halfD, center.Z + halfH), //5: 左后上
                new Vector3(center.X + halfW, center.Y - halfD, center.Z + halfH), //6: 右后上
                new Vector3(center.X + halfW, center.Y - halfD, center.Z - halfH)  //7: 右后下
            ];

            //每个面的顶点索引（每个面4个唯一顶点）
            uint[][] faceVertices = new uint[][]
            {
                [0, 1, 2, 3],   //前（Y正方向）
                [4, 5, 6, 7],   //后（Y负方向）
                [3, 2, 6, 5],   //上（Z正方向）
                [0, 4, 7, 1],   //下（Z负方向）
                [0, 3, 5, 4],   //左（X负方向）
                [1, 7, 6, 2]    //右（X正方向）
            };

            //每个面的法向量
            Vector3[] normals =
            [
                new(0, 1, 0),   //前（Y正）
                new(0, -1, 0),  //后（Y负）
                new(0, 0, 1),   //上（Z正）
                new(0, 0, -1),  //下（Z负）
                new(-1, 0, 0),  //左（X负）
                new(1, 0, 0)    //右（X正）
            ];

            //每个面4个顶点的纹理坐标（左下角为(0,0)，右上角为(1,1)）
            Vector2[][] faceTexCoords = new Vector2[][]
            {
                //前（Y正）
                [
                    new Vector2(0, 0), // 左下
                    new Vector2(1, 0), // 右下
                    new Vector2(1, 1), // 右上
                    new Vector2(0, 1) // 左上
                ],
                //后（Y负）
                [
                    new Vector2(1, 0), //右下
                    new Vector2(0, 0), //左下
                    new Vector2(0, 1), //左上
                    new Vector2(1, 1)  //右上
                ],
                //上（Z正）
                [
                    new Vector2(0, 1), //左上
                    new Vector2(1, 1), //右上
                    new Vector2(1, 0), //右下
                    new Vector2(0, 0)  //左下
                ],
                //下（Z负）
                [
                    new Vector2(0, 0), //左下
                    new Vector2(1, 0), //右下
                    new Vector2(1, 1), //右上
                    new Vector2(0, 1)  //左上
                ],
                //左（X负）
                [
                    new Vector2(1, 0), //右下
                    new Vector2(0, 0), //左下
                    new Vector2(0, 1), //左上
                    new Vector2(1, 1)  //右上
                ],
                //右（X正）
                [
                    new Vector2(0, 0), //左下
                    new Vector2(1, 0), //右下
                    new Vector2(1, 1), //右上
                    new Vector2(0, 1)  //左上
                ]
            };

            List<Vertex> finalVertices = [];
            List<uint> finalIndices = [];

            for (int face = 0; face < 6; face++)
            {
                uint startIndex = (uint)finalVertices.Count;

                //添加4个顶点
                for (int i = 0; i < 4; i++)
                {
                    finalVertices.Add(new Vertex
                    {
                        Position = vertices[faceVertices[face][i]],
                        TextureCoord = faceTexCoords[face][i],
                        Normal = normals[face]
                    });
                }

                //两个三角形：0-1-2 和 0-2-3
                finalIndices.Add(startIndex);
                finalIndices.Add(startIndex + 1);
                finalIndices.Add(startIndex + 2);

                finalIndices.Add(startIndex);
                finalIndices.Add(startIndex + 2);
                finalIndices.Add(startIndex + 3);
            }

            return new MeshGeometry(finalVertices, finalIndices);
        }
        #endregion

        #region # 创建平面 —— static MeshGeometry CreatePlane(float width = 1.0f, float height = 1.0f...
        /// <summary>
        /// 创建平面
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="widthSegments">宽度细分数量</param>
        /// <param name="heightSegments">高度细分数量</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreatePlane(float width = 1.0f, float height = 1.0f, int widthSegments = 1, int heightSegments = 1)
        {
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

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 创建线框 —— static MeshGeometry CreateWireframe(MeshGeometry meshGeometry...
        /// <summary>
        /// 创建线框
        /// </summary>
        /// <param name="meshGeometry">网格模型</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry CreateWireframe(MeshGeometry meshGeometry)
        {
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
                            TextureCoord = v1.TextureCoord,
                            Normal = v1.Normal
                        });
                        vertices.Add(new Vertex
                        {
                            Position = v2.Position,
                            TextureCoord = v2.TextureCoord,
                            Normal = v2.Normal
                        });

                        uint baseIdx = (uint)(vertices.Count - 2);
                        indices.Add(baseIdx);
                        indices.Add(baseIdx + 1);
                    }
                }
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 计算法向量 —— static void CalculateNormals(MeshGeometry meshGeometry)
        /// <summary>
        /// 计算法向量
        /// </summary>
        /// <param name="meshGeometry">网格模型</param>
        public static void CalculateNormals(MeshGeometry meshGeometry)
        {
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
        #endregion

        #region # 合并网格模型 —— static MeshGeometry Combine(MeshGeometry meshGeometry1, MeshGeometry meshGeometry2)
        /// <summary>
        /// 合并网格模型
        /// </summary>
        /// <param name="meshGeometry1">网格模型1</param>
        /// <param name="meshGeometry2">网格模型2</param>
        /// <returns>网格模型</returns>
        public static MeshGeometry Combine(MeshGeometry meshGeometry1, MeshGeometry meshGeometry2)
        {
            List<Vertex> vertices = new(meshGeometry1.Vertices);
            List<uint> indices = new(meshGeometry1.Indices);

            uint vertexOffset = (uint)vertices.Count;
            vertices.AddRange(meshGeometry2.Vertices);

            foreach (uint index in meshGeometry2.Indices)
            {
                indices.Add(index + vertexOffset);
            }

            return new MeshGeometry(vertices, indices);
        }
        #endregion

        #region # 变换网格模型 —— static void Transform(MeshGeometry meshGeometry, Matrix4 transform)
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
        #endregion
    }
}
