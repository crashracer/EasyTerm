﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EasyTermCore;
using System.IO;
using System.Globalization;
using EasyTermViewer.Properties;
using PassLib;

namespace EasyTermViewer
{
    public partial class MainForm : Form
    {
        TermBaseSet _TermbaseSet;
        TermBaseQuery _TermBaseQuery;
        int _IgnoreNotification = 0;
        private PlStorePosition _StorePosition;

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        public MainForm()
        {
            string inipath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            inipath = Path.Combine(inipath, "EasyTermViewer\\profile.ini");
            PlProfile.SetProfileName(inipath, PlProfile.ProfileType.IniFile);
            EasyTermCoreSettings.ProfilePath = inipath;

            _StorePosition = new PlStorePosition();
            _StorePosition.Initialize(this);

            InitializeComponent();

            _TermbaseSet = new TermBaseSet();
            _TermbaseSet.SettingsFile = Environment.ExpandEnvironmentVariables("%appdata%\\EasyTermViewer\\settings.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(_TermbaseSet.SettingsFile));

            _TermbaseSet.LoadStoredAndLocal();
            _TermBaseQuery = _TermbaseSet.Query;

            _TermBaseQuery.TermListResult    += TermBaseQuery_TermListResult;
            _TermBaseQuery.TermInfoResult    += TermBaseQuery_TermInfoResult;
            _TermBaseQuery.TerminologyResult += TermBaseQuery_TerminologyResult;

            _TermListResult = new TermListResultCallback(OnTermListResult);
            _TermInfoResult = new TermInfoResultCallback(OnTermInfoResult);

            lstTerms.TermBaseSet = _TermbaseSet;

            InitializeLanguageComboBoxes();
            InitializeLanguageSelection();
        }


        delegate void TermListResultCallback(TermListResultArgs e);
        TermListResultCallback _TermListResult;

        delegate void TermInfoResultCallback(TermInfoResultArgs e);
        TermInfoResultCallback _TermInfoResult;


        // ********************************************************************************
        /// <summary>
        /// Event Handler for results on TermList query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        void TermBaseQuery_TermListResult(object sender, TermListResultArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(_TermListResult, e);
                return;     
            }
            else
            {
                OnTermListResult(e);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Event Handler for results on TermInfo query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        void TermBaseQuery_TermInfoResult(object sender, TermInfoResultArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(_TermInfoResult, e);
                return;
            }
            else
            {
                OnTermInfoResult(e);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        void TermBaseQuery_TerminologyResult(object sender, EventArgs e)
        {

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
        void OnTermListResult(TermListResultArgs e)
        {
            lstTerms.Initialize(e.Items);
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        void OnTermInfoResult(TermInfoResultArgs e)
        {
            string name = _TermbaseSet.GetDisplayName(e.TermBaseID);

            termInfoControl.SetData(name, e.Info);            
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,29.10.2015</created>
        /// <changed>UPh,29.10.2015</changed>
        // ********************************************************************************
        private void InitializeLanguageSelection()
        {
            _IgnoreNotification++;

            cmdLanguage1.Items.Clear();
            cmdLanguage2.Items.Clear();

            List<CultureInfo> cis = _TermBaseQuery.GetLanguages();

            foreach (CultureInfo ci in cis)
            {
                cmdLanguage1.Items.Add(ci);
                cmdLanguage2.Items.Add(ci);
            }

            if (cmdLanguage1.Items.Count > 0)
                cmdLanguage1.SelectedIndex = 0;
            if (cmdLanguage2.Items.Count > 1)
                cmdLanguage2.SelectedIndex = 1;
            _IgnoreNotification--;

            OnLanguageSelectionChanged();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,29.08.2015</created>
        /// <changed>UPh,29.08.2015</changed>
        // ********************************************************************************
        private void cmdTermBases_Click(object sender, EventArgs e)
        {
            _TermbaseSet.EditTermBases();

            InitializeLanguageSelection();
            InitializeTermList();
        }

        // ********************************************************************************
        /// <summary>
        /// Rebuild listbox with terms
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        void InitializeTermList()
        {
            try
            {
                lstTerms.Clear();
                _TermBaseQuery.RequestTermList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _TermBaseQuery.Dispose();
            _TermBaseQuery = null;
        }

        DateTime _LastFilterTextChange = DateTime.MinValue;

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private void txtFindTerm_TextChanged(object sender, EventArgs e)
        {
            _LastFilterTextChange = DateTime.Now;
            timerFilter.Enabled = true;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private void timerFilter_Tick(object sender, EventArgs e)
        {
            if (_LastFilterTextChange != DateTime.MinValue &&
                (DateTime.Now - _LastFilterTextChange).TotalMilliseconds >= 500)
            {
                lstTerms.Filter(txtFindTerm.Text);

                _LastFilterTextChange = DateTime.MinValue;
                timerFilter.Enabled = false;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private void txtFindTerm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                lstTerms.SelectNextItem(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                lstTerms.SelectNextItem(false);
                e.Handled = true;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,29.10.2015</created>
        /// <changed>UPh,29.10.2015</changed>
        // ********************************************************************************
        private void cmdLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IgnoreNotification > 0)
                return;

            OnLanguageSelectionChanged();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,29.10.2015</created>
        /// <changed>UPh,29.10.2015</changed>
        // ********************************************************************************
        private void OnLanguageSelectionChanged()
        {
            CultureInfo lang1 = cmdLanguage1.SelectedItem as CultureInfo;
            CultureInfo lang2 = cmdLanguage2.SelectedItem as CultureInfo;

            _TermBaseQuery.SetLanguagePair(lang1, lang2);

            InitializeTermList();
            DisplaySelectedTerm();
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,29.10.2015</created>
        /// <changed>UPh,29.10.2015</changed>
        // ********************************************************************************
        private void InitializeLanguageComboBoxes()
        {
            cmdLanguage1.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            cmdLanguage1.ComboBox.DrawItem += ComboBox_DrawItem;

            cmdLanguage2.ComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            cmdLanguage2.ComboBox.DrawItem += ComboBox_DrawItem;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,29.10.2015</created>
        /// <changed>UPh,29.10.2015</changed>
        // ********************************************************************************
        void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            if (cmb == null)
                return;

            if (e.Index < 0 || e.Index >= cmb.Items.Count)
                return;

            CultureInfo ci = cmb.Items[e.Index] as CultureInfo;
            if (ci == null)
                return;

            e.DrawBackground();
            EasyTermCore.Tools.DrawLanguageString(e.Graphics, e.Font, e.ForeColor, e.Bounds, ci);
            e.DrawFocusRectangle();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        private void lstTerms_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplaySelectedTerm();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        private void DisplaySelectedTerm()
        {
            TermListItem item = lstTerms.GetSelectedItem();
            if (item == null)
                return;

            _TermBaseQuery.RequestTermInfo(item);
        }

    }
}
