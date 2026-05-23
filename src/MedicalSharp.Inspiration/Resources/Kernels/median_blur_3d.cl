/// <summary>
/// 3D中值滤波
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
__kernel void median_blur_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize)
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

	float values[27];
	int index = 0;
	for (int dz = -1; dz <= 1; dz++)
	{
		for (int dy = -1; dy <= 1; dy++)
		{
			for (int dx = -1; dx <= 1; dx++)
			{
				values[index++] = read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0)).x;
			}
		}
	}

	//冒泡排序27个数
	for (int i = 0; i < 26; i++)
	{
		for (int j = i + 1; j < 27; j++)
		{
			if (values[i] > values[j])
			{
				float temp = values[i];
				values[i] = values[j];
				values[j] = temp;
			}
		}
	}

	write_imagef(output, position, (float4)(values[13], 0, 0, 0));
}
