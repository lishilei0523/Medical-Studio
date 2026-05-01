using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 切割算法
    /// </summary>
    public static class CutAlgorithms
    {
        #region # 应用矩形切割 —— static void ApplyRectangleCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用矩形切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="center">中心位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public static void ApplyRectangleCut(this VolumeRenderable renderable, float width, float height, Vector3 center, Vector3 normal, Vector3 uAxis, Vector3 vAxis, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //矩形切割计算着色器
            ShaderProgram cutComputer = ComputerManager.RectangleCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置矩形参数
            cutComputer.SetUniformFloat("u_RectHalfWidth", width / 2.0f);
            cutComputer.SetUniformFloat("u_RectHalfHeight", height / 2.0f);
            cutComputer.SetUniformVector3("u_RectCenter", center);
            cutComputer.SetUniformVector3("u_RectNormal", normal);
            cutComputer.SetUniformVector3("u_RectUAxis", uAxis);
            cutComputer.SetUniformVector3("u_RectVAxis", vAxis);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用圆形切割 —— static void ApplyCircleCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用圆形切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="radius">半径</param>
        /// <param name="center">中心位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public static void ApplyCircleCut(this VolumeRenderable renderable, float radius, Vector3 center, Vector3 normal, Vector3 uAxis, Vector3 vAxis, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //圆形切割计算着色器
            ShaderProgram cutComputer = ComputerManager.CircleCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置圆形参数
            cutComputer.SetUniformFloat("u_CircleRadius", radius);
            cutComputer.SetUniformVector3("u_CircleCenter", center);
            cutComputer.SetUniformVector3("u_CircleNormal", normal);
            cutComputer.SetUniformVector3("u_CircleUAxis", uAxis);
            cutComputer.SetUniformVector3("u_CircleVAxis", vAxis);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用椭圆形切割 —— static void ApplyEllipseCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用椭圆形切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="width">宽度（X轴直径）</param>
        /// <param name="height">高度（Y轴直径）</param>
        /// <param name="center">中心位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public static void ApplyEllipseCut(this VolumeRenderable renderable, float width, float height, Vector3 center, Vector3 normal, Vector3 uAxis, Vector3 vAxis, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //椭圆形切割计算着色器
            ShaderProgram cutComputer = ComputerManager.EllipseCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置椭圆形参数
            cutComputer.SetUniformFloat("u_EllipseRadiusX", width / 2.0f);
            cutComputer.SetUniformFloat("u_EllipseRadiusY", height / 2.0f);
            cutComputer.SetUniformVector3("u_EllipseCenter", center);
            cutComputer.SetUniformVector3("u_EllipseNormal", normal);
            cutComputer.SetUniformVector3("u_EllipseUAxis", uAxis);
            cutComputer.SetUniformVector3("u_EllipseVAxis", vAxis);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用立方体切割 —— static void ApplyBoxCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用立方体切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public static void ApplyBoxCut(this VolumeRenderable renderable, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //立方体切割计算着色器
            ShaderProgram cutComputer = ComputerManager.BoxCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置立方体参数
            cutComputer.SetUniformVector3("u_BoxLocalMin", boxLocalMin);
            cutComputer.SetUniformVector3("u_BoxLocalMax", boxLocalMax);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用球体切割 —— static void ApplySphereCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用球体切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="radius">半径</param>
        /// <param name="center">中心位置（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public static void ApplySphereCut(this VolumeRenderable renderable, float radius, Vector3 center, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //球体切割计算着色器
            ShaderProgram cutComputer = ComputerManager.SphereCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置球体参数
            cutComputer.SetUniformFloat("u_SphereRadius", radius);
            cutComputer.SetUniformVector3("u_SphereCenter", center);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用圆柱体切割 —— static void ApplyCylinderCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用圆柱体切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="radius">半径</param>
        /// <param name="height">高度（沿Z轴）</param>
        /// <param name="center">中心位置（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public static void ApplyCylinderCut(this VolumeRenderable renderable, float radius, float height, Vector3 center, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //圆柱体切割计算着色器
            ShaderProgram cutComputer = ComputerManager.CylinderCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置圆柱体参数
            cutComputer.SetUniformFloat("u_CylinderRadius", radius);
            cutComputer.SetUniformFloat("u_CylinderHeight", height);
            cutComputer.SetUniformVector3("u_CylinderCenter", center);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region # 应用凸多面体切割 —— static void ApplyConvexPolyhedronCut(this VolumeRenderable renderable...
        /// <summary>
        /// 应用凸多面体切割
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="planes">平面方程列表（法向量指向外部）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public static unsafe void ApplyConvexPolyhedronCut(this VolumeRenderable renderable, Vector4[] planes, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            #region # 验证

            if (planes == null || planes.Length == 0)
            {
                throw new ArgumentException("平面方程列表不可为空！");
            }

            #endregion

            Matrix4 worldToLocal = localToWorld.Inverted();

            //凸多面体切割计算着色器
            ShaderProgram cutComputer = ComputerManager.ConvexPolyhedronCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            renderable.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //构建SSBO数据
            int bufferSize = sizeof(Vector4) * planes.Length;
            using ShaderStorageBuffer planesBuffer = new ShaderStorageBuffer(bufferSize, BufferUsageHint.DynamicDraw);
            planesBuffer.UpdateRange(planes);

            //设置凸多面体参数
            planesBuffer.Bind(1);
            cutComputer.SetUniformInt("u_PlaneCount", planes.Length);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", renderable.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", renderable.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(renderable.VolumeData.Metadata.VolumeSize);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion
    }
}
