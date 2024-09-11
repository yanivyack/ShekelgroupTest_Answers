using System;
using System.Collections.Generic;
using System.Text;

namespace ENV.Utilities
{
    internal class VisualTrueClass
    {
        HashSet<char> _hebrewChars;
        static int logName = 0;
        static string LogFileName()
        {
            return @"c:\temp\" + logName + ".txt";
        }
        static void StartLog(string what)
        {
            //logName++;
            //if (System.IO.File.Exists(LogFileName()))
            //    System.IO.File.Delete(LogFileName());
            //Log(what);

        }
        static void Log(object what)
        {
            //using (var sw = new System.IO.StreamWriter(LogFileName(), true))
            //    sw.WriteLine(what);
        }
        public VisualTrueClass(string source, bool alsoOem)
        {


            _hebrewChars = alsoOem ? HebrewTextTools._hebrewCharsWithOem : HebrewTextTools._hebrewChars;
            _state = new HebrewCharProcessor(this);

            StartLog("Start visual: " + source);

            foreach (char c in source)
            {
                Log("Read Char: " + c);
                if (_hebrewChars.Contains(c))
                    _state.Hebrew(c);
                else if (c == ' ')
                    _state.Space(c);
                else if (HebrewTextTools._englishChars.Contains(c))
                    _state.English(c);
                else if (HebrewTextTools._numberChars.Contains(c))
                    _state.Number(c);
                else if (HebrewTextTools._bracksChars.Contains(c))
                    _state.Brakets(c);
                else if (HebrewTextTools._numericOperators.Contains(c))
                    _state.NumericOperator(c);
                else
                    _state.Other(c);
            }
            _state.Finish();

        }
        StringBuilder _sb = new StringBuilder();
        int _position = 0;
        void AdvancePositionToCurrentIndex()
        {
            _position = _sb.Length;
        }

        interface CharProcessor
        {
            void Hebrew(char c);
            void English(char c);
            void Number(char c);
            void Space(char c);
            void Brakets(char c);
            void Other(char c);
            void NumericOperator(char c);
            void Finish();
        }
        class HebrewCharProcessor : CharProcessor
        {
            VisualTrueClass _parent;

            public HebrewCharProcessor(VisualTrueClass parent)
            {
                _parent = parent;
            }

            public void Hebrew(char c)
            {
                _parent.AddCharToText(c);
            }

            public void English(char c)
            {
                _parent.SwitchToEnglish(c);
            }

            public void Number(char c)
            {
                _parent.SwitchToNumber(c, delegate { }, delegate { }, false);
            }

            public void Space(char c)
            {
                Hebrew(c);
            }

            public void Brakets(char c)
            {
                Other(c);
            }

            public void Other(char c)
            {
                Hebrew(c);
            }

            public void NumericOperator(char c)
            {
                Other(c);
            }

            public void Finish()
            {

            }
        }
        class EnglishCharProcessor : CharProcessor
        {
            VisualTrueClass _parent;
            List<char> _chars = new List<char>();

            public EnglishCharProcessor(VisualTrueClass parent)
            {
                _parent = parent;
            }

            public void Hebrew(char c)
            {
                Finish();
                _parent.SwitchToHebrew(c);
            }

            public void English(char c)
            {
                _chars.Add(c);

            }

            public void Number(char c)
            {

                _parent.SwitchToNumber(c, delegate { },
                                       delegate (bool obj)
                                       {
                                           Finish();
                                       }, true);
            }

            public void Space(char c)
            {
                //Finish();
                _parent._state = new SpaceAfterEnglish(this);
                _parent._state.Space(c);
            }

            public void Brakets(char c)
            {

                _parent._state = new BraketsCharProcess(_parent, delegate { Finish(); }, false);
                _parent._state.Brakets(c);
            }

            public void Other(char c)
            {
                Brakets(c);
            }

            public void NumericOperator(char c)
            {
                Other(c);
            }

