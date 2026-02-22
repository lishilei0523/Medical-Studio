using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 体积渲染器
    /// </summary>
    public class VolumeRenderer : IDisposable
    {
        /// <summary>
        /// 单位立方体
        /// </summary>
        private readonly VertexBuffer _unitCube;

        /// <summary>
        /// 创建体积渲染器构造器
        /// </summary>
        public VolumeRenderer()
        {
            this._unitCube = CreateUnitCube();
        }


        #region 亮度 —— float Brightness
        /// <summary>
        /// 亮度
        /// </summary>
        public float Brightness { get; private set; }
        #endregion

        #region 密度缩放 —— float DensityScale
        /// <summary>
        /// 密度缩放
        /// </summary>
        public float DensityScale { get; private set; }
        #endregion

        #region 步长 —— int StepSize
        /// <summary>
        /// 步长
        /// </summary>
        public int StepSize { get; private set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        public int MaxStepsCount { get; private set; }
        #endregion

        #region 透明度阈值 —— float OpacityThreshold
        /// <summary>
        /// 透明度阈值
        /// </summary>
        public float OpacityThreshold { get; private set; }
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



        #region 设置材质选项 —— void SetMaterialOptions(float brightness, float densityScale)
        /// <summary>
        /// 设置材质选项
        /// </summary>
        /// <param name="brightness">亮度</param>
        /// <param name="densityScale">密度缩放</param>
        public void SetMaterialOptions(float brightness, float densityScale)
        {
            this.Brightness = brightness;
            this.DensityScale = densityScale;
        }
        #endregion

        #region 设置采样选项 —— void SetSamplingOptions(int stepSize, int maxStepsCount...
        /// <summary>
        /// 设置采样选项
        /// </summary>
        /// <param name="stepSize">步长</param>
        /// <param name="maxStepsCount">最大步数</param>
        /// <param name="opacityThreshold">透明度阈值</param>
        public void SetSamplingOptions(int stepSize, int maxStepsCount, float opacityThreshold)
        {
            this.StepSize = stepSize;
            this.MaxStepsCount = maxStepsCount;
            this.OpacityThreshold = opacityThreshold;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._unitCube.Dispose();
            this.TransferFunction.Dispose();
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
    }
}
