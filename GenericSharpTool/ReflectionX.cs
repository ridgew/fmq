/*****************************************
 * http://www.ntcore.com/exsuite.php
 * Explorer Suite,
 * Kewords:PE, NtHeader, subsystem
 * http://www.cnblogs.com/qaqz111/p/6694227.html
 ****************************************/


namespace GenericSharpTool
{
    using System;
    using System.IO;

    public static partial class ReflectionX
    {
        public static PeType GetPeType(this string filepath)
        {
            using (var fs = File.OpenRead(filepath))
            using (var br = new BinaryReader(fs))
            {
                try
                {
                    if (IMAGE_DOS_SIGNATURE != br.ReadInt16()) return PeType.NotExecutable;

                    fs.Seek(offset__IMAGE_DOS_HEADER__e_lfanew, SeekOrigin.Begin);
                    var offsetNH = br.ReadUInt32();
                    if (offsetNH > fs.Length || 0 != offsetNH % 2 || 0 == offsetNH) return PeType.MsDosExecutable;
                    if (offsetNH + 4 + sizeof__IMAGE_FILE_HEADER + 2 >= fs.Length) return PeType.OptionalHeaderMagicExceedFileLength;

                    fs.Seek(offsetNH, SeekOrigin.Begin);
                    switch (br.ReadInt16())
                    {
                        case IMAGE_NT_SIGNATURE_LOWORD:
                            if (IMAGE_NT_SIGNATURE_HIWORD == br.ReadInt16()) break;
                            return PeType.MsDosExecutable;

                        case IMAGE_OS2_SIGNATURE: return PeType.Windows16BitExecutable;
                        case IMAGE_VXD_SIGNATURE: return PeType.Windows16BitVirtualDeviceDriver;
                        default: return PeType.MsDosExecutable;
                    }

                    //fs.Seek //紧跟前面的 pimNH64->Signature，所以不用 fs.Seek
                    //var machine = br.ReadUInt16();
                    fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS__FileHeader__Characteristics, SeekOrigin.Begin);
                    var characteristics = br.ReadUInt16();
                    //var is32bit = IMAGE_FILE_32BIT_MACHINE & characteristics;
                    var dllfile = IMAGE_FILE_DLL & characteristics;
                    //fs.Seek //紧跟前面的 pimNH64->OptionalHeader.Magic，所以不用 fs.Seek
                    var magic = br.ReadUInt16();

                    //get RVA
                    uint rvaCH;
                    switch (magic)
                    {
                        case IMAGE_NT_OPTIONAL_HDR32_MAGIC:
                            //if (IMAGE_FILE_MACHINE_I386 != machine)
                            //    throw new BadImageFormatException(
                            //        $"幻数值与目标机器类型不一致: IMAGE_NT_HEADERS.OptionalHeader.Magic({magic:X4})/IMAGE_NT_HEADERS.FileHeader.Machine({machine:X4})",
                            //        filepath);
                            if (offsetNH + offset__IMAGE_NT_HEADERS__OptionalHeader + sizeof__IMAGE_OPTIONAL_HEADER32 > fs.Length)
                                return PeType.OptionalHeader32ExceedFileLength;
                            fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS32__OptionalHeader__DataDirectory__14__VirtualAddress, SeekOrigin.Begin);
                            rvaCH = br.ReadUInt32();
                            break;

                        case IMAGE_NT_OPTIONAL_HDR64_MAGIC:
                            //if (IMAGE_FILE_MACHINE_AMD64 != machine)
                            //    throw new BadImageFormatException(
                            //        $"幻数值与目标机器类型不一致: IMAGE_NT_HEADERS.OptionalHeader.Magic({magic:X4})/IMAGE_NT_HEADERS.FileHeader.Machine({machine:X4})",
                            //        filepath);
                            if (offsetNH + offset__IMAGE_NT_HEADERS__OptionalHeader + sizeof__IMAGE_OPTIONAL_HEADER64 > fs.Length)
                                return PeType.OptionalHeader64ExceedFileLength;
                            fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS64__OptionalHeader__DataDirectory__14__VirtualAddress, SeekOrigin.Begin);
                            rvaCH = br.ReadUInt32();
                            break;

                        default:
                            //throw new BadImageFormatException(
                            //    //$"预料之外的幻数值: IMAGE_NT_HEADERS.OptionalHeader.Magic({magic:X4})/IMAGE_NT_HEADERS.FileHeader.Machine({machine:X4})",
                            //    $"预料之外的幻数值: IMAGE_NT_HEADERS.OptionalHeader.Magic({magic:X4})",
                            //    filepath);
                            return PeType.OptionalHeaderMagicNot3264;
                    }

