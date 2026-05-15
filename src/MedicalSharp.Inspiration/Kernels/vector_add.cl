//向量加法内核
__kernel void vector_add(__global float4* a, __global float4* b, __global float4* result)
{
    int gid = get_global_id(0);
    result[gid] = a[gid] + b[gid];
}
