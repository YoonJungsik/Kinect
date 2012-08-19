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

namespace Mosallem.KinectSilverlightLibrary
{
    public enum ImageType : int
    {
        DepthAndPlayerIndex = 0,//NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX 
        Color,//NUI_IMAGE_TYPE_COLOR
        ColorYuv,//NUI_IMAGE_TYPE_COLOR_YUV
        ColorYuvRaw,//NUI_IMAGE_TYPE_COLOR_RAW_YUV
        Depth,//NUI_IMAGE_TYPE_DEPTH
        DepthAndPlayerIndexInColorSpace,//NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX_IN_COLOR_SPACE
        DepthInColorSpace,//NUI_IMAGE_TYPE_DEPTH_IN_COLOR_SPACE
        ColorInDepthSpace,//NUI_IMAGE_TYPE_COLOR_IN_DEPTH_SPACE
    }
}
