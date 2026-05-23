using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 统计算法
    /// </summary>
    public static class StatisticAlgorithms
    {
        #region # 适用立方体统计 —— static StatisticResultEx ApplyBoxAnalyse(this VolumeRenderable renderable...
        /// <summary>
        /// 适用立方体统计
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="boxLocalMin">立方体局部最小点</param>
        /// <param name="boxLocalMax">立方体局部最大点</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（-1=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyBoxAnalyse(this VolumeRenderable renderable, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, int markValue = -1)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //立方体统计计算着色器
            ShaderProgram statisticsComputer = ComputerManager.BoxStatisticsComputer;

            //开启Shader程序
            statisticsComputer.Use();

            //绑定纹理
            renderable.PreviewTexture.BindImageTexture(0, TextureAccess.ReadOnly);
            renderable.MarkTexture.BindImageTexture(1, TextureAccess.ReadOnly);

            //创建并绑定SSBO
            using ShaderStorageBuffer statisticsBuffer = new ShaderStorageBuffer(sizeof(StatisticResultEx), BufferUsageHint.DynamicRead);
            statisticsBuffer.Bind(2);
            statisticsBuffer.Clear();

            //设置立方体参数
            statisticsComputer.SetUniformVector3("u_BoxLocalMin", boxLocalMin);
            statisticsComputer.SetUniformVector3("u_BoxLocalMax", boxLocalMax);
            statisticsComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            statisticsComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            statisticsComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置标记值（-1=全部，0~255=指定标记值）
            statisticsComputer.SetUniformInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //读取结果
            StatisticResultEx gpuResult = statisticsBuffer.Read<StatisticResultEx>();

            //取消使用
            statisticsComputer.Unuse();

            //CPU端补充计算
            float voxelVolume = renderable.VolumeData.Metadata.VoxelVolume;
            float voxelArea = renderable.VolumeData.Metadata.AverageVoxelArea;
            StatisticResult result = gpuResult.ToResult();
            result.CalculateGeometry(voxelVolume, voxelArea);

            return result;
        }
        #endregion
    }
}
