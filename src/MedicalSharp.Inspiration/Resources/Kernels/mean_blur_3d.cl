/// <summary>
/// 3D均值滤波
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
__kernel void mean_blur_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize)
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

	//邻域求和
	float4 sum = (float4)(0.0f);
	int count = 0;
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float4 value = read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0));
				sum += value;
				count++;
			}
		}
	}

	write_imagef(output, position, sum / (float)count);
}
