namespace Pororoca.Domain.Features.Common;

public static class MimeTypesDetector
{
    public const string DefaultMimeTypeForText = "text/plain";
    public const string DefaultMimeTypeForHtml = "text/html";
    public const string DefaultMimeTypeForXml = "text/xml";
    public const string DefaultMimeTypeForJson = "application/json";
    public const string DefaultMimeTypeForProblemJson = "application/problem+json";
    public const string DefaultMimeTypeForJavascript = "application/javascript";
    public const string DefaultMimeTypeForBinary = "application/octet-stream";

    private static readonly string[] textMimeTypes = new[] {
        DefaultMimeTypeForJson,
        DefaultMimeTypeForProblemJson,
        DefaultMimeTypeForText,
        DefaultMimeTypeForHtml,
        DefaultMimeTypeForXml,
        DefaultMimeTypeForJavascript,
        "text/css",
        "text/csv",
        "application/xml"
    };

    private static readonly List<KeyValuePair<string, string>> _mappings = BuildMappings();

    public static readonly List<string> AllMimeTypes = _mappings
                                                       .DistinctBy(kv => kv.Value)
                                                       .Select(kv => kv.Value)
                                                       .ToList();

    /*
    MIME types list extracted from:
    https://www.freeformatter.com/mime-types-list.html
    */

