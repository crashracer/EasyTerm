﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTermCore
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Access to all term base queries
    /// </summary>
    // --------------------------------------------------------------------------------
    public class TermBaseQuery : IDisposable
    {
        private TermBaseSet _TermbaseSet;
        private TermBaseQueryWorker _Worker;

        internal TermBaseQuery(TermBaseSet termbaseSet)
        {
            _TermbaseSet = termbaseSet;
            _Worker = new TermBaseQueryWorker(this,_TermbaseSet.TermBases);
        }

        internal CultureInfo Language1 {get; set;}
        internal CultureInfo Language2 { get; set; }


#region Public query functions

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang1"></param>
        /// <param name="lang2"></param>
        /// <returns></returns>
        /// <created>UPh,24.10.2015</created>
        /// <changed>UPh,24.10.2015</changed>
        // ********************************************************************************
        public void SetLanguagePair(CultureInfo lang1, CultureInfo lang2)
        {
            Language1 = lang1;
            Language2 = lang2;


            // Inform all termbases
            foreach (TermBase termbase in _TermbaseSet.TermBases)
            {
                termbase.InitLanguagePair(Language1, Language2);
            }
        
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="CultureInfo"></typeparam>
        /// <returns></returns>
        /// <created>UPh,30.10.2015</created>
        /// <changed>UPh,30.10.2015</changed>
        // ********************************************************************************
        public List<CultureInfo> GetLanguages()
        {
            List<CultureInfo> cis = new List<CultureInfo>();

            foreach (TermBase termbase in _TermbaseSet.TermBases)
            {
                List<CultureInfo> cis2 = termbase.GetLanguages();
                if (cis2 == null)
                    continue;

                foreach (CultureInfo ci2 in cis2)
                {
                    if (cis.IndexOf(ci2) < 0)
                        cis.Add(ci2);
                }

            }

            return cis;
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
        public void RequestTerminology(string word)
        {
            if (!_Worker.IsStarted)
                _Worker.Start();

            _Worker.PutRequest(TermBaseRequest.MakeTerminologyRequest(word));
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        public void RequestTermList()
        {
            if (!_Worker.IsStarted)
                _Worker.Start();

            _Worker.PutRequest(TermBaseRequest.MakeTermListRequest());
        }

        public void RequestTerm(TermListItem item)
        {
            if (!_Worker.IsStarted)
                _Worker.Start();

            _Worker.PutRequest(TermBaseRequest.MakeTermRequest(item.Term, item.TermID));
        }
        public void RequestTerm(string term)
        {

        }


        // TODO needs EventArgs
        public delegate void TerminologyResultHandler(object sender, EventArgs e);
        public event TerminologyResultHandler TerminologyResult;

        public delegate void TermListResultHandler(object sender, TermListResultArgs e);
        public event TermListResultHandler TermListResult;


        // From worker
        internal void FireTermListResult(int requestid, TermListItems items)
        {
            if (TermListResult == null)
                return;

            TermListResultArgs args = new TermListResultArgs();
            args.RequestID = requestid;
            args.Items = items;

            TermListResult(this, args);
        }
        

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void PauseRequests()
        {
            _Worker.PauseRequests();
        }

        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        internal void ResumeRequests()
        {
            _Worker.ResumeRequests();
        }
        
        
#endregion



        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <created>UPh,24.10.2015</created>
        /// <changed>UPh,24.10.2015</changed>
        // ********************************************************************************
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _bDisposed = false;
        // ********************************************************************************
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        /// <created>UPh,25.10.2015</created>
        /// <changed>UPh,25.10.2015</changed>
        // ********************************************************************************
        protected virtual void Dispose(bool disposing)
        {
            if (_bDisposed)
                return;

            if (disposing)
            {
                _Worker.Stop();

                
            }

            _bDisposed = true;

        }
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    // --------------------------------------------------------------------------------
    public class TermListItem
    {
        public int TermBaseID {get; internal set;}
        public string Term {get; internal set;}
        public long TermID {get; internal set;}

        internal TermListItem () {}
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    // --------------------------------------------------------------------------------
    public class TermListItems : List<TermListItem>
    {
        public void Add(int termbaseID, string term, long termID = 0)
        {
            TermListItem item = new TermListItem();
            item.TermBaseID = termbaseID;
            item.Term = term;
            item.TermID = termID;
            Add(item);
        }
    }

    // --------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    // --------------------------------------------------------------------------------
    public class TermListResultArgs : EventArgs
    {
        public int RequestID {get; set;}
        public TermListItems Items {get; internal set;}
    }

}
