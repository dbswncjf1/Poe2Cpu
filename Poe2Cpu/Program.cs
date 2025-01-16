using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Globalization;

class Program
{
    class Messages
    {
        public string Started { get; set; }
        public string SystemCPUInfo { get; set; }
        public string TotalCPUCores { get; set; }
        public string AvailableCPUNumbers { get; set; }
        public string AllCurrentCPUs { get; set; }
        public string CPUsToSet { get; set; }
        public string AffinityMask { get; set; }
        public string Binary { get; set; }
        public string StartingProcessMonitor { get; set; }
        public string AffinitySet { get; set; }
        public string ProcessID { get; set; }
        public string UsedCPUs { get; set; }
        public string ProcessError { get; set; }
        public string TryingAlternative { get; set; }
        public string AlternativeSuccess { get; set; }
        public string AlternativeFailed { get; set; }
        public string ErrorOccurred { get; set; }
    }

    static Messages GetMessages(string culture)
    {
        switch (culture.ToLower())
        {
            case "ko-kr":
                return new Messages
                {
                    Started = "PathOfExile CPU Affinity Manager 시작됨",
                    SystemCPUInfo = "[시스템 CPU 정보]",
                    TotalCPUCores = "총 CPU 코어 수: ",
                    AvailableCPUNumbers = "사용 가능한 CPU 번호: ",
                    AllCurrentCPUs = "[현재 모든 CPU]",
                    CPUsToSet = "[Path of Exile에 설정할 CPU]",
                    AffinityMask = "[Affinity 마스크]",
                    Binary = "이진수: ",
                    StartingProcessMonitor = "프로세스 모니터링을 시작합니다...",
                    AffinitySet = "CPU Affinity 설정됨",
                    ProcessID = "프로세스 ID: ",
                    UsedCPUs = "사용 CPU: ",
                    ProcessError = "프로세스 처리 중 오류: ",
                    TryingAlternative = "다른 방식으로 시도합니다...",
                    AlternativeSuccess = "대체 방법으로 설정 성공",
                    AlternativeFailed = "대체 방법도 실패: ",
                    ErrorOccurred = "오류 발생: "
                };
            case "ja-jp":
                return new Messages
                {
                    Started = "PathOfExile CPU Affinity Manager 起動",
                    SystemCPUInfo = "[システムCPU情報]",
                    TotalCPUCores = "合計CPUコア数: ",
                    AvailableCPUNumbers = "利用可能なCPU番号: ",
                    AllCurrentCPUs = "[現在のすべてのCPU]",
                    CPUsToSet = "[Path of Exileに設定するCPU]",
                    AffinityMask = "[Affinityマスク]",
                    Binary = "バイナリ: ",
                    StartingProcessMonitor = "プロセスモニタリングを開始します...",
                    AffinitySet = "CPU Affinity設定完了",
                    ProcessID = "プロセスID: ",
                    UsedCPUs = "使用CPU: ",
                    ProcessError = "プロセス処理中のエラー: ",
                    TryingAlternative = "別の方法を試みます...",
                    AlternativeSuccess = "代替方法で設定成功",
                    AlternativeFailed = "代替方法も失敗: ",
                    ErrorOccurred = "エラーが発生しました: "
                };
            default: // English
                return new Messages
                {
                    Started = "PathOfExile CPU Affinity Manager Started",
                    SystemCPUInfo = "[System CPU Information]",
                    TotalCPUCores = "Total CPU Cores: ",
                    AvailableCPUNumbers = "Available CPU Numbers: ",
                    AllCurrentCPUs = "[All Current CPUs]",
                    CPUsToSet = "[CPUs to Set for Path of Exile]",
                    AffinityMask = "[Affinity Mask]",
                    Binary = "Binary: ",
                    StartingProcessMonitor = "Starting process monitoring...",
                    AffinitySet = "CPU Affinity Set",
                    ProcessID = "Process ID: ",
                    UsedCPUs = "Used CPUs: ",
                    ProcessError = "Error processing process: ",
                    TryingAlternative = "Trying alternative method...",
                    AlternativeSuccess = "Alternative method successful",
                    AlternativeFailed = "Alternative method failed: ",
                    ErrorOccurred = "Error occurred: "
                };
        }
    }

    static void Main()
    {
        string currentCulture = CultureInfo.CurrentUICulture.Name;
        Messages msgs = GetMessages(currentCulture);

        Console.WriteLine(msgs.Started);
        int processorCount = Environment.ProcessorCount;
        Console.WriteLine($"\n{msgs.SystemCPUInfo}");
        Console.WriteLine($"{msgs.TotalCPUCores}{processorCount}");
        Console.WriteLine($"{msgs.AvailableCPUNumbers}0-{processorCount - 1}");
        Console.WriteLine($"\n{msgs.AllCurrentCPUs}");
        string allCoresDisplay = string.Join(", ", Enumerable.Range(0, processorCount));
        Console.WriteLine(allCoresDisplay);

        Console.WriteLine($"\n{msgs.CPUsToSet}");
        string selectedCoresDisplay = string.Join(", ", Enumerable.Range(2, processorCount - 2));
        Console.WriteLine(selectedCoresDisplay);

        long desiredAffinity = -1L;
        desiredAffinity &= ~(1L | 1L << 1);

        Console.WriteLine($"\n{msgs.AffinityMask}");
        Console.WriteLine($"{msgs.Binary}{Convert.ToString(desiredAffinity, 2).PadLeft(processorCount, '0')}");
        Console.WriteLine($"\n{msgs.StartingProcessMonitor}\n");

        while (true)
        {
            try
            {
                var processes = Process.GetProcessesByName("PathOfExile_KG");
                foreach (var process in processes)
                {
                    try
                    {
                        long currentAffinity = process.ProcessorAffinity.ToInt64();
                        if (currentAffinity != desiredAffinity)
                        {
                            process.ProcessorAffinity = new IntPtr(desiredAffinity);
                            Console.WriteLine($"[{DateTime.Now}] {msgs.AffinitySet}");
                            Console.WriteLine($"{msgs.ProcessID}{process.Id}");
                            Console.WriteLine($"{msgs.UsedCPUs}{selectedCoresDisplay}\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{msgs.ProcessError}{ex.Message}");
                        Console.WriteLine(msgs.TryingAlternative);
                        try
                        {
                            long altAffinity = process.ProcessorAffinity.ToInt64();
                            altAffinity &= ~(1L | 1L << 1);
                            process.ProcessorAffinity = new IntPtr(altAffinity);
                            Console.WriteLine($"{msgs.AlternativeSuccess}\n");
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine($"{msgs.AlternativeFailed}{ex2.Message}\n");
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{msgs.ErrorOccurred}{ex.Message}\n");
            }
            Thread.Sleep(5000);
        }
    }
}