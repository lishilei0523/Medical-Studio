using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 体积渲染对象
    /// </summary>
    public class VolumeRenderable : IDisposable
    {

        /// <summary>
        /// 单位立方体
        /// </summary>
        private readonly VertexBuffer _unitCube;

        public VolumeRenderable()
        {
            this._unitCube = CreateUnitCube();
        }


        #region 3D纹理 —— Texture3D Texture3D
        /// <summary>
        /// 3D纹理
        /// </summary>
        public Texture3D Texture3D { get; private set; }
        #endregion

        #region 窗宽窗位 —— WindowLevel WindowLevel
        /// <summary>
        /// 窗宽窗位
        /// </summary>
        public WindowLevel WindowLevel { get; private set; }
        #endregion

        #region 传输函数 —— TransferFunction TransferFunction
        /// <summary>
        /// 传输函数
        /// </summary>
        public TransferFunction TransferFunction { get; private set; }
        #endregion



        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._unitCube.Dispose();
        }
        #endregion

        #region 创建单位立方体 —— static VertexBuffer CreateUnitCube()
        /// <summary>
        /// 创建单位立方体
        /// </summary>
        private static VertexBuffer CreateUnitCube()
        {
            Vertex[] vertices =
            [
                new Vertex { Position =new Vector3( -0.5f, -0.5f,  0.5f)},
                new Vertex { Position =new Vector3( 0.5f, -0.5f,   0.5f)},
                new Vertex { Position =new Vector3( 0.5f,  0.5f,   0.5f)},
                new Vertex { Position =new Vector3( -0.5f,  0.5f,  0.5f)},
                new Vertex { Position =new Vector3( -0.5f, -0.5f, -0.5f)},
                new Vertex { Position =new Vector3( 0.5f, -0.5f,  -0.5f)},
                new Vertex { Position =new Vector3( 0.5f,  0.5f,  -0.5f)},
                new Vertex { Position =new Vector3( -0.5f,  0.5f, -0.5f)}
            ];
            uint[] indices =
            [
                0,1,2, 2,3,0, 1,5,6, 6,2,1,
                5,4,7, 7,6,5, 4,0,3, 3,7,4,
                3,2,6, 6,7,3, 4,5,1, 1,0,4
            ];

            MeshGeometry geometry = new MeshGeometry(vertices, indices);
            VertexBuffer vertexBuffer = new VertexBuffer(geometry);

            return vertexBuffer;
        }
        #endregion
    }
}
