using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly Action<(bool timedOut, object? result)> _callback;
    private readonly Timer _timer;
    private int _isCompleted = 0; // Флаг атомарности для предотвращения двойного вызова

    public Guid Id { get; }

    public WaiterState( Guid id, TimeSpan timeout, Action<(bool timedOut, object? result)> callback )
    {
      Id = id;
      _callback = callback;

      // Таймер сработает ровно один раз через заданный TimeSpan
      _timer = new Timer( OnTimeout, null, timeout, Timeout.InfiniteTimeSpan );
    }

    // Вызывается автоматически при срабатывании таймера
    private void OnTimeout( object? state )
    {
      Trigger( timedOut: true, result: null );
    }

    // Метод для безопасного вызова колбэка
    public void Trigger( bool timedOut, object? result )
    {
      // Interlocked гарантирует, что колбэк выполнится только 1 раз (кто первый успел — таймер или Activity)
      if ( Interlocked.Exchange( ref _isCompleted, 1 ) == 0 )
      {
        // Удаляем себя из глобального словаря
        _waiters.TryRemove( Id, out _ );

        // Вызываем делегат сервиса
        _callback.Invoke( (timedOut, result) );

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
  public static Guid RegisterWaiter( TimeSpan timeout, Action<(bool timedOut, object? result)> onFinishOrTimeOut )
  {
    Guid waiterId = Guid.NewGuid();
    var state = new WaiterState( waiterId, timeout, onFinishOrTimeOut );

    _waiters[waiterId] = state;

    return waiterId;
  }

  /// <summary>
  /// Вызывается из Activity для передачи успешного результата.
  /// </summary>
  public static void Signal( Guid waiterId, object? obj )
  {
    if ( _waiters.TryGetValue( waiterId, out var state ) )
    {
      state.Trigger( timedOut: false, result: obj );
    }
    // Если в словаре нет ключа, значит уже сработал таймаут и ожидатель удален. Ничего не делаем.
  }
}
