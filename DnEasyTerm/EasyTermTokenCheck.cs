﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTermCore;
using Pass.AddIn.Core;

namespace DnEasyTerm
{
    public class EasyTermTokenCheck : Pass.AddIn.Framework.TokenCheck
    {
        EasyTermAddInComponent _AddInComponent = null;
        TermBaseSet _TermBaseSet = null;
        TermBaseQuery _Query = null;

        bool bCheckTermUsage = true;
        bool bCheckProhibitedTerms = true;



        public EasyTermTokenCheck()
        {
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="checker"></param>
        /// <returns></returns>
        /// <created>UPh,29.11.2015</created>
        /// <changed>UPh,29.11.2015</changed>
        // ********************************************************************************
        public override uint BeginCheckTokens(Pass.AddIn.Core.CPAIResource resource, Pass.AddIn.Core.CPAITokenCheck checker)
        {
            if (_AddInComponent == null)
            {
                _AddInComponent = AddInInstance as EasyTermAddInComponent;
                if (_AddInComponent == null)
                    return 0;


                _TermBaseSet = _AddInComponent._TermBaseSet;
                if (_TermBaseSet == null)
                    return 0;

                _Query = _TermBaseSet.Query;
            }            

            bCheckTermUsage       = false;
            bCheckProhibitedTerms = false;

            if (_Query == null)
                return 0;

            var stringlist = checker.GetStringList();
            if (stringlist == null)
                return 0;

            var listinfo = stringlist.GetListInfo();
            if (listinfo == null)
                return 0;

            checker.GetOption(PslConstant.TCO_CUSTOM_0, ref bCheckTermUsage);
            checker.GetOption(PslConstant.TCO_CUSTOM_1, ref bCheckProhibitedTerms);
            if (!bCheckProhibitedTerms && !bCheckTermUsage)
                return 0;

            _Query.SetLanguagePair(listinfo.Lang1, listinfo.Lang2);

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="resource"></param>
        /// <param name="checker"></param>
        /// <returns></returns>
        /// <created>UPh,29.11.2015</created>
        /// <changed>UPh,29.11.2015</changed>
        // ********************************************************************************
        public override uint CheckToken(CPAIToken token, Pass.AddIn.Core.CPAIResource resource, CPAITokenCheck checker)
        {
            if (!bCheckProhibitedTerms && !bCheckTermUsage)
                return 0;

            if (_Query == null)
                return 0;

            int nTranslated = (int) token.GetProperty(enmTokenProperties.StateTranslated);
            if (nTranslated == 0)
            {
                // Check source string
                string src = token.GetProperty(enmTokenProperties.Text, PropSelectionType.Source) as string;
 
                if (bCheckProhibitedTerms)
                {
                    DoCheckProhibitedTerm(src, token, checker);
                }

            }
            else
            {
                // Check translation string
                string src = token.GetProperty(enmTokenProperties.Text, PropSelectionType.Source) as string;
                string trn = token.GetProperty(enmTokenProperties.Text, PropSelectionType.Target) as string;

                if (bCheckTermUsage)
                {
                    DoCheckTermUsage(src, trn, token, checker);
                }

                if (bCheckProhibitedTerms)
                {
                    DoCheckProhibitedTerm(trn, token, checker);
                }
            }

            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="trn"></param>
        /// <returns></returns>
        /// <created>UPh,29.11.2015</created>
        /// <changed>UPh,29.11.2015</changed>
        // ********************************************************************************
        public void DoCheckTermUsage(string src, string trn, CPAIToken token, CPAITokenCheck checker)
        {
            RangeMap ranges = new RangeMap();

            List<TerminologyResultArgs> results = _Query.RequestSyncTerminology(src, 0);

            int _CurrentFrom = -1;
            int _CurrentLen  = -1;

            string strMissingTerm = "";

            // Results come sorted by word range. Longer sections (number of words in term) come first
            foreach (TerminologyResultArgs result in results)
            {
                if (ranges.OverlapsRange(result.FindFrom, result.FindLen))
                    continue; // Range already checked

                if (_CurrentFrom != result.FindFrom ||
                    _CurrentLen  != result.FindLen)
                {
                    if (strMissingTerm.Length > 0)
                        checker.ErrorOutput(token, string.Format("ET: Missing term {0}", strMissingTerm));

                    strMissingTerm = "";
                    _CurrentFrom = result.FindFrom;
                    _CurrentLen  = result.FindLen;
                    strMissingTerm = src.Substring(_CurrentFrom, _CurrentLen);
                }

                if (trn.IndexOf(result.Term2, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    // Term found. Mark section as ok
                    ranges.AddRange(result.FindFrom, result.FindLen);
                    strMissingTerm = "";
                }
            }

            if (strMissingTerm.Length > 0)
                checker.ErrorOutput(token, string.Format("ET: Missing term {0}", strMissingTerm));                        
        }

        // ********************************************************************************
        /// <summary>
        /// Checks target language for forbidden terms
        /// </summary>
        /// <param name="src"></param>
        /// <param name="trn"></param>
        /// <param name="token"></param>
        /// <param name="checker"></param>
        /// <created>UPh,20.03.2016</created>
        /// <changed>UPh,20.03.2016</changed>
        // ********************************************************************************
        public void DoCheckProhibitedTerm(string text, CPAIToken token, CPAITokenCheck checker)
        {
            RangeMap ranges = new RangeMap();

            List<TerminologyResultArgs> results = _Query.RequestSyncProhibitedTerminology(text, 0);

            // Results come sorted by word range. Longer sections (number of words in term) come first
            foreach (TerminologyResultArgs result in results)
            {
                if (result.Status != TermStatus.prohibited)
                    continue;

                checker.ErrorOutput(token, string.Format("ET: Prohibited term used: {0}", result.Term1));
            }
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="checker"></param>
        /// <returns></returns>
        /// <created>UPh,29.11.2015</created>
        /// <changed>UPh,29.11.2015</changed>
        // ********************************************************************************
        public override uint EndCheckTokens(Pass.AddIn.Core.CPAIResource resource, Pass.AddIn.Core.CPAITokenCheck checker)
        {
            return 0;
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="check"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <created>UPh,29.11.2015</created>
        /// <changed>UPh,29.11.2015</changed>
        // ********************************************************************************
        public override uint GetCustomCheckOption(int check, ref string text)
        {
            switch (check)
            {
                case PslConstant.TCO_CUSTOM_0:
                    text = "Term ... not used in translation";
                    return 0;

                case PslConstant.TCO_CUSTOM_1:
                text = "Forbidden term used in translation";
                    return 0;
            }

            return 1;
        }
    }

}
