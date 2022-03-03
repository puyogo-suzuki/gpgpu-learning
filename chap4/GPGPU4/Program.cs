using ILGPU;
using ILGPU.Runtime;

void S1Parallel256()
{
    using Context context = Context.Create(builder => builder.Default().Profiling());
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);
    int SIZE = Math.Min(256, accelerator.MaxNumThreadsPerGroup);//256;

    void sum(Index1D index, ArrayView<int> array, int size)
    {

        for(int i = 1; i < size; i *= 2)
        {
            if(index % (i * 2) == 0)
                array[index] += array[index + i];
            // 4.1.1. 次のコードをコメントアウトしてください．
            if(index % 3 == 0)
                Interop.Write("a");

            Group.Barrier(); // <- バリアを入れて直そう（4.1.2.）
        }
    }
    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>, int>(sum);
    
    int[] ary = new int[SIZE];
    // ary = {1, 2, 3, ..., SIZE}
    for(int i = 0; i < ary.Length; ++i)
        ary[i] = i+1;

    using var gpuArray = accelerator.Allocate1D<int>(ary.Length);
    gpuArray.CopyFromCPU(ary);

    using var startMarker = accelerator.AddProfilingMarker();
    // 64並列で実行してみます．
    gpuKernel(SIZE, gpuArray.View, ary.Length);
    using var endMarker = accelerator.AddProfilingMarker();

    accelerator.Synchronize();
    var gpuResult = gpuArray.GetAsArray1D();

    // CPU側でも求めてみます．
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    int cpuResult = ary[0];
    for(int i = 1; i < ary.Length; ++i)
    {
        cpuResult += ary[i];
    }
    stopwatch.Stop();

    Console.WriteLine($@"
==RESULT==
GPU:
  Result: {gpuResult[0]}
  Time  : {(endMarker - startMarker)}
CPU:
  Result: {cpuResult}
  Time  : {stopwatch.Elapsed}
==========
");
}

S1Parallel256();