                    fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS__OptionalHeader__Subsystem, SeekOrigin.Begin);
                    var subSystem = br.ReadInt16();

                    PeType pt;
                    if (0 != rvaCH)
                    {
                        //translate RVA to file offset
                        uint offsetCH = 0;
                        fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS__FileHeader__SizeOfOptionalHeader, SeekOrigin.Begin);
                        uint offsetSH = offsetNH + offset__IMAGE_NT_HEADERS__OptionalHeader + br.ReadUInt16();
                        fs.Seek(offsetNH + offset__IMAGE_NT_HEADERS__FileHeader__NumberOfSections, SeekOrigin.Begin);
                        var numberOfSections = br.ReadUInt16();
                        while (numberOfSections-- > 0)
                        {
                            if (offsetSH + sizeof__IMAGE_SECTION_HEADER > fs.Length) return PeType.SectionHeaderExceedFileLength;
                            fs.Seek(offsetSH + offset__IMAGE_SECTION_HEADER__VirtualAddress, SeekOrigin.Begin);
                            var VirtualAddress = br.ReadUInt32();
                            var SizeOfRawData = br.ReadUInt32();
                            if (VirtualAddress <= rvaCH && rvaCH < VirtualAddress + SizeOfRawData)
                            {
                                offsetCH = br.ReadUInt32() + rvaCH - VirtualAddress;
                                break;
                            }
                            offsetSH += sizeof__IMAGE_SECTION_HEADER;
                        }
                        if (offsetCH + sizeof__IMAGE_COR20_HEADER > fs.Length) return PeType.Cor20HeaderExceedFileLength;
                        fs.Seek(offsetCH + offset__IMAGE_COR20_HEADER__Flags, SeekOrigin.Begin);
                        uint chFlags = br.ReadUInt32();

                        if (0 != (COMIMAGE_FLAGS_ILONLY & chFlags))
                        {
                            if (0 != (COMIMAGE_FLAGS_32BITREQUIRED & chFlags)) //X86
                            {
                                pt = (PeType)PeTypeBitness.X86 | ((0 != (COMIMAGE_FLAGS_32BITPREFERRED & chFlags))
                                    ? (PeType)PeTypePlatform.AnyCPU //AnyCPU32
                                    : (PeType)PeTypePlatform.ILOnly //ILOnly32
                                    );
                            }
                            else //X64
                            {
                                pt = (PeType)PeTypeBitness.X64;
                                switch (magic)
                                {
                                    case IMAGE_NT_OPTIONAL_HDR32_MAGIC: pt |= (PeType)PeTypePlatform.AnyCPU; break; //AnyCPU64
                                    case IMAGE_NT_OPTIONAL_HDR64_MAGIC: pt |= (PeType)PeTypePlatform.ILOnly; break; //ILOnly64
                                }
                            }
                        }
                        else //Mixed
                        {
                            pt = (PeType)PeTypePlatform.Mixing;
                            switch (magic)
                            {
                                case IMAGE_NT_OPTIONAL_HDR32_MAGIC: pt |= (PeType)PeTypeBitness.X86; break; //Mixed32
                                case IMAGE_NT_OPTIONAL_HDR64_MAGIC: pt |= (PeType)PeTypeBitness.X64; break; //Mixed64
                            }
                        }

