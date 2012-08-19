<%@ WebHandler Language="C#" Class="Handler" %>
/*
    [LivingSilverDesk Project]
	LivingSilverDesk Demo - Web Handler
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
using System.Web;
using System.Drawing;

public class Handler : IHttpHandler {
        
    public void ProcessRequest (HttpContext context) {

        if (context.Request.QueryString.HasKeys())
        {
            string fn = context.Request.QueryString.Get("fn");
            string path = context.Server.MapPath(context.Request.QueryString.Get("path"));

            if (fn != null && fn.Equals("info"))
            {
                Bitmap bmpImage = new System.Drawing.Bitmap(path);
                context.Response.Write(bmpImage.Width + "x" + bmpImage.Height);
            }
            else if (fn != null && fn.Equals("resize"))
            {
                int width = context.Request.QueryString.Get("width")==null ? 0 : int.Parse(context.Request.QueryString.Get("width"));
                int height = context.Request.QueryString.Get("height")==null ? 0 : int.Parse(context.Request.QueryString.Get("height"));
                Bitmap bitmap = Resize(path, width, height);
                context.Response.ContentType = "img/png";
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.WriteTo(context.Response.OutputStream);
            }
            else if (fn != null && fn.Equals("tile"))
            {
                int tileLevel = int.Parse(context.Request.QueryString.Get("tileLevel"));
                int tilePosX = int.Parse(context.Request.QueryString.Get("tilePositionX"));
                int tilePosY = int.Parse(context.Request.QueryString.Get("tilePositionY"));
                int tileSizeX = int.Parse(context.Request.QueryString.Get("tileSizeX"));
                int tileSizeY = int.Parse(context.Request.QueryString.Get("tileSizeY"));
                int imageWidth = int.Parse(context.Request.QueryString.Get("imageWidth"));
                int imageHeight = int.Parse(context.Request.QueryString.Get("imageHeight"));
                Bitmap bitmap = DrawTile(path, tileLevel, tilePosX, tilePosY, tileSizeX, tileSizeY,
                    imageWidth, imageHeight);
                if (bitmap != null)
                {
                    context.Response.ContentType = "img/png";
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.WriteTo(context.Response.OutputStream);
                }
            }
        }
    }



    /// Bitmap or null
    public Bitmap Resize(string lcFilename, int lnWidth, int lnHeight)
    {
        System.Drawing.Bitmap bmpOut = null;
        try
        {
            Bitmap loBMP = new Bitmap(lcFilename);
            System.Drawing.Imaging.ImageFormat loFormat = loBMP.RawFormat;

            decimal lnRatio;
            int lnNewWidth = 0;
            int lnNewHeight = 0;

            if (loBMP.Width > loBMP.Height && lnWidth > 0)
            {
                lnRatio = (decimal)lnWidth / loBMP.Width;
                lnNewWidth = lnWidth;

                decimal lnTemp = loBMP.Height * lnRatio;
                lnNewHeight = (int)lnTemp;
            }
            else
            {
                lnRatio = (decimal)lnHeight / loBMP.Height;
                lnNewHeight = lnHeight;
                decimal lnTemp = loBMP.Width * lnRatio;
                lnNewWidth = (int)lnTemp;
            }
            bmpOut = new Bitmap(lnNewWidth, lnNewHeight);
            Graphics g = Graphics.FromImage(bmpOut);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.FillRectangle(Brushes.White, 0, 0, lnNewWidth, lnNewHeight);
            g.DrawImage(loBMP, 0, 0, lnNewWidth, lnNewHeight);
            loBMP.Dispose();
        }
        catch
        {
            return null;
        }
        return bmpOut;
    }


    private Bitmap DrawTile(string path, int tileLevel, int tilePosX, int tilePosY,
        int tileSizeX, int tileSizeY, int imageWidth, int imageHeight)
    {
        Bitmap bmpImage = new System.Drawing.Bitmap(path);

        int maxTileLevel = (int)Math.Max(
            Math.Ceiling(Math.Log(imageWidth, 2)), Math.Ceiling(Math.Log(imageHeight, 2)));

        int currentImageDivisor = maxTileLevel - tileLevel;

        int naturalWidth =
            Math.Max((int)(imageWidth / Math.Pow(2, currentImageDivisor)), tileSizeX);

        int naturalHeight =
            Math.Max((int)(imageHeight / Math.Pow(2, currentImageDivisor)), tileSizeY);

        Bitmap bb = new Bitmap(naturalWidth, naturalHeight);
        Graphics g = Graphics.FromImage((Image)bb);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(bmpImage, 0, 0, naturalWidth, naturalHeight);
        g.Dispose();

        Bitmap b = new Bitmap(tileSizeX, tileSizeY);
        try
        {
            int dx = tileSizeX;
            int dy = tileSizeY;
            if ((tilePosX * tileSizeX) + tileSizeX > naturalWidth) dx = tileSizeX - ((tilePosX * tileSizeX) + tileSizeX - naturalWidth);
            if ((tilePosY * tileSizeY) + tileSizeY > naturalHeight) dy = tileSizeY - ((tilePosY * tileSizeY) + tileSizeY - naturalHeight);
            b = bb.Clone(new System.Drawing.Rectangle(new Point(tilePosX * tileSizeX, tilePosY * tileSizeY), new Size(dx, dy)), bmpImage.PixelFormat);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine("R " + tileLevel + " " + tilePosX + " " + tilePosY);
            System.Diagnostics.Debug.WriteLine(tilePosX * tileSizeX + "," + tilePosY * tileSizeY + " - " + naturalWidth + "," + naturalHeight);
        }

        return (b);
    }
    
        
    public bool IsReusable {
        get {
            return true;
        }
    }

}