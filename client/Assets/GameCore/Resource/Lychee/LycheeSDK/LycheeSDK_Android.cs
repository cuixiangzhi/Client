using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LycheeSDK
{
    namespace Android
    {
        class EOCD
        {
            public uint signature;
            public ushort numberOfThisDisk;
            public ushort numberOfTheDiskWithStartCD;
            public ushort numberOfCentralDirectoryRecords;
            public ushort totalNumberOfCentralDirectoryRecords;
            public uint sizeOfCentralDirectory;
            public uint offsetOfCentralDirectory;
            public byte[] comment;

            public static EOCD Read(BinaryReader reader)
            {
                EOCD result = new EOCD();

                if ((result.signature = reader.ReadUInt32()) != 0x06054b50) {
                    return null;
                }

                result.numberOfThisDisk = reader.ReadUInt16();
                result.numberOfTheDiskWithStartCD = reader.ReadUInt16();
                result.numberOfCentralDirectoryRecords = reader.ReadUInt16();
                result.totalNumberOfCentralDirectoryRecords = reader.ReadUInt16();
                result.sizeOfCentralDirectory = reader.ReadUInt32();
                result.offsetOfCentralDirectory = reader.ReadUInt32();

                ushort commentLength = reader.ReadUInt16();
                result.comment = reader.ReadBytes(commentLength);

                return result;
            }
        }

        class Zip64EOCD
        {
            public uint signature;
            public ulong sizeOfThisRecord;
            public ushort versionMadeBy;
            public ushort versionNeededToExtract;
            public uint numberOfThisDisk;
            public uint numberOfTheDiskWithStartCD;
            public ulong numberOfCentralDirectoryRecords;
            public ulong totalNumberOfCentralDirectoryRecords;
            public ulong sizeOfCentralDirectory;
            public ulong offsetOfCentralDirectory;

            public static Zip64EOCD Read(BinaryReader reader)
            {
                Zip64EOCD result = new Zip64EOCD();

                if ((result.signature = reader.ReadUInt32()) != 0x06064b50) {
                    return null;
                }

                result.sizeOfThisRecord = reader.ReadUInt64();
                result.versionMadeBy = reader.ReadUInt16();
                result.versionNeededToExtract = reader.ReadUInt16();
                result.numberOfThisDisk = reader.ReadUInt32();
                result.numberOfTheDiskWithStartCD = reader.ReadUInt32();
                result.numberOfCentralDirectoryRecords = reader.ReadUInt64();
                result.totalNumberOfCentralDirectoryRecords = reader.ReadUInt64();
                result.sizeOfCentralDirectory = reader.ReadUInt64();
                result.offsetOfCentralDirectory = reader.ReadUInt64();

                return result;
            }
        }

        class Zip64EOCDLocator
        {
            public uint signature;
            public uint numberOfDiskWithZip64EOCD;
            public ulong offsetOfZip64EOCD;
            public uint totalNumberOfDisks;

            public static Zip64EOCDLocator Read(BinaryReader reader)
            {
                Zip64EOCDLocator result = new Zip64EOCDLocator();

                if ((result.signature = reader.ReadUInt32()) != 0x07064b50) {
                    return null;
                }

                result.numberOfDiskWithZip64EOCD = reader.ReadUInt32();
                result.offsetOfZip64EOCD = reader.ReadUInt64();
                result.totalNumberOfDisks = reader.ReadUInt32();

                return result;
            }
        }

        class CentralDirectoryFileHeader
        {
            public uint signature;
            public ushort versionMadeBy;
            public ushort versionNeededToExtract;
            public ushort generalPurposeBitFlag;
            public ushort compressionMethod;
            public uint lastModified;
            public uint crc32;
            public long compressedSize;
            public long uncompressedSize;
            public ushort filenameLength;
            public ushort extraFieldLength;
            public ushort fileCommentLength;
            public int diskNumberStart;
            public ushort internalFileAttributes;
            public uint externalFileAttributes;
            public long relativeOffsetOfLocalHeader;

            public byte[] filename;

            public static CentralDirectoryFileHeader Read(BinaryReader reader)
            {
                CentralDirectoryFileHeader result = new CentralDirectoryFileHeader();

                if ((result.signature = reader.ReadUInt32()) != 0x02014b50) {
                    return null;
                }

                result.versionMadeBy = reader.ReadUInt16();
                result.versionNeededToExtract = reader.ReadUInt16();
                result.generalPurposeBitFlag = reader.ReadUInt16();
                result.compressionMethod = reader.ReadUInt16();
                result.lastModified = reader.ReadUInt32();
                result.crc32 = reader.ReadUInt32();
                result.compressedSize = reader.ReadUInt32();
                result.uncompressedSize = reader.ReadUInt32();
                result.filenameLength = reader.ReadUInt16();
                result.extraFieldLength = reader.ReadUInt16();
                result.fileCommentLength = reader.ReadUInt16();
                result.diskNumberStart = reader.ReadUInt16();
                result.internalFileAttributes = reader.ReadUInt16();
                result.externalFileAttributes = reader.ReadUInt32();
                result.relativeOffsetOfLocalHeader = reader.ReadUInt32();
                result.filename = reader.ReadBytes(result.filenameLength);

                long end = reader.BaseStream.Position +
                    result.extraFieldLength + result.fileCommentLength;

                if (result.extraFieldLength > 4)
                {
                    ushort tag = reader.ReadUInt16();
                    ushort size = reader.ReadUInt16();

                    if (tag == 1)
                    {
                        if (size >= 8 && result.uncompressedSize == uint.MaxValue)
                        {
                            result.uncompressedSize = reader.ReadInt64();
                            size -= 8;
                        }

                        if (size >= 8 && result.compressedSize == uint.MaxValue)
                        {
                            result.compressedSize = reader.ReadInt64();
                            size -= 8;
                        }

                        if (size >= 8 && result.relativeOffsetOfLocalHeader == uint.MaxValue)
                        {
                            result.relativeOffsetOfLocalHeader = reader.ReadInt64();
                            size -= 8;
                        }

                        if (size >= 4 && result.diskNumberStart == ushort.MaxValue)
                        {
                            result.diskNumberStart = reader.ReadInt32();
                            size -= 4;
                        }
                    }
                }

                reader.BaseStream.Seek(end, SeekOrigin.Begin);

                return result;
            }
        }

        class LocalFileHeader
        {
            public uint signature;
            public ushort versionNeededToExtract;
            public ushort generalPurposeBitFlag;
            public ushort compressionMethod;
            public uint lastModified;
            public uint crc32;
            public uint compressedSize;
            public uint uncompressedSize;
            public ushort filenameLength;
            public ushort extraFieldLength;

            public static LocalFileHeader Read(BinaryReader reader)
            {
                LocalFileHeader result = new LocalFileHeader();

                if ((result.signature = reader.ReadUInt32()) != 0x04034b50) {
                    return null;
                }

                result.versionNeededToExtract = reader.ReadUInt16();
                result.generalPurposeBitFlag = reader.ReadUInt16();
                result.compressionMethod = reader.ReadUInt16();
                result.lastModified = reader.ReadUInt32();
                result.crc32 = reader.ReadUInt32();
                result.compressedSize = reader.ReadUInt32();
                result.uncompressedSize = reader.ReadUInt32();
                result.filenameLength = reader.ReadUInt16();
                result.extraFieldLength = reader.ReadUInt16();

                reader.BaseStream.Seek(result.filenameLength, SeekOrigin.Current);
                reader.BaseStream.Seek(result.extraFieldLength, SeekOrigin.Current);

                return result;
            }
        }

        class FileInfo
        {
            public FileInfo(long offset, long size)
            {
                this.offset = offset;
                this.size = size;
            }

            public long offset;
            public long size;
        }

        class Utility
        {
            static int ReadFileBackward(Stream stream, byte[] buffer, int count)
            {
                if (stream.Position <= 0) {
                    return 0;
                }

                if (count > stream.Position) {
                    count = (int)stream.Position;
                }

                stream.Seek(-count, SeekOrigin.Current);

                count = LycheeSDK.Utility.ReadFile(stream, buffer, count);

                stream.Seek(-count, SeekOrigin.Current);

                return count;
            }

            static bool SeekSignatureBackward(Stream stream, uint signatureToFind)
            {
                uint signature = 0;

                byte[] buffer = new byte[32];

                int count;

                while ((count = ReadFileBackward(stream, buffer, buffer.Length)) > 0)
                {
                    while (count > 0)
                    {
                        if ((signature = (signature << 8) | buffer[count - 1]) == signatureToFind) {
                            break;
                        }

                        --count;
                    }

                    if (count > 0) {
                        break;
                    }
                }

                if (count > 0)
                {
                    stream.Seek(count - 1, SeekOrigin.Current);
                    return true;
                }

                return false;
            }

            public static void GetFileInfosFromAPK(FileStream stream, string basePath, Dictionary<string, FileInfo> result)
            {
                BinaryReader reader = new BinaryReader(stream);

                stream.Seek(0, SeekOrigin.End);

                if (!SeekSignatureBackward(stream, 0x06054B50)) {
                    return;
                }

                long OffsetOfEOCD = stream.Position;

                EOCD eocd = EOCD.Read(reader);

                if (eocd.numberOfThisDisk != eocd.numberOfTheDiskWithStartCD) {
                    return;
                }

                if (eocd.numberOfCentralDirectoryRecords != eocd.totalNumberOfCentralDirectoryRecords) {
                    return;
                }

                long numberOfRecords = eocd.numberOfCentralDirectoryRecords;
                long centralDirectoryStart = eocd.offsetOfCentralDirectory;

                if (numberOfRecords == ushort.MaxValue || centralDirectoryStart == uint.MaxValue)
                {
                    stream.Seek(OffsetOfEOCD, SeekOrigin.Begin);

                    if (!SeekSignatureBackward(stream, 0x07064B50)) {
                        return;
                    }

                    Zip64EOCDLocator locator = Zip64EOCDLocator.Read(reader);

                    if (locator.offsetOfZip64EOCD > long.MaxValue) {
                        return;
                    }

                    stream.Seek((long)locator.offsetOfZip64EOCD, SeekOrigin.Begin);

                    Zip64EOCD zip64eocd = Zip64EOCD.Read(reader);

                    if (zip64eocd == null) {
                        return;
                    }

                    if (zip64eocd.numberOfCentralDirectoryRecords != zip64eocd.totalNumberOfCentralDirectoryRecords) {
                        return;
                    }

                    if (zip64eocd.numberOfCentralDirectoryRecords > long.MaxValue) {
                        return;
                    }

                    if (zip64eocd.offsetOfCentralDirectory > long.MaxValue) {
                        return;
                    }

                    numberOfRecords = (long)zip64eocd.numberOfCentralDirectoryRecords;
                    centralDirectoryStart = (long)zip64eocd.offsetOfCentralDirectory;
                }

                stream.Seek(centralDirectoryStart, SeekOrigin.Begin);

                CentralDirectoryFileHeader cdHeader;

                while ((cdHeader = CentralDirectoryFileHeader.Read(reader)) != null)
                {
                    if (cdHeader.compressedSize != cdHeader.uncompressedSize) {
                        continue;
                    }

                    string filename = Encoding.UTF8.GetString(cdHeader.filename).Replace('\\', '/');

                    if (filename.EndsWith("/")) {
                        continue;
                    }

                    if (!filename.StartsWith("assets/", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    filename = filename.Remove(0, 7);

                    if (filename.StartsWith("bin/", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    if (basePath.Length > 0 && !filename.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    filename = filename.Remove(0, basePath.Length);

                    long lastPosition = stream.Position;

                    stream.Seek(cdHeader.relativeOffsetOfLocalHeader, SeekOrigin.Begin);

                    LocalFileHeader lfHeader = LocalFileHeader.Read(reader);

                    if (lfHeader != null) {
                        result.Add(filename, new FileInfo(stream.Position, cdHeader.uncompressedSize));
                    }

                    stream.Seek(lastPosition, SeekOrigin.Begin);
                }
            }
        }

        class SubStream : Stream
        {
            public SubStream(Stream baseStream, long offset, long length)
            {
                baseStream_ = baseStream;
                offset_ = offset;
                length_ = length;
                endOfStream_ = offset + length;
                position_ = 0;
            }

            public override bool CanRead
            {
                get
                {
                    return baseStream_.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return baseStream_.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return baseStream_.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    Check();
                    return length_;
                }
            }

            public override long Position
            {
                get
                {
                    Check();
                    return position_;
                }

                set
                {
                    Seek(value, SeekOrigin.Begin);
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                Check();

                if (baseStream_.Position != offset_ + position_) {
                    baseStream_.Seek(offset_ + position_, SeekOrigin.Begin);
                }

                if (endOfStream_ - baseStream_.Position < count)
                {
                    count = (int)(endOfStream_ - baseStream_.Position);
                }

                count = baseStream_.Read(buffer, offset, count);

                position_ += count;

                return count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                Check();

                if (origin == SeekOrigin.Begin)
                {
                    position_ = offset;
                }
                else if (origin == SeekOrigin.End)
                {
                    position_ = length_ + offset;
                }
                else
                {
                    position_ = position_ + offset;
                }

                if (position_ < 0)
                {
                    throw new ArgumentException("Seeking is attempted before the beginning of the stream.");
                }

                baseStream_.Seek(offset_ + position_, SeekOrigin.Begin);

                return position_;
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (baseStream_ != null) {
                    baseStream_ = null;
                }

                base.Dispose(disposing);
            }

            void Check()
            {
                if (baseStream_ == null)
                {
                    throw new ObjectDisposedException(GetType().ToString(), "Object disposed.");
                }
            }

            Stream baseStream_;
            long offset_;
            long length_;
            long endOfStream_;
            long position_;
        }

        public class APKFileSystem
        {
            public APKFileSystem(string basePath = "")
            {
                basePath = basePath.Replace('\\', '/');

                if (basePath.StartsWith("/")) {
                    basePath.Remove(0, 1);
                }

                if (basePath.Length > 0 && !basePath.EndsWith("/")) {
                    basePath += "/";
                }

                basePath_ = basePath;
                stream_ = File.OpenRead(Application.dataPath);
                fileInfos_ = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);

                Utility.GetFileInfosFromAPK(stream_, basePath_, fileInfos_);
            }

            public bool Exists(string filename)
            {
                return fileInfos_.ContainsKey(filename);
            }

            public string[] GetFiles()
            {
                return fileInfos_.Keys.ToArray();
            }

            public Stream OpenRead(string filename)
            {
                FileInfo fi;

                if (!fileInfos_.TryGetValue(filename, out fi))
                {
                    throw new FileNotFoundException(filename);
                }

                return new SubStream(stream_, fi.offset, fi.size);
            }

            string basePath_;
            FileStream stream_;
            Dictionary<string, FileInfo> fileInfos_;
        }
    }
}

