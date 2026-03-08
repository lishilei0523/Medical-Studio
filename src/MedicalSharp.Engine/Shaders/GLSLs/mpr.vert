#version 330 core
layout(location = 0) in vec3 aPos;

out vec3 vWorldPos;
out vec3 vTexCoord;

uniform mat4 u_ProjectionMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ModelMatrix;

//MPR特定参数
uniform float u_SliceDepth;
uniform int u_PlaneType;  //0=axial, 1=coronal, 2=sagittal

void main() 
{
    //创建切片平面（根据平面类型调整位置）
    vec3 slicePosition = aPos;
    
    //轴向面
    if (u_PlaneType == 0)
    {        
        slicePosition.z = u_SliceDepth - 0.5;
    }
    //冠状面
    else if (u_PlaneType == 1)
    { 
        slicePosition.y = u_SliceDepth - 0.5;
    }
    //矢状面
    else if (u_PlaneType == 2)
    { 
        slicePosition.x = u_SliceDepth - 0.5;
    }
    
    vec4 worldPos = u_ModelMatrix * vec4(slicePosition, 1.0);
    vWorldPos = worldPos.xyz;
    
    //纹理坐标（从局部空间转换）
    vTexCoord = aPos + 0.5;
    
    gl_Position = u_ProjectionMatrix * u_ViewMatrix * worldPos;
}
