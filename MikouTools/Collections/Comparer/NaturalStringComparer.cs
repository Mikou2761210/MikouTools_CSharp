using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MikouTools.Collections.Comparer
{
    public class NaturalStringComparer(bool isAscending = true) : IComparer<string>
    {

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable SYSLIB1054 // コンパイル時に P/Invoke マーシャリング コードを生成するには、'DllImportAttribute' の代わりに 'LibraryImportAttribute' を使用します
        private static extern int StrCmpLogicalW(string x, string y);

#pragma warning restore SYSLIB1054 // コンパイル時に P/Invoke マーシャリング コードを生成するには、'DllImportAttribute' の代わりに 'LibraryImportAttribute' を使用します
#pragma warning restore IDE0079 // 不要な抑制を削除します
        public int Compare(string? x, string? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return 1;
            if (y is null) return -1;
            return isAscending ? StrCmpLogicalW(x, y) : StrCmpLogicalW(y, x);
        }

    }
}
