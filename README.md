# gpgpu-learning
GPGPUを苦しんで覚える

# ディレクトリ構成について
`sprinf(solfile, "chap%d/GPGPU%d/GPGPU%d.sln", chapter_number)`にVisual Studio用のソリューションファイルがあり，  
Visual Studio Codeでは，`sprinf(code_dir, "chap%d", chapter_number)`を開くと実行ができます．

# Table of Contents
変更する可能性が大きいです．
 1. [実行環境の導入](chap1/chapter1.md)
 2. [はじめてのGPGPU](chap2/chapter2.md)
 3. 配列とはじめての画像処理
 4. 同期と集約処理
 5. GPUコアの構造とSIMDと命令セットとプロファイリング
 6. メモリヒエラルキと畳み込みを用いた画像処理
 7. SIMDとJsonのパース
 8. GPUにおける条件分岐と衝突判定（木構造の探索）
 9. OpenCL, CUDA, SYCL, Data Parallel C++, C++ AMP, OpemMP, OpenAAC...