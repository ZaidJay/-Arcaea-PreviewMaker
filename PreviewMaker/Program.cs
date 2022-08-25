using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PreviewMaker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("输入预览开始时间(ms):");
            int start = int.Parse(Console.ReadLine());
            Console.Write("输入预览结束时间(ms):");
            int end = int.Parse(Console.ReadLine());
            int len = end - start;
            if (len < 0)
            {
                Console.Write("错误!开始时间大于结束时间!");
            }
            else
            {
                Process process1 = new Process();
                process1.StartInfo.FileName = "ffmpeg.exe";  // 这里也可以指定ffmpeg的绝对路径
                process1.StartInfo.Arguments = " -i " + "song/base.ogg" + " -y " + "song/base.mp3";
                process1.StartInfo.UseShellExecute = false;
                process1.StartInfo.CreateNoWindow = true;
                process1.StartInfo.RedirectStandardOutput = true;
                process1.StartInfo.RedirectStandardInput = true;
                process1.StartInfo.RedirectStandardError = true;  
                DateTime beginTime = DateTime.Now;
                process1.Start();
                process1.BeginErrorReadLine();   // 开始异步读取
                Console.WriteLine("开始音频转码(base.ogg->base.mp3)...");
                process1.WaitForExit();    // 等待转码完成
                if (process1.ExitCode == 0)
                {
                    int exitCode = process1.ExitCode;
                    DateTime endTime = DateTime.Now;
                    TimeSpan t = endTime - beginTime;
                    double seconds = t.TotalSeconds;
                    Console.WriteLine("base 转码完成!总共用时:" + seconds + "秒");

                    var output = new AudioFileReader("song/base.mp3")
                        .Skip(TimeSpan.FromSeconds(1.0 * start / 1000))
                        .Take(TimeSpan.FromSeconds(1.0 * len / 1000));
                    WaveFileWriter.CreateWaveFile16("song/preview.mp3", output);
                    Console.WriteLine("已生成 preview.mp3 !");

                    Process process2 = new Process();
                    process2.StartInfo.FileName = "ffmpeg.exe";
                    process2.StartInfo.Arguments = " -i " + "song/preview.mp3" + " -y " + "song/preview.ogg";
                    process2.StartInfo.UseShellExecute = false;
                    process2.StartInfo.CreateNoWindow = true;
                    process2.StartInfo.RedirectStandardOutput = true;
                    process2.StartInfo.RedirectStandardInput = true;
                    process2.StartInfo.RedirectStandardError = true;
                    beginTime = DateTime.Now;
                    process2.Start();
                    process2.BeginErrorReadLine();   // 开始异步读取
                    Console.WriteLine("开始音频转码(preview.mp3->preview.ogg)...");
                    process2.WaitForExit();    // 等待转码完成
                    if (process2.ExitCode == 0)
                    {
                        exitCode = process2.ExitCode;
                        endTime = DateTime.Now;
                        t = endTime - beginTime;
                        seconds = t.TotalSeconds;
                        Console.WriteLine("preview 转码完成!总共用时:" + seconds + "秒");
                        Console.Write("已生成 preview.ogg !");
                    }
                    else
                    {
                        Console.WriteLine("程序发生错误,转码失败!");
                    }
                    process2.Close();
                }
                else
                {
                    Console.WriteLine("程序发生错误,转码失败!");
                }
                process1.Close();
                File.Delete("song/base.mp3");
                File.Delete("song/preview.mp3");
            }
            System.Console.ReadKey();
        }
    }
}
