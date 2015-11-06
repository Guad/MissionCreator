using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace ContentCreator.UI
{
    public class TimerBars
    {
        private Dictionary<string, Tuple<string, bool, float>> _valueDictionary;

        public TimerBars()
        {
            _valueDictionary = new Dictionary<string, Tuple<string, bool, float>>();
        }

        public void UpdateValue(string id, string label, bool bar, float value)
        {
            if (_valueDictionary.ContainsKey(id))
            {
                _valueDictionary[id] = new Tuple<string, bool, float>(label, bar, value);
            }
            else
            {
                _valueDictionary.Add(id, new Tuple<string, bool, float>(label, bar, value));
            }
        }

        public void Draw()
        {
            var i = 0;
            foreach (var pair in _valueDictionary)
            {
                var res = UIMenu.GetScreenResolutionMantainRatio();
                var safe = UIMenu.GetSafezoneBounds();

                const int interval = 45;
                
                new ResText(pair.Value.Item1, new Point(Convert.ToInt32(res.Width) - safe.X - 90, Convert.ToInt32(res.Height) - safe.Y - (90 + (i * interval))), 0.3f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Right).Draw();
                //if(!pair.Value.Item2)
                    new ResText((pair.Value.Item3*100).ToString("###") + "%", new Point(Convert.ToInt32(res.Width) - safe.X - 20, Convert.ToInt32(res.Height) - safe.Y - (102 + (i * interval))), 0.5f, Color.White, Common.EFont.ChaletLondon, ResText.Alignment.Right).Draw();
                /*else
                {
                    new Sprite("timerbars", "damagebarfill_64", new Point(Convert.ToInt32(res.Width) - safe.X - 150, Convert.ToInt32(res.Height) - safe.Y - (110 + (i * interval))), new Size((int)(pair.Value.Item3*100), 37), 0f, Color.FromArgb(180, 255, 255, 255)).Draw();
                    new Sprite("timerbars", "damagebar_64", new Point(Convert.ToInt32(res.Width) - safe.X - 150, Convert.ToInt32(res.Height) - safe.Y - (110 + (i * interval))), new Size(100, 37), 0f, Color.FromArgb(180, 255, 255, 255)).Draw();
                }*/
                new Sprite("timerbars", "all_black_bg", new Point(Convert.ToInt32(res.Width) - safe.X - 248, Convert.ToInt32(res.Height) - safe.Y - (100 + (i * interval))), new Size(250, 37), 0f, Color.FromArgb(180, 255, 255, 255)).Draw();
                i++;
            }
        }
    }
}