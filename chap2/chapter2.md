# 2. はじめてのGPGPU
## 2.1. GPGPUとは
GPGPU(General-Purpose computing on Graphics Processing Units)とは，
画像処理向けの演算装置GPU(Graphics Processing Unit)を広範囲の用途に利用して，
計算のパフォーマンスを高めようとするものです．
具体的な用途としては，機械学習(Machine Lerarning)や，物理シミュレーション(Physics Simulation)，暗号解読（Cryptanalysis），データベース（Database）などがあります．
GPUがCPUに対して，どのように性能がより良いのか，というのをまずはこの章で見てみましょう．

## 2.2. GPUを動かしてみよう（S2HelloWorldメソッド）
まずは，GPUを動かしてみます．実行すると，
```
Hello from GPU! 3 + 5 = 8
```
と表示されるはずです．

ソースコードの説明を上からやります．

```csharp
using ILGPU;
using ILGPU.Runtime;
```

これは，ILGPUライブラリを使うという宣言です．

```csharp 
// GPUを動かしてみようのメソッドです
void S2HelloWorld()
{
    // ...
}
```

メソッド定義です．`S2HelloWorld`メソッドを定義しており，ソースコードの最後の方に，

```csharp
S2HelloWorld();
```

というのがあります．これによって呼び出しができます．

`S2HelloWorld`メソッドの中を見ていきましょう．

```csharp
using Context context = Context.CreateDefault();
```

`Context`はGPUを管理してくれるクラスです．

```csharp
using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);
```

これは，`Accelerator`を取得しています．`context.Devices`にコンピュータにある`Accelerator`が格納されており，
`First`メソッドを用いて，CPUではない（普通はGPU）`Accelerator`を1つ取り出しています．
取り出したのは`Device`クラスなので，`CreateAccelerator`メソッドを用いて，`Accelerator`を作ります．

先頭に`using`がありますが，これは，`Context`や`Accelerator`を解放するためにつけています．
C言語の標準ライブラリで`malloc`したものを必ず`free`するように，`Context`や`Accelerator`は解放が必要です．

```csharp
void helloworld(Index1D index, int a, int b)
{
    Interop.WriteLine("Hello from GPU! {0}+{1}={2}", a, b, a + b);
}
```

ここでは，`helloworld`メソッドを定義しています．
`Interop.WriteLine`メソッドにより，画面出力ができ，文字列に`{0}`といった風に記述することによって，
変数の値を出力することができます．文字列の後に，`a, b, a+b`と渡しているので，`{0}`に`a`，`{1}`に`b`，`{2}`に`a+b`の値が出力されます．
（ブレークポイントを置いても実行は止まりません．）

そして，
```csharp
Action<Index1D, int, int> gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, int, int>(helloworld);
```
とすることで，上の`helloworld`メソッドをGPU向けにコンパイルして，`Action`型の値をもらいます．
GPUのプログラムのことをカーネル（kernel）とよく呼びます．

```csharp
gpuKernel(1, 3, 5);
```
という風に書くことで，GPUのプログラムを実行することができます．

```csharp
accelerator.Synchronize();
```
と書くことで，`gpuKernel`の実行が終わるのを待ちます．
待たないと，GPUの動作が終わる前にプログラムが終了してしまうかもしれません．

## 2.3. CPUとGPUはどちらが速い？（S3vsCPUメソッド）
末尾にあるS2HelloWorldメソッドをコメント化して，S3vsCPUメソッドを呼び出すようにしましょう．

```cshrap
// S2HelloWorldを呼び出します．
//S2HelloWorld();
// 2節以降のコードを実行するときは，S2HelloWorldをコメント化して，次のコメントに示すように，呼び出しをします．
S3vsCPU();
```

そして，実行してみましょう．私の環境では次のようになりました．
```
GPU time: 00:00:00.0069624
CPU time: 00:00:00.0001414
```
Σ(￣ロ￣lll)ｶﾞｰﾝ  
GPUの方が遅いですね…
どんなプログラムを実行したのでしょうか．`S3vsCPU`メソッドを見てみましょう．

