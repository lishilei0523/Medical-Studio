typedef struct
{
	float min;
	float max;
	float sum;
} Statistic;

//----OpenCL浮点原子操作----
//OpenCL没有内置float原子操作，通过atomic_cmpxchg模拟
//原理：将float按位视为uint，CAS循环直到写入成功
//uint prev_ptr_value = atomic_cmpxchg(ptr, actual, target);
//atomic_cmpxchg读取ptr的值与actual比较，如果相等，将target值赋给ptr，返回ptr修改之前的值

void atomic_add_float(volatile __global float* summation, float value)
{
	//定义联合体
	union
	{
		float asFloat;
		uint asUInt;
	} actual, target;

	do
	{
		//读取实际值
		actual.asFloat = *summation;

		//计算目标值
		target.asFloat = actual.asFloat + value;

		//尝试把目标值写入*summation
	} while (atomic_cmpxchg(
		(volatile __global uint*)summation,		//目标写入地址：*summation
		actual.asUInt,							//实际值（例如 10.0）
		target.asUInt)							//目标值（例如 15.0）
		!= actual.asUInt);						//返回的值不等于实际值 -> 被别的线程改了 -> 重试
}

void atomic_min_float(volatile __global float* minimum, float value)
{
	//定义联合体
	union
	{
		float asFloat;
		uint asUInt;
	} actual, target;

	do
	{
		//读取当前值
		actual.asFloat = *minimum;

		//计算目标值
		target.asFloat = fmin(value, actual.asFloat);

		//尝试把目标值写入*minimum
	} while (atomic_cmpxchg((volatile __global uint*)minimum, actual.asUInt, target.asUInt) != actual.asUInt);
}

void atomic_max_float(volatile __global float* maximum, float value)
{
	//定义联合体
	union
	{
		float asFloat;
		uint asUInt;
	} actual, target;

	do
	{
		//读取当前值
		actual.asFloat = *maximum;

		//计算目标值
		target.asFloat = fmax(value, actual.asFloat);

		//尝试把目标值写入*maximum
	} while (atomic_cmpxchg((volatile __global uint*)maximum, actual.asUInt, target.asUInt) != actual.asUInt);
}

__kernel void analyse(__global const float* input, __global Statistic* result, const int count)
{
	int index = get_global_id(0);
	if (index >= count)
	{
		return;
	}

	float value = input[index];
	atomic_add_float(&result->sum, value);
	atomic_min_float(&result->min, value);
	atomic_max_float(&result->max, value);
}
