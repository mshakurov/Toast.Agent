
using Microsoft.AspNetCore.Components;

using Toast.Core.Utilities;
using Toast.Server.Data;

namespace Toast.Server.Components.Pages
{
  public partial class DBTables
  {
    [Parameter]
    public Func<Microsoft.EntityFrameworkCore.Metadata.IEntityType, Type?>? GetRendererType { get; set; }

    Microsoft.EntityFrameworkCore.Metadata.IEntityType[] types = [];
    string? alert = null;
    SelectedInfo? selectedType;
    bool metadataPropertiesShow = false;

    class SelectedInfo
    {
      public Microsoft.EntityFrameworkCore.Metadata.IEntityType Type { get; set; }

      public Microsoft.EntityFrameworkCore.Metadata.IProperty[] Properties { get; set; } = [];

      public Microsoft.EntityFrameworkCore.Metadata.IForeignKey[] ReferencingForeignKeys { get; set; } = [];

      public Microsoft.EntityFrameworkCore.Metadata.IForeignKey[] ForeignKeys { get; set; } = [];

      public object[] Rows { get; internal set; } = [];

      public string[] GetPropNames() => Rows.Length == 0 ? Properties.Select( p => p.Name ).ToArray() : Rows[0].GetType().GetProperties().Select( p => p.Name ).ToArray();

      public bool Deleting { get; set; }

      public object? DeletingRow { get; set; }

      public Type? ComponentType { get; private set; }

      public SelectedInfo( Microsoft.EntityFrameworkCore.Metadata.IEntityType selectedType, Func<Microsoft.EntityFrameworkCore.Metadata.IEntityType, Type?>? getRendererType )
      {
        Type = selectedType;

        ComponentType = getRendererType?.Invoke( Type );

        Properties = Type.GetProperties().ToArray();

        ReferencingForeignKeys = Type.GetReferencingForeignKeys().ToArray();

        ForeignKeys = Type.GetDerivedForeignKeys().Concat( Type.GetForeignKeys() ).ToArray();
      }
    }

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
        using var dbContext = await dbFactory.CreateDbContextAsync();

        types = dbContext.Model.GetEntityTypes().ToArray();

        if ( types.Length > 0 )
        {
          if ( selectedType == null )
          {
            if ( commandService.Current.SelectedDBTablesTypeFullName == null )
              selectedType = new SelectedInfo( types[0], GetRendererType );
            else
              selectedType = new SelectedInfo( types.FirstOrDefault( t => t.ClrType.FullName == commandService.Current.SelectedDBTablesTypeFullName ) ?? types[0], GetRendererType );
          }
          else
          {
            selectedType = new SelectedInfo( types.FirstOrDefault( t => t.ClrType.FullName == selectedType.Type.ClrType.FullName ) ?? types[0], GetRendererType );
          }
          commandService.Current.SelectedDBTablesTypeFullName = selectedType.Type.ClrType.FullName;
        }
        else
          selectedType = null;

        alert = null;
      }
      catch ( Exception ex )
      {
        alert = $"Ошибка чтения списка типов: {ex.GetFullMessage()}";
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
        selectedType = new SelectedInfo( sel, GetRendererType );
        commandService.Current.SelectedDBTablesTypeFullName = selectedType.Type.ClrType.FullName;

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

    async Task<List<object>> GetAllEntitiesAsync( Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType )
    {
      // 1. Извлекаем реальный System.Type из метаданных EF Core
      Type clrType = entityType.ClrType;

      using var dbContext = await dbFactory.CreateDbContextAsync();

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
        alert = $"Ошибка чтения данных типа '{entityType.Name}': {ex.GetFullMessage()}";
        return [];
      }
    }

    async Task DeleteRow( object context )
    {
      if ( selectedType == null ) return;

      selectedType.DeletingRow = null;
      selectedType.Deleting = true;
      StateHasChanged();
      try
      {
        using var dbContext = await dbFactory.CreateDbContextAsync();

        var key = selectedType.Type.FindPrimaryKey();

        if ( key != null )
        {
          var keys = key.Properties.Select( p => p.PropertyInfo?.GetValue( context ) ).ToArray();
          if ( keys.Length > 0 )
          {
            var obj = dbContext.Find( selectedType.Type.ClrType, keys );
            if ( obj != null )
              context = obj;
          }
        }

        dbContext.Remove( context );

        var props = dbContext.GetType().GetProperties().Where( p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof( Microsoft.EntityFrameworkCore.DbSet<> ) /*&& p.PropertyType.GenericTypeArguments[0] == selectedType.Type.ClrType*/ ).ToArray();

        await dbContext.SaveChangesAsync();
      }
      catch ( Exception ex )
      {
        alert = $"Ошибка удаления '{context}': {ex.GetFullMessage()}";
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
