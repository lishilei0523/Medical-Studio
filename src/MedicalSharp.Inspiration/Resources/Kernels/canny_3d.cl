/// <summary>
/// 边缘 = 100
/// </summary>
/// <remarks>归一化到SNORM范围[-1, 1]：除以32767</remarks>
__constant float canny_edge_value = 100.0f / 32767.0f;

/// <summary>
/// 背景 = -1024（空气）
/// </summary>
/// <remarks>归一化到SNORM范围[-1, 1]：除以32767</remarks>
__constant float canny_background_value = -1024.0f / 32767.0f;

/// <summary>
/// Canny边缘检测3D
/// </summary>
/// <param name="input">梯度强度图像</param>
/// <param name="output">输出二值图像（边缘=3071，背景=-1024）</param>
/// <param name="lower">低阈值（弱边缘）</param>
/// <param name="upper">高阈值（强边缘）</param>
/// <param name="radius">膨胀半径（用于滞后跟踪，默认1）</param>
/// <remarks>
/// 双阈值检测 + 形态学滞后跟踪
/// 对应OpenCV的Cv2.Canny简化版
/// </remarks>
__kernel void canny_3d(__read_only image3d_t input, __write_only image3d_t output, const float lower, const float upper, const int radius)
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
	float magnitude = read_imagef(input, position).x;

	//强边缘：标记为边缘
	if (magnitude >= upper)
	{
		write_imagef(output, position, (float4)(canny_edge_value, 0, 0, 0));
		return;
	}

	//非边缘：标记为背景
	if (magnitude < lower)
	{
		write_imagef(output, position, (float4)(canny_background_value, 0, 0, 0));

		return;
	}

	//弱边缘：检查周围是否有强边缘（形态学膨胀的等价操作）
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				int nx = x + dx;
				int ny = y + dy;
				int nz = z + dz;
				if (nx < 0 || nx >= width || ny < 0 || ny >= height || nz < 0 || nz >= depth)
				{
					continue;
				}

				float neighborMagnitude = read_imagef(input, (int4)(nx, ny, nz, 0)).x;
				if (neighborMagnitude >= upper)
				{
					write_imagef(output, position, (float4)(canny_edge_value, 0, 0, 0));

					return;
				}
			}
		}
	}

	//没有连接到强边缘的弱边缘：丢弃
	write_imagef(output, position, (float4)(canny_background_value, 0, 0, 0));
}
