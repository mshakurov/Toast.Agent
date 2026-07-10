using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TableBase<TItem> : ComponentBase
{
  [Parameter]
  public RenderFragment? TableHeader { get; set; }
  [Parameter]
  public RenderFragment<TItem>? TableRow { get; set; }
  [Parameter]
  public RenderFragment<TItem>? AfterRow { get; set; }
  [Parameter]
  public TItem[]? Items { get; set; }
  [Parameter]
  public int PageSize { get; set; } = 5;
  [Parameter]
  public int PagerSize { get; set; } = 6;
  [Parameter]
  public Func<int, TItem[], Task<TItem[]>>? OverridePageItemsAsync { get; set; }
  [Parameter]
  public TItem? LocateToItem { get; set; }
  [Parameter]
  public Func<TItem, TItem, bool>? ItemComparer { get; set; }
  [Parameter]
  public RenderFragment? ItemsNotFoundText { get; set; }
  [Parameter]
  public bool Loading { get; set; }
  [Parameter]
  public RenderFragment? ItemsLoadingText { get; set; }

  [Parameter]
  public bool Border { get; set; }

  /// <summary>
  /// Пэйлдер видимый даже если не нужен
  /// </summary>
  [Parameter]
  public bool PagerVisibleAlways { get; set; }

  /// <summary>
  /// Компактная таблица
  /// </summary>
  [Parameter]
  public bool Compact { get; set; }

  public int PagesCount { get; private set; }
  public int CurrentPage { get; private set; } = 1;
  public int PagerStartPage { get; private set; }
  public int PagerEndPage { get; private set; }
  public TItem[]? ItemList { get; private set; }

  private TItem[]? lastItems;

  protected int CurrentPageInternal { get => CurrentPage; set => SetCurrentPage( value ); }

  public string PagePrevDisabledClass => CurrentPage <= 1 ? "disabled" : string.Empty;
  public string PageNextDisabledClass => CurrentPage >= PagesCount ? "disabled" : string.Empty;
  public string PagerPrevDisabledClass => PagerStartPage <= 1 ? "disabled" : string.Empty;
  public string PagerNextDisabledClass => ( PagerEndPage >= PagesCount ) ? "disabled" : string.Empty;


  protected string CompactClass => Compact ? "compact-table" : string.Empty;

  protected bool IsPagerNeeded => Items != null && Items.Length > PageSize;


  protected override async Task OnParametersSetAsync()
  {
    await base.OnParametersSetAsync();

    if ( PageSize < 1 )
      PageSize = 5;

    PagesCount = Items != null ? ( int ) Math.Ceiling( Items.Length / ( decimal ) PageSize ) : 0;

    if ( lastItems != null && Items != null && ( object.ReferenceEquals( lastItems, Items ) || Items.SequenceEqual( lastItems ) ) )
    {

    }
    else
    {
      int curPage = 1;

      if ( LocateToItem != null )
      {
        var cmp = ItemComparer ?? EqualityComparer<TItem>.Default.Equals;

        var list = Items?.ToList() ?? new List<TItem>( 0 );

        var index = list.FindIndex( it => cmp( it, LocateToItem ) );

        if ( index >= 0 )
          curPage = ( index / PageSize ) + 1;
      }

      CurrentPage = curPage;
    }

    await UpdateCurrentPageItemList();

    UpdatePager();

    lastItems = Items;

    await Task.CompletedTask;
  }
  public async Task UpdateList( int currentPage )
  {
    CurrentPage = currentPage;

    await UpdateCurrentPageItemList();

    await Task.CompletedTask;
  }
  private async Task UpdateCurrentPageItemList()
  {
    var query = Items != null ? Items.Skip( ( CurrentPage - 1 ) * PageSize ).Take( PageSize ) : Array.Empty<TItem>();

    if ( OverridePageItemsAsync != null )
      ItemList = await OverridePageItemsAsync( CurrentPage, query.ToArray() );
    else
      ItemList = query.ToArray();
  }
  private void UpdatePager()
  {
    PagerStartPage = PagerSize > 0 ? ( ( CurrentPage / PagerSize ) * PagerSize + 1 ) : 0;
    if ( PagerStartPage + PagerSize < PagesCount )
      PagerEndPage = PagerStartPage + PagerSize - 1;
    else
      PagerEndPage = PagesCount;
  }
  public async Task SetPagerSize( string direction )
  {
    if ( direction == "forward" && PagerEndPage < PagesCount )
    {
      PagerStartPage = PagerEndPage + 1;
      if ( PagerEndPage + PagerSize < PagesCount )
      {
        PagerEndPage = PagerStartPage + PagerSize - 1;
      }
      else
      {
        PagerEndPage = PagesCount;
      }
    }
    else if ( direction == "back" && PagerStartPage > 1 )
    {
      PagerEndPage = PagerStartPage - 1;
      PagerStartPage = PagerStartPage - PagerSize;
    }
    await Task.CompletedTask;
  }
  public async Task NavigateToPage( string direction )
  {
    if ( direction == "next" )
    {
      if ( CurrentPage < PagesCount )
      {
        if ( CurrentPage == PagerEndPage )
        {
          await SetPagerSize( "forward" );
        }
        CurrentPage += 1;
      }
    }
    else if ( direction == "previous" )
    {
      if ( CurrentPage > 1 )
      {
        if ( CurrentPage == PagerStartPage )
        {
          await SetPagerSize( "back" );
        }
        CurrentPage -= 1;
      }
    }
    await UpdateList( CurrentPage );
  }
  protected override void OnAfterRender( bool firstRender )
  {
    base.OnAfterRender( firstRender );
  }

  private void SetCurrentPage( int value )
  {
    CurrentPage = value <= PagesCount ? ( value >= 1 ? value : 1 ) : PagesCount;

    _ = UpdateList( CurrentPage );

    UpdatePager();
  }
}
