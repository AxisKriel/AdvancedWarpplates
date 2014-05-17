using System;
using System.Collections.Generic;
namespace PluginTemplate
{
	public class Warpplate
	{
		public Rectangle Area
		{
			get;
			set;
		}
		public Vector2 WarpplatePos
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}
		public string WarpDest
		{
			get;
			set;
		}
		public bool DisableBuild
		{
			get;
			set;
		}
		public string WorldID
		{
			get;
			set;
		}
		public List<int> AllowedIDs
		{
			get;
			set;
		}
		public int Delay
		{
			get;
			set;
		}
		public string Label
		{
			get;
			set;
		}
		public Warpplate(Vector2 warpplatepos, Rectangle Warpplate, string name, string warpdest, bool disablebuild, string WarpplateWorldIDz, string label) : this()
		{
			this.WarpplatePos = warpplatepos;
			this.Area = Warpplate;
			this.Name = name;
			this.Label = label;
			this.WarpDest = warpdest;
			this.DisableBuild = disablebuild;
			this.WorldID = WarpplateWorldIDz;
			this.Delay = 4;
		}
		public Warpplate()
		{
			this.WarpplatePos = Vector2.Zero;
			this.Area = Rectangle.Empty;
			this.Name = string.Empty;
			this.WarpDest = string.Empty;
			this.DisableBuild = true;
			this.WorldID = string.Empty;
			this.AllowedIDs = new List<int>();
		}
		public bool InArea(Rectangle point)
		{
			return this.Area.Contains(point.X, point.Y);
		}
	}
}
