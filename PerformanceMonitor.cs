public class PerformanceMonitor
{
    public event Action<double> ProgressChanged;
    public event Action<double> SpeedChanged;
    public event Action<string> StatusChanged;
    public event Action<double> RatioChanged;

    private long totalProcessedBytes = 0;
    private long totalCompressedBytes = 0;
    private int totalSamples;
    private int processedSamples;

    private DateTime lastTime;
    private int lastProcessed;

    public void Start(int total, int size)
    {
        totalSamples = total;
        processedSamples = 0;

        lastTime = DateTime.Now;
        lastProcessed = 0;

        StatusChanged?.Invoke("بدء العملية...");
    }

    public void Update(int processed, int compressed)
    {
        processedSamples += processed;

        // 🔥 حساب التقدم (0–100)
        double progress = (double)processedSamples / totalSamples * 100.0;
        ProgressChanged?.Invoke(progress);

        // 🔥 حساب السرعة (عينات/ثانية)
        var now = DateTime.Now;
        double seconds = (now - lastTime).TotalSeconds;

        if (seconds >= 0.2)
        {
            int delta = processedSamples - lastProcessed;
            double speed = delta / seconds;

            SpeedChanged?.Invoke(speed);

            lastTime = now;
            lastProcessed = processedSamples;
        }

        // 🔥 حساب نسبة الضغط
        totalProcessedBytes += processed;     // عدد البايتات الأصلية
        totalCompressedBytes += compressed;   // عدد البايتات المضغوطة

        if (totalProcessedBytes > 0)
        {
            double ratio = (double)totalCompressedBytes / totalProcessedBytes * 100.0;
            RatioChanged?.Invoke(ratio);
        }
    }

    public void Finish()
    {
        StatusChanged?.Invoke("اكتملت العملية");
    }

    public void ReportCanceled()
    {
        StatusChanged?.Invoke("تم إلغاء العملية");
    }
    public void ReportProgress(float value)
    {
        ProgressChanged?.Invoke(value);
    }

}
