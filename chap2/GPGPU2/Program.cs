// ILGPUを使うための宣言
using ILGPU;         // ILGPUを使います！
using ILGPU.Runtime; // ILGPU.Runtimeを使います！

using System.Diagnostics;

// GPUを動かしてみようのメソッドです
void S2HelloWorld()
{
    // ContextはGPUのプログラムコードの管理をしてくれます．
    using Context context = Context.CreateDefault();
    // AcceleratorとはGPUを指します．
    // AcceleratorTypeがCPUではないAcceleratorを取得します．
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    // これはGPUのコードです．
    void helloworld(Index1D index, int a, int b)
    {
        // コンソールに出力したい場合はInterop.WriteLineを使います．
        Interop.WriteLine("Hello from GPU! {0}+{1}={2}", a, b, a + b);
        // {0}にa，{1}にb，{2}にa+bの結果が出力されます．
    }

    // LoadAutoGroupedStreamKernelでhelloworldをGPU向けにコンパイルして，Action型と呼ばれる関数を表す型の値を返します．
    Action<Index1D, int, int> gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, int, int>(helloworld);

    // まずは，最初の1は最初は無視して，3と5を変えてましょう．出力結果が変わるはずです．
    // gpuKernelを何回も呼び出せば，何回も実行されます．
    gpuKernel(1, 3, 5);
    // gpuKernel(1, 5, 5);

    // gpuKernelの実行が終わるのを待ちます．
    accelerator.Synchronize();
}


void S3vsCPU()
{
    using Context context = Context.Create(builder => builder.Default().Profiling());
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    // これはGPUのコードです．
    void LoopOnGPU(Index1D index, int a)
    {
        int res = 0;
        for (int i = 0; i < a; ++i)
            res += i;
        //Interop.WriteLine("Result: {0}", res);
    }
    using var stream = accelerator.CreateStream();
    var gpuKernel = accelerator.LoadAutoGroupedKernel<Index1D, int>(LoopOnGPU);
    // 乱数を10000以上100000以下で生成します．
    // aを大きくしすぎると，画面がフリーズするのでやめましょう．
    int a = new Random().Next(10000, 100000);

    // GPUで実行します
    using var startMarker = stream.AddProfilingMarker(); // プログラムが起動した瞬間の時間を格納します．
    gpuKernel(stream, 1, a);
    using var endMarker = stream.AddProfilingMarker();   // プログラムが終了した瞬間の時間を格納します．
    stream.Synchronize();
    Console.WriteLine("GPU time: {0}", (endMarker - startMarker)); // かかった時間を計測します．

    // CPUで実行します
    var sw = Stopwatch.StartNew(); // ストップウォッチです
    LoopOnGPU(default, a);
    sw.Stop();
    Console.WriteLine("CPU time: {0}", sw.Elapsed);
}

void S4Parallel()
{
    using Context context = Context.CreateDefault();
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    // これはGPUのコードです．
    void HelloFromMultipleGPUCores(Index1D index, int a)
    {
        Interop.WriteLine("Hello from GPU{0}", index);
    }

    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, int>(HelloFromMultipleGPUCores);

    gpuKernel(6, 0);
    accelerator.Synchronize();
}

// S2HelloWorldを呼び出します．
S2HelloWorld();
// 2節以降のコードを実行するときは，S2HelloWorldをコメント化して，次のコメントに示すように，呼び出しをします．
// S3vsCPU();
// S4Parallel();