```csharp
void LoopOnGPU(Index1 index, int a)
{
    int res = 0;
    for (int i = 0; i < a; ++i)
        res += i;
    Interop.WriteLine("Result: {0}", res);
}
```
これは，1～aまでの値を足し合わせる計算です．
ループで実行されていますが，GPUは処理速度が速いわけではないのです．
CPU（Intel i9-12900K）は最大5.1[GHz]の動作周波数で動くのに対し，GPU(Radeon RX 6900XT)は最大2.25[GHz]で，CPUの半分以下の動作周波数です．
雑に，半分の処理速度しかないと考えると，果たして，GPUはCPUよりも性能が良いと言えるのでしょうか？

### 2.3.1 ソースコードの説明
`var`というキーワードが出てきましたが，これは型推論をしてくれるキーワードです．（C++の`auto`と同じです．）
たとえば，
```csharp
var sw = new Stopwatch();
```
で，変数`sw`は`Stopwatch`型と推論されます．これは，次のコードと等価です．
```csharp
Stopwatch sw = new Stopwatch();
```

```csharp
using var stream = accelerator.CreateStream();
```
はGPUの一連の実行の流れを表すクラスです．（いわゆるDirectXで言う`CommandList`や，Vulkanで言う`CommandPool`です．）

```csharp
using var startMarker = stream.AddProfilingMarker(); // プログラムが起動した瞬間の時間を格納します．
gpuKernel(stream, 1, a);
using var endMarker = stream.AddProfilingMarker();   // プログラムが終了した瞬間の時間を格納します．
```
とすることで，時間を測って→`gpuKernel`を実行して→時間を測って，という一連の命令を発行します．
そして，最後に，
```csharp
stream.Synchronize();
```
によって，一連の流れが終了するまで待ちます．

`Stopwatch`クラスはストップウォッチのクラスです．
メソッド`StartNew()`で新しくストップウォッチを作成し，計測を開始します．

`Console.WriteLine`は`Interop.WriteLine`と同様に，画面に出力します．
GPUのコードの中では使えないため，注意してください．

```csharp
LoopOnGPU(default, a);
```
と，`LoopOnGPU`を呼び出していますが，これは`LoopOnGPU`メソッドをCPU上で実行していることになります．
`default`は最初の引数は使わないのでデフォルトの値を渡しているだけです．

## 2.4. 並列実行がGPUは得意（S4Parallelメソッド）
今度は`S4Parallel`メソッドを実行してみましょう．
```
Hello from GPU0
Hello from GPU1
Hello from GPU2
Hello from GPU3
Hello from GPU16
Hello from GPU17
Hello from GPU18
Hello from GPU19
Hello from GPU32
Hello from GPU33
Hello from GPU34
Hello from GPU35
...
```
たくさん文字列が表示されました．
ここで，`S4Parallel`メソッドを見てみましょう．
```csharp
void HelloFromMultipleGPUCores(Index1D index, int a)
{
    Interop.WriteLine("Hello from GPU{0}", index);
}

var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, int>(HelloFromMultipleGPUCores);

gpuKernel(100, 0);
```

まず，`gpuKernel`の呼び出し箇所を見てみましょう．
今まで，第一引数を`1`にしていましたが，`100`を渡しています．
これを`2`に減らしてみると，出力が2行だけになったと思います．
```
Hello from GPU0
Hello from GPU1
```
この引数は繰り返し実行する回数をしているのでしょうか？

じつは，GPUにはたくさんのプロセッサ（コア）が搭載されており，同時に実行する数を示しています．CPUのコア数は128コア（N1 Neoverse）が限界ですが，GPUは6912コア（NVIDIA A100 SMX）もあります．なので，GPUは同時にプログラムを実行することに長けているのです．

表示をよく見てみましょう．
```
...
Hello from GPU3
Hello from GPU16
Hello from GPU17
...
```
GPU3の次にGPU4が来そうですが，実際はGPU16が来ています．
このことからも並列実行されていることがわかります．


そして，うすうす気づいているかもしれませんが，`HelloFromMultipleGPUCores`メソッドの引数`index`に入る値は，私たちが指定した`100`ではなくて，実行しているコアの番号が割り振られます．  
この番号によって配列の参照先を変えたりします．