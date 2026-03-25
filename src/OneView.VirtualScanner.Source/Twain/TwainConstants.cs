namespace OneView.VirtualScanner.Source.Twain;

public static class DG
{
    public const ushort CONTROL = 0x0001;
    public const ushort IMAGE   = 0x0002;
}

public static class DAT
{
    // DG_CONTROL
    public const ushort NULL            = 0x0000;
    public const ushort CAPABILITY      = 0x0001;
    public const ushort EVENT           = 0x0002;
    public const ushort IDENTITY        = 0x0003;
    public const ushort PARENT          = 0x0004;
    public const ushort PENDINGXFERS    = 0x0005;
    public const ushort SETUPMEMXFER    = 0x0006;
    public const ushort SETUPFILEXFER   = 0x0007;
    public const ushort STATUS          = 0x0009;
    public const ushort USERINTERFACE   = 0x000B;
    public const ushort XFERGROUP       = 0x000D;
    public const ushort CUSTOMDSDATA    = 0x000C;
    public const ushort DEVICEEVENT     = 0x000E;
    public const ushort FILESYSTEM      = 0x000F;
    public const ushort PASSTHRU        = 0x0010;
    public const ushort CALLBACK        = 0x0011;
    public const ushort STATUSUTF8      = 0x0012;
    public const ushort METRICS         = 0x0013;
    public const ushort TWAINDIRECT     = 0x0014;
    // DG_IMAGE
    public const ushort IMAGEINFO       = 0x0101;
    public const ushort IMAGELAYOUT     = 0x0102;
    public const ushort IMAGEMEMXFER    = 0x0103;
    public const ushort IMAGENATIVEXFER = 0x0104;
    public const ushort IMAGEFILEXFER   = 0x0105;
    public const ushort CIECOLOR        = 0x0106;
    public const ushort GRAYRESPONSE    = 0x0107;
    public const ushort RGBRESPONSE     = 0x0108;
    public const ushort JPEGCOMPRESSION = 0x0109;
    public const ushort PALETTE8        = 0x010A;
    public const ushort EXTIMAGEINFO    = 0x010B;
    public const ushort FILTER          = 0x010C;
}

public static class MSG
{
    public const ushort NULL          = 0x0000;
    public const ushort CUSTOMBASE    = 0x8000;
    // generic
    public const ushort GET           = 0x0001;
    public const ushort GETCURRENT    = 0x0002;
    public const ushort GETDEFAULT    = 0x0003;
    public const ushort GETFIRST      = 0x0004;
    public const ushort GETNEXT       = 0x0005;
    public const ushort SET           = 0x0006;
    public const ushort RESET         = 0x0007;
    public const ushort QUERYSUPPORT  = 0x0008;
    public const ushort GETHELP       = 0x0009;
    public const ushort GETLABEL      = 0x000A;
    public const ushort GETLABELENUM  = 0x000B;
    public const ushort SETCONSTRAINT = 0x000C;
    // DAT_IDENTITY
    public const ushort OPENDS        = 0x0064;
    public const ushort CLOSEDS       = 0x0065;
    public const ushort USERSELECT    = 0x0060;
    // DAT_USERINTERFACE
    public const ushort ENABLEDS      = 0x0065;
    public const ushort DISABLEDS     = 0x0066;
    public const ushort ENABLEDSUIONLY = 0x0067;
    // DAT_PENDINGXFERS
    public const ushort ENDXFER       = 0x0700;
    public const ushort STOPFEEDER    = 0x0701;
    // DAT_XFERGROUP
    public const ushort GETXFERGROUP  = 0x0001;
    // DAT_EVENT
    public const ushort PROCESSEVENT  = 0x0360;
}

public static class TWRC
{
    public const ushort SUCCESS        = 0x0000;
    public const ushort FAILURE        = 0x0001;
    public const ushort CHECKSTATUS    = 0x0002;
    public const ushort CANCEL         = 0x0003;
    public const ushort DSEVENT        = 0x0004;
    public const ushort NOTDSEVENT     = 0x0005;
    public const ushort XFERDONE       = 0x0006;
    public const ushort ENDOFLIST      = 0x0007;
    public const ushort INFONOTSUPPORTED = 0x0008;
    public const ushort DATANOTAVAILABLE = 0x0009;
    public const ushort BUSY           = 0x000A;
    public const ushort SCANNERLOCKED  = 0x000B;
}

