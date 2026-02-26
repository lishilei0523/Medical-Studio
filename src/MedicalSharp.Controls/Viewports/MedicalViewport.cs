using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using MedicalSharp.Controls.Base;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 医学影像视口
    /// </summary>
    public unsafe class MedicalViewport : OpenTKViewport
    {
        private int _vertexShader;
        private int _fragmentShader;
        private int _shaderProgram;
        private int _vertexBufferObject;
        private int _indexBufferObject;
        private int _vertexArrayObject;
        private readonly Vertex[] _points;
        private readonly ushort[] _indices;

        public MedicalViewport()
        {
            this._points =
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
            this._indices =
            [
                0,1,2, 2,3,0, 1,5,6, 6,2,1,
                5,4,7, 7,6,5, 4,0,3, 3,7,4,
                3,2,6, 6,7,3, 4,5,1, 1,0,4
            ];

            this.PointerPressed += this.OnPointerPressed;
            this.PointerWheelChanged += this.OnPointerWheel;
            this.KeyDown += this.OnKeyDown;
            this.KeyUp += this.OnKeyUp;
        }

        protected override void OnOpenGlInit(GlInterface glInterface)
        {
            base.OnOpenGlInit(glInterface);

            //Shader部分
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Shaders/GLSLs/wireframe.vert");
            program.ReadFragmentShaderFromFile("Shaders/GLSLs/wireframe.frag");
            program.Build();
            this._shaderProgram = program.Id;

            //数据部分
            this._vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this._vertexArrayObject);

            this._vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, this._points.Length * sizeof(Vertex), this._points, BufferUsageHint.StaticDraw);

            this._indexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._indexBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this._indices.Length * sizeof(ushort), this._indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度测试
            GL.Enable(EnableCap.DepthTest);

            //禁用面剔除
            GL.Disable(EnableCap.CullFace);

            //渲染
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._indexBufferObject);
            GL.BindVertexArray(this._vertexArrayObject);
            GL.UseProgram(this._shaderProgram);

            Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 10.0f);
            Vector3 lookDirection = new Vector3(0.0f, 0.0f, -10.0f);
            Vector3 targetPosition = cameraPosition + lookDirection;
            Vector3 upDirection = new Vector3(0.0f, 1.0f, 0.0f);
            float fieldOfView = MathHelper.DegreesToRadians(30.0f);
            float aspect = viewportSize.Width * 1.0f / viewportSize.Height;  //窗口宽高比，应为动态设置
            float nearPlaneDistance = 0.125f;
            float farPlaneDistance = ushort.MaxValue;
            Matrix4 viewMatrix = Matrix4.LookAt(cameraPosition, targetPosition, upDirection);
            Matrix4 projectMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, aspect, nearPlaneDistance, farPlaneDistance);
            Matrix4 rx = Matrix4.CreateRotationX(45);
            Matrix4 ry = Matrix4.CreateRotationY(45);
            Matrix4 rz = Matrix4.CreateRotationZ(45);
            Matrix4 modelMatrix = Matrix4.Identity;
            modelMatrix = modelMatrix * rx;

            int modelLoc = GL.GetUniformLocation(this._shaderProgram, "u_ModelMatrix");
            int viewLoc = GL.GetUniformLocation(this._shaderProgram, "u_ViewMatrix");
            int projectionLoc = GL.GetUniformLocation(this._shaderProgram, "u_ProjectionMatrix");
            GL.UniformMatrix4(modelLoc, false, ref modelMatrix);
            GL.UniformMatrix4(viewLoc, false, ref viewMatrix);
            GL.UniformMatrix4(projectionLoc, false, ref projectMatrix);

            GL.DrawElements(PrimitiveType.Triangles, this._indices.Length, DrawElementsType.UnsignedShort, 0);
        }


        protected override void OnOpenGlDeinit(GlInterface _)
        {
            //Unbind everything
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            //Delete all resources.
            GL.DeleteBuffer(this._vertexBufferObject);
            GL.DeleteBuffer(this._indexBufferObject);
            GL.DeleteVertexArray(this._vertexArrayObject);
            GL.DeleteProgram(this._shaderProgram);
            GL.DeleteShader(this._fragmentShader);
            GL.DeleteShader(this._vertexShader);
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs eventArgs)
        {
            Point position = eventArgs.GetPosition(this);
            Trace.WriteLine(position);
        }

        private void OnPointerWheel(object sender, PointerWheelEventArgs eventArgs)
        {
            Trace.WriteLine($"Delta X: {eventArgs.Delta.X}");
            Trace.WriteLine($"Delta Y: {eventArgs.Delta.Y}");
        }

        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            Trace.WriteLine($"Key down: {eventArgs.Key}");
        }

        private void OnKeyUp(object sender, KeyEventArgs eventArgs)
        {
            Trace.WriteLine($"Key up: {eventArgs.Key}");
        }
    }
}
