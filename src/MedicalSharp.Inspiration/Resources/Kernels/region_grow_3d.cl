/// <summary>
/// Short最大值
/// </summary>
__constant float MAX_16BIT_SIGNED = 32767.0f;

/// <summary>
/// 临时标记A
/// </summary>
__constant uchar TEMP_MARK_A = 254;

/// <summary>
/// 临时标记B
/// </summary>
__constant uchar TEMP_MARK_B = 255;

/// <summary>
/// 区域生长
/// </summary>
/// <param name="preview">预览纹理</param>
/// <param name="markInput">输入标记缓冲区（上轮迭代结果）</param>
/// <param name="markOutput">输出标记缓冲区（本轮迭代结果）</param>
/// <param name="minHU">最小HU值</param>
/// <param name="maxHU">最大HU值</param>
/// <param name="markValue">种子点标记值</param>
/// <param name="newVoxelsCount">新体素原子计数器</param>
__kernel void region_grow(__read_only image3d_t preview, __global uchar* markInput, __global uchar* markOutput,
	const float minHU,
	const float maxHU,
	const uchar markValue,
	__global uint* newVoxelsCount)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(preview);
	int height = get_image_height(preview);
	int depth = get_image_depth(preview);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	//计算体素索引
	int voxelIndex = z * width * height + y * width + x;

	//当前体素已经被标记过，跳过
	uchar currentMark = markInput[voxelIndex];
	if (currentMark == markValue || currentMark == TEMP_MARK_A || currentMark == TEMP_MARK_B)
	{
		return;
	}

	//检查6个相邻体素是否有种子或上一轮临时标记
	int neighborOffsets[18] = {
		 1,  0,  0,   //右
		-1,  0,  0,   //左
		 0,  1,  0,   //上
		 0, -1,  0,   //下
		 0,  0,  1,   //前
		 0,  0, -1    //后
	};

	bool hasNeighborSeed = false;
	for (int index = 0; index < 6; index++)
	{
		int nx = x + neighborOffsets[index * 3 + 0];
		int ny = y + neighborOffsets[index * 3 + 1];
		int nz = z + neighborOffsets[index * 3 + 2];

		//邻居超出体积边界，跳过
		if (nx < 0 || nx >= width || ny < 0 || ny >= height || nz < 0 || nz >= depth)
		{
			continue;
		}

		uchar neighborMark = markInput[nz * width * height + ny * width + nx];

		//邻居是种子点或上一轮临时标记，本轮检查的"种子"
		if (neighborMark == markValue || neighborMark == TEMP_MARK_A)
		{
			hasNeighborSeed = true;
			break;
		}
	}

	//没有找到合格的邻居，不在生长前沿
	if (!hasNeighborSeed)
	{
		return;
	}

	//检查HU值是否在范围内
	float huValue = read_imagef(preview, (int4)(x, y, z, 0)).x * MAX_16BIT_SIGNED;
	if (huValue < minHU || huValue > maxHU)
	{
		return;
	}

	//标记为本轮临时标记
	markOutput[voxelIndex] = TEMP_MARK_B;

	//递增新体素计数
	atomic_inc(newVoxelsCount);
}
