using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENV.Data;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using ENV.Data.DataProvider;
using Firefly.Box;
using System.IO;

namespace ENV.Utilities
{
    public class RandomDataGenerator
    {

        Entity _entity;
        public RandomDataGenerator(Entity entity)
        {
            _entity = entity;
        }
        string GetStateFileName()
        {
            return System.IO.Path.Combine(System.Windows.Forms.Application.UserAppDataPath,
                                          "MockDataGenerator_" + _entity.GetType().FullName + ".xml");
        }
        public void Run()
        {
            var fb = new ENV.UI.FormBuilder("Generate Random Data For " + _entity.Caption);
            var c = new CastColumnSetter();
            foreach (var item in _entity.Columns)
            {
                ENV.UserMethods.CastColumn(item, c);
            }
            c.Load(GetStateFileName());


            var numberOfRows = new NumberColumn("Number Of Rows", "6C") { DefaultValue = 10 };
            fb.AddColumn(numberOfRows);
            fb.AddAction("Generate", () =>
            {
                try
                {
                    var fn = GetStateFileName();
                    c.db.DataSet.WriteXml(fn, System.Data.XmlWriteMode.WriteSchema);
                }
                catch { }
                c.Build();

                int i = 0;
                int failures = 0;
                var p = new ENV.UI.ShowProgressInNewThread("Generating data");
                while (i < numberOfRows)
                {
                    try
                    {
                        _entity.Insert(() =>
                        {
                            p.Progress(i);
                            c.Result.ForEach(y => y.SetValue(i));
                        });
                        i++;
                    }
                    catch
                    {
                        if (failures++ > 100)
                        {
                            p.Dispose();
                            p = null;
                            Message.ShowWarning("Too Many Failed Rows, generated " + i.ToString().Trim() + " rows");
                            break;
                        }

                    }

                }
                if (p != null)
                    p.Dispose();


                new ENV.Utilities.EntityBrowser(_entity, true).Run();
            });
            fb.AddAction("View Table", () =>
            {
                new ENV.Utilities.EntityBrowser(_entity, true).Run();
            });

            fb.AddAction("Delete Existing Rows", () =>
            {

                if (ENV.Common.ShowYesNoMessageBox("Delete All Rows", "Are you sure you want to delete all existing rows?", false))
                    _entity.Delete(new FilterCollection());
            });
            fb.AddAction("Settings", () => c.Display());
            c.Display();
            fb.Run();

        }
        class GenerateDataSettings : ENV.Data.Entity
        {
            [PrimaryKey]
            public readonly NumberColumn Id = new NumberColumn("id");
            public readonly TextColumn ColumnName = new TextColumn("ColumnName", "50");
            public readonly TextColumn ColumnCaption = new TextColumn("Caption", "35");
            public readonly TextColumn Format = new TextColumn("Format", "10");
            public readonly TextColumn Strategy = new TextColumn("Strategy", "100");
            public readonly TextColumn StrategyOptions = new TextColumn("StrategyOptions");
            public readonly TextColumn OptionalValues = new TextColumn("OptionalValues");
            public GenerateDataSettings(IEntityDataProvider db) : base("GenerateDataSettings", db)
            { }
            public bool ParseFromTo(Func<string, string, bool> fromTo)
            {
                var vals = OptionalValues.Trim().ToString();
                if (vals.Length > 0)
                {
                    int minusPosition = vals.IndexOf('-', 1);
                    if (minusPosition >= 0)
                    {
                        var from = vals.Remove(minusPosition);
                        var to = vals.Substring(minusPosition + 1);
                        return fromTo(from, to);


                    }
                }
                return false;
            }
        }




        class CastColumnSetter : UserMethods.IColumnSpecifier
        {
            const string noneStrategy = "None";
            public MemoryDatabase db = new MemoryDatabase();
            int _lastId;
            List<RowInfo> _rows = new List<RowInfo>();
            class RowInfo
            {
                public Dictionary<string, ColumnUpdater> Options = new Dictionary<string, ColumnUpdater>();
                public ColumnBase Column;
            }
            void Add(ColumnBase col, params ColumnUpdater[] moreOptions)
            {
                string strategy = null;
                var options = noneStrategy;
                var used = new HashSet<string>();
                foreach (var item in moreOptions)
                {
                    if (item != null && !used.Contains(item.Name))
                    {
                        used.Add(item.Name);
                        options += ", " + item.Name;

                    }
                    if (strategy == null)
                    {
                        if (item == null)
                            strategy = noneStrategy;
                        else
                            strategy = item.Name;
                    }
                }


                used.Clear();
                var g = new GenerateDataSettings(db);
                var ri = new RowInfo() { Column = col };
                foreach (var item in moreOptions)
                {
                    if (item != null && !used.Contains(item.Name))
                    {
                        used.Add(item.Name);
                        ri.Options.Add(item.Name, item);
                    }
                }
                _rows.Add(ri);
                g.Insert(() =>
                {
                    g.Id.Value = _lastId++;
                    g.ColumnCaption.Value = col.Caption;
                    g.Format.Value = col.Format;
                    g.ColumnName.Value = col.Name;
                    g.StrategyOptions.Value = options;
                    g.Strategy.Value = strategy;

                });
            }
            internal void Display()
            {
                var g = new GenerateDataSettings(db);
                var eb = new myBrowser(g);
                eb.AddTextBoxColumn(g.ColumnCaption, tb =>
                {
                    tb.ReadOnly = true;
                    tb.ResizeToFit(15);
                });
                eb.AddTextBoxColumn(g.Format, tb =>
                {
                    tb.ReadOnly = true;

                });
                eb.AddComboBoxColumn(g.Strategy, cb => { cb.BindValues += (s, e) => e.Value = g.StrategyOptions; });
                eb.AddNonDisplayColumn(g.StrategyOptions);
                eb.AddTextBoxColumn(g.OptionalValues, tb => { tb.ResizeToFit(50); });

                eb.AddAction("Test Values", () =>
                {
                    using (var sw = new System.IO.StringWriter())
                    {
                        bool done = false;
                        SendStrategy(g, s =>
                        {
                            done = true;
                            sw.WriteLine(g.Caption.Trim() + ":");
                            for (int i = 0; i < 20; i++)
                            {
                                s.WriteTestValue(sw, i + 1);
                            }
                            System.Windows.Forms.MessageBox.Show(sw.ToString()); ;
                        });
                        if (!done)
                            System.Windows.Forms.MessageBox.Show("No Values");

                    }
                });
                eb.Run();



            }

            public void Build()
            {
                Result.Clear();
                var g = new GenerateDataSettings(db);
                g.ForEachRow(() =>
                {
                    SendStrategy(g, Result.Add);
                });
            }

