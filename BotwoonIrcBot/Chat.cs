using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Botwoon.Data;

namespace BotwoonIrcBot
{
    public partial class Chat : Form
    {
        private string lastVerb;
        private string lastText;

        public Chat()
        {
            InitializeComponent();
        }

        public void AddChat(string sender, string text)
        {
            var output = "";
            var verb = "";

            if (text.StartsWith("ACTION "))
                output = string.Format("{0} {1}", sender, text.Substring(8));
            else if (text == lastText)
            {
                var verbList = new[] { "repeats", "echoes", "reiterates" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);

                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.EndsWith("!"))
            {
                var verbList = new[] {"exclaims", "shouts", "yells", "screams", "booms", "calls"};

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.EndsWith("?"))
            {
                var verbList = new[] { "asks", "inquires", "requests", "begs the question" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.ToLower().StartsWith("hm"))
            {
                var verbList = new[] { "thinks", "wonders", "ponders" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.ToLower().StartsWith("http://") || text.ToLower().StartsWith("https://") || text.ToLower().StartsWith("ftp://"))
            {
                var verbList = new[] { "links", "shares", "posts" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.ToLower().StartsWith("lol") || text.ToLower().StartsWith("heh") || text.ToLower().StartsWith("lmao") || text.ToLower().StartsWith("rofl"))
            {
                var verbList = new[] { "laughs", "rejoices", "giggles", "cheers", "smirks" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (text.ToLower().StartsWith("!"))
            {
                var verbList = new[] { "commands", "orders", "dictates", "insists" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else if (lastText.EndsWith("?"))
            {
                var verbList = new[] { "answers", "replies", "responds", "acknowledges", "explains" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);

                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }
            else
            {
                var verbList = new[] { "says", "states", "remarks", "reports", "adds" };

                do
                {
                    verb = verbList[(Randomizer.GetRandomizer().GetRandomNumber(verbList.Count() - 1))];
                } while (verb == lastVerb);
    
                output = string.Format("{0} {1}, \"{2}\"", sender, verb, text);
            }

            lastText = text;
            lastVerb = verb;
            
            CustomLabel textItem = new CustomLabel();
            textItem.Dock = DockStyle.Bottom;
            textItem.Font = new Font("Eras Demi ITC", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textItem.Countdown = Settings.Default.chatDisplay;
            textItem.AutoSize = false;

            Graphics graphics = Graphics.FromImage(new Bitmap(1, 1));
            textItem.Height = (int)(graphics.MeasureString(output, textItem.Font, Width).Height);
            textItem.Text = output;
            Controls.Add(textItem);
        }

        private void fadeTimer_Tick(object sender, EventArgs e)
        {
            foreach (var control in Controls)
            {
                if (control is CustomLabel)
                {
                    var textItem = (CustomLabel) control;
                    textItem.Countdown -= 1;
                    if (textItem.Countdown < 1)
                    {
                        Controls.Remove(textItem);
                        textItem.Dispose();
                    }
                }
            }
        }
    }

    public class CustomLabel : Label
    {
        public CustomLabel()
        {
            OutlineForeColor = Color.White;
            OutlineWidth = 2;
        }

        public int Countdown { get; set; }
        public Color OutlineForeColor { get; set; }
        public float OutlineWidth { get; set; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            using (GraphicsPath gp = new GraphicsPath())
            using (Pen outline = new Pen(OutlineForeColor, OutlineWidth) { LineJoin = LineJoin.Round })
            using (StringFormat sf = new StringFormat())
            using (Brush foreBrush = new SolidBrush(ForeColor))
            {
                gp.AddString(Text, Font.FontFamily, (int)Font.Style, Font.Size, new Rectangle(0, 0, (int)(ClientRectangle.Width / 1.3f), (int)(ClientRectangle.Height / 1.35f)), sf);
                e.Graphics.ScaleTransform(1.3f, 1.35f);
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawPath(outline, gp);
                e.Graphics.FillPath(foreBrush, gp);
            }
        }
    }
}
