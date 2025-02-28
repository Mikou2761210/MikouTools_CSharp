using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// MultiLevelCascadeFilteredViewBase の公開APIを抽象化したインターフェース例
    /// </summary>
    /// <typeparam name="FilterKey">フィルターキーの型</typeparam>
    /// <typeparam name="ItemValue">コレクションに格納するアイテムの型</typeparam>
    /// <typeparam name="TFiltered">
    /// フィルター済みビューの型。IMultiLevelCascadeFilteredView を実装している必要があります。
    /// </typeparam>
    public interface IMultiLevelCascadeFilteredView<FilterKey, ItemValue, TFiltered> : IEnumerable<ItemValue>
        where FilterKey : notnull
        where ItemValue : notnull
        where TFiltered : IMultiLevelCascadeFilteredView<FilterKey, ItemValue, TFiltered>
    {
        /// <summary>
        /// インデクサで指定したインデックスのアイテムを取得します。
        /// </summary>
        ItemValue this[int index] { get; set; }

        /// <summary>
        /// ビュー内のアイテム数を取得します。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// デフォルトの比較方法でソートします。
        /// </summary>
        bool Sort();

        /// <summary>
        /// 指定の IComparer を用いてソートします。
        /// </summary>
        bool Sort(IComparer<ItemValue>? comparer);

        /// <summary>
        /// 指定の範囲を指定の IComparer でソートします。
        /// </summary>
        bool Sort(int index, int count, IComparer<ItemValue>? comparer);

        /// <summary>
        /// 指定の Comparison デリゲートを用いてソートします。
        /// </summary>
        bool Sort(Comparison<ItemValue> comparison);


        /// <summary>
        /// 最後に使用したソート方法で再ソートを実行します。
        /// </summary>
        bool RedoLastSort();

        /// <summary>
        /// 自身と子のフィルタービューすべてに対して再ソートを実行します。
        /// </summary>
        bool RedoLastSortRecursively();

        /// <summary>
        /// フィルター関数を変更し、ビューの更新を行います。
        /// </summary>
        bool ChangeFilter(Func<ItemValue, bool>? filterFunc);

        /// <summary>
        /// 指定したフィルターキーに対応する子フィルタービューを取得します。
        /// </summary>
        TFiltered? GetFilterView(FilterKey filterName);

        /// <summary>
        /// 指定したフィルターキーで子フィルタービューを追加します。（IComparer 版）
        /// </summary>
        void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter, IComparer<ItemValue>? comparer);

        /// <summary>
        /// 指定したフィルターキーで子フィルタービューを追加します。（Comparison 版）
        /// </summary>
        void AddFilterView(FilterKey filterName, Func<ItemValue, bool>? filter , Comparison<ItemValue> comparison);

        /// <summary>
        /// 指定したフィルターキーに対応する子フィルタービューを削除します。
        /// </summary>
        void RemoveFilterView(FilterKey filterName);
    }
}