            public void Finish()
            {
                _parent.Insert(_chars);
            }
            class SpaceAfterEnglish : CharProcessor
            {
                EnglishCharProcessor _parent;

                public SpaceAfterEnglish(EnglishCharProcessor parent)
                {
                    _parent = parent;
                }

                List<char> _spaces = new List<char>();
                public void Hebrew(char c)
                {
                    _parent.Finish();
                    AddSpacesAtEnd();
                    _parent._parent.SwitchToHebrew(c);
                }

                public void English(char c)
                {
                    _parent._chars.AddRange(_spaces);
                    _parent._parent._state = _parent;
                    _parent._parent._state.English(c);

                }

                public void Number(char c)
                {
                    _parent.Finish();
                    _parent._parent.SwitchToNumber(c, delegate (bool reverse)
                                                          {
                                                              if (reverse)
                                                                  _spaces.Reverse();
                                                              _parent._parent.Insert(_spaces);
                                                          }, delegate { }, true);
                }

                void AddSpacesAtEnd()
                {
                    foreach (char c in _spaces)
                    {
                        _parent._parent._sb.Append(c);
                    }
                }

                public void Space(char c)
                {
                    _spaces.Add(c);
                }

                public void Brakets(char c)
                {
                    _parent.Finish();

                    _parent._parent._state = new BraketsCharProcess(_parent._parent,
                                                      delegate (bool advancePosition)
                                                      {

                                                          if (advancePosition)
                                                              _parent._parent.AdvancePositionToCurrentIndex();
                                                          if (advancePosition)
                                                              _spaces.Reverse();
                                                          _parent._parent.Insert(_spaces);
                                                      }, false);
                    _parent._parent._state.Brakets(c);


                }

                public void Other(char c)
                {
                    _spaces.Add(c);
                }

                public void NumericOperator(char c)
                {
                    Other(c);
                }

                public void Finish()
                {
                    AddSpacesAtEnd();
                    _parent.Finish();
                }
            }
        }

        class NumberCharProcessor : CharProcessor
        {
            VisualTrueClass _parent;
            Action<bool> _spacesBeforeNumber;
            Action<bool> _englishBeforeNumber;
            List<char> _chars = new List<char>();
            bool _immediateAferEnglish = false;

            public NumberCharProcessor(VisualTrueClass parent, Action<bool> spacesBeforeNumber, Action<bool> englishBeforeNumber, bool immediateAferEnglish)
            {
                _immediateAferEnglish = immediateAferEnglish;
                _parent = parent;
                _spacesBeforeNumber = spacesBeforeNumber;
                _englishBeforeNumber = englishBeforeNumber;
            }



            public void Hebrew(char c)
            {
                _englishBeforeNumber(true);

                _parent.AdvancePositionToCurrentIndex();

                AddCharsToString(true);
                _spacesBeforeNumber(true);
                _parent.SwitchToHebrew(c);
            }
            void AddCharsToString(bool reverse)
            {
                if (reverse && _chars.Count > 0)
                {
                    _chars = NumberReverser.Reverse(_chars, !_immediateAferEnglish);
                    _chars.Reverse();
                    if (_immediateAferEnglish)
                    {
                        for (int i = 0; i < _chars.Count; i++)
                        {
                            if (_chars[i] == ' ')
                                continue;
                            _chars[i] = HebrewTextTools.ReverseBracks(_chars[i]);
                            break;
                        }
                        
                    }

                }


                _parent.Insert(_chars);
            }

            public void English(char c)
            {
                _englishBeforeNumber(false);

                _spacesBeforeNumber(false);
                AddCharsToString(false);
                _parent.SwitchToEnglish(c);
            }

            public void Number(char c)
            {
                _chars.Add(c);
                _parent._state = this;

            }

            public void Space(char c)
            {
                Log("Number space");
                _chars.Add(c);
                //_parent._state = new SpaceAfterNumber(this);
                //_parent._state.Space(c);
            }