    private static List<KeyValuePair<string, string>> BuildMappings() =>
        new()
        {
            // Some extensions need to be first because of file extension detection,
            // otherwise, other file extensions will be mapped first for content type "text/plain", or "text/html"
            new("txt", "text/plain"), // Text File
            new("html", "text/html"), // HyperText Markup Language (HTML)
            new("xml", "text/xml"), // XML - Extensible Markup Language
            new("xml", "application/xml"),
            new("jpeg", "image/jpeg"), // JPEG Image
            new("jpg", "image/jpeg"), // JPEG Image

            new("123", "application/vnd.lotus-1-2-3"), // Lotus 1-2-3
            new("3dm", "x-world/x-3dmf"),
            new("3dmf", "x-world/x-3dmf"),
            new("3dml", "text/vnd.in3d.3dml"), // In3D - 3DML
            new("3g2", "video/3gpp2"), // 3GP2
            new("3gp", "video/3gpp"), // 3GP
            new("7z", "application/x-7z-compressed"), // 7-Zip
            new("a", "application/octet-stream"),
            new("aab", "application/x-authorware-bin"), // Adobe (Macropedia) Authorware - Binary File
            new("aac", "audio/x-aac"), // Advanced Audio Coding (AAC)
            new("aam", "application/x-authorware-map"), // Adobe (Macropedia) Authorware - Map
            new("aas", "application/x-authorware-seg"), // Adobe (Macropedia) Authorware - Segment File
            new("abc", "text/vnd.abc"),
            new("abw", "application/x-abiword"), // AbiWord
            new("ac", "application/pkix-attr-cert"), // Attribute Certificate
            new("acc", "application/vnd.americandynamics.acc"), // Active Content Compression
            new("ace", "application/x-ace-compressed"), // Ace Archive
            new("acgi", "text/html"),
            new("acu", "application/vnd.acucobol"), // ACU Cobol
            new("adp", "audio/adpcm"), // Adaptive differential pulse-code modulation
            new("aep", "application/vnd.audiograph"), // Audiograph
            new("afl", "video/animaflex"),
            new("afp", "application/vnd.ibm.modcap"), // MO:DCA-P
            new("ahead", "application/vnd.ahead.space"), // Ahead AIR Application
            new("ai", "application/postscript"), // PostScript
            new("aif", "audio/aiff"),
            new("aif", "audio/x-aiff"), // Audio Interchange File Format
            new("aifc", "audio/aiff"),
            new("aifc", "audio/x-aiff"),
            new("aiff", "audio/aiff"),
            new("aiff", "audio/x-aiff"),
            new("aim", "application/x-aim"),
            new("aip", "text/x-audiosoft-intra"),
            new("air", "application/vnd.adobe.air-application-installer-package+zip"), // Adobe AIR Application
            new("ait", "application/vnd.dvb.ait"), // Digital Video Broadcasting
            new("ami", "application/vnd.amiga.ami"), // AmigaDE
            new("ani", "application/x-navi-animation"),
            new("aos", "application/x-nokia-9000-communicator-add-on-software"),
            new("apk", "application/vnd.android.package-archive"), // Android Package Archive
            new("application", "application/x-ms-application"), // Microsoft ClickOnce
            new("apr", "application/vnd.lotus-approach"), // Lotus Approach
            new("aps", "application/mime"),
            new("arc", "application/octet-stream"),
            new("arj", "application/arj"),
            new("arj", "application/octet-stream"),
            new("art", "image/x-jg"),
            new("asf", "video/x-ms-asf"), // Microsoft Advanced Systems Format (ASF)
            new("asm", "text/x-asm"),
            new("aso", "application/vnd.accpac.simply.aso"), // Simply Accounting
            new("asp", "text/asp"),
            new("asx", "application/x-mplayer2"),
            new("asx", "video/x-ms-asf"),
            new("asx", "video/x-ms-asf-plugin"),
            new("atc", "application/vnd.acucorp"), // ACU Cobol
            new("atom", "application/atom+xml"), // Atom Syndication Format
            new("atomcat", "application/atomcat+xml"), // Atom Publishing Protocol
            new("atomsvc", "application/atomsvc+xml"), // Atom Publishing Protocol Service Document
            new("atx", "application/vnd.antix.game-component"), // Antix Game Player
            new("au", "audio/basic"), // Sun Audio - Au file format
            new("au", "audio/x-au"),
            new("avi", "application/x-troff-msvideo"),
            new("avi", "video/avi"),
            new("avi", "video/msvideo"),
            new("avi", "video/x-msvideo"), // Audio Video Interleave (AVI)
            new("avs", "video/avs-video"),
            new("aw", "application/applixware"), // Applixware
            new("azf", "application/vnd.airzip.filesecure.azf"), // AirZip FileSECURE
            new("azs", "application/vnd.airzip.filesecure.azs"), // AirZip FileSECURE
            new("azw", "application/vnd.amazon.ebook"), // Amazon Kindle eBook format
            new("bcpio", "application/x-bcpio"), // Binary CPIO Archive
            new("bdf", "application/x-font-bdf"), // Glyph Bitmap Distribution Format
            new("bdm", "application/vnd.syncml.dm+wbxml"), // SyncML - Device Management
            new("bed", "application/vnd.realvnc.bed"), // RealVNC
            new("bh2", "application/vnd.fujitsu.oasysprs"), // Fujitsu Oasys
            new("bin", "application/mac-binary"),
            new("bin", "application/macbinary"),
            new("bin", "application/octet-stream"),
            new("bin", "application/x-binary"),
            new("bin", "application/x-macbinary"),
            new("bm", "image/bmp"),
            new("bmi", "application/vnd.bmi"), // BMI Drawing Data Interchange
            new("bmp", "image/bmp"), // Bitmap Image File
            new("bmp", "image/x-windows-bmp"),
            new("boo", "application/book"),
            new("book", "application/book"),
            new("box", "application/vnd.previewsystems.box"), // Preview Systems ZipLock/VBox
            new("boz", "application/x-bzip2"),
            new("bsh", "application/x-bsh"),
            new("btif", "image/prs.btif"), // BTIF
            new("bz", "application/x-bzip"), // Bzip Archive
            new("bz2", "application/x-bzip2"), // Bzip2 Archive
            new("c", "text/plain"),
            new("c", "text/x-c"), // C Source File
            new("c++", "text/plain"),
            new("c11amc", "application/vnd.cluetrust.cartomobile-config"), // ClueTrust CartoMobile - Config
            new("c11amz", "application/vnd.cluetrust.cartomobile-config-pkg"), // ClueTrust CartoMobile - Config Package
            new("c4g", "application/vnd.clonk.c4group"), // Clonk Game
            new("cab", "application/vnd.ms-cab-compressed"), // Microsoft Cabinet File
            new("car", "application/vnd.curl.car"), // CURL Applet
            new("cat", "application/vnd.ms-pki.seccat"), // Microsoft Trust UI Provider - Security Catalog
            new("cc", "text/plain"),
            new("cc", "text/x-c"),
            new("ccad", "application/clariscad"),
            new("cco", "application/x-cocoa"),
            new("ccxml", "application/ccxml+xml,"), // Voice Browser Call Control
            new("cdbcmsg", "application/vnd.contact.cmsg"), // CIM Database
            new("cdf", "application/cdf"),
            new("cdf", "application/x-cdf"),
            new("cdf", "application/x-netcdf"),
            new("cdkey", "application/vnd.mediastation.cdkey"), // MediaRemote
            new("cdmia", "application/cdmi-capability"), // Cloud Data Management Interface (CDMI) - Capability
            new("cdmic", "application/cdmi-container"), // Cloud Data Management Interface (CDMI) - Contaimer
            new("cdmid", "application/cdmi-domain"), // Cloud Data Management Interface (CDMI) - Domain
            new("cdmio", "application/cdmi-object"), // Cloud Data Management Interface (CDMI) - Object
            new("cdmiq", "application/cdmi-queue"), // Cloud Data Management Interface (CDMI) - Queue
            new("cdx", "chemical/x-cdx"), // ChemDraw eXchange file
            new("cdxml", "application/vnd.chemdraw+xml"), // CambridgeSoft Chem Draw
            new("cdy", "application/vnd.cinderella"), // Interactive Geometry Software Cinderella
            new("cer", "application/pkix-cert"), // Internet Public Key Infrastructure - Certificate
            new("cer", "application/x-x509-ca-cert"),
            new("cgm", "image/cgm"), // Computer Graphics Metafile
            new("cha", "application/x-chat"),
            new("chat", "application/x-chat"), // pIRCh
            new("chm", "application/vnd.ms-htmlhelp"), // Microsoft Html Help File
            new("chrt", "application/vnd.kde.kchart"), // KDE KOffice Office Suite - KChart
            new("cif", "chemical/x-cif"), // Crystallographic Interchange Format
            new("cii", "application/vnd.anser-web-certificate-issue-initiation"), // ANSER-WEB Terminal Client - Certificate Issue
            new("cil", "application/vnd.ms-artgalry"), // Microsoft Artgalry
            new("cla", "application/vnd.claymore"), // Claymore Data Files
            new("class", "application/java"),
            new("class", "application/java-byte-code"),
            new("class", "application/java-vm"), // Java Bytecode File
            new("class", "application/x-java-class"),
            new("clkk", "application/vnd.crick.clicker.keyboard"), // CrickSoftware - Clicker - Keyboard
            new("clkp", "application/vnd.crick.clicker.palette"), // CrickSoftware - Clicker - Palette
            new("clkt", "application/vnd.crick.clicker.template"), // CrickSoftware - Clicker - Template
            new("clkw", "application/vnd.crick.clicker.wordbank"), // CrickSoftware - Clicker - Wordbank
            new("clkx", "application/vnd.crick.clicker"), // CrickSoftware - Clicker
            new("clp", "application/x-msclip"), // Microsoft Clipboard Clip
            new("cmc", "application/vnd.cosmocaller"), // CosmoCaller
            new("cmdf", "chemical/x-cmdf"), // CrystalMaker Data Format
            new("cml", "chemical/x-cml"), // Chemical Markup Language
            new("cmp", "application/vnd.yellowriver-custom-menu"), // CustomMenu
            new("cmx", "image/x-cmx"), // Corel Metafile Exchange (CMX)
            new("cod", "application/vnd.rim.cod"),
            new("com", "application/octet-stream"),
            new("com", "text/plain"),
            new("conf", "text/plain"),
            new("cpio", "application/x-cpio"), // CPIO Archive
            new("cpp", "text/x-c"),
            new("cpt", "application/mac-compactpro"), // Compact Pro
            new("cpt", "application/x-compactpro"),
            new("cpt", "application/x-cpt"),
            new("crd", "application/x-mscardfile"), // Microsoft Information Card
            new("crl", "application/pkcs-crl"),
            new("crl", "application/pkix-crl"),
            new("crl", "application/pkix-crl"), // Internet Public Key Infrastructure - Certificate Revocation Lists
            new("crt", "application/pkix-cert"),
            new("crt", "application/x-x509-ca-cert"),
            new("crt", "application/x-x509-user-cert"),
            new("cryptonote", "application/vnd.rig.cryptonote"), // CryptoNote
            new("csh", "application/x-csh"),
            new("csh", "application/x-csh"), // C Shell Script
            new("csh", "text/x-script.csh"),
            new("csml", "chemical/x-csml"), // Chemical Style Markup Language
            new("csp", "application/vnd.commonspace"), // Sixth Floor Media - CommonSpace
            new("css", "application/x-pointplus"),
            new("css", "text/css"),
            new("css", "text/css"), // Cascading Style Sheets (CSS)
            new("csv", "text/csv"), // Comma-Seperated Values
            new("cu", "application/cu-seeme"), // CU-SeeMe
            new("curl", "text/vnd.curl"), // Curl - Applet
            new("cww", "application/prs.cww"),
            new("cxx", "text/plain"),
            new("dae", "model/vnd.collada+xml"), // COLLADA
            new("daf", "application/vnd.mobius.daf"), // Mobius Management Systems - UniversalArchive
            new("davmount", "application/davmount+xml"), // Web Distributed Authoring and Versioning
            new("dcr", "application/x-director"),
            new("dcurl", "text/vnd.curl.dcurl"), // Curl - Detached Applet
            new("dd2", "application/vnd.oma.dd2+xml"), // OMA Download Agents
            new("ddd", "application/vnd.fujixerox.ddd"), // Fujitsu - Xerox 2D CAD Data
            new("deb", "application/x-debian-package"), // Debian Package
            new("deepv", "application/x-deepv"),
            new("def", "text/plain"),
            new("der", "application/x-x509-ca-cert"), // X.509 Certificate
            new("dfac", "application/vnd.dreamfactory"), // DreamFactory
            new("dif", "video/x-dv"),
            new("dir", "application/x-director"), // Adobe Shockwave Player
            new("dis", "application/vnd.mobius.dis"), // Mobius Management Systems - Distribution Database
            new("djvu", "image/vnd.djvu"), // DjVu
            new("dl", "video/dl"),
            new("dl", "video/x-dl"),
            new("dmg", "application/x-apple-diskimage"), // Apple Disk Image
            new("dna", "application/vnd.dna"), // New Moon Liftoff/DNA
            new("doc", "application/msword"), // Microsoft Word
            new("docm", "application/vnd.ms-word.document.macroenabled.12"), // Microsoft Word - Macro-Enabled Document
            new("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"), // Microsoft Office - OOXML - Word Document
            new("dot", "application/msword"),
            new("dotm", "application/vnd.ms-word.template.macroenabled.12"), // Microsoft Word - Macro-Enabled Template
            new("dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"), // Microsoft Office - OOXML - Word Document Template
            new("dp", "application/commonground"),
            new("dp", "application/vnd.osgi.dp"), // OSGi Deployment Package
            new("dpg", "application/vnd.dpgraph"), // DPGraph
            new("dra", "audio/vnd.dra"), // DRA Audio
            new("drw", "application/drafting"),
            new("dsc", "text/prs.lines.tag"), // PRS Lines Tag
            new("dssc", "application/dssc+der"), // Data Structure for the Security Suitability of Cryptographic Algorithms
            new("dtb", "application/x-dtbook+xml"), // Digital Talking Book
            new("dtd", "application/xml-dtd"), // Document Type Definition
            new("dts", "audio/vnd.dts"), // DTS Audio
            new("dtshd", "audio/vnd.dts.hd"), // DTS High Definition Audio
            new("dump", "application/octet-stream"),
            new("dv", "video/x-dv"),
            new("dvi", "application/x-dvi"), // Device Independent File Format (DVI)
            new("dwf", "drawing/x-dwf (old)"),
            new("dwf", "model/vnd.dwf"), // Autodesk Design Web Format (DWF)
            new("dwg", "application/acad"),
            new("dwg", "image/vnd.dwg"), // DWG Drawing
            new("dwg", "image/x-dwg"),
            new("dxf", "application/dxf"),
            new("dxf", "image/vnd.dwg"),
            new("dxf", "image/vnd.dxf"), // AutoCAD DXF
            new("dxf", "image/x-dwg"),
            new("dxp", "application/vnd.spotfire.dxp"), // TIBCO Spotfire
            new("dxr", "application/x-director"),
            new("ecelp4800", "audio/vnd.nuera.ecelp4800"), // Nuera ECELP 4800
            new("ecelp7470", "audio/vnd.nuera.ecelp7470"), // Nuera ECELP 7470
            new("ecelp9600", "audio/vnd.nuera.ecelp9600"), // Nuera ECELP 9600
            new("edm", "application/vnd.novadigm.edm"), // Novadigm's RADIA and EDM products
            new("edx", "application/vnd.novadigm.edx"), // Novadigm's RADIA and EDM products
            new("efif", "application/vnd.picsel"), // Pcsel eFIF File
            new("ei6", "application/vnd.pg.osasli"), // Proprietary P&G Standard Reporting System
            new("el", "text/x-script.elisp"),
            new("elc", "application/x-bytecode.elisp (compiled elisp)"),
            new("elc", "application/x-elc"),
            new("eml", "message/rfc822"), // Email Message
            new("emma", "application/emma+xml"), // Extensible MultiModal Annotation
            new("env", "application/x-envoy"),
            new("eol", "audio/vnd.digital-winds"), // Digital Winds Music
            new("eot", "application/vnd.ms-fontobject"), // Microsoft Embedded OpenType
            new("eps", "application/postscript"),
            new("epub", "application/epub+zip"), // Electronic Publication
            new("es", "application/ecmascript"), // ECMAScript
            new("es", "application/x-esrehber"),
            new("es3", "application/vnd.eszigno3+xml"), // MICROSEC e-Szign?
            new("esf", "application/vnd.epson.esf"), // QUASS Stream Player
            new("etx", "text/x-setext"), // Setext
            new("evy", "application/envoy"),
            new("evy", "application/x-envoy"),
            new("exe", "application/octet-stream"),
            new("exe", "application/x-msdownload"), // Microsoft Application
            new("exi", "application/exi"), // Efficient XML Interchange
            new("ext", "application/vnd.novadigm.ext"), // Novadigm's RADIA and EDM products
            new("ez2", "application/vnd.ezpix-album"), // EZPix Secure Photo Album
            new("ez3", "application/vnd.ezpix-package"), // EZPix Secure Photo Album
            new("f", "text/plain"),
            new("f", "text/x-fortran"), // Fortran Source File
            new("f4v", "video/x-f4v"), // Flash Video
            new("f77", "text/x-fortran"),
            new("f90", "text/plain"),
            new("f90", "text/x-fortran"),
            new("fbs", "image/vnd.fastbidsheet"), // FastBid Sheet
            new("fcs", "application/vnd.isac.fcs"), // International Society for Advancement of Cytometry
            new("fdf", "application/vnd.fdf"), // Forms Data Format
            new("fe_launch", "application/vnd.denovo.fcselayout-link"), // FCS Express Layout Link
            new("fg5", "application/vnd.fujitsu.oasysgp"), // Fujitsu Oasys
            new("fh", "image/x-freehand"), // FreeHand MX
            new("fif", "application/fractals"),
            new("fif", "image/fif"),
            new("fig", "application/x-xfig"), // Xfig
            new("fli", "video/fli"),
            new("fli", "video/x-fli"), // FLI/FLC Animation Format
            new("flo", "application/vnd.micrografx.flo"), // Micrografx
            new("flo", "image/florian"),
            new("flv", "video/x-flv"), // Flash Video
            new("flw", "application/vnd.kde.kivio"), // KDE KOffice Office Suite - Kivio
            new("flx", "text/vnd.fmi.flexstor"), // FLEXSTOR
            new("fly", "text/vnd.fly"), // mod_fly / fly.cgi
            new("fm", "application/vnd.framemaker"), // FrameMaker Normal Format
            new("fmf", "video/x-atomic3d-feature"),
            new("fnc", "application/vnd.frogans.fnc"), // Frogans Player
            new("for", "text/plain"),
            new("for", "text/x-fortran"),
            new("fpx", "image/vnd.fpx"), // FlashPix
            new("fpx", "image/vnd.net-fpx"),
            new("frl", "application/freeloader"),
            new("fsc", "application/vnd.fsc.weblaunch"), // Friendly Software Corporation
            new("fst", "image/vnd.fst"), // FAST Search & Transfer ASA
            new("ftc", "application/vnd.fluxtime.clip"), // FluxTime Clip
            new("fti", "application/vnd.anser-web-funds-transfer-initiation"), // ANSER-WEB Terminal Client - Web Funds Transfer
            new("funk", "audio/make"),
            new("fvt", "video/vnd.fvt"), // FAST Search & Transfer ASA
            new("fxp", "application/vnd.adobe.fxp"), // Adobe Flex Project
            new("fzs", "application/vnd.fuzzysheet"), // FuzzySheet
            new("g", "text/plain"),
            new("g2w", "application/vnd.geoplan"), // GeoplanW
            new("g3", "image/g3fax"), // G3 Fax Image
            new("g3w", "application/vnd.geospace"), // GeospacW
            new("gac", "application/vnd.groove-account"), // Groove - Account
            new("gdl", "model/vnd.gdl"), // Geometric Description Language (GDL)
            new("geo", "application/vnd.dynageo"), // DynaGeo
            new("gex", "application/vnd.geometry-explorer"), // GeoMetry Explorer
            new("ggb", "application/vnd.geogebra.file"), // GeoGebra
            new("ggt", "application/vnd.geogebra.tool"), // GeoGebra
            new("ghf", "application/vnd.groove-help"), // Groove - Help
            new("gif", "image/gif"), // Graphics Interchange Format
            new("gim", "application/vnd.groove-identity-message"), // Groove - Identity Message
            new("gl", "video/gl"),
            new("gl", "video/x-gl"),
            new("gmx", "application/vnd.gmx"), // GameMaker ActiveX
            new("gnumeric", "application/x-gnumeric"), // Gnumeric
            new("gph", "application/vnd.flographit"), // NpGraphIt
            new("gqf", "application/vnd.grafeq"), // GrafEq
            new("gram", "application/srgs"), // Speech Recognition Grammar Specification
            new("grv", "application/vnd.groove-injector"), // Groove - Injector
            new("grxml", "application/srgs+xml"), // Speech Recognition Grammar Specification - XML
            new("gsd", "audio/x-gsm"),
            new("gsf", "application/x-font-ghostscript"), // Ghostscript Font
            new("gsm", "audio/x-gsm"),
            new("gsp", "application/x-gsp"),
            new("gss", "application/x-gss"),
            new("gtar", "application/x-gtar"), // GNU Tar Files
            new("gtm", "application/vnd.groove-tool-message"), // Groove - Tool Message
            new("gtw", "model/vnd.gtw"), // Gen-Trix Studio
            new("gv", "text/vnd.graphviz"), // Graphviz
            new("gxt", "application/vnd.geonext"), // GEONExT and JSXGraph
            new("gz", "application/x-compressed"),
            new("gz", "application/x-gzip"),
            new("gzip", "application/x-gzip"),
            new("gzip", "multipart/x-gzip"),
            new("h", "text/plain"),
            new("h", "text/x-h"),
            new("h261", "video/h261"), // H.261
            new("h263", "video/h263"), // H.263
            new("h264", "video/h264"), // H.264
            new("hal", "application/vnd.hal+xml"), // Hypertext Application Language
            new("hbci", "application/vnd.hbci"), // Homebanking Computer Interface (HBCI)
            new("hdf", "application/x-hdf"), // Hierarchical Data Format
            new("help", "application/x-helpfile"),
            new("hgl", "application/vnd.hp-hpgl"),
            new("hh", "text/plain"),
            new("hh", "text/x-h"),
            new("hlb", "text/x-script"),
            new("hlp", "application/hlp"),
            new("hlp", "application/winhlp"), // WinHelp
            new("hlp", "application/x-helpfile"),
            new("hlp", "application/x-winhelp"),
            new("hpg", "application/vnd.hp-hpgl"),
            new("hpgl", "application/vnd.hp-hpgl"),
            new("hpgl", "application/vnd.hp-hpgl"), // HP-GL/2 and HP RTL
            new("hpid", "application/vnd.hp-hpid"), // Hewlett Packard Instant Delivery
            new("hps", "application/vnd.hp-hps"), // Hewlett-Packard's WebPrintSmart
            new("hqx", "application/binhex"),
            new("hqx", "application/binhex4"),
            new("hqx", "application/mac-binhex"),
            new("hqx", "application/mac-binhex40"), // Macintosh BinHex 4.0
            new("hqx", "application/x-binhex40"),
            new("hqx", "application/x-mac-binhex40"),
            new("hta", "application/hta"),
            new("htc", "text/x-component"),
            new("htke", "application/vnd.kenameaapp"), // Kenamea App
            new("htm", "text/html"),
            new("htmls", "text/html"),
            new("htt", "text/webviewhtml"),
            new("htx", "text/html"),
            new("hvd", "application/vnd.yamaha.hv-dic"), // HV Voice Dictionary
            new("hvp", "application/vnd.yamaha.hv-voice"), // HV Voice Parameter
            new("hvs", "application/vnd.yamaha.hv-script"), // HV Script
            new("i2g", "application/vnd.intergeo"), // Interactive Geometry Software
            new("icc", "application/vnd.iccprofile"), // ICC profile
            new("ice", "x-conference/x-cooltalk"), // CoolTalk
            new("ico", "image/x-icon"), // Icon Image
            new("ics", "text/calendar"), // iCalendar
            new("idc", "text/plain"),
            new("ief", "image/ief"), // Image Exchange Format
            new("iefs", "image/ief"),
            new("ifm", "application/vnd.shana.informed.formdata"), // Shana Informed Filler
            new("iges", "application/iges"),
            new("iges", "model/iges"),
            new("igl", "application/vnd.igloader"), // igLoader
            new("igm", "application/vnd.insors.igm"), // IOCOM Visimeet
            new("igs", "application/iges"),
            new("igs", "model/iges"), // Initial Graphics Exchange Specification (IGES)
            new("igx", "application/vnd.micrografx.igx"), // Micrografx iGrafx Professional
            new("iif", "application/vnd.shana.informed.interchange"), // Shana Informed Filler
            new("ima", "application/x-ima"),
            new("imap", "application/x-httpd-imap"),
            new("imp", "application/vnd.accpac.simply.imp"), // Simply Accounting - Data Import
            new("ims", "application/vnd.ms-ims"), // Microsoft Class Server
            new("inf", "application/inf"),
            new("ins", "application/x-internett-signup"),
            new("ip", "application/x-ip2"),
            new("ipfix", "application/ipfix"), // Internet Protocol Flow Information Export
            new("ipk", "application/vnd.shana.informed.package"), // Shana Informed Filler
            new("irm", "application/vnd.ibm.rights-management"), // IBM DB2 Rights Manager
            new("irp", "application/vnd.irepository.package+xml"), // iRepository / Lucidoc Editor
            new("isu", "video/x-isvideo"),
            new("it", "audio/it"),
            new("itp", "application/vnd.shana.informed.formtemplate"), // Shana Informed Filler
            new("iv", "application/x-inventor"),
            new("ivp", "application/vnd.immervision-ivp"), // ImmerVision PURE Players
            new("ivr", "i-world/i-vrml"),
            new("ivu", "application/vnd.immervision-ivu"), // ImmerVision PURE Players
            new("ivy", "application/x-livescreen"),
            new("jad", "text/vnd.sun.j2me.app-descriptor"), // J2ME App Descriptor
            new("jam", "application/vnd.jam"), // Lightspeed Audio Lab
            new("jam", "audio/x-jam"),
            new("jar", "application/java-archive"), // Java Archive
            new("jav", "text/plain"),
            new("jav", "text/x-java-source"),
            new("java", "text/plain"),
            new("java", "text/x-java-source"),
            new("java", "text/x-java-source,java"), // Java Source File
            new("jcm", "application/x-java-commerce"),
            new("jfif", "image/jpeg"),
            new("jfif", "image/pjpeg"),
            new("jfif-tbnl", "image/jpeg"),
            new("jisp", "application/vnd.jisp"), // RhymBox
            new("jlt", "application/vnd.hp-jlyt"), // HP Indigo Digital Press - Job Layout Languate
            new("jnlp", "application/x-java-jnlp-file"), // Java Network Launching Protocol
            new("joda", "application/vnd.joost.joda-archive"), // Joda Archive
            new("jpe", "image/jpeg"),
            new("jpe", "image/pjpeg"),
            new("jpeg", "image/pjpeg"),
            new("jpg", "image/pjpeg"),
            new("jpg", "image/x-citrix-jpeg"), // JPEG Image (Citrix client)
            new("jpgv", "video/jpeg"), // JPGVideo
            new("jpm", "video/jpm"), // JPEG 2000 Compound Image File Format
            new("jps", "image/x-jps"),
            new("js", "application/ecmascript"),
            new("js", "application/javascript"), // JavaScript
            new("js", "application/x-javascript"),
            new("js", "text/ecmascript"),
            new("js", "text/javascript"),
            new("json", "application/json"), // JavaScript Object Notation (JSON)
            new("json", "application/problem+json"),
            new("jut", "image/jutvision"),
            new("kar", "audio/midi"),
            new("kar", "music/x-karaoke"),
            new("karbon", "application/vnd.kde.karbon"), // KDE KOffice Office Suite - Karbon
            new("kfo", "application/vnd.kde.kformula"), // KDE KOffice Office Suite - Kformula
            new("kia", "application/vnd.kidspiration"), // Kidspiration
            new("kml", "application/vnd.google-earth.kml+xml"), // Google Earth - KML
            new("kmz", "application/vnd.google-earth.kmz"), // Google Earth - Zipped KML
            new("kne", "application/vnd.kinar"), // Kinar Applications
            new("kon", "application/vnd.kde.kontour"), // KDE KOffice Office Suite - Kontour
            new("kpr", "application/vnd.kde.kpresenter"), // KDE KOffice Office Suite - Kpresenter
            new("ksh", "application/x-ksh"),
            new("ksh", "text/x-script.ksh"),
            new("ksp", "application/vnd.kde.kspread"), // KDE KOffice Office Suite - Kspread
            new("ktx", "image/ktx"), // OpenGL Textures (KTX)
            new("ktz", "application/vnd.kahootz"), // Kahootz
            new("kwd", "application/vnd.kde.kword"), // KDE KOffice Office Suite - Kword
            new("la", "audio/nspaudio"),
            new("la", "audio/x-nspaudio"),
            new("lam", "audio/x-liveaudio"),
            new("lasxml", "application/vnd.las.las+xml"), // Laser App Enterprise
            new("latex", "application/x-latex"), // LaTeX
            new("lbd", "application/vnd.llamagraphics.life-balance.desktop"), // Life Balance - Desktop Edition
            new("lbe", "application/vnd.llamagraphics.life-balance.exchange+xml"), // Life Balance - Exchange Format
            new("les", "application/vnd.hhe.lesson-player"), // Archipelago Lesson Player
            new("lha", "application/lha"),
            new("lha", "application/octet-stream"),
            new("lha", "application/x-lha"),
            new("lhx", "application/octet-stream"),
            new("link66", "application/vnd.route66.link66+xml"), // ROUTE 66 Location Based Services
            new("list", "text/plain"),
            new("lma", "audio/nspaudio"),
            new("lma", "audio/x-nspaudio"),
            new("log", "text/plain"),
            new("lrm", "application/vnd.ms-lrm"), // Microsoft Learning Resource Module
            new("lsp", "application/x-lisp"),
            new("lsp", "text/x-script.lisp"),
            new("lst", "text/plain"),
            new("lsx", "text/x-la-asf"),
            new("ltf", "application/vnd.frogans.ltf"), // Frogans Player
            new("ltx", "application/x-latex"),
            new("lvp", "audio/vnd.lucent.voice"), // Lucent Voice
            new("lwp", "application/vnd.lotus-wordpro"), // Lotus Wordpro
            new("lzh", "application/octet-stream"),
            new("lzh", "application/x-lzh"),
            new("lzx", "application/lzx"),
            new("lzx", "application/octet-stream"),
            new("lzx", "application/x-lzx"),
            new("m", "text/plain"),
            new("m", "text/x-m"),
            new("m1v", "video/mpeg"),
            new("m21", "application/mp21"), // MPEG-21
            new("m2a", "audio/mpeg"),
            new("m2v", "video/mpeg"),
            new("m3u", "audio/x-mpegurl"), // M3U (Multimedia Playlist)
            new("m3u", "audio/x-mpequrl"),
            new("m3u8", "application/vnd.apple.mpegurl"), // Multimedia Playlist Unicode
            new("m4v", "video/x-m4v"), // M4v
            new("ma", "application/mathematica"), // Mathematica Notebooks
            new("mads", "application/mads+xml"), // Metadata Authority Description Schema
            new("mag", "application/vnd.ecowin.chart"), // EcoWin Chart
            new("man", "application/x-troff-man"),
            new("map", "application/x-navimap"),
            new("mar", "text/plain"),
            new("mathml", "application/mathml+xml"), // Mathematical Markup Language
            new("mbd", "application/mbedlet"),
            new("mbk", "application/vnd.mobius.mbk"), // Mobius Management Systems - Basket file
            new("mbox", "application/mbox"), // Mbox database files
            new("mc$", "application/x-magic-cap-package-1.0"),
            new("mc1", "application/vnd.medcalcdata"), // MedCalc
            new("mcd", "application/mcad"),
            new("mcd", "application/vnd.mcd"), // Micro CADAM Helix D&D
            new("mcd", "application/x-mathcad"),
            new("mcf", "image/vasa"),
            new("mcf", "text/mcf"),
            new("mcp", "application/netmc"),
            new("mcurl", "text/vnd.curl.mcurl"), // Curl - Manifest File
            new("md", "text/markdown"), // Markdown
            new("md", "text/x-markdown"),
            new("mdb", "application/x-msaccess"), // Microsoft Access
            new("mdi", "image/vnd.ms-modi"), // Microsoft Document Imaging Format
            new("me", "application/x-troff-me"),
            new("meta4", "application/metalink4+xml"), // Metalink
            new("mets", "application/mets+xml"), // Metadata Encoding and Transmission Standard
            new("mfm", "application/vnd.mfmp"), // Melody Format for Mobile Platform
            new("mgp", "application/vnd.osgeo.mapguide.package"), // MapGuide DBXML
            new("mgz", "application/vnd.proteus.magazine"), // EFI Proteus
            new("mht", "message/rfc822"),
            new("mhtml", "message/rfc822"),
            new("mid", "application/x-midi"),
            new("mid", "audio/midi"), // MIDI - Musical Instrument Digital Interface
            new("mid", "audio/x-mid"),
            new("mid", "audio/x-midi"),
            new("mid", "music/crescendo"),
            new("mid", "x-music/x-midi"),
            new("midi", "application/x-midi"),
            new("midi", "audio/midi"),
            new("midi", "audio/x-mid"),
            new("midi", "audio/x-midi"),
            new("midi", "music/crescendo"),
            new("midi", "x-music/x-midi"),
            new("mif", "application/vnd.mif"), // FrameMaker Interchange Format
            new("mif", "application/x-frame"),
            new("mif", "application/x-mif"),
            new("mime", "message/rfc822"),
            new("mime", "www/mime"),
            new("mj2", "video/mj2"), // Motion JPEG 2000
            new("mjf", "audio/x-vnd.audioexplosion.mjuicemediafile"),
            new("mjpg", "video/x-motion-jpeg"),
            new("mlp", "application/vnd.dolby.mlp"), // Dolby Meridian Lossless Packing
            new("mm", "application/base64"),
            new("mm", "application/x-meme"),
            new("mmd", "application/vnd.chipnuts.karaoke-mmd"), // Karaoke on Chipnuts Chipsets
            new("mme", "application/base64"),
            new("mmf", "application/vnd.smaf"), // SMAF File
            new("mmr", "image/vnd.fujixerox.edmics-mmr"), // EDMICS 2000
            new("mny", "application/x-msmoney"), // Microsoft Money
            new("mod", "audio/mod"),
            new("mod", "audio/x-mod"),
            new("mods", "application/mods+xml"), // Metadata Object Description Schema
            new("moov", "video/quicktime"),
            new("mov", "video/quicktime"),
            new("movie", "video/x-sgi-movie"),
            new("movie", "video/x-sgi-movie"), // SGI Movie
            new("mp2", "audio/mpeg"),
            new("mp2", "audio/x-mpeg"),
            new("mp2", "video/mpeg"),
            new("mp2", "video/x-mpeg"),
            new("mp2", "video/x-mpeq2a"),
            new("mp3", "audio/mpeg3"),
            new("mp3", "audio/x-mpeg-3"),
            new("mp3", "video/mpeg"),
            new("mp3", "video/x-mpeg"),
            new("mp4", "application/mp4"), // MPEG4
            new("mp4", "video/mp4"), // MPEG-4 Video
            new("mp4a", "audio/mp4"), // MPEG-4 Audio
            new("mpa", "audio/mpeg"),
            new("mpa", "video/mpeg"),
            new("mpc", "application/vnd.mophun.certificate"), // Mophun Certificate
            new("mpc", "application/x-project"),
            new("mpe", "video/mpeg"),
            new("mpeg", "video/mpeg"), // MPEG Video
            new("mpg", "audio/mpeg"),
            new("mpg", "video/mpeg"),
            new("mpga", "audio/mpeg"), // MPEG Audio
            new("mpkg", "application/vnd.apple.installer+xml"), // Apple Installer Package
            new("mpm", "application/vnd.blueice.multipass"), // Blueice Research Multipass
            new("mpn", "application/vnd.mophun.application"), // Mophun VM
            new("mpp", "application/vnd.ms-project"), // Microsoft Project
            new("mpt", "application/x-project"),
            new("mpv", "application/x-project"),
            new("mpx", "application/x-project"),
            new("mpy", "application/vnd.ibm.minipay"), // MiniPay
            new("mqy", "application/vnd.mobius.mqy"), // Mobius Management Systems - Query File
            new("mrc", "application/marc"), // MARC Formats
            new("mrcx", "application/marcxml+xml"), // MARC21 XML Schema
            new("ms", "application/x-troff-ms"),
            new("mscml", "application/mediaservercontrol+xml"), // Media Server Control Markup Language
            new("mseq", "application/vnd.mseq"), // 3GPP MSEQ File
            new("msf", "application/vnd.epson.msf"), // QUASS Stream Player
            new("msh", "model/mesh"), // Mesh Data Type
            new("msl", "application/vnd.mobius.msl"), // Mobius Management Systems - Script Language
            new("msty", "application/vnd.muvee.style"), // Muvee Automatic Video Editing
            new("mts", "model/vnd.mts"), // Virtue MTS
            new("mus", "application/vnd.musician"), // MUsical Score Interpreted Code Invented for the ASCII designation of Notation
            new("musicxml", "application/vnd.recordare.musicxml+xml"), // Recordare Applications
            new("mv", "video/x-sgi-movie"),
            new("mvb", "application/x-msmediaview"), // Microsoft MediaView
            new("mwf", "application/vnd.mfer"), // Medical Waveform Encoding Format
            new("mxf", "application/mxf"), // Material Exchange Format
            new("mxl", "application/vnd.recordare.musicxml"), // Recordare Applications
            new("mxml", "application/xv+xml"), // MXML
            new("mxs", "application/vnd.triscape.mxs"), // Triscape Map Explorer
            new("mxu", "video/vnd.mpegurl"), // MPEG Url
            new("my", "audio/make"),
            new("mzz", "application/x-vnd.audioexplosion.mzz"),
            new("n-gage", "application/vnd.nokia.n-gage.symbian.install"), // IANA: N-Gage Game Installer
            new("n3", "text/n3"), // Notation3
            new("nap", "image/naplps"),
            new("naplps", "image/naplps"),
            new("nbp", "application/vnd.wolfram.player"), // Mathematica Notebook Player
            new("nc", "application/x-netcdf"), // Network Common Data Form (NetCDF)
            new("ncm", "application/vnd.nokia.configuration-message"),
            new("ncx", "application/x-dtbncx+xml"), // Navigation Control file for XML (for ePub)
            new("ngdat", "application/vnd.nokia.n-gage.data"), // IANA: N-Gage Game Data
            new("ngdat", "application/vnd.nokia.n-gage.data"), // N-Gage Game Data
            new("nif", "image/x-niff"),
            new("niff", "image/x-niff"),
            new("nix", "application/x-mix-transfer"),
            new("nlu", "application/vnd.neurolanguage.nlu"), // neuroLanguage
            new("nml", "application/vnd.enliven"), // Enliven Viewer
            new("nnd", "application/vnd.noblenet-directory"), // NobleNet Directory
            new("nns", "application/vnd.noblenet-sealer"), // NobleNet Sealer
            new("nnw", "application/vnd.noblenet-web"), // NobleNet Web
            new("npx", "image/vnd.net-fpx"), // FlashPix
            new("nsc", "application/x-conference"),
            new("nsf", "application/vnd.lotus-notes"), // Lotus Notes
            new("nvd", "application/x-navidoc"),
            new("o", "application/octet-stream"),
            new("oa2", "application/vnd.fujitsu.oasys2"), // Fujitsu Oasys
            new("oa3", "application/vnd.fujitsu.oasys3"), // Fujitsu Oasys
            new("oas", "application/vnd.fujitsu.oasys"), // Fujitsu Oasys
            new("obd", "application/x-msbinder"), // Microsoft Office Binder
            new("oda", "application/oda"), // Office Document Architecture
            new("odb", "application/vnd.oasis.opendocument.database"), // OpenDocument Database
            new("odc", "application/vnd.oasis.opendocument.chart"), // OpenDocument Chart
            new("odf", "application/vnd.oasis.opendocument.formula"), // OpenDocument Formula
            new("odft", "application/vnd.oasis.opendocument.formula-template"), // OpenDocument Formula Template
            new("odg", "application/vnd.oasis.opendocument.graphics"), // OpenDocument Graphics
            new("odi", "application/vnd.oasis.opendocument.image"), // OpenDocument Image
            new("odm", "application/vnd.oasis.opendocument.text-master"), // OpenDocument Text Master
            new("odp", "application/vnd.oasis.opendocument.presentation"), // OpenDocument Presentation
            new("ods", "application/vnd.oasis.opendocument.spreadsheet"), // OpenDocument Spreadsheet
            new("odt", "application/vnd.oasis.opendocument.text"), // OpenDocument Text
            new("oga", "audio/ogg"), // Ogg Audio
            new("ogv", "video/ogg"), // Ogg Video
            new("ogx", "application/ogg"), // Ogg
            new("omc", "application/x-omc"),
            new("omcd", "application/x-omcdatamaker"),
            new("omcr", "application/x-omcregerator"),
            new("onetoc", "application/onenote"), // Microsoft OneNote
            new("opf", "application/oebps-package+xml"), // Open eBook Publication Structure
            new("org", "application/vnd.lotus-organizer"), // Lotus Organizer
            new("osf", "application/vnd.yamaha.openscoreformat"), // Open Score Format
            new("osfpvg", "application/vnd.yamaha.openscoreformat.osfpvg+xml"), // OSFPVG
            new("otc", "application/vnd.oasis.opendocument.chart-template"), // OpenDocument Chart Template
            new("otf", "application/x-font-otf"), // OpenType Font File
            new("otg", "application/vnd.oasis.opendocument.graphics-template"), // OpenDocument Graphics Template
            new("oth", "application/vnd.oasis.opendocument.text-web"), // Open Document Text Web
            new("oti", "application/vnd.oasis.opendocument.image-template"), // OpenDocument Image Template
            new("otp", "application/vnd.oasis.opendocument.presentation-template"), // OpenDocument Presentation Template
            new("ots", "application/vnd.oasis.opendocument.spreadsheet-template"), // OpenDocument Spreadsheet Template
            new("ott", "application/vnd.oasis.opendocument.text-template"), // OpenDocument Text Template
            new("oxt", "application/vnd.openofficeorg.extension"), // Open Office Extension
            new("p", "text/x-pascal"), // Pascal Source File
            new("p10", "application/pkcs10"), // PKCS #10 - Certification Request Standard
            new("p10", "application/x-pkcs10"),
            new("p12", "application/pkcs-12"),
            new("p12", "application/x-pkcs12"), // PKCS #12 - Personal Information Exchange Syntax Standard
            new("p7a", "application/x-pkcs7-signature"),
            new("p7b", "application/x-pkcs7-certificates"), // PKCS #7 - Cryptographic Message Syntax Standard (Certificates)
            new("p7c", "application/pkcs7-mime"),
            new("p7c", "application/x-pkcs7-mime"),
            new("p7m", "application/pkcs7-mime"), // PKCS #7 - Cryptographic Message Syntax Standard
            new("p7m", "application/x-pkcs7-mime"),
            new("p7r", "application/x-pkcs7-certreqresp"), // PKCS #7 - Cryptographic Message Syntax Standard (Certificate Request Response)
            new("p7s", "application/pkcs7-signature"), // PKCS #7 - Cryptographic Message Syntax Standard
            new("p8", "application/pkcs8"), // PKCS #8 - Private-Key Information Syntax Standard
            new("par", "text/plain-bas"), // BAS Partitur Format
            new("part", "application/pro_eng"),
            new("pas", "text/pascal"),
            new("paw", "application/vnd.pawaafile"), // PawaaFILE
            new("pbd", "application/vnd.powerbuilder6"), // PowerBuilder
            new("pbm", "image/x-portable-bitmap"), // Portable Bitmap Format
            new("pcf", "application/x-font-pcf"), // Portable Compiled Format
            new("pcl", "application/vnd.hp-pcl"), // HP Printer Command Language
            new("pcl", "application/x-pcl"),
            new("pclxl", "application/vnd.hp-pclxl"), // PCL 6 Enhanced (Formely PCL XL)
            new("pct", "image/x-pict"),
            new("pcurl", "application/vnd.curl.pcurl"), // CURL Applet
            new("pcx", "image/x-pcx"), // PCX Image
            new("pdb", "application/vnd.palm"), // PalmOS Data
            new("pdb", "chemical/x-pdb"),
            new("pdf", "application/pdf"), // Adobe Portable Document Format
            new("pfa", "application/x-font-type1"), // PostScript Fonts
            new("pfr", "application/font-tdpfr"), // Portable Font Resource
            new("pfunk", "audio/make"),
            new("pfunk", "audio/make.my.funk"),
            new("pgm", "image/x-portable-graymap"), // Portable Graymap Format
            new("pgn", "application/x-chess-pgn"), // Portable Game Notation (Chess Games)
            new("pgp", "application/pgp-encrypted"), // Pretty Good Privacy
            new("pgp", "application/pgp-signature"), // Pretty Good Privacy - Signature
            new("pic", "image/pict"),
            new("pic", "image/x-pict"), // PICT Image
            new("pict", "image/pict"),
            new("pjpeg", "image/pjpeg"), // JPEG Image (Progressive)
            new("pkg", "application/x-newton-compatible-pkg"),
            new("pki", "application/pkixcmp"), // Internet Public Key Infrastructure - Certificate Management Protocole
            new("pkipath", "application/pkix-pkipath"), // Internet Public Key Infrastructure - Certification Path
            new("pko", "application/vnd.ms-pki.pko"),
            new("pl", "text/plain"),
            new("pl", "text/x-script.perl"),
            new("plb", "application/vnd.3gpp.pic-bw-large"), // 3rd Generation Partnership Project - Pic Large
            new("plc", "application/vnd.mobius.plc"), // Mobius Management Systems - Policy Definition Language File
            new("plf", "application/vnd.pocketlearn"), // PocketLearn Viewers
            new("pls", "application/pls+xml"), // Pronunciation Lexicon Specification
            new("plx", "application/x-pixclscript"),
            new("pm", "image/x-xpixmap"),
            new("pm", "text/x-script.perl-module"),
            new("pm4", "application/x-pagemaker"),
            new("pm5", "application/x-pagemaker"),
            new("pml", "application/vnd.ctc-posml"), // PosML
            new("png", "image/png"), // Portable Network Graphics (PNG)
            new("png", "image/x-citrix-png"), // Portable Network Graphics (PNG) (Citrix client)
            new("png", "image/x-png"), // Portable Network Graphics (PNG) (x-token)
            new("pnm", "application/x-portable-anymap"),
            new("pnm", "image/x-portable-anymap"), // Portable Anymap Image
            new("portpkg", "application/vnd.macports.portpkg"), // MacPorts Port System
            new("pot", "application/mspowerpoint"),
            new("pot", "application/vnd.ms-powerpoint"),
            new("potm", "application/vnd.ms-powerpoint.template.macroenabled.12"), // Microsoft PowerPoint - Macro-Enabled Template File
            new("potx", "application/vnd.openxmlformats-officedocument.presentationml.template"), // Microsoft Office - OOXML - Presentation Template
            new("pov", "model/x-pov"),
            new("ppa", "application/vnd.ms-powerpoint"),
            new("ppam", "application/vnd.ms-powerpoint.addin.macroenabled.12"), // Microsoft PowerPoint - Add-in file
            new("ppd", "application/vnd.cups-ppd"), // Adobe PostScript Printer Description File Format
            new("ppm", "image/x-portable-pixmap"), // Portable Pixmap Format
            new("pps", "application/mspowerpoint"),
            new("pps", "application/vnd.ms-powerpoint"),
            new("ppsm", "application/vnd.ms-powerpoint.slideshow.macroenabled.12"), // Microsoft PowerPoint - Macro-Enabled Slide Show File
            new("ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"), // Microsoft Office - OOXML - Presentation (Slideshow)
            new("ppt", "application/mspowerpoint"),
            new("ppt", "application/powerpoint"),
            new("ppt", "application/vnd.ms-powerpoint"), // Microsoft PowerPoint
            new("ppt", "application/x-mspowerpoint"),
            new("pptm", "application/vnd.ms-powerpoint.presentation.macroenabled.12"), // Microsoft PowerPoint - Macro-Enabled Presentation File
            new("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"), // Microsoft Office - OOXML - Presentation
            new("ppz", "application/mspowerpoint"),
            new("prc", "application/x-mobipocket-ebook"), // Mobipocket
            new("pre", "application/vnd.lotus-freelance"), // Lotus Freelance
            new("pre", "application/x-freelance"),
            new("prf", "application/pics-rules"), // PICSRules
            new("prt", "application/pro_eng"),
            new("ps", "application/postscript"),
            new("psb", "application/vnd.3gpp.pic-bw-small"), // 3rd Generation Partnership Project - Pic Small
            new("psd", "application/octet-stream"),
            new("psd", "image/vnd.adobe.photoshop"), // Photoshop Document
            new("psf", "application/x-font-linux-psf"), // PSF Fonts
            new("pskcxml", "application/pskc+xml"), // Portable Symmetric Key Container
            new("ptid", "application/vnd.pvi.ptid1"), // Princeton Video Image
            new("pub", "application/x-mspublisher"), // Microsoft Publisher
            new("pvb", "application/vnd.3gpp.pic-bw-var"), // 3rd Generation Partnership Project - Pic Var
            new("pvu", "paleovu/x-pv"),
            new("pwn", "application/vnd.3m.post-it-notes"), // 3M Post It Notes
            new("pwz", "application/vnd.ms-powerpoint"),
            new("py", "text/x-script.phyton"),
            new("pya", "audio/vnd.ms-playready.media.pya"), // Microsoft PlayReady Ecosystem
            new("pyc", "application/x-bytecode.python"),
            new("pyv", "video/vnd.ms-playready.media.pyv"), // Microsoft PlayReady Ecosystem Video
            new("qam", "application/vnd.epson.quickanime"), // QuickAnime Player
            new("qbo", "application/vnd.intu.qbo"), // Open Financial Exchange
            new("qcp", "audio/vnd.qcelp"),
            new("qd3", "x-world/x-3dmf"),
            new("qd3d", "x-world/x-3dmf"),
            new("qfx", "application/vnd.intu.qfx"), // Quicken
            new("qif", "image/x-quicktime"),
            new("qps", "application/vnd.publishare-delta-tree"), // PubliShare Objects
            new("qt", "video/quicktime"), // Quicktime Video
            new("qtc", "video/x-qtc"),
            new("qti", "image/x-quicktime"),
            new("qtif", "image/x-quicktime"),
            new("qxd", "application/vnd.quark.quarkxpress"), // QuarkXpress
            new("ra", "audio/x-pn-realaudio"),
            new("ra", "audio/x-pn-realaudio-plugin"),
            new("ra", "audio/x-realaudio"),
            new("ram", "audio/x-pn-realaudio"), // Real Audio Sound
            new("rar", "application/x-rar-compressed"), // RAR Archive
            new("ras", "application/x-cmu-raster"),
            new("ras", "image/cmu-raster"),
            new("ras", "image/x-cmu-raster"),
            new("ras", "image/x-cmu-raster"),
            new("rast", "image/cmu-raster"),
            new("rcprofile", "application/vnd.ipunplugged.rcprofile"), // IP Unplugged Roaming Client
            new("rdf", "application/rdf+xml"), // Resource Description Framework
            new("rdz", "application/vnd.data-vision.rdz"), // RemoteDocs R-Viewer
            new("rep", "application/vnd.businessobjects"), // BusinessObjects
            new("res", "application/x-dtbresource+xml"), // Digital Talking Book - Resource File
            new("rexx", "text/x-script.rexx"),
            new("rf", "image/vnd.rn-realflash"),
            new("rgb", "image/x-rgb"), // Silicon Graphics RGB Bitmap
            new("rif", "application/reginfo+xml"),
            new("rip", "audio/vnd.rip"), // Hit'n'Mix
            new("rl", "application/resource-lists+xml"), // XML Resource Lists
            new("rlc", "image/vnd.fujixerox.edmics-rlc"), // EDMICS 2000
            new("rld", "application/resource-lists-diff+xml"), // XML Resource Lists Diff
            new("rm", "application/vnd.rn-realmedia"),
            new("rm", "application/vnd.rn-realmedia"),
            new("rm", "audio/x-pn-realaudio"),
            new("rmi", "audio/mid"),
            new("rmm", "audio/x-pn-realaudio"),
            new("rmp", "audio/x-pn-realaudio"),
            new("rmp", "audio/x-pn-realaudio-plugin"), // Real Audio Sound
            new("rms", "application/vnd.jcp.javame.midlet-rms"), // Mobile Information Device Profile
            new("rnc", "application/relax-ng-compact-syntax"), // Relax NG Compact Syntax
            new("rng", "application/ringing-tones"),
            new("rng", "application/vnd.nokia.ringing-tone"),
            new("rnx", "application/vnd.rn-realplayer"),
            new("roff", "application/x-troff"),
            new("rp", "image/vnd.rn-realpix"),
            new("rp9", "application/vnd.cloanto.rp9"), // RetroPlatform Player
            new("rpm", "audio/x-pn-realaudio-plugin"),
            new("rpss", "application/vnd.nokia.radio-presets"), // Nokia Radio Application - Preset
            new("rpst", "application/vnd.nokia.radio-preset"), // Nokia Radio Application - Preset
            new("rq", "application/sparql-query"), // SPARQL - Query
            new("rs", "application/rls-services+xml"), // XML Resource Lists
            new("rsd", "application/rsd+xml"), // Really Simple Discovery
            new("rss", "application/rss+xml"), // RSS - Really Simple Syndication
            new("rt", "text/richtext"),
            new("rt", "text/vnd.rn-realtext"),
            new("rtf", "application/rtf"), // Rich Text Format
            new("rtf", "application/x-rtf"),
            new("rtf", "text/richtext"),
            new("rtx", "application/rtf"),
            new("rtx", "text/richtext"), // Rich Text Format (RTF)
            new("rv", "video/vnd.rn-realvideo"),
            new("s", "text/x-asm"), // Assembler Source File
            new("s3m", "audio/s3m"),
            new("saf", "application/vnd.yamaha.smaf-audio"), // SMAF Audio
            new("saveme", "application/octet-stream"),
            new("sbk", "application/x-tbook"),
            new("sbml", "application/sbml+xml"), // Systems Biology Markup Language
            new("sc", "application/vnd.ibm.secure-container"), // IBM Electronic Media Management System - Secure Container
            new("scd", "application/x-msschedule"), // Microsoft Schedule+
            new("scm", "application/vnd.lotus-screencam"), // Lotus Screencam
            new("scm", "application/x-lotusscreencam"),
            new("scm", "text/x-script.guile"),
            new("scm", "text/x-script.scheme"),
            new("scm", "video/x-scm"),
            new("scq", "application/scvp-cv-request"), // Server-Based Certificate Validation Protocol - Validation Request
            new("scs", "application/scvp-cv-response"), // Server-Based Certificate Validation Protocol - Validation Response
            new("scurl", "text/vnd.curl.scurl"), // Curl - Source Code
            new("sda", "application/vnd.stardivision.draw"),
            new("sdc", "application/vnd.stardivision.calc"),
            new("sdd", "application/vnd.stardivision.impress"),
            new("sdkm", "application/vnd.solent.sdkm+xml"), // SudokuMagic
            new("sdml", "text/plain"),
            new("sdp", "application/sdp"), // Session Description Protocol
            new("sdp", "application/x-sdp"),
            new("sdr", "application/sounder"),
            new("sdw", "application/vnd.stardivision.writer"),
            new("sea", "application/sea"),
            new("sea", "application/x-sea"),
            new("see", "application/vnd.seemail"), // SeeMail
            new("seed", "application/vnd.fdsn.seed"), // Digital Siesmograph Networks - SEED Datafiles
            new("sema", "application/vnd.sema"), // Secured eMail
            new("semd", "application/vnd.semd"), // Secured eMail
            new("semf", "application/vnd.semf"), // Secured eMail
            new("ser", "application/java-serialized-object"), // Java Serialized Object
            new("set", "application/set"),
            new("setpay", "application/set-payment-initiation"), // Secure Electronic Transaction - Payment
            new("setreg", "application/set-registration-initiation"), // Secure Electronic Transaction - Registration
            new("sfd-hdstx", "application/vnd.hydrostatix.sof-data"), // Hydrostatix Master Suite
            new("sfs", "application/vnd.spotfire.sfs"), // TIBCO Spotfire
            new("sgl", "application/vnd.stardivision.writer-global"),
            new("sgm", "text/sgml"),
            new("sgm", "text/x-sgml"),
            new("sgml", "text/sgml"), // Standard Generalized Markup Language (SGML)
            new("sgml", "text/x-sgml"),
            new("sh", "application/x-bsh"),
            new("sh", "application/x-sh"), // Bourne Shell Script
            new("sh", "application/x-shar"),
            new("sh", "text/x-script.sh"),
            new("shar", "application/x-bsh"),
            new("shar", "application/x-shar"), // Shell Archive
            new("shf", "application/shf+xml"), // S Hexdump Format
            new("shtml", "text/html"),
            new("shtml", "text/x-server-parsed-html"),
            new("sid", "audio/x-psid"),
            new("sis", "application/vnd.symbian.install"), // Symbian Install Package
            new("sit", "application/x-sit"),
            new("sit", "application/x-stuffit"), // Stuffit Archive
            new("sitx", "application/x-stuffitx"), // Stuffit Archive
            new("skd", "application/x-koan"),
            new("skm", "application/x-koan"),
            new("skp", "application/vnd.koan"), // SSEYO Koan Play File
            new("skp", "application/x-koan"),
            new("skt", "application/x-koan"),
            new("sl", "application/x-seelogo"),
            new("sldm", "application/vnd.ms-powerpoint.slide.macroenabled.12"), // Microsoft PowerPoint - Macro-Enabled Open XML Slide
            new("sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"), // Microsoft Office - OOXML - Presentation (Slide)
            new("slt", "application/vnd.epson.salt"), // SimpleAnimeLite Player
            new("sm", "application/vnd.stepmania.stepchart"), // StepMania
            new("smf", "application/vnd.stardivision.math"),
            new("smi", "application/smil"),
            new("smi", "application/smil+xml"), // Synchronized Multimedia Integration Language
            new("smil", "application/smil"),
            new("snd", "audio/basic"),
            new("snd", "audio/x-adpcm"),
            new("snf", "application/x-font-snf"), // Server Normal Format
            new("sol", "application/solids"),
            new("spc", "application/x-pkcs7-certificates"),
            new("spc", "text/x-speech"),
            new("spf", "application/vnd.yamaha.smaf-phrase"), // SMAF Phrase
            new("spl", "application/futuresplash"),
            new("spl", "application/x-futuresplash"), // FutureSplash Animator
            new("spot", "text/vnd.in3d.spot"), // In3D - 3DML
            new("spp", "application/scvp-vp-response"), // Server-Based Certificate Validation Protocol - Validation Policies - Response
            new("spq", "application/scvp-vp-request"), // Server-Based Certificate Validation Protocol - Validation Policies - Request
            new("spr", "application/x-sprite"),
            new("sprite", "application/x-sprite"),
            new("src", "application/x-wais-source"), // WAIS Source
            new("sru", "application/sru+xml"), // Search/Retrieve via URL Response Format
            new("srx", "application/sparql-results+xml"), // SPARQL - Results
            new("sse", "application/vnd.kodak-descriptor"), // Kodak Storyshare
            new("ssf", "application/vnd.epson.ssf"), // QUASS Stream Player
            new("ssi", "text/x-server-parsed-html"),
            new("ssm", "application/streamingmedia"),
            new("ssml", "application/ssml+xml"), // Speech Synthesis Markup Language
            new("sst", "application/vnd.ms-pki.certstore"),
            new("st", "application/vnd.sailingtracker.track"), // SailingTracker
            new("stc", "application/vnd.sun.xml.calc.template"), // OpenOffice - Calc Template (Spreadsheet)
            new("std", "application/vnd.sun.xml.draw.template"), // OpenOffice - Draw Template (Graphics)
            new("step", "application/step"),
            new("stf", "application/vnd.wt.stf"), // Worldtalk
            new("sti", "application/vnd.sun.xml.impress.template"), // OpenOffice - Impress Template (Presentation)
            new("stk", "application/hyperstudio"), // Hyperstudio
            new("stl", "application/sla"),
            new("stl", "application/vnd.ms-pki.stl"), // Microsoft Trust UI Provider - Certificate Trust Link
            new("stl", "application/x-navistyle"),
            new("stp", "application/step"),
            new("str", "application/vnd.pg.format"), // Proprietary P&G Standard Reporting System
            new("stw", "application/vnd.sun.xml.writer.template"), // OpenOffice - Writer Template (Text - HTML)
            new("sub", "image/vnd.dvb.subtitle"), // Close Captioning - Subtitle
            new("sus", "application/vnd.sus-calendar"), // ScheduleUs
            new("sv4cpio", "application/x-sv4cpio"), // System V Release 4 CPIO Archive
            new("sv4crc", "application/x-sv4crc"), // System V Release 4 CPIO Checksum Data
            new("svc", "application/vnd.dvb.service"), // Digital Video Broadcasting
            new("svd", "application/vnd.svd"), // SourceView Document
            new("svf", "image/vnd.dwg"),
            new("svf", "image/x-dwg"),
            new("svg", "image/svg+xml"), // Scalable Vector Graphics (SVG)
            new("svr", "application/x-world"),
            new("svr", "x-world/x-svr"),
            new("swf", "application/x-shockwave-flash"), // Adobe Flash
            new("swi", "application/vnd.aristanetworks.swi"), // Arista Networks Software Image
            new("sxc", "application/vnd.sun.xml.calc"), // OpenOffice - Calc (Spreadsheet)
            new("sxd", "application/vnd.sun.xml.draw"), // OpenOffice - Draw (Graphics)
            new("sxg", "application/vnd.sun.xml.writer.global"), // OpenOffice - Writer (Text - HTML)
            new("sxi", "application/vnd.sun.xml.impress"), // OpenOffice - Impress (Presentation)
            new("sxm", "application/vnd.sun.xml.math"), // OpenOffice - Math (Formula)
            new("sxw", "application/vnd.sun.xml.writer"), // OpenOffice - Writer (Text - HTML)
            new("t", "application/x-troff"),
            new("t", "text/troff"), // troff
            new("talk", "text/x-speech"),
            new("tao", "application/vnd.tao.intent-module-archive"), // Tao Intent
            new("tar", "application/x-tar"), // Tar File (Tape Archive)
            new("tbk", "application/toolbook"),
            new("tbk", "application/x-tbook"),
            new("tcap", "application/vnd.3gpp2.tcap"), // 3rd Generation Partnership Project - Transaction Capabilities Application Part
            new("tcl", "application/x-tcl"), // Tcl Script
            new("tcl", "text/x-script.tcl"),
            new("tcsh", "text/x-script.tcsh"),
            new("teacher", "application/vnd.smart.teacher"), // SMART Technologies Apps
            new("tei", "application/tei+xml"), // Text Encoding and Interchange
            new("tex", "application/x-tex"), // TeX
            new("texi", "application/x-texinfo"),
            new("texinfo", "application/x-texinfo"), // GNU Texinfo Document
            new("text", "application/plain"),
            new("text", "text/plain"),
            new("tfi", "application/thraud+xml"), // Sharing Transaction Fraud Data
            new("tfm", "application/x-tex-tfm"), // TeX Font Metric
            new("tgz", "application/gnutar"),
            new("tgz", "application/x-compressed"),
            new("thmx", "application/vnd.ms-officetheme"), // Microsoft Office System Release Theme
            new("tif", "image/tiff"),
            new("tif", "image/x-tiff"),
            new("tiff", "image/tiff"), // Tagged Image File Format
            new("tiff", "image/x-tiff"),
            new("tmo", "application/vnd.tmobile-livetv"), // MobileTV
            new("torrent", "application/x-bittorrent"), // BitTorrent
            new("tpl", "application/vnd.groove-tool-template"), // Groove - Tool Template
            new("tpt", "application/vnd.trid.tpt"), // TRI Systems Config
            new("tr", "application/x-troff"),
            new("tra", "application/vnd.trueapp"), // True BASIC
            new("trm", "application/x-msterminal"), // Microsoft Windows Terminal Services
            new("tsd", "application/timestamped-data"), // Time Stamped Data Envelope
            new("tsi", "audio/tsp-audio"),
            new("tsp", "application/dsptype"),
            new("tsp", "audio/tsplayer"),
            new("tsv", "text/tab-separated-values"), // Tab Seperated Values
            new("ttf", "application/x-font-ttf"), // TrueType Font
            new("ttl", "text/turtle"), // Turtle (Terse RDF Triple Language)
            new("turbot", "image/florian"),
            new("twd", "application/vnd.simtech-mindmapper"), // SimTech MindMapper
            new("txd", "application/vnd.genomatix.tuxedo"), // Genomatix Tuxedo Framework
            new("txf", "application/vnd.mobius.txf"), // Mobius Management Systems - Topic Index File
            // --> Moved to first position: new("txt", "text/plain"), // Text File <--
            new("ufd", "application/vnd.ufdl"), // Universal Forms Description Language
            new("uil", "text/x-uil"),
            new("umj", "application/vnd.umajin"), // UMAJIN
            new("uni", "text/uri-list"),
            new("unis", "text/uri-list"),
            new("unityweb", "application/vnd.unity"), // Unity 3d
            new("unv", "application/i-deas"),
            new("uoml", "application/vnd.uoml+xml"), // Unique Object Markup Language
            new("uri", "text/uri-list"), // URI Resolution Services
            new("uris", "text/uri-list"),
            new("ustar", "application/x-ustar"),
            new("ustar", "application/x-ustar"), // Ustar (Uniform Standard Tape Archive)
            new("ustar", "multipart/x-ustar"),
            new("utz", "application/vnd.uiq.theme"), // User Interface Quartz - Theme (Symbian)
            new("uu", "application/octet-stream"),
            new("uu", "text/x-uuencode"), // UUEncode
            new("uue", "text/x-uuencode"),
            new("uva", "audio/vnd.dece.audio"), // DECE Audio
            new("uvh", "video/vnd.dece.hd"), // DECE High Definition Video
            new("uvi", "image/vnd.dece.graphic"), // DECE Graphic
            new("uvm", "video/vnd.dece.mobile"), // DECE Mobile Video
            new("uvp", "video/vnd.dece.pd"), // DECE PD Video
            new("uvs", "video/vnd.dece.sd"), // DECE SD Video
            new("uvu", "video/vnd.uvvu.mp4"), // DECE MP4
            new("uvv", "video/vnd.dece.video"), // DECE Video
            new("vcd", "application/x-cdlink"), // Video CD
            new("vcf", "text/x-vcard"), // vCard
            new("vcg", "application/vnd.groove-vcard"), // Groove - Vcard
            new("vcs", "text/x-vcalendar"), // vCalendar
            new("vcx", "application/vnd.vcx"), // VirtualCatalog
            new("vda", "application/vda"),
            new("vdo", "video/vdo"),
            new("vew", "application/groupwise"),
            new("vis", "application/vnd.visionary"), // Visionary
            new("viv", "video/vivo"),
            new("viv", "video/vnd.vivo"), // Vivo
            new("vivo", "video/vivo"),
            new("vivo", "video/vnd.vivo"),
            new("vmd", "application/vocaltec-media-desc"),
            new("vmf", "application/vocaltec-media-file"),
            new("voc", "audio/voc"),
            new("voc", "audio/x-voc"),
            new("vos", "video/vosaic"),
            new("vox", "audio/voxware"),
            new("vqe", "audio/x-twinvq-plugin"),
            new("vqf", "audio/x-twinvq"),
            new("vql", "audio/x-twinvq-plugin"),
            new("vrml", "application/x-vrml"),
            new("vrml", "model/vrml"),
            new("vrml", "x-world/x-vrml"),
            new("vrt", "x-world/x-vrt"),
            new("vsd", "application/vnd.visio"), // Microsoft Visio
            new("vsd", "application/x-visio"),
            new("vsdx", "application/vnd.visio2013"), // Microsoft Visio 2013
            new("vsf", "application/vnd.vsf"), // Viewport+
            new("vst", "application/x-visio"),
            new("vsw", "application/x-visio"),
            new("vtu", "model/vnd.vtu"), // Virtue VTU
            new("vxml", "application/voicexml+xml"), // VoiceXML
            new("w60", "application/wordperfect6.0"),
            new("w61", "application/wordperfect6.1"),
            new("w6w", "application/msword"),
            new("wad", "application/x-doom"), // Doom Video Game
            new("wav", "audio/wav"),
            new("wav", "audio/x-wav"), // Waveform Audio File Format (WAV)
            new("wax", "audio/x-ms-wax"), // Microsoft Windows Media Audio Redirector
            new("wb1", "application/x-qpro"),
            new("wbmp", "image/vnd.wap.wbmp"), // WAP Bitamp (WBMP)
            new("wbs", "application/vnd.criticaltools.wbs+xml"), // Critical Tools - PERT Chart EXPERT
            new("wbxml", "application/vnd.wap.wbxml"), // WAP Binary XML (WBXML)
            new("web", "application/vnd.xara"),
            new("weba", "audio/webm"), // Open Web Media Project - Audio
            new("webm", "video/webm"), // Open Web Media Project - Video
            new("webp", "image/webp"), // WebP Image
            new("wg", "application/vnd.pmi.widget"), // Qualcomm's Plaza Mobile Internet
            new("wgt", "application/widget"), // Widget Packaging and XML Configuration
            new("wiz", "application/msword"),
            new("wk1", "application/x-123"),
            new("wm", "video/x-ms-wm"), // Microsoft Windows Media
            new("wma", "audio/x-ms-wma"), // Microsoft Windows Media Audio
            new("wmd", "application/x-ms-wmd"), // Microsoft Windows Media Player Download Package
            new("wmf", "application/x-msmetafile"), // Microsoft Windows Metafile
            new("wmf", "windows/metafile"),
            new("wml", "text/vnd.wap.wml"), // Wireless Markup Language (WML)
            new("wmlc", "application/vnd.wap.wmlc"), // Compiled Wireless Markup Language (WMLC)
            new("wmls", "text/vnd.wap.wmlscript"), // Wireless Markup Language Script (WMLScript)
            new("wmlsc", "application/vnd.wap.wmlscriptc"), // WMLScript
            new("wmv", "video/x-ms-wmv"), // Microsoft Windows Media Video
            new("wmx", "video/x-ms-wmx"), // Microsoft Windows Media Audio/Video Playlist
            new("wmz", "application/x-ms-wmz"), // Microsoft Windows Media Player Skin Package
            new("woff", "application/x-font-woff"), // Web Open Font Format
            new("word", "application/msword"),
            new("wp", "application/wordperfect"),
            new("wp5", "application/wordperfect"),
            new("wp5", "application/wordperfect6.0"),
            new("wp6", "application/wordperfect"),
            new("wpd", "application/vnd.wordperfect"), // Wordperfect
            new("wpd", "application/wordperfect"),
            new("wpd", "application/x-wpwin"),
            new("wpl", "application/vnd.ms-wpl"), // Microsoft Windows Media Player Playlist
            new("wps", "application/vnd.ms-works"), // Microsoft Works
            new("wq1", "application/x-lotus"),
            new("wqd", "application/vnd.wqd"), // SundaHus WQ
            new("wri", "application/mswrite"),
            new("wri", "application/x-mswrite"), // Microsoft Wordpad
            new("wri", "application/x-wri"),
            new("wrl", "application/x-world"),
            new("wrl", "model/vrml"), // Virtual Reality Modeling Language
            new("wrl", "x-world/x-vrml"),
            new("wrz", "model/vrml"),
            new("wrz", "x-world/x-vrml"),
            new("wsc", "text/scriplet"),
            new("wsdl", "application/wsdl+xml"), // WSDL - Web Services Description Language
            new("wspolicy", "application/wspolicy+xml"), // Web Services Policy
            new("wsrc", "application/x-wais-source"),
            new("wtb", "application/vnd.webturbo"), // WebTurbo
            new("wtk", "application/x-wintalk"),
            new("wvx", "video/x-ms-wvx"), // Microsoft Windows Media Video Playlist
            new("x-png", "image/png"),
            new("x3d", "application/vnd.hzn-3d-crossword"), // 3D Crossword Plugin
            new("xap", "application/x-silverlight-app"), // Microsoft Silverlight
            new("xar", "application/vnd.xara"), // CorelXARA
            new("xbap", "application/x-ms-xbap"), // Microsoft XAML Browser Application
            new("xbd", "application/vnd.fujixerox.docuworks.binder"), // Fujitsu - Xerox DocuWorks Binder
            new("xbm", "image/x-xbitmap"), // X BitMap
            new("xbm", "image/x-xbm"),
            new("xbm", "image/xbm"),
            new("xdf", "application/xcap-diff+xml"), // XML Configuration Access Protocol - XCAP Diff
            new("xdm", "application/vnd.syncml.dm+xml"), // SyncML - Device Management
            new("xdp", "application/vnd.adobe.xdp+xml"), // Adobe XML Data Package
            new("xdr", "video/x-amt-demorun"),
            new("xdssc", "application/dssc+xml"), // Data Structure for the Security Suitability of Cryptographic Algorithms
            new("xdw", "application/vnd.fujixerox.docuworks"), // Fujitsu - Xerox DocuWorks
            new("xenc", "application/xenc+xml"), // XML Encryption Syntax and Processing
            new("xer", "application/patch-ops-error+xml"), // XML Patch Framework
            new("xfdf", "application/vnd.adobe.xfdf"), // Adobe XML Forms Data Format
            new("xfdl", "application/vnd.xfdl"), // Extensible Forms Description Language
            new("xgz", "xgl/drawing"),
            new("xhtml", "application/xhtml+xml"), // XHTML - The Extensible HyperText Markup Language
            new("xif", "image/vnd.xiff"), // eXtended Image File Format (XIFF)
            new("xl", "application/excel"),
            new("xla", "application/excel"),
            new("xla", "application/x-excel"),
            new("xla", "application/x-msexcel"),
            new("xlam", "application/vnd.ms-excel.addin.macroenabled.12"), // Microsoft Excel - Add-In File
            new("xlb", "application/excel"),
            new("xlb", "application/vnd.ms-excel"),
            new("xlb", "application/x-excel"),
            new("xlc", "application/excel"),
            new("xlc", "application/vnd.ms-excel"),
            new("xlc", "application/x-excel"),
            new("xld", "application/excel"),
            new("xld", "application/x-excel"),
            new("xlk", "application/excel"),
            new("xlk", "application/x-excel"),
            new("xll", "application/excel"),
            new("xll", "application/vnd.ms-excel"),
            new("xll", "application/x-excel"),
            new("xlm", "application/excel"),
            new("xlm", "application/vnd.ms-excel"),
            new("xlm", "application/x-excel"),
            new("xls", "application/excel"),
            new("xls", "application/vnd.ms-excel"), // Microsoft Excel
            new("xls", "application/x-excel"),
            new("xls", "application/x-msexcel"),
            new("xlsb", "application/vnd.ms-excel.sheet.binary.macroenabled.12"), // Microsoft Excel - Binary Workbook
            new("xlsm", "application/vnd.ms-excel.sheet.macroenabled.12"), // Microsoft Excel - Macro-Enabled Workbook
            new("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"), // Microsoft Office - OOXML - Spreadsheet
            new("xlt", "application/excel"),
            new("xlt", "application/x-excel"),
            new("xltm", "application/vnd.ms-excel.template.macroenabled.12"), // Microsoft Excel - Macro-Enabled Template File
            new("xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"), // Microsoft Office - OOXML - Spreadsheet Template
            new("xlv", "application/excel"),
            new("xlv", "application/x-excel"),
            new("xlw", "application/excel"),
            new("xlw", "application/vnd.ms-excel"),
            new("xlw", "application/x-excel"),
            new("xlw", "application/x-msexcel"),
            new("xm", "audio/xm"),
            new("xmz", "xgl/movie"),
            new("xo", "application/vnd.olpc-sugar"), // Sugar Linux Application Bundle
            new("xop", "application/xop+xml"), // XML-Binary Optimized Packaging
            new("xpi", "application/x-xpinstall"), // XPInstall - Mozilla
            new("xpix", "application/x-vnd.ls-xpix"),
            new("xpm", "image/x-xpixmap"), // X PixMap
            new("xpm", "image/xpm"),
            new("xpr", "application/vnd.is-xpr"), // Express by Infoseek
            new("xps", "application/vnd.ms-xpsdocument"), // Microsoft XML Paper Specification
            new("xpw", "application/vnd.intercon.formnet"), // Intercon FormNet
            new("xslt", "application/xslt+xml"), // XML Transformations
            new("xsm", "application/vnd.syncml+xml"), // SyncML
            new("xspf", "application/xspf+xml"), // XSPF - XML Shareable Playlist Format
            new("xsr", "video/x-amt-showrun"),
            new("xul", "application/vnd.mozilla.xul+xml"), // XUL - XML User Interface Language
            new("xwd", "image/x-xwd"),
            new("xwd", "image/x-xwindowdump"), // X Window Dump
            new("xyz", "chemical/x-pdb"),
            new("xyz", "chemical/x-xyz"), // XYZ File Format
            new("yaml", "text/yaml"), // YAML Ain't Markup Language / Yet Another Markup Language
            new("yang", "application/yang"), // YANG Data Modeling Language
            new("yin", "application/yin+xml"), // YIN (YANG - XML)
            new("z", "application/x-compress"),
            new("z", "application/x-compressed"),
            new("zaz", "application/vnd.zzazz.deck+xml"), // Zzazz Deck
            new("zip", "application/x-compressed"),
            new("zip", "application/x-zip-compressed"),
            new("zip", "application/zip"), // Zip Archive
            new("zip", "multipart/x-zip"),
            new("zir", "application/vnd.zul"), // Z.U.L. Geometry
            new("zmm", "application/vnd.handheld-entertainment+xml"), // ZVUE Media Manager
            new("zoo", "application/octet-stream"),
            new("zsh", "text/x-script.zsh")
        };

    public static bool TryFindMimeTypeForFile(string fileNameOrPath, out string? mimeType)
    {
        string fileExtensionWithoutDot = FileNameUtils.GetFileExtensionWithoutDot(fileNameOrPath) ?? string.Empty;

        KeyValuePair<string, string>? match =
            _mappings.FirstOrDefault(m => string.Equals(fileExtensionWithoutDot, m.Key, StringComparison.InvariantCultureIgnoreCase));

        mimeType = match?.Value;
        return mimeType != null;
    }

    public static bool TryFindFileExtensionForContentType(string contentType, out string? fileExtensionWithoutDot)
    {
        KeyValuePair<string, string>? match =
            _mappings.FirstOrDefault(m => contentType.Contains(m.Value, StringComparison.InvariantCultureIgnoreCase));

        fileExtensionWithoutDot = match?.Key;
        return fileExtensionWithoutDot != null;
    }

    public static bool IsJsonContent(string contentType) =>
        contentType.Contains("json");

    public static bool IsTextContent(string contentType) =>
        textMimeTypes.Any(mt =>
            contentType.Contains(mt, StringComparison.InvariantCultureIgnoreCase)
        ); // This should cover most text content types
}