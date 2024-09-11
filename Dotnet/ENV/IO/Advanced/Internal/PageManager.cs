using System;
using System.Collections.Generic;
using Firefly.Box.Printing;

namespace ENV.IO.Advanced.Internal
{
    class PageManager
    {
        Func<bool> _autoNewPage;
        public PageManager(Func<bool> autoNewPage)
        {
            _autoNewPage = autoNewPage;
        }

        bool _pageStarted = false;
        double CurrentPageOfset
        {
            get
            {
                return _lastHeightAdded() + _knownHeight;
            }
        }
        bool _closed = false;
        public void Close()
        {
            _closed = true;
            ClosePreviousPage();
            _pageStarted = false;
        }
        int _pageHeight = 60;
        public void SetPageHeight(int val)
        {
            _pageHeight = val;
            if (val == 0)
                _pageHeight = int.MaxValue;
        }

        bool _pageClosedWithoutNewPage = false;
        void ClosePreviousPage()
        {
            if (!_pageStarted || _pageClosedWithoutNewPage) return;
            PageEnded = true;
            _inNewPage = true;
            
            PageFooters();
            _inNewPage = false;
            _pageClosedWithoutNewPage = true;
        }
        bool _inNewPage = false;
        double _knownHeight;
        Func<double> _lastHeightAdded = () => 0;
        public void Print(double height,Action doPrint)
        {
            Print(() => height, doPrint);
        }
        bool _printingNewPage = false;
        internal void Print(Func<double> height,Action doPrint)
        {
            if (_suspendAll)
                return;
            if (!_inTheMiddle&&( _autoNewPage() && (height() > HeightUntilEndOfPage + 0.2 && !_inNewPage || _newPageOnNextWrite)))
            {
                _newPageOnNextWrite = false;
                if (_printingNewPage)
                {
                }
                else
                {
                    _printingNewPage = true;
                    try
                    {
                        NewPageDueToLackOfHeight();
                        CheckIfPageStarted();
                    }
                    finally
                    {
                        _printingNewPage = false;
                    }
                }
            }
            else
                CheckIfPageStarted();
            _inNewPage = false;
            doPrint();
            _knownHeight = CurrentPageOfset;
            _lastHeightAdded = height;
        }
        
        void NewPageDueToLackOfHeight()
        {
            if (!_pageStarted)
                return;
            NewPage();
        }

        int _pageNumber = 0;
        
       

        internal void CheckIfPageStarted()
        {
            if (_pageStarted) return;

            _pageNumber++;
            _pageStarted = true;
            _anythingWasPrinted = true;
            PageEnded = false;
            PageHeaders();
            _inNewPage = true;
        }
        bool _anythingWasPrinted = false;
        internal bool AnythingWasEverPrinted()
        {
            return _anythingWasPrinted;
        }

        public void NewPage()
        {
            
            if (!_pageStarted||_inNewPage||Height==0)
                return;
            
            ClosePreviousPage();
            _pageStarted = false;
            _pageClosedWithoutNewPage = false;
            _knownHeight = 0;
            _lastHeightAdded = () => 0;
        }

        public int PageCount
        {
            get
            {
                return _pageNumber + (!_pageStarted ? 1 : 0);
            }
        }

        public double HeightUntilEndOfPage
        {
            get { return _pageHeight - (CurrentPageOfset > 0 ? CurrentPageOfset : PageHeaderHeight) - PageFooterHeight; }
        }


        public double Height
        {
            get
            {
                return CurrentPageOfset;
            }
        }

        public int PageHeight
        {
            get { return _pageHeight; }
        }


        bool _printingHeaders = false;
        void PageHeaders()
        {
            if (NewPageEvent != null)
                NewPageEvent();
        }

        public event Action NewPageEvent;
        public event EndPageDelegate PageEnds;
        public int PageFooterHeight { get; set; }

        public int PageHeaderHeight { get; set; }

        bool _suspendAll = false;
        void PageFooters()
        {
            if (PageFooterHeight == 0)
                return;
            var x = _inTheMiddle;
            _inTheMiddle = true;
            try {
                _suspendAll = true; try
                {
                    Firefly.Box.Context.Current.RunExpressionCommands();
                }
                finally
                {
                    _suspendAll = false;
                }
                if (PageEnds != null)
                    PageEnds(_closed);
            }
            finally { _inTheMiddle = x; }
        }
        public bool PageEnded { get; private set; }
        public void EndCurrentPageWithoutStartingANewOne()
        {
            if (!_pageStarted || _inNewPage || Height == 0)
                return;

            ClosePreviousPage();
        }
        bool _newPageOnNextWrite = false;
        public bool NewPageOnNextWrite

        {set
            {
                _newPageOnNextWrite = value;
            }
            get {
                return _newPageOnNextWrite;
            }
        }
        bool _inTheMiddle = false;
        internal void AboutToPrint(double sectionHeight, Action newPageStartedDueToLackOfSpace, bool autoNewPage)
        {
            if (_inTheMiddle)
                return;
            _inTheMiddle = true;
            try
            {
                if (sectionHeight > HeightUntilEndOfPage + 0.2 && Height > 0)
                {
                    if (autoNewPage)
                    {
                        newPageStartedDueToLackOfSpace();
                        CheckIfPageStarted();
                    }
                    else
                        EndCurrentPageWithoutStartingANewOne();
                }
                else
                    CheckIfPageStarted();
            }
            finally
            {
                _inTheMiddle = false;
            }
        }

     
    }
}