            public void Brakets(char c)
            {
                Log("number Brakets");
                _chars.Add(c);
            }

            public void Other(char c)
            {
                _chars.Add(c);
            }
            public void NumericOperator(char c)
            {
                _chars.Add(c);
                _immediateAferEnglish = false;
            }


            public void Finish()
            {
                Finish(true);
            }

            public void Finish(bool reverse)
            {
                Log("Number finish");
                _englishBeforeNumber(true);
                _parent.AdvancePositionToCurrentIndex();
                AddCharsToString(reverse);
                _spacesBeforeNumber(true);
            }
        }
        class BraketsCharProcess : CharProcessor
        {
            VisualTrueClass _parent;
            List<char> _separatorChars = new List<char>();
            Action<bool> _saveTheNumber;
            bool _reverse = false;

            public BraketsCharProcess(VisualTrueClass parent, Action<bool> saveTheNumber, bool reverse)
            {
                _parent = parent;
                _saveTheNumber = saveTheNumber;
                _reverse = reverse;
            }


            public void Hebrew(char c)
            {
                _saveTheNumber(true);
                foreach (char separatorChar in _separatorChars)
                {
                    _parent._sb.Append(HebrewTextTools.ReverseBracks(separatorChar));
                }
                _separatorChars.Clear();
                _parent.SwitchToHebrew(c);
            }



            public void English(char c)
            {
                _saveTheNumber(false);
                _parent.Insert(_separatorChars);
                _parent.SwitchToEnglish(c);
            }

            public void Number(char c)
            {

                _parent.SwitchToNumber(c, delegate { },
                                       delegate (bool obj)
                                       {
                                           _saveTheNumber(obj);
                                           if (obj)
                                           {
                                               _parent.AdvancePositionToCurrentIndex();

                                               _separatorChars.Reverse();
                                           }
                                           if (_reverse && obj)
                                               _parent.Insert(HebrewTextTools.ReverseBracks(new string(_separatorChars.ToArray())));
                                           else
                                               _parent.Insert(_separatorChars);
                                       }, false);
            }

            public void Space(char c)
            {
                _separatorChars.Add(c);
            }

            public void Brakets(char c)
            {
                _separatorChars.Add(c);
            }

            public void Other(char c)
            {
                _separatorChars.Add(c);
            }

            public void NumericOperator(char c)
            {
                Other(c);
            }

            public void Finish()
            {
                _saveTheNumber(true);
                foreach (char separatorChar in _separatorChars)
                {
                    _parent._sb.Append(HebrewTextTools.ReverseBracks(separatorChar));
                }
            }
        }
        public void SwitchToHebrew(char c)
        {
            _state = new HebrewCharProcessor(this);
            _state.Hebrew(c);

        }
        public void SwitchToEnglish(char c)
        {
            _state = new EnglishCharProcessor(this);
            _state.English(c);
        }
        void SwitchToNumber(char c, Action<bool> spacesBeforeNumber, Action<bool> englishBeforeNumber, bool immediateAferEnglish)
        {
            _state = new NumberCharProcessor(this, spacesBeforeNumber, englishBeforeNumber, immediateAferEnglish);
            _state.Number(c);
        }

        CharProcessor ___state;
        CharProcessor _state
        {
            set
            {
                Log("State changed to " + value);
                Log("Sb:" + _sb.ToString());
                ___state = value;
            }
            get { return ___state; }
        }
        

        public void AddCharToText(char c)
        {
            c = HebrewTextTools.ReverseBracks(c);
            Log("Add Char To Text:" + c);
            _sb.Append(c);
            AdvancePositionToCurrentIndex();
        }


        public override string ToString()
        {
            return _sb.ToString();
        }

        public void Insert(IEnumerable<char> chars)
        {
            foreach (char c in chars)
            {
                _sb.Insert(_position, c);
            }
        }
    }
}