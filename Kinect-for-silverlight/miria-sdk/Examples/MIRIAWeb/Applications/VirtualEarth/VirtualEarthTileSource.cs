using System;
using System.Text;
using System.Windows.Media;

using System.Windows.Browser;
using System.IO;

namespace MIRIAWeb.Applications.VirtualEarth
{
    // Example Code from
    // http://soulsolutions.com.au/Blog/tabid/73/EntryID/471/Default.aspx

	public class VirtualEarthTileSource : MultiScaleTileSource
	{
		const string Protocol = "http://";
		const string TilePath = ".ortho.tiles.virtualearth.net/tiles/";
		const string Prefix = "h";
		const string Suffix = ".jpeg?g=159";

        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// ref. http://msdn.microsoft.com/en-us/library/bb259689.aspx
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>A string containing the QuadKey.</returns>
        static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
        {
			StringBuilder quadKey = new StringBuilder();
			for (int i = levelOfDetail; i > 0; i--) {
				char digit = '0';
				int mask = 1 << (i - 1);
				if ((tileX & mask) != 0) {
					digit++;
				}
				if ((tileY & mask) != 0) {
					digit++;
					digit++;
				}
				quadKey.Append(digit);
			}
			return quadKey.ToString();
		}

		protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY, System.Collections.Generic.IList<object> tileImageLayerSources) {
            string basepath = Path.GetDirectoryName(HtmlPage.Document.DocumentUri.OriginalString).Replace("\\", "/").Replace("http:/", "http://");
            int zoom = tileLevel - 8;
			if (zoom > 0)
            {
				string QuadKey = TileXYToQuadKey(tilePositionX, tilePositionY, zoom);
				string VEUrl = Protocol + Prefix + QuadKey[QuadKey.Length - 1] + TilePath + Prefix + QuadKey + Suffix;
				tileImageLayerSources.Add(new Uri(VEUrl));
			} else {
                string localPath = string.Format(basepath + "/VE_files/{0}/{1}_{2}.jpg", tileLevel, tilePositionX, tilePositionY);
				Uri localUri = new Uri(localPath);
				tileImageLayerSources.Add(localUri);
			}
		}

		public VirtualEarthTileSource()
			: base(134217728, 134217728, 256, 256, 0) {
		}
	}
}
