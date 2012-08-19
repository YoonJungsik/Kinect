using System;
using System.Net;
using System.Runtime.InteropServices;


namespace SilverlightApplication1
{


    [StructLayout(LayoutKind.Sequential)]
    public class NUI_IMAGE_FRAME
    {
        public UInt64 liTimeStamp;
        public uint dwFrameNumber;
        public NUI_IMAGE_TYPE eImageType;
        public NUI_IMAGE_RESOLUTION eResolution;
        public IntPtr pFrameTexture; //INuiFrameTexture へのポインタ
        public uint dwFrameFlags;
        public NUI_IMAGE_VIEW_AREA ViewArea;
    }

    public struct NUI_IMAGE_VIEW_AREA
    {
        public int Zoom;//eDigitalZoom_NotUsed
        public int CenterX;//lCenterX_NotUsed long(C++) -> int(C#)
        public int CenterY;//lCenterY_NotUsed long(C++) -> int(C#)
    }

    public struct NUI_SURFACE_DESC
    {
        public int Width;
        public int Height;
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }



    public interface INuiFrameTexture
    {
        [PreserveSig]
        int BufferLen();

        [PreserveSig]
        int Pitch();

        [PreserveSig]
        HRESULT LockRect(
            uint Level,

            [MarshalAs(UnmanagedType.LPWStr)]
            ref IntPtr pLockedRect, //NUI_LOCKED_RECT

            [MarshalAs(UnmanagedType.LPWStr)]
            ref IntPtr pRect,//

            uint Flags
        );

        [PreserveSig]
        HRESULT GetLevelDesc(
            [MarshalAs(UnmanagedType.LPWStr)]
            uint Level,

            [MarshalAs(UnmanagedType.LPWStr)]
            NUI_SURFACE_DESC pDesc
            );

        [PreserveSig]
        HRESULT UnlockRect(
            [MarshalAs(UnmanagedType.LPWStr)]
            uint Level
            );
    }


    //[StructLayout(LayoutKind.Sequential)]
    //public class NativePlanarImage : INuiFrameTexture
    //{
    //    public int BufferLen() { return 0; }

    //    public int Pitch() { return 0; }

    //    public HRESULT LockRect(
    //        uint Level,

    //        [MarshalAs(UnmanagedType.LPWStr)]
    //        ref IntPtr pLockedRect, //NUI_LOCKED_RECT

    //        [MarshalAs(UnmanagedType.LPWStr)]
    //        ref IntPtr pRect,//

    //        uint Flags
    //    ){return 0;}

    //    public HRESULT GetLevelDesc(
    //        [MarshalAs(UnmanagedType.LPWStr)]
    //        uint Level,

    //        [MarshalAs(UnmanagedType.LPWStr)]
    //        NUI_SURFACE_DESC pDesc
    //        ){return 0;}

    //    public HRESULT UnlockRect(
    //        [MarshalAs(UnmanagedType.LPWStr)]
    //        uint Level
    //        ){return 0;}
    //}

    //public delegate HRESULT CallBackLockRectProc(
    //uint Level
    //, ref IntPtr pLockedRect// NUI_LOCKED_RECT *pLockedRect
    //, ref IntPtr pRect // RECT *pRect
    //, uint Flags
    //);



    public struct NUI_LOCKED_RECT
    {
        public int Pitch;//
        public int size;//
        public IntPtr pBits;//
    }



    //public enum ImageType : int
    //{
    //    DepthAndPlayerIndex = 0,//NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX 
    //    Color,//NUI_IMAGE_TYPE_COLOR
    //    ColorYuv,//NUI_IMAGE_TYPE_COLOR_YUV
    //    ColorYuvRaw,//NUI_IMAGE_TYPE_COLOR_RAW_YUV
    //    Depth,//NUI_IMAGE_TYPE_DEPTH
    //    DepthAndPlayerIndexInColorSpace,//NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX_IN_COLOR_SPACE
    //    DepthInColorSpace,//NUI_IMAGE_TYPE_DEPTH_IN_COLOR_SPACE
    //    ColorInDepthSpace,//NUI_IMAGE_TYPE_COLOR_IN_DEPTH_SPACE
    //}

    //public enum ImageResolution : int
    //{
    //    Invalid = -1, //NUI_IMAGE_RESOLUTION_INVALID 
    //    Resolution80x60 = 0,//NUI_IMAGE_RESOLUTION_80x60 = 0
    //    Resolution320x240,// NUI_IMAGE_RESOLUTION_320x240
    //    Resolution640x480,// NUI_IMAGE_RESOLUTION_640x480
    //    Resolution1280x1024//NUI_IMAGE_RESOLUTION_1280x1024  
    //}
    //[StructLayout(LayoutKind.Sequential)]
    //public struct ImageViewArea
    //{
    //    public int Zoom;//eDigitalZoom_NotUsed
    //    public long CenterX;//lCenterX_NotUsed
    //    public long CenterY;//lCenterY_NotUsed

    //}


    //public class ImageFrame
    //{
    //    [StructLayout(LayoutKind.Sequential)]
    //    internal class NativeImageFrame
    //    {
    //        public UInt64 liTimeStamp;
    //        public uint dwFrameNumber;
    //        public ImageType eImageType;
    //        public ImageResolution eResolution;
    //        public IntPtr pFrameTexture;
    //        public uint dwFrameFlags_NotUsed;
    //        public ImageViewArea ViewArea_NotUsed;
    //    }

    //}




}