            private void SendStrategy(GenerateDataSettings g, Action<ColumnUpdater> add)
            {
                if (g.Strategy != noneStrategy)
                {
                    ColumnUpdater r;
                    if (_rows[g.Id].Options.TryGetValue(g.Strategy.Trim(), out r))
                    {
                        r._init(_rows[g.Id].Column, g);
                        add(r);

                    }
                }
            }

            internal void Load(string fileName)
            {
                try
                {
                    var savedDb = new MemoryDatabase();
                    if (!System.IO.File.Exists(fileName))
                        return;
                    savedDb.DataSet.ReadXml(fileName);
                    var saved = new GenerateDataSettings(savedDb);
                    var g = new GenerateDataSettings(db);
                    var bp = new BusinessProcess { From = saved };
                    bp.Relations.Add(g, g.ColumnName.IsEqualTo(saved.ColumnName));
                    bp.ForEachRow(() =>
                    {
                        if (bp.Relations[g].RowFound)
                        {
                            if (_rows[g.Id].Options.ContainsKey(saved.Strategy.Trim()))
                            {
                                g.Strategy.Value = saved.Strategy;
                                g.OptionalValues.Value = saved.OptionalValues;
                            }
                        }
                    });
                }
                catch { }
            }

            class myBrowser : ENV.Utilities.EntityBrowser
            {

                public myBrowser(Entity e) : base(e, true)
                {

                    AllowInsert = false;
                    AllowDelete = false;
                    Activity = Activities.Update;
                }


            }

            public List<ColumnUpdater> Result = new List<ColumnUpdater>();
            public void DoOnColumn(TypedColumnBase<Text> column)
            {
                ColumnUpdater theDefault = null;

                var name = column.Caption.ToLower().Trim();
                if (name.Contains("country"))
                    theDefault = new Countries();
                else if (name.Contains("city"))
                    theDefault = new Cities();
                else if (name.Contains("title"))
                    theDefault = new Titles();
                else if (name.Contains("name"))
                {
                    if (name.Contains("first"))
                        theDefault = new FirstNames();
                    else if (name.Contains("last"))
                        theDefault = new LastNames();
                    else if (name.Contains("full"))
                        theDefault = new FullNames();
                    else if (name.Contains("department"))
                        theDefault = new CorporateDepartments();
                    else theDefault = new Names();
                }
                else if (name.Contains("address") || name.Contains("street"))
                    theDefault = new Address();

                Add(column, theDefault,
                    new RandomChars(),
                    new TextNumberFromRange(),
                    new ValuesFromList(),
                    new FirstNames(),
                    new LastNames(),
                    new FullNames(),
                    new Titles(),
                    new Countries(),
                    new Cities(),
                    new Address(),
                    new Names(),
                    new CorporateDepartments(),
                    new RetailDepartments()
                    );
            }

            public void DoOnColumn(TypedColumnBase<Number> column)
            {
                Add(column, new NumberFromRange(), new OrdinalNumber(), new ValuesFromList());
            }

            public void DoOnColumn(TypedColumnBase<Date> column)
            {
                var n = column.Name.Trim().ToLower();
                ColumnUpdater theDefault = new DateFromRange();
                if (n.Contains("birth"))
                    theDefault = new BirthDate();

                Add(column, theDefault, new DateFromRange(), new BirthDate(), new ValuesFromList());
            }


            public void DoOnColumn(TypedColumnBase<Time> column)
            {
                Add(column, new TimeFromRange(), new ValuesFromList());
            }

            public void DoOnColumn(TypedColumnBase<Bool> column)
            {
                Add(column, new BooleanData(), new True(), new False());
            }

            public void DoOnColumn(TypedColumnBase<byte[]> column)
            {
            }

            public void DoOnUnknownColumn(ColumnBase column)
            {
            }


        }


