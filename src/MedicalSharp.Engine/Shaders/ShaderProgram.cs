using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace MedicalSharp.Engine.Shaders
{
    /// <summary>
    /// Shader程序
    /// </summary>
    public class ShaderProgram : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建Shader程序构造器
        /// </summary>
        public ShaderProgram()
        {
            this.Id = GL.CreateProgram();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException("创建Shader程序失败！");
            }

            #endregion
        }

        #endregion

        #region # 属性

        #region Shader程序Id —— int Id
        /// <summary>
        /// Shader程序Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 顶点Shader源文本 —— string VertexShaderSource
        /// <summary>
        /// 顶点Shader源文本
        /// </summary>
        public string VertexShaderSource { get; private set; }
        #endregion

        #region 片元Shader源文本 —— string FragmentShaderSource
        /// <summary>
        /// 片元Shader源文本
        /// </summary>
        public string FragmentShaderSource { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置顶点Shader源文本 —— void SetVertexShaderSource(string sourceText)
        /// <summary>
        /// 设置顶点Shader源文本
        /// </summary>
        /// <param name="sourceText">Shader源文本</param>
        public void SetVertexShaderSource(string sourceText)
        {
            this.VertexShaderSource = sourceText.RemoveComments();
        }
        #endregion

        #region 设置片元Shader源文本 —— void SetFragmentShaderSource(string sourceText)
        /// <summary>
        /// 设置片元Shader源文本
        /// </summary>
        /// <param name="sourceText">Shader源文本</param>
        public void SetFragmentShaderSource(string sourceText)
        {
            this.FragmentShaderSource = sourceText.RemoveComments();
        }
        #endregion

        #region 读取顶点Shader文件 —— void ReadVertexShaderFromFile(string filePath)
        /// <summary>
        /// 读取顶点Shader文件
        /// </summary>
        /// <param name="filePath">Shader文件路径</param>
        public void ReadVertexShaderFromFile(string filePath)
        {
            this.VertexShaderSource = File.ReadAllText(filePath).RemoveComments();
        }
        #endregion

        #region 读取片元Shader文件 —— void ReadFragmentShaderFromFile(string filePath)
        /// <summary>
        /// 读取片元Shader文件
        /// </summary>
        /// <param name="filePath">Shader文件路径</param>
        public void ReadFragmentShaderFromFile(string filePath)
        {
            this.FragmentShaderSource = File.ReadAllText(filePath).RemoveComments();
        }
        #endregion

        #region 构建程序 —— void Build()
        /// <summary>
        /// 构建程序
        /// </summary>
        public void Build()
        {
            int vertexShaderId = ShaderManager.CompileShader(this.VertexShaderSource, ShaderType.VertexShader);
            int fragmentShaderId = ShaderManager.CompileShader(this.FragmentShaderSource, ShaderType.FragmentShader);

            //链接Shader程序 
            GL.AttachShader(this.Id, vertexShaderId);
            GL.AttachShader(this.Id, fragmentShaderId);
            GL.LinkProgram(this.Id);

            GL.GetProgram(this.Id, GetProgramParameterName.LinkStatus, out int success);
            if (success <= 0)
            {
                GL.GetShaderInfoLog(this.Id, out string logInfo);
                throw new RuntimeBinderInternalCompilerException(logInfo);
            }

            //编译完成后清理Shader
            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
        }
        #endregion

        #region 使用程序 —— void Use()
        /// <summary>
        /// 使用程序
        /// </summary>
        public void Use()
        {
            GL.UseProgram(this.Id);
        }
        #endregion

        #region 取消使用程序 —— void Unuse()
        /// <summary>
        /// 取消使用程序
        /// </summary>
        public void Unuse()
        {
            GL.UseProgram(0);
        }
        #endregion

        #region 设置Uniform布尔值 —— void SetUniformBoolean(string key, bool value)
        /// <summary>
        /// 设置Uniform布尔值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformBoolean(string key, bool value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform1(uniformId, Convert.ToByte(value));
        }
        #endregion

        #region 设置Uniform数值 —— void SetUniformInt(string key, int value)
        /// <summary>
        /// 设置Uniform数值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformInt(string key, int value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform1(uniformId, value);
        }
        #endregion

        #region 设置Uniform数值 —— void SetUniformFloat(string key, float value)
        /// <summary>
        /// 设置Uniform数值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformFloat(string key, float value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform1(uniformId, value);
        }
        #endregion

        #region 设置Uniform数值 —— void SetUniformDouble(string key, double value)
        /// <summary>
        /// 设置Uniform数值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformDouble(string key, double value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform1(uniformId, value);
        }
        #endregion

        #region 设置Uniform二维向量 —— void SetUniformVector2(string key, in Vector2 value)
        /// <summary>
        /// 设置Uniform二维向量
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformVector2(string key, in Vector2 value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform2(uniformId, value);
        }
        #endregion

        #region 设置Uniform三维向量 —— void SetUniformVector3(string key, in Vector3 value)
        /// <summary>
        /// 设置Uniform三维向量
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformVector3(string key, in Vector3 value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform3(uniformId, value);
        }
        #endregion

        #region 设置Uniform四维向量 —— void SetUniformVector4(string key, in Vector4 value)
        /// <summary>
        /// 设置Uniform四维向量
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformVector4(string key, in Vector4 value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.Uniform4(uniformId, value);
        }
        #endregion

        #region 设置Uniform矩阵 —— void SetUniformMatrix4(string key, Matrix4 value)
        /// <summary>
        /// 设置Uniform矩阵
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetUniformMatrix4(string key, Matrix4 value)
        {
            int uniformId = GL.GetUniformLocation(this.Id, key);
            GL.UniformMatrix4(uniformId, false, ref value);
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            GL.DeleteProgram(this.Id);
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
