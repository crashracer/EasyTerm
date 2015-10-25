﻿using EasyTermCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyTermViewer
{
    class TermListBox : ListView
    {
        TermListItems _Items;
        int[] _Filter;
        int _FilterSize = -1;

        public TermListBox()
        {
            VirtualMode = true;
            FullRowSelect = true;
        }



        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void Initialize(TermListItems items)
        {
            if (Columns.Count == 0)
                Columns.Add("Term");
            Columns[0].Width = 200;

            FullRowSelect = true;

            _Items = items;
            _Filter = null;
            _FilterSize = -1;

            UpdateDisplay();

        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void Filter(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                _FilterSize = -1;
                UpdateDisplay();
                return;
            }

            if (_Filter == null)
                _Filter = new int[_Items.Count];

            _FilterSize = 0;

            for (int i = 0; i < _Items.Count; i++)
            {
                TermListItem item = _Items[i];

                if (item.Term.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) < 0)
                    continue;

                _Filter[_FilterSize++] = i;
            }


            UpdateDisplay();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        void UpdateDisplay()
        {
            if (_Filter != null && _FilterSize >= 0)
                VirtualListSize = _FilterSize;
            else
                VirtualListSize = _Items.Count;

            _LVICache = null;
            Invalidate();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inx"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        TermListItem GetItemAt(int inx)
        {
            if (inx < 0)
                return null;

            if (_Filter != null && _FilterSize >= 0)
            {
                if (inx >= _FilterSize)
                    return null;

                inx = _Filter[inx];
            }

            if (inx >= _Items.Count)
                return null;

            return _Items[inx];

        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inx"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        ListViewItem CreateLVItemAt(int inx)
        {
            ListViewItem item = new ListViewItem("");

            TermListItem termitem = GetItemAt(inx);
            if (termitem != null)
            {
                item.SubItems[0].Text = termitem.Term;
            }

            return item;

        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {

            if (_LVICache != null && e.ItemIndex >= _LVIFirstItem && e.ItemIndex < _LVIFirstItem + _LVICache.Length)
            {
                e.Item = _LVICache[e.ItemIndex - _LVIFirstItem];
            }
            else
            {
                e.Item = CreateLVItemAt(e.ItemIndex);
            }
        }

        ListViewItem[] _LVICache;
        int _LVIFirstItem;

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        protected override void OnCacheVirtualItems(CacheVirtualItemsEventArgs e)
        {
            //We've gotten a request to refresh the cache.
            //First check if it's really necessary.
            if (_LVICache != null && e.StartIndex >= _LVIFirstItem && e.EndIndex <= _LVIFirstItem + _LVICache.Length)
            {
                //If the newly requested cache is a subset of the old cache, 
                //no need to rebuild everything, so do nothing.
                return;
            }

            //Now we need to rebuild the cache.
            _LVIFirstItem = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1; //indexes are inclusive
            _LVICache = new ListViewItem[length];

            //Fill the cache with the appropriate ListViewItems.
            for (int i = 0; i < length; i++)
            {
                _LVICache[i] = CreateLVItemAt(_LVIFirstItem + i);
            }

        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bNext"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void SelectNextItem(bool bNext)
        {
            if (VirtualListSize == 0)
                return;

            if (SelectedIndices.Count == 0)
            {
                SelectedIndices.Add(0);
                return;
            }

            int sel = SelectedIndices[0];

            if (bNext)
            {
                if (sel >= VirtualListSize - 1)
                    return;
                sel++;
            }
            else
            {
                if (sel <= 0)
                    return;
                sel--;
            }

            SelectedIndices.Clear();
            SelectedIndices.Add(sel);
            EnsureVisible(sel);
        }
    }
}
