using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;
using Wobble.Assets;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Quaver.Graphics.Graphing
{
    public static class Graph
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
        public static Texture2D CreateStaticScatterPlot(List<Point> points, Vector2 size, Color bgColor,
            int dotWidth, Dictionary<int, Color> customLines = null, Dictionary<int, Color> customDotColorRange = null)
        {
            // Create the graphics of the chart.
            var chart = new Chart
            {
                Size = new Size((int)size.X, (int)size.Y),
                ChartAreas =
                {
                    new ChartArea
                    {
                        BackColor = bgColor,
                        Position = new ElementPosition(0, 0, 100, 100),
                        AxisX = {Enabled = AxisEnabled.False},
                        AxisY =
                        {
                            LabelStyle = {Enabled = false},
                            MajorTickMark = {Enabled = false},
                            MinorTickMark = {Enabled = false},
                            MajorGrid =
                            {
                                Enabled = false,
                                LineWidth = 0
                            },
                            LineWidth = 0
                        },
                        BorderColor = Colors.XnaToSystemDrawing(Colors.DarkGray),
                        BorderDashStyle = ChartDashStyle.Solid,
                        BorderWidth = 2
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

            chart.Series["Series1"].Points
                .DataBindXY(points.Select(x => x.X).ToList(), points.Select(x => x.Y).ToList());

            if (customLines != null)
            {
                foreach (var line in customLines)
                {
                    chart.ChartAreas.First().AxisY.StripLines.Add(new StripLine()
                    {
                        Interval = 0,
                        IntervalOffset = line.Key,
                        StripWidth = 0.1f,
                        BackColor = Color.FromArgb(line.Value.R / 4, line.Value.G / 4, line.Value.B / 4)
                    });
                }

                // Set minimum and maximum based on the custom lines.
                chart.ChartAreas.First().AxisY.IsStartedFromZero = false;
                chart.ChartAreas.First().AxisY.Minimum = customLines.Keys.Min() - 55;
                chart.ChartAreas.First().AxisY.Maximum = customLines.Keys.Max() + 55;
            }

            for (var i = 0; i < chart.Series["Series1"].Points.Count; i++)
            {
                var point = chart.Series["Series1"].Points[i];
                point.MarkerSize = dotWidth;

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

            var bm = new Bitmap((int)size.X - 1, (int)size.Y - 1);
            chart.DrawToBitmap(bm, new Rectangle(0, 0, (int)size.X, (int)size.Y));

            return AssetLoader.LoadTexture2D(bm, ImageFormat.Png);
        }

        /// <summary>
        ///     Creates a line graph with a set of points.
        /// </summary>
        /// <returns></returns>
        public static Texture2D CreateStaticLine(List<Point> points, Vector2 size, int lineWidth)
        {
            // Create the graphics of the chart.
            var chart = new Chart
            {
                Size = new Size((int)size.X, (int)size.Y),
                ChartAreas =
                {
                    new ChartArea
                    {
                        BackColor = Color.Black,
                        Position = new ElementPosition(0, 0, 100, 100),
                        AxisX = {Enabled = AxisEnabled.False},
                        AxisY =
                        {
                            LabelStyle = {Enabled = false},
                            MajorTickMark = {Enabled = false},
                            MinorTickMark = {Enabled = false},
                            MajorGrid =
                            {
                                Enabled = false,
                                LineWidth = 0
                            },
                            LineWidth = 0
                        },
                        BorderColor = Colors.XnaToSystemDrawing(Colors.DarkGray),
                        BorderDashStyle = ChartDashStyle.Solid,
                        BorderWidth = 2
                    },
                },
                Series =
                {
                    new Series
                    {
                        Name = "Series1",
                        ChartType = SeriesChartType.Line,
                        BorderWidth = lineWidth
                    }
                }
            };

            chart.Series["Series1"].Points.DataBindXY(points.Select(x => x.X).ToList(), points.Select(x => x.Y).ToList());

            foreach (var point in chart.Series["Series1"].Points)
            {
                if (point.YValues[0] >= 90)
                    point.Color = Color.Green;
                else if (point.YValues[0] >= 60)
                    point.Color = Color.Yellow;
                else if (point.YValues[0] >= 30)
                    point.Color = Color.Orange;
                else if (point.YValues[0] >= 0)
                    point.Color = Color.Red;
            }

            // draw!
            chart.Invalidate();

            // Draw chart to a bitmap.
            var bm = new Bitmap((int)size.X - 1, (int)size.Y - 1);
            chart.DrawToBitmap(bm, new Rectangle(0, 0, (int)size.X, (int)size.Y));

            return AssetLoader.LoadTexture2D(bm, ImageFormat.Png);
        }
    }
}
