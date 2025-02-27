using System;
using System.Collections.Generic;

namespace MikouTools.Collections.Specialized.MultiLevelCascadeFilterSort
{
    /// <summary>
    /// MultiLevelCascadeCollectionBase の公開APIを抽象化したインターフェース例
    /// </summary>
    /// <typeparam name="FilterKey">フィルターキーの型</typeparam>
    /// <typeparam name="ItemValue">コレクションに格納するアイテムの型</typeparam>
    /// <typeparam name="TFiltered">
    /// フィルター済みビューの型。IMultiLevelCascadeFilteredView を実装している必要があります。
    /// </typeparam>
    public interface IMultiLevelCascadeCollection<FilterKey, ItemValue, TFiltered>
        where FilterKey : notnull
        where ItemValue : notnull
        where TFiltered : IMultiLevelCascadeFilteredView<FilterKey, ItemValue, TFiltered>
    {
        /// <summary>
        /// コレクション内のすべてのユニークなIDを取得します。
        /// </summary>
        IEnumerable<int> GetIDs();

        /// <summary>
        /// コレクション内のすべてのアイテムを取得します。
        /// </summary>
        IEnumerable<ItemValue> GetValues();

        /// <summary>
        /// 指定したIDのアイテムを取得または設定します。
        /// </summary>
        ItemValue this[int id] { get; set; }

        /// <summary>
        /// 新たなアイテムを追加し、そのIDを返します。
        /// </summary>
        int Add(ItemValue item);

        /// <summary>
        /// 複数のアイテムを一括で追加します。
        /// </summary>
        void AddRange(IEnumerable<ItemValue> items);

        /// <summary>
        /// 指定したアイテムを削除します。
        /// </summary>
        bool Remove(ItemValue item);

        /// <summary>
        /// 指定したIDのアイテムを削除します。
        /// </summary>
        bool RemoveId(int id);

        /// <summary>
        /// 指定したフィルターキーに対応するフィルタービューを取得します。
        /// </summary>
        TFiltered? GetFilterView(FilterKey key);

        /// <summary>
        /// 指定したフィルターキーでフィルタービューを追加します。（IComparer 版）
        /// </summary>
        void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter , IComparer<ItemValue>? comparer);

        /// <summary>
        /// 指定したフィルターキーでフィルタービューを追加します。（Comparison 版）
        /// </summary>
        void AddFilterView(FilterKey filterKey, Func<ItemValue, bool>? filter , Comparison<ItemValue> comparison);

        /// <summary>
        /// 指定したフィルターキーに対応するフィルタービューを削除します。
        /// </summary>
        void RemoveFilterView(FilterKey key);
    }


}