/*
    [MIRIA Project]
	ImageTileSource class for loading full high resolutions image
    as a MultiScaleImage.
    (requires Handler.ashx - see Demo Web Project)
	http://www.generoso.info/livingsilverdesk/

	Copyright (c) 2007-2008 Generoso Martello <generoso@martello.com>

	Permission is hereby granted, free of charge, to any person obtaining
	a copy of this software and associated documentation files
	(the "Software"), to deal in the Software without restriction,
	including without limitation the rights to use, copy, modify, merge,
	publish, distribute, sublicense, and/or sell copies of the Software,
	and to permit persons to whom the Software is furnished to do so,
	subject to the following conditions:

	The above copyright notice and this permission notice shall be
	included in all copies or substantial portions of the Software.

	Any person wishing to distribute modifications to the Software is
	requested to send the modifications to the original developer so that
	they can be incorporated into the canonical version.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
	EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
	IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
	ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
	CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
	WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

    
    Version History:
    
    14-09-2008: version 1.0
                - First public release
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

using System.Windows.Browser;
using System.IO;

namespace MIRIAWeb.Applications.MultiScaleImage
{
    public class ImageTileSource : MultiScaleTileSource
    {
        private string _basepath = "http://win.generoso.info/";
            //System.IO.Path.GetDirectoryName(HtmlPage.Document.DocumentUri.OriginalString).Replace("\\", "/").Replace("http:/", "http://");

        int _tileSizeX;
        int _tileSizeY;
        int _imageWidth;
        int _imageHeight;
        string _path;

        public ImageTileSource(string imagePath, int imageWith, int imageHeight, int tileSizeX, int tileSizeY) :
                                base(imageWith, imageHeight, tileSizeX, tileSizeY, 0)
        {
            _path = imagePath;
            _tileSizeX = tileSizeX;
            _tileSizeY = tileSizeY;
            _imageWidth = imageWith;
            _imageHeight = imageHeight;
        }

        protected override void GetTileLayers(int tileLevel, int tilePositionX, int tilePositionY,
                                                IList<object> tileImageLayerSources)
        {
            string source = string.Format(
              _basepath + "/MIRIA-Web/Handler.ashx?fn=tile" +
              "&tileLevel={0}&tilePositionX={1}&tilePositionY={2}&tileSizeX={3}&tileSizeY={4}" +
              "&imageWidth={5}&imageHeight={6}&path={7}",
              tileLevel, tilePositionX, tilePositionY, _tileSizeX, _tileSizeY,
              _imageWidth, _imageHeight, _path);

            Uri uri = new Uri(source, UriKind.Absolute);

            tileImageLayerSources.Add(uri);
        }
    }
}
