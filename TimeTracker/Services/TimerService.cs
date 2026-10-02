using System.Windows.Threading;

namespace TimeTracker.Services;

public class TimerService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private DateTime _sessionStart;
    private int _intervalMinutes;
    private TimeSpan? _lastWorkDayStart;
    private TimeSpan? _lastWorkDayEnd;

    public event Action? TimerElapsed;
    public event Action<TimeSpan>? Tick;

    public bool IsRunning { get; private set; }
    public bool IsOutsideWorkHours { get; private set; }
    public TimeSpan Remaining { get; private set; }
    public DateTime SessionStart => _sessionStart;

    public TimerService(int intervalMinutes)
    {
        _intervalMinutes = intervalMinutes;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
    }

    public void SetInterval(int minutes)
    {
        _intervalMinutes = minutes;
        if (IsRunning) Restart();
    }

    /// <summary>
    /// Starts or recalculates the timer aligned to the work day start and end.
    /// If outside work hours, the timer stays idle until workDayStart.
    /// </summary>
    public void StartAligned(TimeSpan workDayStart, TimeSpan? workDayEnd = null)
    {
        _lastWorkDayStart = workDayStart;
        _lastWorkDayEnd = workDayEnd;

        var now = DateTime.Now.TimeOfDay;
        var intervalSpan = TimeSpan.FromMinutes(_intervalMinutes);

        if (workDayEnd.HasValue && CheckIsOutsideWorkHours(now, workDayStart, workDayEnd.Value))
        {
            IsOutsideWorkHours = true;
            TimeSpan untilStart;
            if (now < workDayStart)
                untilStart = workDayStart - now;
            else
                untilStart = (TimeSpan.FromDays(1) - now) + workDayStart;

            Remaining = untilStart;
            _sessionStart = DateTime.Now;
        }
        else
        {
            IsOutsideWorkHours = false;
            var diff = now - workDayStart;
            var ticksInInterval = intervalSpan.Ticks;
            var mod = diff.Ticks % ticksInInterval;
            if (mod < 0) mod += ticksInInterval;

            var elapsedInInterval = TimeSpan.FromTicks(mod);
            Remaining = intervalSpan - elapsedInInterval;
            _sessionStart = DateTime.Now - elapsedInInterval;
        }

        IsRunning = true;
        _timer.Start();
        Tick?.Invoke(Remaining);
    }

    public static bool CheckIsOutsideWorkHours(TimeSpan now, TimeSpan start, TimeSpan end)
    {
        if (start == end) return false;
        if (start < end)
            return now < start || now >= end;
        else
            return now >= end && now < start;
    }

    public void Start()
    {
        _sessionStart = DateTime.Now;
        Remaining = TimeSpan.FromMinutes(_intervalMinutes);
        IsRunning = true;
        _timer.Start();
    }

    public void Restart()
    {
        _sessionStart = DateTime.Now;
        Remaining = TimeSpan.FromMinutes(_intervalMinutes);
    }

    public void Stop()
    {
        _timer.Stop();
        IsRunning = false;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Remaining = Remaining.Subtract(TimeSpan.FromSeconds(1));
        Tick?.Invoke(Remaining);

        if (Remaining <= TimeSpan.Zero)
        {
            var wasOutside = IsOutsideWorkHours;
            if (!wasOutside)
            {
                TimerElapsed?.Invoke();
            }

            if (_lastWorkDayStart.HasValue)
            {
                StartAligned(_lastWorkDayStart.Value, _lastWorkDayEnd);
            }
            else
            {
                Remaining = TimeSpan.FromMinutes(_intervalMinutes);
            }
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
