// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using NStack;
using Terminal.Gui;

namespace OutGridView.Cmdlet
{
    internal class GridViewDataSource : IListDataSource
    {
        public List<GridViewRow> GridViewRowList { get; set; }

        public int Count => GridViewRowList.Count;

        public GridViewDataSource(List<GridViewRow> itemList)
        {
            GridViewRowList = itemList;
        }

        public bool IsMarked(int item) => GridViewRowList[item].IsMarked;

        public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width)
        {
            container.Move(col, line);
            RenderUstr(driver, GridViewRowList[item].DisplayString, col, line, width);
        }

        public void SetMark(int item, bool value)
        {
            GridViewRowList[item].IsMarked = value;
        }

        void RenderUstr (ConsoleDriver driver, ustring ustr, int col, int line, int width)
        {
            int byteLen = ustr.Length;
            int used = 0;
            for (int i = 0; i < byteLen;)
            {
                (var rune, var size) = Utf8.DecodeRune(ustr, i, i - byteLen);
                var count = Rune.ColumnWidth(rune);
                if (used + count >= width) break;
                driver.AddRune(rune);
                used += count;
                i += size;
            }

            for (; used < width; used++)
            {
                driver.AddRune(' ');
            }
        }
    }
}
