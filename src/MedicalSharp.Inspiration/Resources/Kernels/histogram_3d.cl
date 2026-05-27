/// <summary>
/// SNORM系数
/// </summary>
__constant float MAX_16BIT_SIGNED = 32767.0f;

/// <summary>
/// 3D灰度直方图统计（全局内存分块版）
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="globalHist">全局直方图缓冲区（groupsCount × bins个uint，每个工作组独占一块）</param>
/// <param name="bins">桶数量（HU 值范围，如 4096）</param>
/// <param name="minHU">最小HU</param>
/// <param name="maxHU">最大HU</param>
/// <remarks>每个工作组在全局内存中维护私有直方图，完成后由CPU做最终归约</remarks>
__kernel void histogram_3d(__read_only image3d_t input, __global uint* globalHist, const int bins, const float minHU, const float maxHU)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(input);
	int height = get_image_height(input);
	int depth = get_image_depth(input);

	//计算工作组ID和组内线程ID
	int groupId = get_group_id(0) * get_num_groups(1) * get_num_groups(2)
		+ get_group_id(1) * get_num_groups(2)
		+ get_group_id(2);
	int localId = get_local_id(0) * get_local_size(1) * get_local_size(2)
		+ get_local_id(1) * get_local_size(2)
		+ get_local_id(2);
	int localSize = get_local_size(0) * get_local_size(1) * get_local_size(2);

	//每个工作组在全局内存中独占一块区域：groupHist = globalHist + groupId * bins
	__global uint* groupHist = globalHist + groupId * bins;

	//初始化工作组私有直方图
	for (int index = localId; index < bins; index += localSize)
	{
		groupHist[index] = 0;
	}

	barrier(CLK_GLOBAL_MEM_FENCE);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	//读取体素值，确定桶索引
	int4 position = (int4)(x, y, z, 0);
	float huValue = read_imagef(input, position).x * MAX_16BIT_SIGNED;

	//计算桶索引
	float binFloat = (huValue - minHU) / (maxHU - minHU) * (float)bins;
	int bin = (int)clamp(binFloat, 0.0f, (float)(bins - 1));

	//原子累加到工作组私有直方图
	atomic_inc(&groupHist[bin]);
}
