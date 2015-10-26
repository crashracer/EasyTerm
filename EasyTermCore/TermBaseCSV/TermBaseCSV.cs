﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTermCore
{
    class TermBaseCSV : TermBase
    {
        FileStream _Stream;

        List<CultureInfo> _Languages;
        int _LangIndex1 = -1;
        int _LangIndex2 = -1;

        List<Tuple<string, string>>_Terms = new List<Tuple<string,string>>();


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal override void OnOpenFile()
        {
            try
            {
                _Stream = new FileStream(File.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);


                using (StreamReader reader = new StreamReader(_Stream, Encoding.Default, true, 1024, true))
                {
                    // Read first line to get languages
                    string line = reader.ReadLine();
                    ParseLanguages(line);
                }


            }
            catch (Exception)
            {
                _Stream = null;
                _Languages = null;
                                
            }                         
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang1"></param>
        /// <param name="lang2"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal override void InitLanguagePair(CultureInfo lang1, CultureInfo lang2)
        {
            int index1 = FindLanguage(lang1);
            int index2 = FindLanguage(lang2);

            _LangIndex2 = index2;

            if (_LangIndex1 != index1)
            {
                _LangIndex1 = index1;
                ParseLines();
            }
        }


        // ********************************************************************************
        /// <summary>
        /// Determine column count and language per column
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private void ParseLanguages(string line)
        {
            _Languages = new List<CultureInfo>();
            string [] fields = line.Split('\t');

            foreach (string field in fields)
            {
                CultureInfo info;

                try
                {
                    info  = CultureInfo.GetCultureInfo(field);
                }
                catch (Exception)
                {
                    info = null;
                }

                _Languages.Add(info);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        private int FindLanguage(CultureInfo lang)
        {
            int iBest = -1;
            int iRate = 0;

            for (int i = 0; i < _Languages.Count; i++)
            {
                int iRate2 = 0;
                CultureInfo lang2 = _Languages[i];

                if (lang.LCID == lang2.LCID)
                {
                    iRate2 = 2;
                }

                if (iRate2 > iRate)
                {
                    iBest = i;
                    iRate = iRate2;
                }

            }

            return iBest;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        void ParseLines()
        {
            _Terms.Clear();

            _Stream.Seek(0, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(_Stream, Encoding.Default, true, 1024, true))
            {
                for(;;)
                {
                    long pos = _Stream.Position;

                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    string [] fields = line.Split('\t');
                    if (_LangIndex1 >= fields.Length ||
                        _LangIndex2 >= fields.Length)
                        continue;

                    var tuple = new Tuple<string, string>(fields[_LangIndex1], fields[_LangIndex2]);
                    _Terms.Add(tuple);
                }   
            }

        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="bAbort"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal override void GetTermList(TermListItems items)
        {
            if (_Stream == null)
                return;      
                
            foreach (var tuple in _Terms)
            {
                TermListItem item = new TermListItem();

                item.Term = tuple.Item1;
                items.Add(item);
            }      
        }

    }
}