                        //file ext & subsystem
                        if (0 == dllfile)
                        {
                            pt |= (PeType)PeTypeExt.Exe;
                            switch (subSystem)
                            {
                                case IMAGE_SUBSYSTEM_WINDOWS_GUI: pt |= (PeType)PeTypeSubSystem.WindowsGui; break;
                                case IMAGE_SUBSYSTEM_WINDOWS_CUI: pt |= (PeType)PeTypeSubSystem.ConsoleCui; break;
                            }
                        }
                        else
                        {
                            pt |= (PeType)PeTypeExt.Dll;
                        }
                    }
                    else //Native
                    {
                        pt = (PeType)PeTypePlatform.Native;
                        switch (magic)
                        {
                            case IMAGE_NT_OPTIONAL_HDR32_MAGIC: pt |= (PeType)PeTypeBitness.X86; break; //Native32
                            case IMAGE_NT_OPTIONAL_HDR64_MAGIC: pt |= (PeType)PeTypeBitness.X64; break; //Native64
                        }

                        //file ext & subsystem
                        if (IMAGE_SUBSYSTEM_NATIVE == subSystem)
                        {
                            pt |= (PeType)PeTypeExt.Drv;
                        }
                        else if (0 == dllfile)
                        {
                            pt |= (PeType)PeTypeExt.Exe;
                            switch (subSystem)
                            {
                                case IMAGE_SUBSYSTEM_WINDOWS_GUI: pt |= (PeType)PeTypeSubSystem.WindowsGui; break;
                                case IMAGE_SUBSYSTEM_WINDOWS_CUI: pt |= (PeType)PeTypeSubSystem.ConsoleCui; break;
                            }
                        }
                        else
                        {
                            pt |= (PeType)PeTypeExt.Dll;
                        }
                    }
                    return pt;
                }
                catch (Exception ex)
                {
                    return PeType.ErrorOccurred;
                }
            }
        }

        private const int offset__IMAGE_DOS_HEADER__e_lfanew = 60;
        private const int offset__IMAGE_NT_HEADERS__FileHeader__NumberOfSections = 4 + 2;
        private const int offset__IMAGE_NT_HEADERS__FileHeader__SizeOfOptionalHeader = 4 + 16;
        private const int offset__IMAGE_NT_HEADERS__FileHeader__Characteristics = 4 + 18;
        private const int offset__IMAGE_NT_HEADERS__OptionalHeader = 4 + sizeof__IMAGE_FILE_HEADER;
        private const int offset__IMAGE_NT_HEADERS__OptionalHeader__Subsystem = offset__IMAGE_NT_HEADERS__OptionalHeader + 4 * 17;
        private const int offset__IMAGE_NT_HEADERS32__OptionalHeader__DataDirectory__14__VirtualAddress = offset__IMAGE_NT_HEADERS__OptionalHeader + 96 + 8 * 14;
        private const int offset__IMAGE_NT_HEADERS64__OptionalHeader__DataDirectory__14__VirtualAddress = offset__IMAGE_NT_HEADERS__OptionalHeader + 112 + 8 * 14;
        private const int offset__IMAGE_SECTION_HEADER__VirtualAddress = 8 + 4;
        private const int offset__IMAGE_COR20_HEADER__Flags = 4 + 2 + 2 + 8;

        private const int sizeof__IMAGE_FILE_HEADER = 20;
        private const int sizeof__IMAGE_OPTIONAL_HEADER32 = 96 + 8 * 16;
        private const int sizeof__IMAGE_OPTIONAL_HEADER64 = 112 + 8 * 16;
        private const int sizeof__IMAGE_SECTION_HEADER = 40;
        private const int sizeof__IMAGE_COR20_HEADER = 72;

        private const int IMAGE_DOS_SIGNATURE = 0x5A4D;
        private const int IMAGE_OS2_SIGNATURE = 0x454E;
        private const int IMAGE_VXD_SIGNATURE = 0x454C;
        private const int IMAGE_NT_SIGNATURE_LOWORD = 0x4550;
        private const int IMAGE_NT_SIGNATURE_HIWORD = 0x0000;
        private const int IMAGE_FILE_MACHINE_I386 = 0x014C;
        private const int IMAGE_FILE_MACHINE_AMD64 = 0x8664;
        private const int IMAGE_FILE_32BIT_MACHINE = 0x0100;
        private const int IMAGE_FILE_DLL = 0x2000;
        private const int IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10B;
        private const int IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B;

        //private const int IMAGE_SUBSYSTEM_UNKNOWN = 0; // Unknown subsystem.
        private const int IMAGE_SUBSYSTEM_NATIVE = 1; // Image doesn't require a subsystem.
        private const int IMAGE_SUBSYSTEM_WINDOWS_GUI = 2; // Image runs in the Windows GUI subsystem.
        private const int IMAGE_SUBSYSTEM_WINDOWS_CUI = 3; // Image runs in the Windows character subsystem.
        //private const int IMAGE_SUBSYSTEM_OS2_CUI = 5; // image runs in the OS/2 character subsystem.
        //private const int IMAGE_SUBSYSTEM_POSIX_CUI = 7; // image runs in the Posix character subsystem.
        //private const int IMAGE_SUBSYSTEM_NATIVE_WINDOWS = 8; // image is a native Win9x driver.
        //private const int IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9; // Image runs in the Windows CE subsystem.
        //private const int IMAGE_SUBSYSTEM_EFI_APPLICATION = 10; //
        //private const int IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11; //
        //private const int IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12; //
        //private const int IMAGE_SUBSYSTEM_EFI_ROM = 13;
        //private const int IMAGE_SUBSYSTEM_XBOX = 14;
        //private const int IMAGE_SUBSYSTEM_WINDOWS_BOOT_APPLICATION = 16;
        //private const int IMAGE_SUBSYSTEM_XBOX_CODE_CATALOG = 17;

        private const int COMIMAGE_FLAGS_ILONLY = 0x00000001;
        private const int COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002;
        //private const int COMIMAGE_FLAGS_IL_LIBRARY = 0x00000004;
        //private const int COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008;
        //private const int COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010;
        //private const int COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000;
        private const int COMIMAGE_FLAGS_32BITPREFERRED = 0x00020000;
    }

    [Flags]
    public enum PeTypePlatform
    {
        AnyCPU = 0x0001,
        ILOnly = 0x0002,
        Native = 0x0004,
        Mixing = 0x0008,

        Unknow = 0x0000,
        BitMask = 0x00FF,
        BitMastDotNet = 0x0007,
    }

    [Flags]
    public enum PeTypeBitness
    {
        X64 = 0x0100,
        X86 = 0x0200,

        NoBit = 0x0000,
        BitMask = 0x0F00,
    }

    [Flags]
    public enum PeTypeExt
    {
        Exe = 0x1000,
        Dll = 0x2000,
        Drv = 0x3000,

        NoExt = 0x0000,
        BitMask = 0xF000,
    }

    [Flags]
    public enum PeTypeSubSystem
    {
        //DeviceDrv = 0x1,
        WindowsGui = 0x2,
        ConsoleCui = 0x3,
        //Win9xDrv = 0x8,

        Unknow = 0x0,
        BitMask = 0xF,
    }

    [Flags]
    public enum PeType
    {
        AnyCPU32BitExeWindowsGui = PeTypePlatform.AnyCPU | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        AnyCPU64BitExeWindowsGui = PeTypePlatform.AnyCPU | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        ILOnly32BitExeWindowsGui = PeTypePlatform.ILOnly | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        ILOnly64BitExeWindowsGui = PeTypePlatform.ILOnly | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        Native32BitExeWindowsGui = PeTypePlatform.Native | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        Native64BitExeWindowsGui = PeTypePlatform.Native | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        Mixing32BitExeWindowsGui = PeTypePlatform.Mixing | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,
        Mixing64BitExeWindowsGui = PeTypePlatform.Mixing | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.WindowsGui,

        AnyCPU32BitExeConsoleCui = PeTypePlatform.AnyCPU | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        AnyCPU64BitExeConsoleCui = PeTypePlatform.AnyCPU | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        ILOnly32BitExeConsoleCui = PeTypePlatform.ILOnly | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        ILOnly64BitExeConsoleCui = PeTypePlatform.ILOnly | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        Native32BitExeConsoleCui = PeTypePlatform.Native | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        Native64BitExeConsoleCui = PeTypePlatform.Native | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        Mixing32BitExeConsoleCui = PeTypePlatform.Mixing | PeTypeBitness.X86 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,
        Mixing64BitExeConsoleCui = PeTypePlatform.Mixing | PeTypeBitness.X64 | PeTypeExt.Exe | PeTypeSubSystem.ConsoleCui,

        AnyCPU32BitExe = PeTypePlatform.AnyCPU | PeTypeBitness.X86 | PeTypeExt.Exe,
        AnyCPU64BitExe = PeTypePlatform.AnyCPU | PeTypeBitness.X64 | PeTypeExt.Exe,
        ILOnly32BitExe = PeTypePlatform.ILOnly | PeTypeBitness.X86 | PeTypeExt.Exe,
        ILOnly64BitExe = PeTypePlatform.ILOnly | PeTypeBitness.X64 | PeTypeExt.Exe,
        Native32BitExe = PeTypePlatform.Native | PeTypeBitness.X86 | PeTypeExt.Exe,
        Native64BitExe = PeTypePlatform.Native | PeTypeBitness.X64 | PeTypeExt.Exe,
        Mixing32BitExe = PeTypePlatform.Mixing | PeTypeBitness.X86 | PeTypeExt.Exe,
        Mixing64BitExe = PeTypePlatform.Mixing | PeTypeBitness.X64 | PeTypeExt.Exe,

        AnyCPU32BitDll = PeTypePlatform.AnyCPU | PeTypeBitness.X86 | PeTypeExt.Dll,
        AnyCPU64BitDll = PeTypePlatform.AnyCPU | PeTypeBitness.X64 | PeTypeExt.Dll,
        ILOnly32BitDll = PeTypePlatform.ILOnly | PeTypeBitness.X86 | PeTypeExt.Dll,
        ILOnly64BitDll = PeTypePlatform.ILOnly | PeTypeBitness.X64 | PeTypeExt.Dll,
        Native32BitDll = PeTypePlatform.Native | PeTypeBitness.X86 | PeTypeExt.Dll,
        Native64BitDll = PeTypePlatform.Native | PeTypeBitness.X64 | PeTypeExt.Dll,
        Mixing32BitDll = PeTypePlatform.Mixing | PeTypeBitness.X86 | PeTypeExt.Dll,
        Mixing64BitDll = PeTypePlatform.Mixing | PeTypeBitness.X64 | PeTypeExt.Dll,

        AnyCPU32BitDrv = PeTypePlatform.AnyCPU | PeTypeBitness.X86 | PeTypeExt.Drv,
        AnyCPU64BitDrv = PeTypePlatform.AnyCPU | PeTypeBitness.X64 | PeTypeExt.Drv,
        ILOnly32BitDrv = PeTypePlatform.ILOnly | PeTypeBitness.X86 | PeTypeExt.Drv,
        ILOnly64BitDrv = PeTypePlatform.ILOnly | PeTypeBitness.X64 | PeTypeExt.Drv,
        Native32BitDrv = PeTypePlatform.Native | PeTypeBitness.X86 | PeTypeExt.Drv,
        Native64BitDrv = PeTypePlatform.Native | PeTypeBitness.X64 | PeTypeExt.Drv,
        Mixing32BitDrv = PeTypePlatform.Mixing | PeTypeBitness.X86 | PeTypeExt.Drv,
        Mixing64BitDrv = PeTypePlatform.Mixing | PeTypeBitness.X64 | PeTypeExt.Drv,

        MsDosExecutable,
        Windows16BitExecutable,
        Windows16BitVirtualDeviceDriver,

        Unknow = PeTypePlatform.Unknow | PeTypeBitness.NoBit | PeTypeExt.NoExt,

        ErrorOccurred = int.MinValue,
        NotExecutable,
        OptionalHeaderMagicNot3264,
        OptionalHeaderMagicExceedFileLength,
        OptionalHeader32ExceedFileLength,
        OptionalHeader64ExceedFileLength,
        SectionHeaderExceedFileLength,
        Cor20HeaderExceedFileLength,
    }
}