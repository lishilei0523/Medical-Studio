/// <summary>
/// 向量加法
/// </summary>
/// <param name="vectorA">向量A</param>
/// <param name="vectorB">向量B</param>
/// <param name="result">结果向量</param>
/// <param name="count">向量数量</param>
__kernel void vector_add(__global const float4* vectorA, __global const float4* vectorB, __global float4* result, const int count)
{
	int index = get_global_id(0);
	if (index >= count)
	{
		return;
	}

	result[index] = vectorA[index] + vectorB[index];
}
