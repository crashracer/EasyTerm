﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EasyTermUnitTests")]

namespace EasyTermCore
{
    public class TermBaseSet
    {
        public string SettingsFile { get; set; }

        // Access for term base queries
        public TermBaseQuery Query{get; private set;}

        internal TermBases TermBases {get; private set;}


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        public TermBaseSet()
        {
            Files = new List<TermBaseFile>();
            TermBases = new TermBases();
            Query = new TermBaseQuery(this);
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,28.08.2015</created>
        /// <changed>UPh,28.08.2015</changed>
        // ********************************************************************************
        public void ClearData()
        {
            Files.Clear();
        }

        // ********************************************************************************
        /// <summary>
        /// Find a TermBaseFile by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        internal TermBaseFile FindTermBaseID(int id)
        {
            foreach (TermBaseFile file in Files)
            {
                if (file.ID == id)
                    return file;
            }

            return null;
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termbaseID"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        public Color GetDisplayColor(int termbaseID)
        {
            TermBaseFile file = FindTermBaseID(termbaseID);
            if (file != null)
                return file.DisplayColor;

            return Color.Empty;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="termbaseID"></param>
        /// <returns></returns>
        /// <created>UPh,01.11.2015</created>
        /// <changed>UPh,01.11.2015</changed>
        // ********************************************************************************
        public string GetDisplayName(int termbaseID)
        {
            TermBaseFile file = FindTermBaseID(termbaseID);
            if (file != null)
                return file.DisplayName;

            return "";
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <created>UPh,28.08.2015</created>
        /// <changed>UPh,28.08.2015</changed>
        // ********************************************************************************
        public void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(SettingsFile, settings))
            {
                writer.WriteStartElement("termbase_set");

                writer.WriteStartElement("termbases");

                foreach (TermBaseFile file in Files)
                {
                    writer.WriteStartElement("file");
                    writer.WriteAttributeString("path", file.StoragePath);
                    writer.WriteAttributeString("active", file.Active ? "true" : "false");

                    string attColor = string.Format("#{0:X2}{1:X2}{2:X2}",
                        file.DisplayColor.R,
                        file.DisplayColor.G,
                        file.DisplayColor.B);
                    writer.WriteAttributeString("display_color", attColor);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <created>UPh,28.08.2015</created>
        /// <changed>UPh,28.08.2015</changed>
        // ********************************************************************************
        private void Load()
        {
            ClearData();
            if (!File.Exists(SettingsFile))
                return;

            using (XmlReader reader = XmlReader.Create(SettingsFile))
            {
                CXmlNode elRoot = new CXmlNode();
                if (!reader.ReadRootElement(ref elRoot))
                    return;

                CXmlNode elChild = new CXmlNode();
                while (elRoot.ReadChildNode(ref elChild))
                {
                    if (elChild.IsElement("termbases"))
                    {
                        CXmlNode elTermBase = new CXmlNode();
                        while (elChild.ReadChildNode(ref elTermBase, "file"))
                        {
                            string path;
                            bool active;
                            if (elTermBase.GetAttribute("path", out path) &&
                                elTermBase.GetBoolAttribute("active", out active))
                            {
                                TermBaseFile file = new TermBaseFile(path);
                                file.Active = active;
                                Files.Add(file);

                                Color displayColor;
                                if (elTermBase.GetColorAttribute("display_color", out displayColor))
                                    file.DisplayColor = displayColor;

                            }

                        }
                    }
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,29.08.2015</created>
        /// <changed>UPh,29.08.2015</changed>
        // ********************************************************************************
        public void LoadStoredAndLocal()
        {
            Load();
            if (AddLocalFiles())
                Save();

            UpdateTermBases();
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,29.08.2015</created>
        /// <changed>UPh,29.08.2015</changed>
        // ********************************************************************************
        private bool AddLocalFiles()
        {
            string localdir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            bool bChanged = false;

            List<string> searchfolders = new List<string>();
            for(int n = 1; n < 8; n++)
            {
                string pathvar = string.Format("TermBaseFolder{0}", n);
                string path = ConfigurationManager.AppSettings[pathvar];
                if (string.IsNullOrEmpty(path))
                    break;



                string searchpath;
                if (path.StartsWith("."))
                    searchpath = Path.GetFullPath(Path.Combine(localdir, path));
                else
                    searchpath = path;

                searchfolders.Add(searchpath);

                Environment.SetEnvironmentVariable(pathvar, searchpath);
            }

            // Hold local files to find which one can be deleted
            List<TermBaseFile> localFiles = new List<TermBaseFile>();
            foreach (TermBaseFile file in Files)
            {
                if (file.IsAutomatic)
                    localFiles.Add(file);
            }

            // Loop directories where we expect termbases
            for (int iRelDir = 0; iRelDir < searchfolders.Count; iRelDir++)
            {
                string reldir = searchfolders[iRelDir];

                string searchpath;
                if (reldir.StartsWith("."))
                    searchpath = Path.GetFullPath(Path.Combine(localdir, reldir));
                else
                    searchpath = reldir;
                if (!Directory.Exists(searchpath))
                    continue;

                // Collect files in that directory
                var filteredFiles = Directory
                    .EnumerateFiles(searchpath)
                    .Where(file => string.Compare(Path.GetExtension(file), ".tbx", true) == 0 ||
                                   string.Compare(Path.GetExtension(file), ".sdltb", true) == 0 || 
                                   string.Compare(Path.GetExtension(file), ".csv", true) == 0)
                    .ToList();

                // Loop files
                foreach (string filepath in filteredFiles)
                {
                    string storagePath = string.Format("%TermBaseFolder{0}%\\{1}", iRelDir + 1,
                        Path.GetFileName(filepath));
                    
                    TermBaseFile existingFile = FindStoragePath(storagePath);
                    if (existingFile == null)
                    {
                        // Add new file
                        existingFile = new TermBaseFile(storagePath);
                        existingFile.Active = true;
                        Files.Add(existingFile);
                        bChanged = true;
                    }
                    else
                    {
                        // Remove from localFiles
                        int inx = localFiles.IndexOf(existingFile);
                        if (inx >= 0)
                            localFiles.RemoveAt(inx);
                    }
                }
            }

            // Remove remaining local files
            foreach (TermBaseFile file in localFiles)
            {
                int inx = Files.IndexOf(file);
                if (inx >= 0)
                {
                    Files.RemoveAt(inx);
                }
            }

            return bChanged;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <created>UPh,29.08.2015</created>
        /// <changed>UPh,29.08.2015</changed>
        // ********************************************************************************
        TermBaseFile FindStoragePath(string path)
        {
            foreach (TermBaseFile file in Files)
            {
                if (string.Compare(file.StoragePath, path, true) == 0)
                    return file;
            }

            return null;
        }

        internal List<TermBaseFile> Files { get; private set; }



        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        public bool EditTermBases()
        {
            Query.PauseRequests();
            Query.ResetIndex();

            bool bDataChanged = false;

            try
            {
                using (TermBaseSelectionForm form = new TermBaseSelectionForm(this))
                {
                    form.ShowDialog();
                    bDataChanged = form.DataChanged;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Query.ResetIndex();
                Query.ResumeRequests();
                UpdateTermBases();
            }

            return bDataChanged;
        }

        // ********************************************************************************
        /// <summary>
        /// Updates the actual termbase list to be in sync with this TermBaseFile list
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        void UpdateTermBases()
        {
            TermBases.Update(this, Query.LCID1, Query.LCID2);
        }

    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    // --------------------------------------------------------------------------------
    internal class TermBaseFile
    {
        static int NextID = 1;

        public TermBaseFile(string storagePath)
        {
            ID = NextID++;
            StoragePath = storagePath;
            DisplayColor = Color.White;
        }

        public int ID {get; private set;}
        public string StoragePath {get; private set;}              // Path of file, starts with %local% if loaded automatically
        public string FilePath
        {
            get
            {
                string path = Environment.ExpandEnvironmentVariables(StoragePath);
                return path;
            }
        }
        public bool Active { get; set; }    // True = file is active
        public bool IsAutomatic
        {
            get { return StoragePath.StartsWith("%TermBaseFolder", StringComparison.InvariantCultureIgnoreCase); }
        }

        public Color DisplayColor { get; set; }

        public string DisplayName
        {
            get
            {
                return Path.GetFileName(StoragePath);
            }
        }

        public string OpenError{get; internal set;}
    }
}
