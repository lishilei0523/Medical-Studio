namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// Used in GL.Arb.DrawArraysInstanced, GL.Arb.DrawElementsInstanced and 32 other functions
    /// </summary>
    public enum GraphicPrimitiveType : byte
    {
        /// <summary>
        /// [requires: v1.0] Original was GL_POINTS = 0x0000
        /// </summary>
        Points = 0,

        /// <summary>
        /// [requires: v1.0] Original was GL_LINES = 0x0001
        /// </summary>
        Lines = 1,

        /// <summary>
        /// [requires: v1.0] Original was GL_LINE_LOOP = 0x0002
        /// </summary>
        LineLoop = 2,

        /// <summary>
        /// [requires: v1.0] Original was GL_LINE_STRIP = 0x0003
        /// </summary>
        LineStrip = 3,

        /// <summary>
        /// [requires: v1.0 or ARB_tessellation_shader] Original was GL_TRIANGLES = 0x0004
        /// </summary>
        Triangles = 4,

        /// <summary>
        /// [requires: v1.0] Original was GL_TRIANGLE_STRIP = 0x0005
        /// </summary>
        TriangleStrip = 5,

        /// <summary>
        /// [requires: v1.0] Original was GL_TRIANGLE_FAN = 0x0006
        /// </summary>
        TriangleFan = 6,

        /// <summary>
        /// [requires: v4.0] Original was GL_QUADS = 0x0007
        /// </summary>
        Quads = 7,

        /// <summary>
        /// Original was GL_QUADS_EXT = 0x0007
        /// </summary>
        QuadsExt = 7,

        /// <summary>
        /// [requires: v3.2] Original was GL_LINES_ADJACENCY = 0x000A
        /// </summary>
        LinesAdjacency = 10,

        /// <summary>
        /// [requires: ARB_geometry_shader4] Original was GL_LINES_ADJACENCY_ARB = 0x000A
        /// </summary>
        LinesAdjacencyArb = 10,

        /// <summary>
        /// Original was GL_LINES_ADJACENCY_EXT = 0x000A
        /// </summary>
        LinesAdjacencyExt = 10,

        /// <summary>
        /// [requires: v3.2] Original was GL_LINE_STRIP_ADJACENCY = 0x000B
        /// </summary>
        LineStripAdjacency = 11,

        /// <summary>
        /// [requires: ARB_geometry_shader4] Original was GL_LINE_STRIP_ADJACENCY_ARB = 0x000B
        /// </summary>
        LineStripAdjacencyArb = 11,

        /// <summary>
        /// Original was GL_LINE_STRIP_ADJACENCY_EXT = 0x000B
        /// </summary>
        LineStripAdjacencyExt = 11,

        /// <summary>
        /// [requires: v3.2] Original was GL_TRIANGLES_ADJACENCY = 0x000C
        /// </summary>
        TrianglesAdjacency = 12,

        /// <summary>
        /// [requires: ARB_geometry_shader4] Original was GL_TRIANGLES_ADJACENCY_ARB = 0x000C
        /// </summary>
        TrianglesAdjacencyArb = 12,

        /// <summary>
        /// Original was GL_TRIANGLES_ADJACENCY_EXT = 0x000C
        /// </summary>
        TrianglesAdjacencyExt = 12,

        /// <summary>
        /// [requires: v3.2] Original was GL_TRIANGLE_STRIP_ADJACENCY = 0x000D
        /// </summary>
        TriangleStripAdjacency = 13,

        /// <summary>
        /// [requires: ARB_geometry_shader4] Original was GL_TRIANGLE_STRIP_ADJACENCY_ARB = 0x000D
        /// </summary>
        TriangleStripAdjacencyArb = 13,

        /// <summary>
        /// Original was GL_TRIANGLE_STRIP_ADJACENCY_EXT = 0x000D
        /// </summary>
        TriangleStripAdjacencyExt = 13,

        /// <summary>
        /// [requires: v4.0 or ARB_tessellation_shader, NV_gpu_shader5] Original was GL_PATCHES = 0x000E
        /// </summary>
        Patches = 14,

        /// <summary>
        /// Original was GL_PATCHES_EXT = 0x000E
        /// </summary>
        PatchesExt = 14
    }
}
