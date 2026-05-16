typedef struct
{
	float HU;
	short Label;
	uchar Visited;
	uchar Padding;
} Voxel;

__kernel void modify_voxels(__global const Voxel* input, __global Voxel* output, const int count)
{
	int gid = get_global_id(0);
	if (gid >= count)
	{
		return;
	}

	output[gid].HU = input[gid].HU * 2.0f;
	output[gid].Label = input[gid].Label + 100;
	output[gid].Visited = input[gid].Visited;
	output[gid].Padding = 0;
}
