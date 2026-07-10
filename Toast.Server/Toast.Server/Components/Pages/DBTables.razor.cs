
using Microsoft.AspNetCore.Components;

using Toast.Server.Data;

namespace Toast.Server.Components.Pages
{
  public partial class DBTables
  {
    Microsoft.EntityFrameworkCore.Metadata.IEntityType[] types = [];
    string? alert = null;

    class SelectedInfo
    {
      public Microsoft.EntityFrameworkCore.Metadata.IEntityType Type { get; set; }

      public Microsoft.EntityFrameworkCore.Metadata.IProperty[] Properties { get; set; } = [];

      public object[] Rows { get; internal set; } = [];

      public string[] GetPropNames() => Rows.Length == 0 ? Properties.Select( p => p.Name ).ToArray() : Rows[0].GetType().GetProperties().Select( p => p.Name ).ToArray();

      public bool Deleting { get; set; }

      public SelectedInfo( Microsoft.EntityFrameworkCore.Metadata.IEntityType selectedType )
      {
        Type = selectedType;

        Properties = Type.GetProperties().ToArray();
      }
    }

    SelectedInfo? selectedType;

    string typeListInfo => $"Types: {types.Length}: " + string.Join( ", ", types.Select( t => $"[{t.Name} (Flds:{t.GetProperties().Count()})]" ) );

    protected async override Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();

      await LoadTables();
    }

    async Task LoadTables()
    {
      alert = "... загрузка данных...";
      StateHasChanged();

      try
      {
        types = dbContext.Model.GetEntityTypes().ToArray();

        if ( types.Length > 0 )
          if ( selectedType == null )
            selectedType = new SelectedInfo( types[0] );
          else
              if ( !types.Contains( selectedType.Type ) )
            selectedType = null;
          else
            selectedType = null;
        alert = null;
      }
      catch ( Exception ex )
      {
        alert = $"Ошибка чтения списка типов: {ex}";
      }

      StateHasChanged();

      await LoadData();
    }

    async Task SelectedTypeChanged( ChangeEventArgs args )
    {
      alert = null;
      StateHasChanged();

      var sel = args.Value is string typeName ? types.First( t => t.ClrType.FullName == typeName ) : selectedType?.Type;
      if ( sel != null )
      {
        selectedType = new SelectedInfo( sel );
        StateHasChanged();
        await LoadData();
      }
      else
        selectedType = null;
    }

    private async Task LoadData()
    {
      if ( selectedType == null ) return;

      selectedType.Rows = ( await GetAllEntitiesAsync( selectedType.Type ) ).ToArray();
    }

    private async Task LoadDataFromUI()
    {
      await LoadData();
    }

    async Task<List<object>> GetAllEntitiesAsync( Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType )
    {
      // 1. Извлекаем реальный System.Type из метаданных EF Core
      Type clrType = entityType.ClrType;

      //var setMethod = dbContext.GetType().GetMethods().FirstOrDefault( m => m.Name == "Set" && m.GetParameters().Length > 0 );
      var setMethod = dbContext.GetType().GetMethod( "Set", [typeof( string )] );
      if ( setMethod == null )
      {
        alert = "Не найден метод Set(string)";
        return [];
      }

      // 2. Получаем доступ к таблице через нетипизированный Set
      // Он возвращает IQueryable, где элементами являются ваши сущности
      try
      {
        var set = await Task.Run( () => setMethod.MakeGenericMethod( entityType.ClrType ).Invoke( dbContext, new object?[] { entityType.Name } ) as IEnumerable<object> );

        // 3. Скачиваем данные из SQL Server. 
        // Так как метод асинхронный, используем Cast<object>() перед вызовом ToListAsync
        return set?.ToList() ?? [];
      }
      catch ( Exception ex )
      {
        alert = $"Ошибка чтения данных типа '{entityType.Name}': {ex}";
        return [];
      }
    }

    async Task DeleteRow( object context )
    {
      if ( selectedType == null ) return;

      selectedType.Deleting = true;
      try
      {
        dbContext.Remove( context );

        await dbContext.SaveChangesAsync();
      }
      catch(Exception ex)
      {
        alert = $"Ошибка удаления '{context}': {ex}";
      }
      finally
      {
        selectedType.Deleting = false;

        StateHasChanged();
      }

      await LoadData();
    }
  }
}