        static Random rand = new Random(Guid.NewGuid().GetHashCode());
        static T RandFromArray<T>(T[] _val)
        {
            return _val[Rand(0, _val.Length - 1)];
        }
        static int Rand(int min, int max)
        {

            return rand.Next(min, max == int.MaxValue ? max : max + 1);
        }
        class RandomChars : ColumnUpdater
        {
            char[] _val = "ABCDEFGHIJKLMNOPQRSTUVWXYZ -abcdefghijklmnopqrstuvwxyz".ToCharArray();
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                if (!string.IsNullOrEmpty(g.OptionalValues))
                    _val = g.OptionalValues.TrimEnd().ToString().ToCharArray();
            }
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                var maxLength = col.FormatInfo.MaxLength;
                var s = "";
                for (int i = 0; i < Rand(0, maxLength - 1); i++)
                {
                    s += RandFromArray(_val).ToString();
                }
                return s;
            }
        }

        abstract class ColumnUpdater
        {
            public ColumnUpdater()
            {
                Name = GetType().Name;
            }

            public void SetValue(int rowNum)
            {
                _column.Value = GetValue(_column, rowNum);
            }
            protected abstract object GetValue(ColumnBase col, int rowNum);

            public string Name { get; set; }

            internal void _init(ColumnBase col, GenerateDataSettings g)
            {
                _column = col;
                Init(col, g);
            }
            ColumnBase _column;
            protected virtual void Init(ColumnBase col, GenerateDataSettings g)
            {

            }

            internal void WriteTestValue(StringWriter sw, int rownum)
            {
                sw.WriteLine(GetValue(_column, rownum));
            }
        }
        class DateFromRange : ColumnUpdater
        {
            public DateFromRange()
            {
                Name = "Date";
            }
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                var c = (DateColumn)col;
                Date f = Date.Empty;
                Date t = Date.Empty;
                g.ParseFromTo((from, to) =>
                {
                    f = (Date)c.Parse(from, c.Format);
                    t = (Date)c.Parse(to, c.Format);

                    return true;
                });
                if (f == Date.Empty)
                    f = new Date(1995, 1, 1);
                if (t < f)
                    t = Date.Now;
                _min = ENV.UserMethods.Instance.ToNumber(f);
                _max = ENV.UserMethods.Instance.ToNumber(t);

            }
            int _min = 0, _max = int.MaxValue;
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return ENV.UserMethods.Instance.ToDate(Rand(_min, _max));
            }
        }
        class BirthDate : ColumnUpdater
        {

            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {

                _min = ENV.UserMethods.Instance.ToNumber(new Date(Date.Now.Year - 70, 1, 1));
                _max = ENV.UserMethods.Instance.ToNumber(new Date(Date.Now.Year - 20, 1, 1));

            }
            int _min = 0, _max = int.MaxValue;
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return ENV.UserMethods.Instance.ToDate(Rand(_min, _max));
            }
        }
        class TimeFromRange : ColumnUpdater
        {
            public TimeFromRange()
            {
                Name = "Time";
            }
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                var c = (TimeColumn)col;
                Time f = Time.StartOfDay;
                Time t = Time.StartOfDay;
                g.ParseFromTo((from, to) =>
                {
                    f = (Time)c.Parse(from, c.Format);
                    t = (Time)c.Parse(to, c.Format);

                    return true;
                });
                if (t == Time.StartOfDay)
                {
                    f = new Time(8, 0, 0);
                    t = new Time(18, 0, 0);
                }
                _min = ENV.UserMethods.Instance.ToNumber(f);
                _max = ENV.UserMethods.Instance.ToNumber(t);

            }
            int _min = 0, _max = int.MaxValue;
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return ENV.UserMethods.Instance.ToTime(Rand(_min, _max));
            }
        }
        class True : ColumnUpdater
        {
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return true;
            }
        }
        class False : ColumnUpdater
        {
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return false;
            }
        }
        class BooleanData : ColumnUpdater
        {
            public BooleanData()
            {
                Name = "Boolean";
            }
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {


            }

            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return Rand(0, 1) == 1;
            }
        }

        class TextNumberFromRange : ColumnUpdater
        {
            public TextNumberFromRange()
            {
                Name = "Number";
            }
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                var c = (TextColumn)col;
                var y = c.FormatInfo.MaxDataLength;
                if (y <= 0)
                    y = 5;
                _min = 0;
                _max = 1;
                for (int i = 0; i < y; i++)
                {
                    _max *= 10;
                }
                g.ParseFromTo((from, to) =>
                {
                    var f = Number.Parse(from);
                    var t = Number.Parse(to);
                    if (t != 0 && t < _max)
                    {
                        _max = t;
                        _min = f;
                    }
                    return true;
                });

            }
            int _min = 0, _max = int.MaxValue;
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return Rand(_min, _max).ToString();
            }
        }
        class NumberFromRange : ColumnUpdater
        {
            public NumberFromRange()
            {
                Name = "Number";
            }
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                var c = (NumberColumn)col;

                if (!g.ParseFromTo((from, to) =>
                {
                    _min = Number.Parse(from);
                    _max = Number.Parse(to);
                    return true;
                }))
                {
                    _max = 1;
                    for (int i = 0; i < c.FormatInfo.WholeDigits; i++)
                    {
                        _max *= 10;
                    }
                    _max -= 1;
                    _min = 0;
                    if (c.FormatInfo.SupportsMinus)
                        _min = -_max;
                }
                for (int i = 0; i < c.FormatInfo.DecimalDigits; i++)
                {
                    _min *= 10;
                    _max *= 10;
                }

                if (_min < int.MinValue)
                    _min = int.MinValue;
                if (_max > int.MaxValue)
                    _max = int.MaxValue;


            }
            Number _min = 0, _max = 0;
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                var c = (NumberColumn)col;
                Number x = Rand(_min, _max);
                for (int i = 0; i < c.FormatInfo.DecimalDigits; i++)
                {
                    x /= 10;
                }
                return x;
            }
        }
        static string[] GenerateArrayFromString(string g)
        {
            var v = new List<string>();
            foreach (var item in g.TrimEnd().ToString().Split(',', '\r', '\n'))
            {
                var x = item.Trim();
                if (!string.IsNullOrWhiteSpace(item))
                    v.Add(x);
            }
            return v.ToArray();
        }
        class ValuesFromList : ColumnUpdater
        {
            string[] _values;
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                _values = GenerateArrayFromString(g.OptionalValues);
            }
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return col.Parse(RandFromArray(_values), col.Format);
            }
        }
        #region 
        static string[] _firstNames = GenerateArrayFromString(
        #region 
@"May
Aaron
Abagail
Abby
Abdul
Abey
Abraham
Adelheid
Adelice
Adelind
Adolphus
Aggi
Aggie
Agnella
Agosto
Ahmad
Aime
Ainslee
Alaine
Alayne
Albrecht
Alene
Alie
Alisa
Alistair
Allis
Ally
Aloisia
Alphard
Alphonse
Alvis
Alysa
Alysia
Amabelle
Amalea
Amalee
Ambrosius
Ame
Ami
Amy
Ana
Analiese
Anallese
Andria
Anestassia
Angele
Angelina
Angus
Annabel
Annalee
Annamarie
Anne
Annetta
Anthiathia
Archibold
Ardyce
Aretha
Ariella
Arielle
Aristotle
Arlan
Arlee
Arlen
Arlette
Armand
Arne
Arnoldo
Arvin
Asa
Ashil
Astrid
Atalanta
Augusta
Auguste
Aundrea
Aurea
Aurelea
Austina
Austine
Averyl
Babbette
Babs
Baillie
Bar
Barby
Barry
Bart
Barty
Bary
Batsheva
Beau
Belva
Ben
Benedetta
Bennett
Benni
Benny
Bernadine
Bernardo
Berri
Berrie
Bethina
Bette
Betty
Beulah
Bianca
Billie
Billye
Bink
Blake
Blondie
Bobine
Bondy
Boony
Bradford
Brady
Brandais
Branden
Brantley
Bree
Brena
Brendis
Brewer
Brigitta
Brigitte
Brina
Brit
Brita
Britni
Brittaney
Brittney
Brnaba
Bruno
Bryna
Buck
Buckie
Burk
Byrom
Cad
Caitlin
Callie
Camille
Candace
Candie
Carina
Caritta
Carlita
Carly
Carmela
Carolan
Casper
Cassandry
Caterina
Catha
Catherine
Cathy
Cecil
Ceil
Celene
Cesaro
Cesya
Chane
Charil
Charleen
Charlton
Chas
Chelsie
Chere
Cherin
Cheslie
Chevalier
Chlo
Chloette
Chrisse
Christian
Christy
Chrisy
Clarey
Clarine
Clarisse
Clayborne
Clementius
Clyve
Codee
Codie
Coleman
Collete
Colman
Concordia
Constantina
Corby
Cordelia
Cordelie
Corena
Cornell
Corrie
Cos
Cosette
Cosmo
Courtney
Craig
Crin
Cristiano
Cristin
Cthrine
Cully
Curcio
Curtis
Dacey
Dale
Dalila
Dalli
Damita
Dana
Danica
Danice
Daniela
Danna
Danni
Danyelle
Danyette
Daphne
Darius
Darwin
Darya
Davey
Davidson
De witt
Deane
Deborah
Deirdre
Delcine
Della
Denis
Denise
Denyse
Derek
Derick
Des
Desiri
Desmond
Devlin
Devy
Diarmid
Dickie
Dido
Dimitri
Dimitry
Dino
Dixie
Domenic
Doralia
Dorice
Dorisa
Doroteya
Dorree
Dorris
Dorry
Dottie
Douglass
Dre
Drucie
Drucill
Drud
Drusie
Dulce
Dulcinea
Durante
Dyan
Eal
Easter
Eberto
Edd
Eddie
Edi
Edouard
Edsel
Eduino
Ekaterina
Elayne
Eldin
Electra
Elianore
Elissa
Ellery
Elnore
Elset
Elva
Elvina
Emelina
Emeline
Emile
Emiline
Emlyn
Emlynn
Emmerich
Emmery
Enos
Enrichetta
Erda
Ericha
Ermengarde
Ernaline
Ernesto
Esra
Essie
Estelle
Estrellita
Ethel
Ethelbert
Ethelin
Eugenius
Evangelin
Evania
Evie
Fabien
Fannie
Fedora
Felice
Fiann
Filmer
Fitz
Fleming
Flossie
Francisco
Francklyn
Franky
Franni
Frasco
Fraze
Fredek
Frederigo
Freeland
Friedrich
Fulton
Gabbie
Gabe
Gabrila
Gael
Gale
Gannie
Gannon
Gardie
Gare
Garik
Garner
Garret
Garrick
Gasparo
Gavra
Gavrielle
Gayla
Gaylene
Gayler
Genia
Genni
Gennie
Georgeta
Georgetta
Gerda
Gerek
Gerri
Gerry
Gerta
Gerti
Gertrude
Gianna
Gilbertine
Gilburt
Gilli
Gilly
Ginevra
Giraldo
Glenden
Gloria
Goddart
Godfree
Godfry
Godwin
Goldia
Grace
Gradey
Granger
Grant
Grazia
Gretchen
Gretel
Griffy
Gualterio
Guenna
Guglielma
Guillemette
Gunilla
Gunner
Gunther
Gwen
Gwenore
Hadley
Hale
Ham
Hana
Hannah
Hannie
Hans
Harmon
Harper
Harrietta
Harriot
Harriott
Harris
Harry
Hastie
Heath
Helge
Hendrick
Henri
Henriette
Hercule
Hermon
Hilda
Hildy
Hill
Hillary
Hillier
Hollis
Horst
Huntington
Hurleigh
Husein
Ibby
Idalia
Idalina
Ignace
Ileana
Ilyse
Inga
Ingeberg
Ingunna
Iosep
Irving
Isidora
Isidoro
Ivan
Ives
Ivonne
Jacenta
Jacobo
Jacquelyn
Jakob
Jammal
Jane
Janella
Janetta
Janifer
Janina
Jany
Jarid
Jase
Jasmine
Jayme
Jeanelle
Jeannie
Jed
Jemie
Jemmie
Jenilee
Jenine
Jenn
Jennee
Jeno
Jeramie
Jere
Jereme
Jeremy
Jerrilyn
Jess
Jessamyn
Jessee
Jessi
Jillana
Jimmy
Joan
Joby
Jobyna
Jodie
Johna
Jordon
Jorgan
Joseph
Josselyn
Jourdan
Judas
Jude
Julee
Julia
Juliann
Juliette
Justin
Justis
Kacy
Kaela
Kaitlin
Kalila
Kalindi
Kalvin
Kandy
Karel
Karita
Karoly
Kasey
Kaspar
Kassey
Kate
Katharine
Kathi
Kathy
Katie
Katrina
Kaylil
Keary
Kelbee
Kerk
Kerrie
Kerwin
Keven
Kiley
Kimball
Kingston
Kippy
Kirbie
Kissie
Klarrisa
Knox
Konrad
Koren
Kris
Kristan
Kristo
Krysta
Krystyna
Kurt
Ladonna
Lamar
Lane
Lanie
Larissa
Lars
Laure
Leanor
Leda
Lee
Leela
Leicester
Leif
Lenna
Lenora
Leola
Leone
Leonidas
Leonora
Leontine
Leslie
Leticia
Letti
Lexie
Lexine
Liana
Lianne
Lilias
Lilla
Lind
Link
Linnell
Lira
Lisbeth
Lisette
Loleta
Loraine
Lorena
Lorianne
Lorri
Lothaire
Louie
Louis
Lovell
Lucais
Lucias
Lucinda
Lucine
Lucio
Lucretia
Luella
Luisa
Lyn
Lynna
Lynnett
Maddalena
Maddy
Madelene
Madeline
Maegan
Mair
Maire
Maison
Maitilde
Maje
Malchy
Malva
Marcelo
Marena
Marge
Margit
Maribelle
Marietta
Marigold
Marilee
Marissa
Maritsa
Marjorie
Marjy
Markus
Marney
Martguerita
Maryellen
Marylin
Maryrose
Mason
Mathilde
Matthus
Max
Mechelle
Megan
Melisande
Mellisa
Mercy
Merilee
Merl
Merle
Merna
Merrick
Merv
Meta
Micaela
Michaela
Michelle
Mignon
Miles
Milli
Millisent
Miranda
Missie
Modesta
Morgan
Morgen
Morgun
Morie
Morissa
Morly
Moselle
Muhammad
Murielle
Myrta
Nadeen
Nancey
Nancie
Nanete
Nanette
Nanice
Natasha
Natividad
Neala
Neall
Neile
Neill
Nelli
Netti
Netty
Nev
Nichole
Nickolai
Nicolai
Niki
Nikolos
Ninnetta
Ninon
Nissie
Normy
Nyssa
Oates
Obidiah
Ode
Odelia
Odette
Ofilia
Olenka
Olwen
Onfroi
Oralie
Orelia
Orv
Osborn
Osbourn
Pablo
Pascal
Patricia
Patton
Patty
Paule
Paulie
Pavia
Pavlov
Pedro
Pen
Penelope
Penny
Perice
Perri
Peta
Pete
Petra
Petronia
Petronille
Pietro
Prent
Prescott
Priscilla
Prissie
Prue
Queenie
Quentin
Quillan
Quinn
Quintana
Rabi
Ramona
Rana
Randene
Randolph
Raymund
Reade
Reba
Rebbecca
Rebeka
Reed
Rees
Reese
Reg
Regina
Reinwald
Remy
Renata
Revkah
Rex
Reynard
Rhiamon
Rhodia
Richard
Rickie
Riordan
Rip
Robbie
Robby
Robbyn
Robina
Rochell
Rochelle
Rochette
Rockie
Rod
Rodina
Romain
Romy
Ronalda
Rosana
Roseann
Roshelle
Rosita
Rozanna
Rubi
Ruddy
Rudyard
Rustin
Ruthi
Ryley
Sada
Sadella
Sal
Samara
Sampson
Sande
Sanders
Sandor
Sanson
Saw
Sawyere
Say
Scarface
Scarlet
Scarlett
Schuyler
Selena
Selma
Seth
Seymour
Shae
Shana
Shandra
Shanie
Shara
Sharity
Sharl
Sharyl
Shaw
Shea
Sheeree
Shelli
Shellie
Shellysheldon
Shep
Sherie
Sherilyn
Sherrie
Sherwin
Shurwood
Sianna
Sibby
Sid
Simonette
Sinclare
Skylar
Skyler
Sophey
Spense
Staffard
Stanfield
Stanislaw
Starr
Stefanie
Stephani
Steven
Stevena
Susette
Sutherlan
Suzy
Tamiko
Tammy
Tanny
Tate
Tatum
Ted
Teddi
Teodor
Teodora
Teri
Terrill
Tersina
Tess
Thelma
Thibaut
Thomasina
Thornie
Tiertza
Timmie
Timofei
Timothy
Tine
Tome
Tony
Tonya
Tori
Townie
Tracy
Travers
Travis
Tris
Trstram
Trudey
Trudy
Tyne
Ugo
Ulises
Ulrike
Una
Ursulina
Ursuline
Vachel
Val
Valencia
Valentia
Valentijn
Vallie
Vally
Vanessa
Veradis
Vere
Verge
Verne
Vick
Vin
Viola
Viole
Virgie
Virginie
Vitia
Viva
Viviene
Von
Vonnie
Wadsworth
Wain
Waiter
Ward
Warner
Web
Weber
Weidar
Wendi
Wernher
Weston
Wilburt
Will
Willey
William
Winny
Winona
Wolf
Wood
Yank
Yankee
Yard
Yorker
Yul
Yuma
Yuri
Yurik
Yves
Zaccaria
Zachary
Zak
Zarah
Zarla
Zebulon
Zorina
"
        #endregion
),
            _lastNames = GenerateArrayFromString(
        #region
                @"Abell
Abrahamoff
Absalom
Acton
Adamczyk
Afonso
Agirre
Alabastar
Alam
Alishoner
Allsobrook
Allsworth
Almond
Althrop
Ambroziak
Amiranda
Amoore
Ander
Anderl
Andreucci
Antonsson
Antoshin
Apedaile
Apted
Arend
Argabrite
Arghent
Arnould
Arran
Asbury
Ascroft
Ashforth
Ashlin
Aspinell
Atkinson
Attle
Aubert
Backson
Baggalley
Baggelley
Balsdone
Balshaw
Banbridge
Bardwell
Barens
Barks
Barlow
Barnet
Barnsdall
Baroc
Barratt
Bartholat
Basile
Baston
Batistelli
Batterson
Bauchop
Baynard
Beakes
Beals
Behan
Belamy
Bellini
Benbow
Beneyto
Beniesh
Benito
Bennough
Bernhart
Berni
Bernier
Berrill
Beteriss
Bikker
Bille
Billo
Billyard
Binny
Birden
Bispham
Blackshaw
Blaes
Blakden
Blakesley
Blankenship
Blann
Blasing
Blaszczak
Blazey
Blinder
Blogg
Blood
Bloxsom
Blumson
Blunn
Bode
Bogaert
Bonallick
Bonanno
Bonhill
Borsi
Bortolussi
Boshers
Boston
Bottom
Bowhey
Bowra
Boxall
Bracchi
Bradnocke
Bramont
Brandenberg
Brankley
Brazel
Breissan
Brenton
Brewitt
Bridden
Bris
Brockelsby
Brose
Brosoli
Brower
Bruhn
Brumen
Buckel
Bulfit
Bullivant
Bunney
Bunten
Burkwood
Burnapp
Burrows
Burston
Burstow
Bushnell
Busk
Bussy
But
Bute
Bygreaves
Byrkmyr
Caghan
Cahillane
Calcut
Camel
Cammidge
Camous
Campkin
Capon
Capstaff
Cardenoso
Cargill
Carroll
Case
Cathesyed
Cattach
Cereceres
Championnet
Champken
Champniss
Cheese
Chipping
Chittleburgh
Choldcroft
Chuney
Churchyard
Cicco
Clarae
Clawe
Claypole
Cleator
Clever
Cleynaert
Clinch
Clorley
Cody
Cohan
Colgrave
Cominetti
Comusso
Connerry
Conor
Cordaroy
Corday
Cornelissen
Cosely
Cosgry
Cosslett
Costain
Costar
Cosyns
Cotherill
Couch
Couchman
Coull
Covert
Coyett
Coyle
Crabbe
Craig
Crang
Crannis
Cristofol
Croley
Cromack
Cromar
Crosland
Crowest
Crowthe
Crudge
Culligan
Cuncarr
Curbishley
Curnow
Cutcliffe
Da Costa
Danbury
Danels
Danielski
Danslow
Dargie
Darley
Dart
Darville
D'Aubney
Daughtrey
Davidovitch
Davitti
De Angelis
De Brett
Debney
Deeves
Del Dello
Delion
D'Emanuele
Denes
Denning
Dering
Dessent
Di Francesco
Di Pietro
Dibden
Dibson
Dickie
Digle
Dignall
Dignum
Dikels
Dilleway
Dobrowski
Dominicacci
Donaghie
Donaghy
Donaho
Doni
Donizeau
Dorgon
Dormon
Dow
Dowdell
Downey
Doxsey
Dradey
Drakers
Drexel
Druhan
Duckels
Duckering
Duckitt
Duffan
Duffrie
Duffyn
Dugall
Duigan
Dumper
Dundendale
Dunley
Dunmore
Dunne
Duny
Dupree
Durtnal
Dykins
Eatock
Ecclestone
Echelle
Edelman
Edgett
Edlington
Edridge
Effemy
Elijahu
Ellick
Elnor
Elsley
Elt
Emblow
Ennew
Ephson
Eslie
Espinal
Espinet
Estrella
Everingham
Eversley
Fadian
Faichney
Fairbrass
Fancott,
Farden
Farrier
Favel
Fazakerley
Febre
Fedorchenko
Feechan
Feifer
Felipe
Fellis
Ferencz
Ferrari
Fessler
Fetter
Fibben
Fidelus
Fincke
Fitzharris
Flasby
Flattman
Flecknoe
Fleischer
Flisher
Folkerts
Founds
Francesch
Francke
Franiak
Frankes
Frayling
Freeborn
Freeburn
Frenzel;
Froggatt
Fryatt
Fulger
Furney
Fursse
Gaber
Gager
Gain
Gainsborough
Gallego
Gallen
Galletly
Galliford
Gambie
Gane
Gard
Gayter
Gedge
Geeves
Geffe
Gethyn
Giacomuzzo
Giblin
Gifkins
Gillbey
Gilroy
Giovannoni
Girdlestone
Girtin
Glowacha
Goady
Godwin
Goggin
Goldwater
Goligly
Goneau
Gorden
Goschalk
Goscomb
Gracey
Grafton
Greenmon
Gridley
Grigolli
Grime
Grimmolby
Grishaev
Gritsaev
Grizard
Grogor
Gronow
Guerra
Guppie
Gurnett
Gush
Gutherson
Gwin
Hadwin
Halfhead
Hallard
Hamblin
Hanburry
Hannam
Harbard
Harsum
Hasely
Hasnip
Hasted
Haukey
Hawk
Hawthorne
Haydn
Haydney
Haynes
Hazlewood
Heald
Hefforde
Helsdon
Hemms
Hendricks
Hendrikse
Henrot
Hess
Hessentaler
Hewson
Hindrick
Hitzmann
Horsburgh
Hounsom
Huckin
Hughill
Hull
Hurtic
Hyam
Iannini
Ickovici
Idel
Iley
Illidge
Illyes
Ineson
Instock
Ironmonger
Ishaki
Island
Issit
Itzcak
Itzkovsky
Ivashin
Izkovici
Jaimez
Jakobssen
Jakubovsky
Jancic
Janes
Janko
Janouch
Janus
Jarmyn
Jarrold
Jaszczak
Jevons
Jonk
Joscelyn
Josefsen
Josipovic
Jouandet
Juan
Jurasz
Kabos
Kalinovich
Kaminski
Kasper
Keaveney
Kenwell
Kerne
Kerrich
Kettles
Kettlestringe
Kibby
Kike
Kimberley
Kingerby
Kinnie
Kinson
Kinton
Kirckman
Kisbey
Kitter
Klaff
Klimczak
Klossek
Kneel
Koenen
Kohnert
Kornas
Ksandra
Kuhndel
Kupka
Kyte
Lainton
Laity
Lamblot
Lamerton
L'Amie
Lamond
Lander
Lante
Lardner
Lassell
Lassells
Latch
Lawful
Layborn
Leagas
Ledbury
Leedal
Leeds
Leere
Lefeaver
Lefeuvre
Lefridge
Legen
Leggott
Leheude
Leif
Lennarde
Lescop
Levens
Levinge
Lewinton
Lghan
Liddiatt
Lipyeat
Lisett
Lissandrini
Lithcow
Liven
Lockey
Lomansey
Lonergan
Longworth
Lorence
Lorens
Lorriman
Lovel
Lucchi
Luckey
Luckwell
Luetkemeyers
Luscombe
Mabley
MacAvaddy
MacCafferky
MacCarter
MacClenan
MacConchie
MacConnechie
MacDonell
MacGillavery
MacGuigan
MacIlory
MacMorland
MacMychem
MacNeilage
MacPharlain
MacTague
MacWhan
Maddison
Mailes
Malbon
Malster
Manby
Mapother
Marchello
Marcoolyn
Marjot
Marquiss
Marrett
Martellini
Masden
Masey
Mathou
Matusiak
Matyushenko
Maxsted
Mc Combe
McAllaster
McAllen
McCallam
McClunaghan
McCluskey
McClymont
McConnel
McDugal
McElvine
McGeachy
McIlory
McInnerny
McKeady
McKeag
McMurtyr
McSparran
Meaney
Megarrell
Melbourne
Mellanby
Melody
Melpuss
Menicomb
Meo
Meriet
Merrgan
Merrikin
Merrisson
Mesnard
Mewes
Mewha
Middlebrook
Millward
Milvarnie
Minillo
Misken
Mollitt
Molohan
Moncur
Moogan
Moralee
Morrill
Moseley
Motton
Moultrie
Moxstead
Moyne
Muddle
Muino
Muncer
Mungin
Murdoch
Nagle
Narracott
Nasi
Nathon
Negro
Nemchinov
Nestoruk
Nevins
New
Newlan
Newling
Newlove
Nicholl
Ninnoli
Noblet
Noddings
Norkett
Northam
Nussen
O' Hanvey
Obispo
O'Carran
O'Connell
O'Corhane
O'Dulchonta
Ogan
O'Gready
O'Hannen
O'Henehan
Okey
O'Kinedy
Olech
Olek
Olesen
Olligan
Olsen
Ondricek
Onraet
Orneles
Oscroft
Osipov
Ostrich
Ovenell
Ozintsev
Paddeley
Paddick
Padfield
Panks
Pantridge
Pardi
Parlatt
Paulus
Pearce
Pedler
Peeter
Peinton
Penella
Pengelly
Pengilly
Peniello
Pennells
Pennings
Penrose
Penwright
Peplay
Peregrine
Perree
Perry
Persence
Persey
Petrello
Petriello
Petrillo
Phalp
Philcott
Piaggia
Pickavant
Pickhaver
Pickle
Pietrzyk
Pinck
Pirt
Pisco
Piscopiello
Pledge
Pocock
Poli
Polino
Pollastrone
Polye
Pont
Porte
Possa
Pott
Powys
Prantoni
Prazor
Preskett
Prestige
Proudlock
Prozescky
Quaintance
Quinnell
Radnage
Ramplee
Rannie
Rasher
Rathe
Ratlee
Ravillas
Rayner
Reace
Reddell
Redon
Regina
Regus
Rennison
Reston
Rettie
Ricardou
Richardin
Richardon
Riddiough
Ride
Riglar
Rignall
Riolfi
Riolfo
Rival
Riveles
Robardey
Robarts
Robbel
Robberecht
Robe
Robeiro
Robke
Rock
Rogerson
Roney
Roobottom
Rosentholer
Rothman
Rout
Routh
Rowat
Rowen
Roy
Royan
Royce
Roycroft
Royste
Rozzell
Rudgerd
Rudiger
Ruecastle
Ruff
Rugg
Rulton
Rumford
Rundle
Saffer
Sainer
Samber
Sambidge
Sandercroft
Sapsford
Sargeaunt
Saywood
Scapelhorn
Scare
Schwieso
Scorrer
Scorton
Seago
Seakings
Secrett
Sedcole
Semerad
Senett
Sergeant
Severwright
Sewill
Sewter
Shacklady
Shakelady
Sharkey
Shaughnessy
Shave
Shaxby
Sheaber
Shelbourne
Shenfish
Shills
Shimmin
Sidery
Sidon
Sinson
Skarman
Skett
Skewes
Slator
Slavin
Sleany
Slite
Smartman
Smillie
Smissen
Smitham
Soffe
Soigne
Somner
Soppett
Speakman
Speek
Spellissy
Sperrett
Sperring
Spincke
St Clair
Stachini
Stallibrass
Stallworthy
Stammers
Stanners
Stanworth
Starbucke
Starie
Stellman
Stephens
Stickins
Stirland
Stoodley
Striker
Styche
Sudell
Suff
Swales
Swayton
Swindle
Swyer-Sexey
Sygroves
Syseland
Tapply
Tarbath
Tattersill
Tedahl
Thaw
Thomassin
Thorrold
Tick
Tipler
Tipperton
Tockell
Toffetto
Toffolo
Tollit
Tolworthie
Tomaello
Tomasoni
Tomaszek
Torbard
Torrejon
Tourot
Town
Toyne
Trowell
Trueman
Truluck
Truwert
Tschursch
Tubbs
Tunbridge
Turbern
Turnell
Turneux
Turnpenny
Tydd
Ughi
Ughini
Union
Unwin
Usborn
Valdes
Vallender
Van Castele
Van der Mark
Van Hesteren
Vanini
Vannozzii
Varfolomeev
Vasiljevic
Vassano
Vassman
Vaughten
Ventum
Vereker
Very
Vickarman
Vickers
Vickery
Vieyra
Villar
Vorley
Wailes
Wakelin
Waliszek
Wandrach
Warham
Waterhous
Webborn
Weich
Welham
Westall
Westmancoat
Weyman
Weymont
Whittick
Wickling
Wickson
Wiffield
Wilford
Willas
Winchcombe
Winckles
Windrass
Wisam
Witcherley
Witherdon
Wodeland
Woffinden
Wolseley
Woolston
Wooster
Worboy
Worters
Wynrehame
Yanov
Yarnley
Yeates
Yegorovnin
Yeulet
Yurenin
Zarb
Zuann
")
        #endregion
            , _corporateDepartments = GenerateArrayFromString(
        #region
                @"Accounting
Business Development
Engineering
Human Resources
Legal
Marketing
Product Management
Research and Development
Sales
Services
Support
Training
")
        #endregion

            , _retailDepartments = GenerateArrayFromString(
        #region
                @"Automotive
Baby
Beauty
Books
Clothing
Computers
Electronics
Games
Garden
Grocery
Health
Home
Industrial
Jewelery
Kids
Movies
Music
Outdoors
Shoes
Sports
Tools
Toys
"
        #endregion
)
            , _countries = GenerateArrayFromString(
        #region
                @"Afghanistan
Albania
Angola
Argentina
Armenia
Bangladesh
Belarus
Belize
Bolivia
Bosnia and Herzegovina
Brazil
Bulgaria
Burkina Faso
Cambodia
Cameroon
Canada
Cape Verde
Central African Republic
Chile
China
Colombia
Costa Rica
Croatia
Cuba
Cyprus
Czech Republic
Democratic Republic of the Congo
Dominican Republic
Ecuador
Egypt
Estonia
Ethiopia
Finland
France
Germany
Greece
Guadeloupe
Guam
Guatemala
Haiti
Honduras
Hungary
Iceland
Indonesia
Iran
Iraq
Ireland
Israel
Italy
Ivory Coast
Jamaica
Japan
Jordan
Kazakhstan
Kosovo
Latvia
Lebanon
Liberia
Libya
Lithuania
Macedonia
Madagascar
Malaysia
Malta
Marshall Islands
Mexico
Moldova
Mongolia
Morocco
Namibia
Nepal
Netherlands
New Zealand
Nicaragua
Nigeria
North Korea
Norway
Oman
Pakistan
Palau
Palestinian Territory
Panama
Paraguay
Peru
Philippines
Poland
Portugal
Puerto Rico
Reunion
Russia
Saint Pierre and Miquelon
Samoa
Saudi Arabia
Serbia
Sierra Leone
Slovenia
South Africa
South Korea
South Sudan
Spain
Sri Lanka
Sudan
Swaziland
Sweden
Syria
Tajikistan
Tanzania
Thailand
Tunisia
Turkey
Uganda
Ukraine
United Arab Emirates
United States
Uruguay
Uzbekistan
Venezuela
Vietnam
Yemen
Zambia
"
        #endregion
)
            , _cities = GenerateArrayFromString(
        #region
                @"Abu Dhabi
Abuja
Accra
Adamstown
Addis Ababa
Algiers
Alofi
Amman
Amsterdam
Andorra la Vella
Ankara
Antananarivo
Apia
Ashgabat
Asmara
Astana
Asunción
Athens
Avarua
Baghdad
Baku
Bamako
Bandar Seri Begawan
Bangkok
Bangui
Banjul
Basse-Terre
Basseterre
Beijing
Beirut
Belgrade
Belmopan
Berlin 
Bern
Bishkek
Bissau
Bogotá
Brasília
Bratislava
Brazzaville
Bridgetown
Brussels
Bucharest
Budapest
Buenos Aires
Bujumbura
Cairo
Canberra
Caracas
Castries
Cayenne
Charlotte Amalie
Chișinău
Cockburn Town
Conakry
Copenhagen
Dakar
Damascus
Dhaka
Dili
Djibouti
Dodoma 
Doha
Douglas
Dublin
Dushanbe
Edinburgh of the Seven Seas
El Aioun 
Tifariti 
Episkopi Cantonment
Flying Fish Cove
Fort-de-France
Freetown
Funafuti
Gaborone
George Town
Georgetown
Georgetown
Gibraltar
Grozny
Guatemala City
Gustavia
Hagåtña
Hamilton
Hanga Roa
Hanoi
Harare
Hargeisa
Havana
Helsinki
Hong Kong
Honiara
Islamabad
Jakarta
Jamestown
Jerusalem 
Juba
Kabul
Kampala
Kathmandu
Khartoum
Kiev
Kigali
King Edward Point
Kingston
Kingston
Kingstown
Kinshasa
Kuala Lumpur 
Putrajaya
Kuwait City
Libreville
Lilongwe
Lima
Lisbon
Ljubljana
Lomé
London
Luanda
Lusaka
Luxembourg
Madrid
Majuro
Malabo
Malé
Mamoudzou
Managua
Manama
Manila
Maputo
Marigot
Maseru
Mata-Utu
Mbabane
Lobamba 
Mexico City
Minsk
Mogadishu
Monaco
Monrovia
Montevideo
Moroni
Moscow
Muscat
N'Djamena
Nairobi
Nassau
Naypyidaw
New Delhi
Ngerulmud
Niamey
Nicosia
Nicosia
Nouakchott
Nouméa
Nukuʻalofa
Nuuk
Oranjestad
Oslo
Ottawa
Ouagadougou
Pago Pago
Palikir
Panama City
Papeete
Paramaribo
Paris
Philipsburg
Phnom Penh
Plymouth 
Brades Estate 
Podgorica 
Cetinje
Port Louis
Port Moresby
Port of Spain
Port Vila
Port-au-Prince
Porto-Novo 
Cotonou 
Prague
Praia
Pretoria 
Bloemfontein 
Cape Town 
Pristina
Pyongyang
Quito
Rabat
Jerusalem 
Reykjavík
Riga
Riyadh
Road Town
Rome
Roseau
Saint-Denis
Saipan
San José
San Juan
San Marino
San Salvador
Sana'a (de jure)
Santiago
Santo Domingo
Sarajevo
Seoul
Singapore
Skopje
Sofia
St. George's
St. Helier
St. John's
St. Peter Port
St. Pierre
Stanley
Stepanakert
Stockholm
Sukhumi
Suva
São Tomé
Taipei
Tallinn
Tarawa
Tashkent
Tbilisi 
Tegucigalpa
Tehran
The Valley
Thimphu
Tirana
Tiraspol
Tokyo
Tripoli
Tskhinvali
Tunis
Tórshavn
Ulaanbaatar
Vaduz
Valletta
Vatican City
Victoria
Vienna
Vientiane
Vilnius
Warsaw
Washington, D.C.
Wellington
West Island
Willemstad
Windhoek
Yaoundé
Yerevan
Zagreb
Tel Aviv
New York
Los Angeles
"
        #endregion
)
            , _titles = GenerateArrayFromString(
        #region
                @"Account Coordinator
Account Executive
Account Representative 
Accountant 
Accounting Assistant 
Actuary
Administrative Assistant 
Administrative Officer
Analog Circuit Design manager
Analyst Programmer
Assistant Manager
Assistant Media Planner
Assistant Professor
Associate Professor
Automation Specialist 
Biostatistician 
Budget/Accounting Analyst 
Business Systems Development Analyst
Chemical Engineer
Chief Design Engineer
Civil Engineer
Clinical Specialist
Community Outreach Specialist
Compensation Analyst
Computer Systems Analyst 
Cost Accountant
Data Coordiator
Database Administrator 
Dental Hygienist
Design Engineer
Desktop Support Technician
Developer 
Director of Sales
Editor
Electrical Engineer
Engineer 
Environmental Specialist
Environmental Tech
Executive Secretary
Financial Advisor
Financial Analyst
Food Chemist
General Manager
Geological Engineer
Geologist 
GIS Technical Architect
Graphic Designer
Health Coach 
Help Desk Operator
Help Desk Technician
Human Resources Assistant 
Human Resources Manager
Information Systems Manager
Internal Auditor
Junior Executive
Legal Assistant
Librarian
Marketing Assistant
Marketing Manager
Mechanical Systems Engineer
Media Manager 
Nuclear Power Engineer
Nurse
Nurse Practicioner
Occupational Therapist
Office Assistant 
Operator
Paralegal
Payment Adjustment Coordinator
Pharmacist
Physical Therapy Assistant
Product Engineer
Professor
Programmer Analyst 
Programmer 
Project Manager
Quality Control Specialist
Quality Engineer
Recruiter
Recruiting Manager
Registered Nurse
Research Assistant 
Research Associate
Research Nurse
Safety Technician 
Sales Associate
Sales Representative
Senior Cost Accountant
Senior Developer
Senior Editor
Senior Financial Analyst
Senior Quality Engineer
Senior Sales Associate
Social Worker
Software Consultant
Software Engineer 
Software Test Engineer 
Speech Pathologist
Staff Accountant 
Staff Scientist
Statistician 
Structural Analysis Engineer
Structural Engineer
Systems Administrator 
Tax Accountant
Teacher
Technical Writer
VP Accounting
VP Marketing
VP Product Management
VP Quality Control
VP Sales
Web Designer 
Web Developer 
"
        #endregion
                )
            , _nameParts = GenerateArrayFromString(@"Red,Green,Blue,Black,Pink,Orange,White,Horse,Dog,Dragon,Cat,Bug,Snow,Sun,Grass,Wall,Jack,John,Bay");
        #endregion

        class FirstNames : PreparedValues
        {
            public FirstNames() : base("First Names", _firstNames) { }
        }
        class LastNames : PreparedValues
        {
            public LastNames() : base("Last Names", _lastNames) { }
        }
        class Countries : PreparedValues
        {
            public Countries() : base("Countries", _countries) { }
        }
        class Cities : PreparedValues
        {
            public Cities() : base("Cities", _cities) { }
        }
        class CorporateDepartments : PreparedValues
        {
            public CorporateDepartments() : base("Corporate Departments", _corporateDepartments) { }
        }
        class RetailDepartments : PreparedValues
        {
            public RetailDepartments() : base("Retail Departments", _retailDepartments) { }
        }
        class Titles : PreparedValues
        {
            public Titles() : base("Title", _titles) { }
        }
        class FullNames : ColumnUpdater
        {
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return (RandFromArray(_firstNames) + " " + RandFromArray(_lastNames)).Trim();
            }
        }
        class Names : ColumnUpdater
        {
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return GenerateName();
            }
        }
        private static string GenerateName()
        {
            string r = "";
            for (int i = 0; i < Rand(1, 3); i++)
            {
                r = (r + " " + RandFromArray(_nameParts)).Trim();
            }

            return r;
        }
        class Address : ColumnUpdater
        {
            string[] _suffix = new string[] { "st", "blvd", "lane", "avanue" };
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return Rand(1, 100).ToString() + " " + GenerateName() + " " + RandFromArray(_suffix);
            }
        }
        class PreparedValues : ColumnUpdater
        {
            string[] _values;
            public PreparedValues(string name, string[] values)
            {
                Name = name;
                _values = values;
            }
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return col.Parse(RandFromArray(_values), col.Format);
            }
        }

        class OrdinalNumber : ColumnUpdater
        {
            int _num;
            protected override void Init(ColumnBase col, GenerateDataSettings g)
            {
                _num = 0;
                var nc = col as ENV.Data.NumberColumn;
                if (nc != null)
                {
                    var e = nc.Entity as ENV.Data.Entity;
                    if (e != null && e.Exists())
                    {
                        _num = e.Max(nc);
                    }
                }
            }
            protected override object GetValue(ColumnBase col, int rowNum)
            {
                return col.Parse((++_num).ToString(), col.Format);
            }
        }
    }
}
