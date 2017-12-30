using System.Collections.Generic;
using System.Diagnostics;

namespace osu_database_reader.Components.HitObjects
{
    public class HitObjectSlider : HitObject
    {
        public CurveType CurveType;
        public List<Vector2> Points = new List<Vector2>();  //does not include initial point!
        public int RepeatCount;
        public double Length; //seems to be length in o!p, so it doesn't have to be calculated?

        public void ParseSliderSegments(string sliderString)
        {
            string[] split = sliderString.Split('|');
            foreach (var s in split) {
                if (s.Length == 1) {   //curve type
                    switch (s[0]) {
                        case 'L':
                            CurveType = CurveType.Linear;
                            break;
                        case 'C':
                            CurveType = CurveType.Catmull;
                            break;
                        case 'P':
                            CurveType = CurveType.Perfect;
                            break;
                        case 'B':
                            CurveType = CurveType.Bezier;
                            break;
                    }
                    continue;
                }
                string[] split2 = s.Split(':');
                Debug.Assert(split2.Length == 2);

                Points.Add(new Vector2(
                    int.Parse(split2[0]), 
                    int.Parse(split2[1])));
            }
        }
    }
}
