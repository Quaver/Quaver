using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Color = System.Drawing.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Quaver.Graphics.Graphing
{
    internal static class Graph
    {
        /// <summary>
        ///     Creates a scatter plot from a list of points and returns a
        ///     Texture2D of it. This method is VERY slow, so if you're going to use this,
        ///     be sure to pre-load it an do it on another thread or task, because it's literally...
        ///     very slow. Needs a more efficient method, but it was good in theory.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="size"></param>
        /// <param name="bgColor"></param>
        /// <param name="dotWidth"></param>
        /// <param name="customLines"></param>
        /// <param name="customDotColorRange"></param>
        /// <returns></returns>
        internal static Texture2D CreateStaticScatterPlot(List<Point> points, Vector2 size, Color bgColor,
            int dotWidth, Dictionary<int, Color> customLines = null, Dictionary<int, Color> customDotColorRange = null)
        {
            // Create the graphics of the chart.
            var chart = new Chart
            {
                Size = new Size((int) size.X, (int) size.Y),
                ChartAreas =
                {
                    new ChartArea
                    {
                        BackColor = bgColor,
                        Position = new ElementPosition(0, 0, 100, 100),
                        AxisX = { Enabled = AxisEnabled.False },
                        AxisY =
                        {
                            LabelStyle = { Enabled = false },
                            MajorTickMark = { Enabled = false },
                            MinorTickMark = { Enabled = false },
                            MajorGrid =
                            {
                                Enabled = false,
                                LineWidth = 0
                            },
                            LineWidth = 0
                        }
                    },
                },
                Series =
                {
                    new Series
                    {
                        Name = "Series1",
                        ChartType = SeriesChartType.Point
                    }
                }
            };

            chart.Series["Series1"].Points.DataBindXY(points.Select(x => x.X).ToList(), points.Select(x => x.Y).ToList());

            for (var i = 0; i < chart.Series["Series1"].Points.Count; i++)
            {
                var point = chart.Series["Series1"].Points[i];
                point.MarkerSize = dotWidth;

                if (customLines != null)
                {
                    foreach (var line in customLines)
                    {
                        chart.ChartAreas.First().AxisY.StripLines.Add(new StripLine()
                        {
                            Interval = 0,
                            IntervalOffset = line.Key,
                            StripWidth = 0.1f,
                            BackColor = line.Value
                        });
                    }

                    // Set minimum and maximum based on the custom lines.
                    chart.ChartAreas.First().AxisY.IsStartedFromZero = false;
                    chart.ChartAreas.First().AxisY.Minimum = customLines.Keys.Min() - 10; //
                    chart.ChartAreas.First().AxisY.Maximum = customLines.Keys.Max() + 10;
                }

                if (customDotColorRange != null)
                {
                    // Turn the dictionary into a list and sort it so we get the correct values.
                    var list = customDotColorRange.Keys.ToList();
                    list.Sort();

                    foreach (var key in list)
                    {
                        if (points[i].Y > key)
                            continue;

                        point.Color = customDotColorRange[key];
                        break;
                    }
                }
            }

            // Draw
            chart.Invalidate();

            var bm = new Bitmap((int) size.X - 1, (int) size.Y - 1);
            chart.DrawToBitmap(bm, new Rectangle(0, 0, (int) size.X, (int)size.Y));

            return ResourceHelper.LoadTexture2DFromPng(bm);
        }
    }
}