/*
|| 
|| MIRIA - http://mono-mig.sourceforge.net
|| Copyright (C) 2008-2011 Generoso Martello <generoso@martello.com>
||
|| This program is free software: you can redistribute it and/or modify
|| it under the terms of the GNU General Public License as published by
|| the Free Software Foundation, either version 3 of the License, or
|| (at your option) any later version.
||  
|| This program is distributed in the hope that it will be useful,
|| but WITHOUT ANY WARRANTY; without even the implied warranty of
|| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
|| GNU General Public License for more details.
|| 
|| You should have received a copy of the GNU General Public License
|| along with this program. If not, see
|| <http://www.gnu.org/licenses/>.
|| 
*/

using System;
using System.Windows;

namespace MIRIA.Interaction.MultiTouch
{

    public class FingerTouchEventArgs
    {
        public Finger Finger { get; set; }
        public bool Handled { get; set; }

        public FingerTouchEventArgs(Finger fingerinfo)
        {
            Finger = new Finger(fingerinfo.Identifier, fingerinfo.StartPosition)
            {
                Position = fingerinfo.Position
            };
        }
    }

    public class Finger
    {
        private string _identifier;
        private Point _postition;
        private Point _startposition;
        private Point _shift;

        public Finger Clone()
        {
            Finger f = new Finger(_identifier, _startposition);
            f.Position = _postition;
            f.Shift = _shift;
            return f;
        }

        public Finger(string id, Point p)
        {
            this._identifier = id;
            _postition = _startposition = p;
            _shift = new Point();
        }
        public string Identifier
        {
            get { return _identifier; }
        }
        public Point Shift
        {
            get { return _shift; }
            set { _shift = value; }
        }
        public double PathLength
        {
            get
            {
                double l = MIG.Utility.Vector2D.Distance(_startposition, _postition);
                return l;
            }
        }
        public double PathAngle
        {
            get
            {
                double a = MIG.Utility.Vector2D.GetAngle(_startposition, _postition);
                return a;
            }
        }
        public Point StartPosition
        {
            get
            {
                return _startposition;
            }
        }
        public Point Position
        {
            get
            {
                return _postition;
            }
            set
            {
                _shift = new Point(value.X - _postition.X, value.Y - _postition.Y);
                _postition = value;
            }
        }
    }
    public struct FingersStartInfo
    {
        public Point A;
        public Point B;
        public Point C;

        public FingersStartInfo(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
