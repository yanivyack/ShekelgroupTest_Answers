using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ENV.Utilities
{
    public partial class MemoryMonitor : Form
    {
        Func<long> _getVal;

        public MemoryMonitor()
        {
            InitializeComponent();

            var p = System.Diagnostics.Process.GetCurrentProcess();

            graph.AddSeires("Controller", Color.Red, 1 *100 , () => MemoryTracker.ItemCount(), true);
            graph.AddSeires("Managed Memory", Color.DarkBlue, 1, () => GC.GetTotalMemory(true) / 1024, true);
            try
            {
                
                _getVal = () => new PerformanceCounter("Process", "Working Set - Private", p.ProcessName).RawValue / 1024;
                _getVal().ToString();
                graph.AddSeires("Private Working Set ", Color.DarkOrange, 1, _getVal, true);
            }
            catch
            {
                graph.AddSeires("Private Memory (aprox)", Color.Pink, 1, () => p.PrivateMemorySize / 1024, true);
            }

            graph.AddSeires("Handle Count", Color.Green, 1 * 50, () => p.HandleCount, true);
            graph.AddSeires("Working Set", Color.SkyBlue, 1, () => p.WorkingSet64 / 1024, false);
            graph.AddSeires("Peak Working", Color.MediumSeaGreen, 1, () => p.PeakWorkingSet64 / 1024, false);


            components = new Container();
            var t = new Timer(components) { Interval = 1000 };
            t.Tick += delegate
            {
                p.Refresh();
                graph.ReevaluateData();
            };
            t.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            graph.ResetBaseLine();
        }

        public static void Run()
        {
            var t = new System.Threading.Thread(() => new MemoryMonitor().ShowDialog());
            t.Start();
        }
    }

    class GraphControl : System.Windows.Forms.Control
    {
        const string format = "###,###,###,###";
        private Panel _panel;
        public GraphControl(Panel panel)
        {
            DoubleBuffered = true;
            _panel = panel;
        }

        private List<GraphLine> _lines = new List<GraphLine>();

        public void ReevaluateData()
        {
            foreach (var line in _lines)
            {
                line.ReevaluateData();
            }
            Invalidate();
        }

        public void ResetBaseLine()
        {
            foreach (var line in _lines)
            {
                line.ResetBaseLine();
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);
            foreach (var line in _lines)
            {
                line.OnPaint(Height, Width, e.Graphics);
            }
        }

        public void AddSeires(string counterName, Color color, int factor, Func<long> getVal, bool enabled)
        {
            _lines.Add(new GraphLine(getVal, color, _panel, _controlsTop, counterName, factor, enabled));
            _controlsTop += 17;
        }

        int _controlsTop = 24;

        internal class GraphLine
        {
            float[] _readouts = new float[100];
            int _position;
            long _lastValue;
            long _baseLine;
            static long _scale = 10;
            Func<long> _getVal;
            Pen _pen, _baseLinePen;
            long _factor;
            bool _enabled;

            Panel _panel;
            CheckBox _enabledCheckBox;
            TextBox _counterNameTextBox;
            TextBox _baseLineTextBox;
            TextBox _deltaFromBaseTextBox;
            TextBox _lastValueTextBox;
            TextBox _factorTextBox;

            public GraphLine(Func<long> getVal, Color color, Panel panel, int controlsTop, string counterName, int factor, bool enabled)
            {
                _enabled = enabled;
                _factor = factor;
                _getVal = getVal;
                _pen = new Pen(color);
                _baseLinePen = new Pen(color)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                };
                _panel = panel;
                _enabledCheckBox = new CheckBox
                {
                    Top = controlsTop + 1,
                    Left = 10,
                    Width = 19,
                    Height = 20,
                    Checked = _enabled,
                };
                _enabledCheckBox.CheckedChanged += (sender, e) => _enabled = !_enabled;

                _counterNameTextBox = new TextBox
                {
                    Top = controlsTop,
                    Left = 30,
                    Text = counterName,
                    ForeColor = color,
                };
                _baseLineTextBox = new TextBox
                {
                    Top = controlsTop,
                    Left = 130,
                    ForeColor = color,
                    TextAlign = HorizontalAlignment.Right,
                };
                _deltaFromBaseTextBox = new TextBox
                {
                    Top = controlsTop,
                    Left = 230,
                    ForeColor = color,
                    TextAlign = HorizontalAlignment.Right,
                };
                _lastValueTextBox = new TextBox
                {
                    Top = controlsTop,
                    Left = 330,
                    ForeColor = color,
                    TextAlign = HorizontalAlignment.Right,
                };
                
                _panel.Controls.Add(_enabledCheckBox);
                _panel.Controls.Add(_counterNameTextBox);
                _panel.Controls.Add(_baseLineTextBox);
                _panel.Controls.Add(_deltaFromBaseTextBox);
                _panel.Controls.Add(_lastValueTextBox);
                
            }

            public void ResetBaseLine()
            {
                BaseLine = _lastValue;
            }
            long BaseLine
            {
                get { return _baseLine; }
                set
                {
                    _baseLine = value;
                    _scale = 10;
                    while (_baseLine * 2 > _scale)
                        _scale *= 2;
                    _baseLineTextBox.Text = _baseLine.ToString(format);
                }
            }

            public void OnPaint(int Height, int Width, Graphics Graphics)
            {
                if (_enabled)
                {
                    var points = new List<Point>();
                    int count = 0;

                    for (int i = _position; i < _readouts.Length; i++)
                    {
                        points.Add(new Point(count++ * Width / _readouts.Length, Height - (int)(_readouts[i] * Height / _scale)));
                    }
                    for (int i = 0; i < _position; i++)
                    {
                        points.Add(new Point(count++ * Width / _readouts.Length, Height - (int)(_readouts[i] * Height / _scale)));
                    }
                    Graphics.DrawLines(_pen, points.ToArray());
                    int height = Height - (int)(BaseLine * Height * _factor / _scale);
                    Graphics.DrawLine(_baseLinePen, 0, height, Width, height);
                }
            }

            public void ReevaluateData()
            {
                long value = _getVal();
                _lastValue = value;
                _readouts[_position++] = value * _factor;
                if (_position >= _readouts.Length)
                    _position = 0;
                while (value > _scale)
                {
                    _scale *= 2;
                }
                _lastValueTextBox.Text = _lastValue.ToString(format);
                _deltaFromBaseTextBox.Text = (_lastValue - BaseLine).ToString(format);
            }
        }
    }

    partial class MemoryMonitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MemoryMonitor
            // 
            this.ClientSize = new System.Drawing.Size(600, 900);
            this.Name = "MemoryMonitor";
            this.ResumeLayout(false);

            this.panel1 = new System.Windows.Forms.Panel();
            this.setBaseLineButton = new System.Windows.Forms.Button();
            this.baseLineLable = new System.Windows.Forms.Label();
            this.deltaFromBaseLabel = new System.Windows.Forms.Label();
            this.lastValueLable = new System.Windows.Forms.Label();
            this.factorLable = new Label();
            this.counterLable = new Label();
            this.graph = new GraphControl(this.panel1);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(counterLable);
            this.panel1.Controls.Add(this.baseLineLable);
            this.panel1.Controls.Add(this.deltaFromBaseLabel);
            this.panel1.Controls.Add(this.lastValueLable);
            this.panel1.Controls.Add(this.factorLable);
            this.panel1.Controls.Add(this.setBaseLineButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 342);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 150);
            this.panel1.TabIndex = 0;
            // 
            // setBaseLineButton
            // 
            this.setBaseLineButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.setBaseLineButton.Location = new System.Drawing.Point(690, 9);
            this.setBaseLineButton.Name = "setBaseLineButton";
            this.setBaseLineButton.Size = new System.Drawing.Size(100, 25);
            this.setBaseLineButton.TabIndex = 5;
            this.setBaseLineButton.Text = "Set Base Line";
            this.setBaseLineButton.UseVisualStyleBackColor = true;
            this.setBaseLineButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // counterLable
            // 
            this.counterLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.counterLable.AutoSize = true;
            this.counterLable.Location = new System.Drawing.Point(30, 9);
            this.counterLable.Name = "counterLable";
            this.counterLable.Size = new System.Drawing.Size(80, 13);
            this.counterLable.TabIndex = 4;
            this.counterLable.Text = "Counter";
            // 
            // baseLineLable
            // 
            this.baseLineLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.baseLineLable.AutoSize = true;
            this.baseLineLable.Location = new System.Drawing.Point(130, 9);
            this.baseLineLable.Name = "baseLineLable";
            this.baseLineLable.Size = new System.Drawing.Size(80, 13);
            this.baseLineLable.TabIndex = 4;
            this.baseLineLable.Text = "Base Line";
            // 
            // deltaFromBaseLabel
            // 
            this.deltaFromBaseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.deltaFromBaseLabel.AutoSize = true;
            this.deltaFromBaseLabel.Location = new System.Drawing.Point(230, 9);
            this.deltaFromBaseLabel.Name = "deltaFromBaseLabel";
            this.deltaFromBaseLabel.Size = new System.Drawing.Size(80, 13);
            this.deltaFromBaseLabel.TabIndex = 4;
            this.deltaFromBaseLabel.Text = "Delta From Base";
            // 
            // lastValueLable
            // 
            this.lastValueLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.lastValueLable.AutoSize = true;
            this.lastValueLable.Location = new System.Drawing.Point(330, 9);
            this.lastValueLable.Name = "lastValueLable";
            this.lastValueLable.Size = new System.Drawing.Size(80, 13);
            this.lastValueLable.TabIndex = 2;
            this.lastValueLable.Text = "Last Value";
            // 
            // factorLable
            // 
            this.factorLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.factorLable.AutoSize = true;
            this.factorLable.Location = new System.Drawing.Point(430, 9);
            this.factorLable.Name = "factorValueLable";
            this.factorLable.Size = new System.Drawing.Size(80, 13);
            this.factorLable.TabIndex = 2;
            this.factorLable.Text = "Factor";
            // 
            // graph
            // 
            this.graph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graph.Location = new System.Drawing.Point(0, 0);
            this.graph.Name = "graph";
            this.graph.Size = new System.Drawing.Size(800, 480);
            this.graph.TabIndex = 1;
            // 
            // MemoryMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.graph);
            this.Controls.Add(this.panel1);
            this.Name = "MemoryMonitor";
            this.Text = "MemoryMonitor";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private GraphControl graph;
        private System.Windows.Forms.Label deltaFromBaseLabel;
        private System.Windows.Forms.Label lastValueLable;
        private System.Windows.Forms.Label baseLineLable;
        private System.Windows.Forms.Label counterLable;
        private System.Windows.Forms.Label factorLable;
        private System.Windows.Forms.Button setBaseLineButton;
    }
}