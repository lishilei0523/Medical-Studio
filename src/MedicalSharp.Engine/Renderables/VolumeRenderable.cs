using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 体积渲染对象
    /// </summary>
    public class VolumeRenderable : Renderable, IDisposable
    {
        /// <summary>
        /// 单位立方体
        /// </summary>
        private readonly VertexBuffer _unitCube;

        /// <summary>
        /// 
        /// </summary>
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

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext context)
        {

        }
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


        //Private

        #region 创建单位立方体 —— static VertexBuffer CreateUnitCube()
        /// <summary>
        /// 创建单位立方体
        /// </summary>
        private static VertexBuffer CreateUnitCube()
        {
            Vertex[] vertices =
            [
                new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, -0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, -0.5f) }
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


        /// <summary>
        /// 采样设置
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SamplingOptions
        {
            /// <summary>
            /// 步长
            /// </summary>
            public int StepSize;

            /// <summary>
            /// 最大步数
            /// </summary>
            public int MaxStepsCount;

            /// <summary>
            /// 透明度阈值
            /// </summary>
            public float OpacityThreshold;
        }

        /// <summary>
        /// 材质设置
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialOptions
        {
            /// <summary>
            /// 亮度
            /// </summary>
            public float Brightness;

            /// <summary>
            /// 密度缩放
            /// </summary>
            public float DensityScale;
        }
    }
}
