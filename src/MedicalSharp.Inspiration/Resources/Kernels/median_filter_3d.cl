/// <summary>
/// 3D中值滤波
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
__kernel void median_filter_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(input);
	int height = get_image_height(input);
	int depth = get_image_depth(input);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	int4 position = (int4)(x, y, z, 0);
	int radius = kernelSize / 2;

	//边界：保留原值
	if (x < radius || x >= width - radius ||
		y < radius || y >= height - radius ||
		z < radius || z >= depth - radius)
	{
		float4 value = read_imagef(input, position);
		write_imagef(output, position, value);

		return;
	}

	//读取邻域值到数组
	int totalCount = kernelSize * kernelSize * kernelSize;
	float values[125]; //最大支持5×5×5=125，更大核需要更大的数组

	int index = 0;
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float4 value = read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0));
				values[index++] = value.x;
			}
		}
	}

	//排序取中位数（冒泡排序，小数据量足够）
	for (int i = 0; i < totalCount - 1; i++)
	{
		for (int j = i + 1; j < totalCount; j++)
		{
			if (values[i] > values[j])
			{
				float tmp = values[i];
				values[i] = values[j];
				values[j] = tmp;
			}
		}
	}

	float median = values[totalCount / 2];
	write_imagef(output, position, (float4)(median, 0, 0, 0));
}