public static class TWCC
{
    public const ushort SUCCESS        = 0x0000;
    public const ushort BUMMER         = 0x0001;
    public const ushort LOWMEMORY      = 0x0002;
    public const ushort NODS           = 0x0003;
    public const ushort MAXCONNECTIONS = 0x0004;
    public const ushort OPERATIONERROR = 0x0005;
    public const ushort BADCAP         = 0x0006;
    public const ushort BADPROTOCOL    = 0x000A;
    public const ushort BADVALUE       = 0x000B;
    public const ushort SEQERROR       = 0x000C;
    public const ushort BADDEST        = 0x000D;
    public const ushort CAPUNSUPPORTED = 0x000E;
    public const ushort CAPBADOPERATION = 0x000F;
    public const ushort CAPSEQERROR    = 0x0010;
    public const ushort DENIED         = 0x0011;
    public const ushort FILEEXISTS     = 0x0012;
    public const ushort FILENOTFOUND   = 0x0013;
    public const ushort NOTEMPTY       = 0x0014;
    public const ushort PAPERJAM       = 0x0015;
    public const ushort PAPERDOUBLEFEED = 0x0016;
    public const ushort FILEWRITEERROR = 0x0017;
    public const ushort CHECKDEVICEONLINE = 0x0018;
    public const ushort INTERLOCK      = 0x0019;
    public const ushort DAMAGEDCORNER  = 0x001A;
    public const ushort FOCUSERROR     = 0x001B;
    public const ushort DOCTOOLIGHT    = 0x001C;
    public const ushort DOCTOODARK     = 0x001D;
    public const ushort NOMEDIA        = 0x001E;
}

public static class CAP
{
    public const ushort XFERCOUNT        = 0x0001;
    public const ushort ICAP_COMPRESSION = 0x0100;
    public const ushort ICAP_PIXELTYPE   = 0x0101;
    public const ushort ICAP_UNITS       = 0x0102;
    public const ushort ICAP_XFERMECH    = 0x0103;
    public const ushort ICAP_AUTOBRIGHT  = 0x1100;
    public const ushort ICAP_BRIGHTNESS  = 0x1101;
    public const ushort ICAP_CONTRAST    = 0x1103;
    public const ushort ICAP_XRESOLUTION = 0x1118;
    public const ushort ICAP_YRESOLUTION = 0x1119;
    public const ushort ICAP_PHYSICALWIDTH  = 0x111A;
    public const ushort ICAP_PHYSICALHEIGHT = 0x111B;
    public const ushort ICAP_SUPPORTEDSIZES = 0x1107;
    public const ushort ICAP_FRAMES      = 0x1103;
    public const ushort FEEDERENABLED    = 0x1001;
    public const ushort FEEDERLOADED     = 0x1002;
    public const ushort PAPERFEEDORDER   = 0x1007;
    public const ushort DUPLEXENABLED    = 0x1013;
    public const ushort DUPLEX           = 0x1012;
    public const ushort UICONTROLLABLE   = 0x100E;
    public const ushort INDICATORS       = 0x100F;
    public const ushort SUPPORTEDCAPS    = 0x1001; // alias
    public const ushort CAP_SUPPORTEDCAPS = 0x0003;
    public const ushort CAP_AUTOFEED     = 0x1006;
    public const ushort CAP_CLEARPAGE    = 0x1008;
    public const ushort CAP_FEEDPAGE     = 0x1009;
    public const ushort CAP_REWINDPAGE   = 0x100A;
}

public static class TWTY
{
    public const ushort INT8   = 0x0000;
    public const ushort INT16  = 0x0001;
    public const ushort INT32  = 0x0002;
    public const ushort UINT8  = 0x0003;
    public const ushort UINT16 = 0x0004;
    public const ushort UINT32 = 0x0005;
    public const ushort BOOL   = 0x0006;
    public const ushort FIX32  = 0x0007;
    public const ushort FRAME  = 0x0008;
    public const ushort STR32  = 0x0009;
    public const ushort STR64  = 0x000A;
    public const ushort STR128 = 0x000B;
    public const ushort STR255 = 0x000C;
}

public static class TWON
{
    public const ushort ARRAY       = 0x0003;
    public const ushort ENUMERATION = 0x0004;
    public const ushort ONEVALUE    = 0x0005;
    public const ushort RANGE       = 0x0006;
}

public static class TWPT
{
    public const ushort BW    = 0x0000;
    public const ushort GRAY  = 0x0001;
    public const ushort RGB   = 0x0002;
    public const ushort PALETTE = 0x0003;
    public const ushort CMY   = 0x0004;
    public const ushort CMYK  = 0x0005;
    public const ushort YUV   = 0x0006;
    public const ushort YUVK  = 0x0007;
    public const ushort CIEXYZ = 0x0008;
}

public static class TWSX
{
    public const ushort NATIVE = 0x0000;
    public const ushort FILE   = 0x0001;
    public const ushort MEMORY = 0x0002;
    public const ushort MEMFILE = 0x0004;
}

public static class TWSS
{
    public const ushort NONE      = 0x0000;
    public const ushort A4LETTER  = 0x0001;
    public const ushort B5LETTER  = 0x0002;
    public const ushort USLETTER  = 0x0003;
    public const ushort USLEGAL   = 0x0004;
}
