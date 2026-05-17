typedef struct
{
	float HU;
	short Label;
	unsigned char Visited;
	unsigned char Padding;
} Voxel;

__kernel void modify_voxels(__global const Voxel* input, __global Voxel* output, const int count)
{
	int index = get_global_id(0);
	if (index >= count)
	{
		return;
	}

	output[index].HU = input[index].HU * 2.0f;
	output[index].Label = input[index].Label + 100;
	output[index].Visited = input[index].Visited;
	output[index].Padding = 0;
}
