﻿


    enum HRESULT : uint
    {
        S_FALSE = 0x0001,
        S_OK = 0x00000000,
        E_INVALIDARG = 0x80070057,
        E_OUTOFMEMORY = 0x8007000E
    }

    enum NUI_INITIALIZE_FLAG : uint
    {
        USES_AUDIO = 0x10000000,
        USES_DEPTH_AND_PLAYER_INDEX = 0x00000001,
        USES_COLOR = 0x00000002,
        USES_SKELETON = 0x00000008,
        USES_DEPTH = 0x00000020,
    }


    enum NUI_IMAGE_TYPE
    {
        NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX	= 0,
	    NUI_IMAGE_TYPE_COLOR	= ( NUI_IMAGE_TYPE_DEPTH_AND_PLAYER_INDEX + 1 ) ,
	    NUI_IMAGE_TYPE_COLOR_YUV	= ( NUI_IMAGE_TYPE_COLOR + 1 ) ,
	    NUI_IMAGE_TYPE_COLOR_RAW_YUV	= ( NUI_IMAGE_TYPE_COLOR_YUV + 1 ) ,
	    NUI_IMAGE_TYPE_DEPTH	= ( NUI_IMAGE_TYPE_COLOR_RAW_YUV + 1 ) 
    }

    enum NUI_IMAGE_RESOLUTION
    {	
        NUI_IMAGE_RESOLUTION_INVALID	= -1,
	    NUI_IMAGE_RESOLUTION_80x60	= 0,
	    NUI_IMAGE_RESOLUTION_320x240	= ( NUI_IMAGE_RESOLUTION_80x60 + 1 ) ,
	    NUI_IMAGE_RESOLUTION_640x480	= ( NUI_IMAGE_RESOLUTION_320x240 + 1 ) ,
	    NUI_IMAGE_RESOLUTION_1280x960	= ( NUI_IMAGE_RESOLUTION_640x480 + 1 ) 
    }