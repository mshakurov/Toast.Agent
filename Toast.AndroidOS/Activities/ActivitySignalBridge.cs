using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.AndroidOS.Activities;

public static class ActivitySignalBridge
{
  public static string C_EXTRA_WAITER_ID = "EXTRA_WAITER_ID";

  // Внутренний класс, представляющий состояние одного ожидания
  private class WaiterState : IDisposable
  {
    private readonly Action<(bool timedOut, string log)> _onFinishOrTimeOut;
    private readonly Timer _timer;
    private int _isCompleted = 0; // Флаг атомарности для предотвращения двойного вызова
    private StringBuilder _log;
    private Stopwatch _swStarted;

    public Guid Id { get; }

    public WaiterState( Guid id, TimeSpan timeout, Action<(bool timedOut, string log)> onFinishOrTimeOut )
    {
      Id = id;
      _onFinishOrTimeOut = onFinishOrTimeOut;
      _log = new();
      _swStarted = Stopwatch.StartNew();
      LogLine( "Created" );

      // Таймер сработает ровно один раз через заданный TimeSpan
      _timer = new Timer( OnTimeout, null, timeout, Timeout.InfiniteTimeSpan );
    }

    // Вызывается автоматически при срабатывании таймера
    private void OnTimeout( object? state )
    {
      Trigger( timedOut: true, message: "OnTimeout" );
    }

    public void LogLine( string? message )
    {
      _log.AppendLine( $"{_swStarted.Elapsed:c} | {message}" );
    }

    // Метод для безопасного вызова колбэка
    public void Trigger( bool timedOut, string? message )
    {
      LogLine( $"Trigger: {message}" );

      // Interlocked гарантирует, что колбэк выполнится только 1 раз (кто первый успел — таймер или Activity)
      if ( Interlocked.Exchange( ref _isCompleted, 1 ) == 0 )
      {
        // Удаляем себя из глобального словаря
        _waiters.TryRemove( Id, out _ );

        // Вызываем делегат сервиса
        _onFinishOrTimeOut.Invoke( (timedOut, _log.ToString()) );

        // Освобождаем ресурсы таймера
        Dispose();
      }
    }

    public void Dispose()
    {
      _timer.Dispose();
    }
  }

  // Потокобезопасный словарь для параллельных задач
  private static readonly ConcurrentDictionary<Guid, WaiterState> _waiters = new();

  /// <summary>
  /// Регистрация нового ожидателя.
  /// </summary>
  public static Guid RegisterWaiter( TimeSpan timeout, Action<(bool timedOut, string log)> onFinishOrTimeOut )
  {
    Guid waiterId = Guid.NewGuid();
    var state = new WaiterState( waiterId, timeout, onFinishOrTimeOut );

    _waiters[waiterId] = state;

    return waiterId;
  }

  /// <summary>
  /// Вызывает запись сигнала (лог) для ожидателя
  /// </summary>
  /// <param name="waiterId"></param>
  /// <param name="message"></param>
  public static void Signal( Guid waiterId, string? message )
  {
    if ( _waiters.TryGetValue( waiterId, out var state ) )
    {
      state.LogLine( message: message );
    }
    // Если в словаре нет ключа, значит уже сработал таймаут и ожидатель удален. Ничего не делаем.
  }

  /// <summary>
  /// Вызывается из Activity для передачи успешного результата.
  /// </summary>
  public static void Finish( Guid waiterId, string? message )
  {
    if ( _waiters.TryGetValue( waiterId, out var state ) )
    {
      state.Trigger( timedOut: false, message: message );
    }
    // Если в словаре нет ключа, значит уже сработал таймаут и ожидатель удален. Ничего не делаем.
  }
}
