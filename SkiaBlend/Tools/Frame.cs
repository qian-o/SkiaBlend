namespace SkiaBlend.Tools;

public abstract unsafe class Frame : IDisposable
{
    protected int width;
    protected int height;
    protected nint pixels;
    protected bool isReady;

    public int Width => width;

    public int Height => height;

    public nint Pixels => pixels;

    public bool IsReady => isReady;

    /// <summary>
    /// 重置帧大小。
    /// </summary>
    /// <param name="w">w</param>
    /// <param name="h">h</param>
    public abstract void Resize(int w, int h);

    /// <summary>
    /// 绘制帧。
    /// </summary>
    /// <param name="frame">frame</param>
    /// <param name="matrix">matrix</param>
    public abstract void DrawFrame(Frame frame, float ox, float oy, float sx, float sy);

    /// <summary>
    /// 清理。
    /// </summary>
    public abstract void Destroy();

    public void Dispose()
    {
        Destroy();

        GC.SuppressFinalize(this);
    }
}
