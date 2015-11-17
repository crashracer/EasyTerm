﻿using EasyTermCore;
using Pass.AddIn.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnEasyTerm
{
    public class EasyTermTranslations : Pass.AddIn.Framework.Translations
    {
        EasyTermAddInComponent _AddInComponent = null;
        TermBaseSet _TermBaseSet = null;
        TermBaseQuery _Query = null;

        public EasyTermTranslations()
        {
            
        }

        // ********************************************************************************
        /// <summary>                       
        /// 
        /// </summary>
        /// <param name="transData"></param>
        /// <returns></returns>
        /// <created>UPh,07.11.2015</created>
        /// <changed>UPh,07.11.2015</changed>
        // ********************************************************************************
        public override uint GetTranslateData(CPAITranslateData transData)
        {
            transData.Type = TranslationType.Terminology;

            
            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,07.11.2015</created>
        /// <changed>UPh,07.11.2015</changed>
        // ********************************************************************************
        void InitializeTermbase()
        {
            _AddInComponent = AddInInstance as EasyTermAddInComponent;
            if (_AddInComponent == null)
                return;



            _TermBaseSet = _AddInComponent._TermBaseSet;
            _Query = _TermBaseSet.Query;

            _Query.TerminologyResult += Query_TerminologyResult;
        }


        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        /// <created>UPh,14.11.2015</created>
        /// <changed>UPh,14.11.2015</changed>
        // ********************************************************************************
        void Query_TerminologyResult(object sender, TerminologyResultArgs e)
        {
            string origin = string.Concat("EasyTerm:", e.Origin);

            _AddInComponent.ApplicationTools.SendTerminology(PAIAddIn, (int) e.RequestID, e.FindFrom, e.FindLen, e.Term1, e.Term2, origin, e.Description);
        }

        int _LCID1 = -1;
        int _LCID2 = -1;

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// <created>UPh,17.11.2015</created>
        /// <changed>UPh,17.11.2015</changed>
        // ********************************************************************************
        private bool PrepareRequest(CPAITranslations trans)
        {
            if (_TermBaseSet == null)
                InitializeTermbase();

            if (_TermBaseSet == null)
                return false;

            int lcid1 = -1;
            int lcid2 = -1;
            trans.GetLanguages(ref lcid1, ref lcid2);

            if (_LCID1 != lcid1 || _LCID2 != lcid2)
            {
                _Query.SetLanguagePair(lcid1, lcid2);
                _LCID1 = lcid1;
                _LCID2 = lcid2;
            }

            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lcid1"></param>
        /// <param name="lcid2"></param>
        /// <returns></returns>
        /// <created>UPh,17.11.2015</created>
        /// <changed>UPh,17.11.2015</changed>
        // ********************************************************************************
        private bool PrepareRequest(int lcid1, int lcid2)
        {
            if (_TermBaseSet == null)
                InitializeTermbase();

            if (_TermBaseSet == null)
                return false;

            if (_LCID1 != lcid1 || _LCID2 != lcid2)
            {
                _Query.SetLanguagePair(lcid1, lcid2);
                _LCID1 = lcid1;
                _LCID2 = lcid2;
            }

            return true;

        }

        
        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="trans"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        /// <created>UPh,07.11.2015</created>
        /// <changed>UPh,07.11.2015</changed>
        // ********************************************************************************
        public override uint GetTerminology(string str, CPAITranslations trans, long cookie)
        {
            if (!PrepareRequest(trans))
                return 1;

            _Query.RequestTerminology(str, cookie);

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// <created>UPh,16.11.2015</created>
        /// <changed>UPh,16.11.2015</changed>
        // ********************************************************************************
        public override uint GetSyncTerminology(string str, CPAITranslations trans)
        {
            if (!PrepareRequest(trans))
                return 1;


            List<TerminologyResultArgs> results = _Query.RequestSyncTerminology(str, 0);

            foreach (TerminologyResultArgs e in results)
            {
                trans.AddTerminology(e.FindFrom, e.FindLen, e.Term1, e.Term2, e.Origin, e.Description);
            }

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// <created>UPh,17.11.2015</created>
        /// <changed>UPh,17.11.2015</changed>
        // ********************************************************************************
        public override uint SearchTerminology(string term, CPAITranslations trans)
        {
            if (!PrepareRequest(trans))
                return 1;

            List<TerminologyResultArgs> results = _Query.RequestSyncSingleTerm(term, 0);

            foreach (TerminologyResultArgs e in results)
            {
                trans.AddTerminology(e.FindFrom, e.FindLen, e.Term1, e.Term2, e.Origin, e.Description);
            }

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="lang1"></param>
        /// <param name="lang2"></param>
        /// <returns></returns>
        /// <created>UPh,16.11.2015</created>
        /// <changed>UPh,16.11.2015</changed>
        // ********************************************************************************
        public override uint LookupTerminology(string str, int lcid1, int lcid2)
        {
            if (!PrepareRequest(lcid1, lcid2))
                return 1;

            List<TermInfo> items = _Query.RequestSyncTermInfos(str);
            if (items == null || items.Count == 0)
                return 1;

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// Called when application exits
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,08.11.2015</created>
        /// <changed>UPh,08.11.2015</changed>
        // ********************************************************************************
        internal void StopRequests()
        {
            _Query.StopRequests();
        }
    }
}
