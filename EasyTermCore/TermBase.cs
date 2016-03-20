﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTermCore
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Abstract base class for all term bases
    /// </summary>
    // --------------------------------------------------------------------------------
    abstract internal class TermBase
    {
        internal TermBaseFile File {get; set;}

        internal abstract void OnOpenFile();
        internal abstract void OnCloseFile();
        internal abstract List<int> GetLanguages();
        internal abstract void InitLanguagePair(int lcid1, int lcid2);
        internal abstract bool HasLanguagePair();
        internal abstract void GetTermList(TermListItems items, IAbortTermQuery abort, bool bTargetLanguage);
        internal abstract bool GetTermInfo(int termID, out TermInfo info, IAbortTermQuery abort);
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// List of active term bases
    /// </summary>
    // --------------------------------------------------------------------------------
    internal class TermBases : List<TermBase>
    {
        // ********************************************************************************
        /// <summary>
        /// Syncs this TermBase list with the TermBaseSet
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void Update(TermBaseSet set, int lcid1, int lcid2)
        {
            TermBases newList = new TermBases();

            foreach (TermBaseFile file in set.Files)
            {
                file.OpenError = "";

                if (!file.Active)
                    continue;

                // Check if we already have this file
                TermBase tb = FindTermBase(file);
                if (tb != null)
                {
                    // Add existing termbase
                    newList.Add(tb);
                    Remove(tb);
                }                
                else
                {
                    // Add new termbase
                    tb = TermBaseFactory.CreateTermBase(file);
                    if (tb != null)
                    {
                        newList.Add(tb);
                        tb.InitLanguagePair(lcid1, lcid2);
                    }
                }

            }

            // Copy new list
            Clear();
            AddRange(newList);
        }

        // ********************************************************************************
        /// <summary>
        /// Find a term base given by the attached TermBaseFile
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal TermBase FindTermBase(TermBaseFile file)
        {
            foreach (TermBase tb in this)
            {
                if (tb.File  == file)
                    return tb;
            }

            return null;
        }

        // ********************************************************************************
        /// <summary>
        /// Find a termbase given by it's ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <created>UPh,31.10.2015</created>
        /// <changed>UPh,31.10.2015</changed>
        // ********************************************************************************
        public TermBase FindTermBase(int id)
        {
            foreach (TermBase tb in this)
            {
                if (tb.File.ID == id)
                    return tb;
            }

            return null;
        }
    }


    // --------------------------------------------------------------------------------
    /// <summary>
    /// Creates the actual term bases
    /// </summary>
    // --------------------------------------------------------------------------------
    class TermBaseFactory
    {
        // ********************************************************************************
        /// <summary>
        /// Create the correct term base instance, depending on the file extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        static public TermBase CreateTermBase(TermBaseFile file)
        {
            try
            {
                file.OpenError = "";
                TermBase termBase = null;

                string ext = Path.GetExtension(file.StoragePath);

                if (string.Compare(ext, ".csv", true) == 0)
                {
                    termBase = new TermBaseCSV();
                }
                else if (string.Compare(ext, ".tbx", true) == 0)
                {
                    termBase = new TermBaseTBX();
                }
                else if (string.Compare(ext, ".sdltb", true) == 0)
                {
                    termBase = new TermBaseDB();
                }

                if (termBase != null)
                {
                    termBase.File = file;
                    termBase.OnOpenFile();
                }
                else
                {
                    file.OpenError = string.Format("Unknown term base type: {0}", ext);
                }

                return termBase;

            }
            catch (Exception ex)
            {
                file.OpenError = ex.Message;
                return null;
            }
        }
    }

}
