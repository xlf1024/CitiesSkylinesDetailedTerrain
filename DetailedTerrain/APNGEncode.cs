using ICSharpCode.SharpZipLib.Checksums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DetailedTerrain.APNG {
    class APNGEncode {
        static byte[] EncodeChunk(char[] name, byte[] data) {
            var encoded = new byte[4 + 4 + data.Length + 4];
            BitConverter.GetBytes(data.Length).CopyTo(encoded, 0);
            name.CopyTo(encoded, 4);
            data.CopyTo(encoded, 8);
            var crcCalc = new Crc32();
            crcCalc.Update(encoded, 4, data.Length + 4);
            BitConverter.GetBytes(Convert.ToInt32(crcCalc.Value)).CopyTo(encoded, encoded.Length - 4);
            return encoded;
        }
    }
    enum BitDepth : byte {
        One = 1,
        Two = 2,
        Four = 4,
        Eight = 8,
        Sixteen = 16
    }
    enum ColourType : byte {
        Greyscale = 0b000,
        Truecolour = 0b010,
        Indexed = 0b011,
        AlphaGrey = 0b100,
        AlphaColour = 0b110
    }
    enum CompressionMethod : byte {
        Deflate = 0
    }
    enum FilterMethod : byte {
        Default = 0
    }
    enum InterlaceMethod : byte {
        None = 0,
        Adam7 = 1
    }

    abstract class APNGChunk {
        public abstract byte[] ChunkType { get; }
        public byte[] Encode() {
            var data = EncodeData();
            var encoded = new byte[4 + 4 + data.Length + 4];
            BitConverter.GetBytes(data.Length).CopyTo(encoded, 0);
            ChunkType.CopyTo(encoded, 4);
            data.CopyTo(encoded, 8);
            var crcCalc = new Crc32();
            crcCalc.Update(encoded, 4, data.Length + 4);
            BitConverter.GetBytes(Convert.ToInt32(crcCalc.Value)).CopyTo(encoded, encoded.Length - 4);
            return encoded;
        }
        protected abstract byte[] EncodeData();
        protected APNGChunk() { }
    }
    class IHDRChunk : APNGChunk {
        readonly static byte[] _chunkType = Encoding.ASCII.GetBytes("IHDR");
        public override byte[] ChunkType => _chunkType;
        uint width;
        uint height;
        BitDepth bitDepth;
        ColourType colourType;
        CompressionMethod compressionMethod;
        FilterMethod filterMethod;
        InterlaceMethod interlaceMethod;

        protected override byte[] EncodeData() {
            var encoded = new byte[13];
            BitConverter.GetBytes(width).CopyTo(encoded, 0);
            BitConverter.GetBytes(height).CopyTo(encoded, 4);
            encoded[8] = (byte)bitDepth;
            encoded[9] = (byte)colourType;
            encoded[10] = (byte)compressionMethod;
            encoded[11] = (byte)filterMethod;
            encoded[12] = (byte)interlaceMethod;
            return encoded;
        }
        protected IHDRChunk(byte[] encoded) : base() {
            width = BitConverter.ToUInt32(encoded, 0);
            height = BitConverter.ToUInt32(encoded, 4);
            bitDepth = (BitDepth)encoded[8];
            colourType = (ColourType)encoded[9];
            compressionMethod = (CompressionMethod)encoded[10];
            filterMethod = (FilterMethod)encoded[11];
            interlaceMethod = (InterlaceMethod)encoded[12];
        }
    }
    struct Colour24 {
        byte r;
        byte g;
        byte b;
    }
    struct Colour48 {
        ushort r;
        ushort g;
        ushort b;
    }
    class PLTE {
        Colour24[] palletteEntries;
    }
    struct IDAT {
        byte[] data;
    }
    struct IEND { }
    struct tRNS_0 {
        ushort transparentBrightness;
    }
    struct tRNS_2 {
        Colour48 transparentColour;
    }
    struct tRNS_3 {
        byte[] palleteEntries;
    }
    struct bKGD_0 {
        ushort backgroundBrightness;
    }
    struct bKGD_2 {
        Colour48 backgroundColour;
    }
    struct bKGD_3 {
        byte backgroundIndex;
    }
    struct bKGD_4 {
        ushort backgroundBrightness;
    }
    struct bKGD_6 {
        Colour48 backgroundColour;
    }
    struct acTL {
        uint num_frames;
        uint num_plays;
    }
    enum Dispose_Op : byte {
        APNG_DISPOSE_OP_NONE = 0,
        APNG_DISPOSE_OP_BACKGROUND = 1,
        APNG_DISPOSE_OP_PREVIOUS = 2
    }
    enum Blend_Op : byte {
        APNG_BLEND_OP_SOURCE = 0,
        APNG_BLEND_OP_OVER = 1
    }
    [StructLayout(LayoutKind.Sequential)]
    struct fcTL {
        uint sequence_number;
        uint width;
        uint height;
        uint x_offset;
        uint y_offset;
        ushort delay_num;
        ushort delay_den;
        Dispose_Op dispose_op;
        Blend_Op blend_op;
    }

    struct fdAT {
        uint sequence_number;
        byte[] frame_data;
    }

}
