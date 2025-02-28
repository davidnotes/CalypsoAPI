using CalypsoAPI;
using CalypsoAPI.Events;
using CalypsoAPI.Models;
using System.Collections.ObjectModel;
using CalypsoAPI.WebApi;

namespace CalypsoMonitor.Services
{
    public class CalypsoService : IDisposable
    {
        private Calypso? _calypso;
        private readonly ILogger<CalypsoService> _logger;
        private readonly IConfiguration _configuration;

        public bool IsConnected => _calypso?.IsRunning ?? false;
        public Status CurrentStatus => _calypso?.State.Status ?? Status.Stopped;
        public MeasurementPlanInfo? CurrentPlan => _calypso?.State.MeasurementPlan;
        public MeasurementResult? CurrentResults => _calypso?.State.LatestMeasurementResults;

        public event Action? OnStatusChanged;
        public event Action<string>? OnError;
        public event Action<string>? OnMeasurementStarted;
        public event Action<string>? OnMeasurementFinished;

        public ObservableCollection<string> EventLog { get; } = new ObservableCollection<string>();

        public CalypsoService(ILogger<CalypsoService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (_calypso != null && _calypso.IsRunning)
                {
                    await _calypso.StopAsync();
                }

                string observerPath = _configuration["Calypso:ObserverPath"] ?? @"C:\Users\Public\Documents\Zeiss\CMMObserver";
                string apiUrl = _configuration["Calypso:ApiUrl"] ?? "http://localhost:5000";

                _calypso = new CalypsoBuilder()
                    .Configure(config => { config.CMMObserverFolderPath = observerPath; })
                    .AddWebApi(webHostBuilder => { webHostBuilder.UseUrls(apiUrl); })
                    .Build();

                // 绑定事件
                _calypso.MeasurementStarted += Calypso_MeasurementStarted;
                _calypso.MeasurementFinished += Calypso_MeasurementFinished;
                _calypso.MeasurementStopped += Calypso_MeasurementStopped;
                _calypso.MeasurementPaused += Calypso_MeasurementPaused;
                _calypso.MeasurementContinued += Calypso_MeasurementContinued;
                _calypso.MeasurementError += Calypso_MeasurementError;
                _calypso.CalypsoException += Calypso_CalypsoException;

                // 启动
                await _calypso.StartAsync();
                
                AddLogEntry("已连接到Calypso");
                OnStatusChanged?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接到Calypso时出错");
                OnError?.Invoke($"连接错误: {ex.Message}");
                AddLogEntry($"连接错误: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_calypso != null && _calypso.IsRunning)
            {
                try
                {
                    await _calypso.StopAsync();
                    _calypso.Dispose();
                    _calypso = null;
                    
                    AddLogEntry("已断开与Calypso的连接");
                    OnStatusChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "断开Calypso连接时出错");
                    OnError?.Invoke($"断开连接错误: {ex.Message}");
                    AddLogEntry($"断开连接错误: {ex.Message}");
                }
            }
        }

        private void Calypso_MeasurementStarted(object? sender, MeasurementStartEventArgs e)
        {
            AddLogEntry($"测量开始: {e.MeasurementPlanInfo.PartNumber}");
            OnMeasurementStarted?.Invoke(e.MeasurementPlanInfo.PartNumber);
            OnStatusChanged?.Invoke();
        }

        private void Calypso_MeasurementFinished(object? sender, MeasurementFinishEventArgs e)
        {
            AddLogEntry($"测量完成: {e.MeasurementPlanInfo.PartNumber}, 公差状态: {e.MeasurementResult.ToleranceState}");
            OnMeasurementFinished?.Invoke(e.MeasurementPlanInfo.PartNumber);
            OnStatusChanged?.Invoke();
        }

        private void Calypso_MeasurementStopped(object? sender, EventArgs e)
        {
            AddLogEntry("测量已停止");
            OnStatusChanged?.Invoke();
        }

        private void Calypso_MeasurementPaused(object? sender, EventArgs e)
        {
            AddLogEntry("测量已暂停");
            OnStatusChanged?.Invoke();
        }

        private void Calypso_MeasurementContinued(object? sender, EventArgs e)
        {
            AddLogEntry("测量已继续");
            OnStatusChanged?.Invoke();
        }

        private void Calypso_MeasurementError(object? sender, EventArgs e)
        {
            AddLogEntry("测量出错");
            OnError?.Invoke("测量出错");
            OnStatusChanged?.Invoke();
        }

        private void Calypso_CalypsoException(object? sender, CalypsoExceptionEventArgs e)
        {
            AddLogEntry($"Calypso异常: {e.Exception.Message}");
            OnError?.Invoke($"Calypso异常: {e.Exception.Message}");
            OnStatusChanged?.Invoke();
        }

        private void AddLogEntry(string message)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            _logger.LogInformation(entry);
            
            // 在UI线程上更新集合
            System.Threading.SynchronizationContext.Current?.Post(_ => 
            {
                EventLog.Insert(0, entry);
                // 保持日志不超过100条
                while (EventLog.Count > 100)
                {
                    EventLog.RemoveAt(EventLog.Count - 1);
                }
            }, null);
        }

        public void Dispose()
        {
            if (_calypso != null && _calypso.IsRunning)
            {
                _calypso.StopAsync().Wait();
                _calypso.Dispose();
                _calypso = null;
            }
        }
    }
} 