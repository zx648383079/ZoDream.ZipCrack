#include "pch.h"
#include "zip.hpp"
#include <algorithm>
#include <map>
#include <iterator>

namespace
{

template <typename T, std::size_t N = sizeof(T)>
std::istream& read(std::istream& is, T& x)
{
    static_assert(N <= sizeof(T), "read requires output type to have at least N bytes");

    // We make no assumption about platform endianness.
    x = T();
    for(std::size_t index = 0; index < N; index++)
        x |= static_cast<T>(is.get()) << (8 * index);

    return is;
}

std::istream& read(std::istream& is, std::string& string, std::size_t length)
{
    // TODO ����ת��
    // auto codec = CodeConverter("gb2312", "utf-8");
    // codec.convert(in_gb2312, strlen(in_gb2312), out, OUTLEN)
    string.resize(length);
    return is.read(&string[0], string.size());
}

template <typename T, std::size_t N = sizeof(T)>
std::ostream& write(std::ostream& os, const T& x)
{
    static_assert(N <= sizeof(T), "write requires input type to have at least N bytes");

    // We make no assumption about platform endianness.
    for(std::size_t index = 0; index < N; index++)
        os.put(lsb(x >> (8 * index)));

    return os;
}

enum class Signature : uint32
{
    LOCAL_FILE_HEADER = 0x04034b50,
    CENTRAL_DIRECTORY_HEADER = 0x02014b50,
    ZIP64_EOCD = 0x06064b50,
    ZIP64_EOCD_LOCATOR = 0x07064b50,
    EOCD = 0x06054b50
};

bool checkSignature(std::istream& is, const Signature& signature)
{
    uint32 sig;
    return read(is, sig) && sig == static_cast<uint32>(signature);
}

} // namespace

ZipError::ZipError(const std::string& description)
: BaseError("Zip error", description)
{}

ZipIterator::ZipIterator(std::istream& is)
: m_is(&is)
{
    ++(*this);
}

ZipIterator& ZipIterator::operator++()
{
    if(!checkSignature(*m_is, Signature::CENTRAL_DIRECTORY_HEADER))
        return *this = ZipIterator();

    uint16 flags;
    uint16 method;

    uint32 uncompressedSize;
    uint16 filenameLength;
    uint16 extraFieldLength;
    uint16 fileCommentLength;

    m_is->seekg(4, std::ios::cur);
    read(*m_is, flags);
    read(*m_is, method);

    m_entry.encryption =
        flags & 1 ?
            method == 99 || (flags >> 6) & 1 ?
                ZipEntry::Encryption::Unsupported :
                ZipEntry::Encryption::Traditional :
            ZipEntry::Encryption::None;

    m_entry.compression =
        method == 0 ?
            ZipEntry::Compression::Stored :
            method == 8 ?
                ZipEntry::Compression::Deflate :
                ZipEntry::Compression::Unknown;

    m_is->seekg(4, std::ios::cur);
    read(*m_is, m_entry.crc32);
    read<uint64, 4>(*m_is, m_entry.size);
    read(*m_is, uncompressedSize);
    read(*m_is, filenameLength);
    read(*m_is, extraFieldLength);
    read(*m_is, fileCommentLength);
    m_is->seekg(8, std::ios::cur);
    read<uint64, 4>(*m_is, m_entry.offset);
    read(*m_is, m_entry.name, filenameLength);

    for(int remaining = extraFieldLength; remaining > 0; )
    {
        // read extra field header
        uint16 id;
        uint16 size;
        read(*m_is, id);
        read(*m_is, size);
        remaining -= 4 + size;

        if(id == 0x0001) // Zip64 extended information
        {
            // process data block
            if(8 <= size && uncompressedSize == MASK_0_32)
            {
                m_is->seekg(8, std::ios::cur);
                size -= 8;
            }
            if(8 <= size && m_entry.size == MASK_0_32)
            {
                read(*m_is, m_entry.size);
                size -= 8;
            }
            if(8 <= size && m_entry.offset == MASK_0_32)
            {
                read(*m_is, m_entry.offset);
                size -= 8;
            }
        }
        else if(id == 0x7075) // Info-ZIP Unicode Path
        {
            uint32 nameCrc32 = MASK_0_32;
            for (byte b : m_entry.name)
                nameCrc32 = Crc32Tab::crc32(nameCrc32, b);
            nameCrc32 ^= MASK_0_32;

            uint32 expectedNameCrc32;
            m_is->seekg(1, std::ios::cur);
            read(*m_is, expectedNameCrc32);
            size -= 5;

            if (nameCrc32 == expectedNameCrc32)
                read(*m_is, m_entry.name, size);
        }
        else
        {
            // skip this data block
            m_is->seekg(size, std::ios::cur);
        }
    }

    m_is->seekg(fileCommentLength, std::ios::cur);

    if(!*m_is)
        throw ZipError("could not read central directory header");

    return *this;
}

ZipIterator ZipIterator::operator++(int)
{
    ZipIterator copy = *this;
    ++(*this);
    return copy;
}

ZipIterator locateZipEntries(std::istream& is)
{
    uint64 centralDirectoryOffset;

    // find end of central directory signature
    {
        uint32 signature;
        uint16 commentLength = 0;
        do
        {
            is.seekg(-22 - commentLength, std::ios::end);
        } while(read(is, signature) && signature != static_cast<uint32>(Signature::EOCD) && commentLength++ < MASK_0_16);

        if(!is || signature != static_cast<uint32>(Signature::EOCD))
            throw ZipError("could not find end of central directory signature");
    }

    // read end of central directory record
    {
        uint16 disk;

        read(is, disk);
        is.seekg(10, std::ios::cur);
        read<uint64, 4>(is, centralDirectoryOffset);

        if(!is)
            throw ZipError("could not read end of central directory record");
        if(disk != 0)
            throw ZipError("split zip archives are not supported");
    }

    // look for Zip64 end of central directory locator
    is.seekg(-40, std::ios::cur);
    if(checkSignature(is, Signature::ZIP64_EOCD_LOCATOR))
    {
        uint64 zip64EndOfCentralDirectoryOffset;

        is.seekg(4, std::ios::cur);
        read(is, zip64EndOfCentralDirectoryOffset);

        if(!is)
            throw ZipError("could not read Zip64 end of central directory locator record");

        // read Zip64 end of central directory record
        is.seekg(zip64EndOfCentralDirectoryOffset, std::ios::beg);
        if(checkSignature(is, Signature::ZIP64_EOCD))
        {
            uint16 versionNeededToExtract;

            is.seekg(10, std::ios::cur);
            read(is, versionNeededToExtract);
            is.seekg(32, std::ios::cur);
            read(is, centralDirectoryOffset);

            if(!is)
                throw ZipError("could not read Zip64 end of central directory record");
            if(versionNeededToExtract >= 62) // Version 6.2 introduces central directory encryption.
                throw ZipError("central directory encryption is not supported");
        }
        else
            throw ZipError("could not find Zip64 end of central directory record");
    }

    is.seekg(centralDirectoryOffset, std::ios::beg);

    return ZipIterator(is);
}

std::istream& openZipEntry(std::istream& is, const ZipEntry& entry)
{
    is.seekg(entry.offset, std::ios::beg);
    if(!checkSignature(is, Signature::LOCAL_FILE_HEADER))
        throw ZipError("could not find local file header");

    // skip local file header
    uint16 nameSize, extraSize;
    is.seekg(22, std::ios::cur);
    read(is, nameSize);
    read(is, extraSize);
    is.seekg(nameSize + extraSize, std::ios::cur);

    return is;
}


std::ifstream openZipEntry(const std::string& archive, const std::string& entry, ZipEntry::Encryption expected, std::size_t& size)
{
    std::ifstream is = openInput(archive);

    // find an entry with the given name
    ZipIterator it = std::find_if(locateZipEntries(is), ZipIterator(),
        [&entry](const ZipEntry& e)
        {
            return e.name == entry;
        });

    if(it == ZipIterator())
        throw ZipError("found no entry "+entry+" in archive "+archive);

    // check encryption algorithm
    if(it->encryption != expected)
    {
        if(it->encryption == ZipEntry::Encryption::None)
            throw ZipError("entry "+entry+" in archive "+archive+" is not encrypted");
        else if(expected == ZipEntry::Encryption::None)
            throw ZipError("entry "+entry+" in archive "+archive+" is encrypted");
        else
            throw ZipError("entry "+entry+" in archive "+archive+" is encrypted with an unsupported algorithm");
    }

    openZipEntry(is, *it);
    size = it->size;

    return is;
}

bytevec loadZipEntry(const std::string& archive, const std::string& entry, ZipEntry::Encryption expected, std::size_t size)
{
    std::size_t entrySize;
    std::ifstream is = openZipEntry(archive, entry, expected, entrySize);
    return loadStream(is, (std::min)(entrySize, size));
}

void changeKeys(std::istream& is, std::ostream& os, const Keys& oldKeys, const Keys& newKeys, Logger& logger)
{
    // Store encrypted entries local file header offset and packed size.
    // Use std::map to sort them by local file header offset.
    std::map<uint64, uint64> packedSizeByLocalOffset;
    std::for_each(locateZipEntries(is), ZipIterator(),
        [&packedSizeByLocalOffset](const ZipEntry& e)
        {
            if(e.encryption == ZipEntry::Encryption::Traditional)
                packedSizeByLocalOffset.insert({e.offset, e.size});
        });

    // Rewind input stream and iterate on encrypted entries to change the keys, copy the rest.
    is.seekg(0, std::ios::beg);
    uint64 currentOffset = 0;

    int progress = 0;
    int total = packedSizeByLocalOffset.size();
    logger.Progress(progress, total);
    for(const std::pair<uint64, uint64>& pair : packedSizeByLocalOffset)
    {
        const uint64& localHeaderOffset = pair.first,
                      packedSize = pair.second;

        if(currentOffset < localHeaderOffset)
        {
            std::copy_n(std::istreambuf_iterator<char>(is), localHeaderOffset - currentOffset, std::ostreambuf_iterator<char>(os));
            is.get();
        }

        if(!checkSignature(is, Signature::LOCAL_FILE_HEADER))
            throw ZipError("could not find local file header");

        write(os, static_cast<uint32>(Signature::LOCAL_FILE_HEADER));

        std::copy_n(std::istreambuf_iterator<char>(is), 22, std::ostreambuf_iterator<char>(os));
        is.get();

        uint16 filenameLength, extraSize;
        read(is, filenameLength);
        read(is, extraSize);
        write(os, filenameLength);
        write(os, extraSize);

        if(0 < filenameLength + extraSize)
        {
            std::copy_n(std::istreambuf_iterator<char>(is), filenameLength + extraSize, std::ostreambuf_iterator<char>(os));
            is.get();
        }

        Keys decrypt = oldKeys,
             encrypt = newKeys;
        std::istreambuf_iterator<char> in(is);
        std::generate_n(std::ostreambuf_iterator<char>(os), packedSize,
            [&in, &decrypt, &encrypt]() -> char
            {
                byte p = *in++ ^ decrypt.getK();
                byte c = p ^ encrypt.getK();
                decrypt.update(p);
                encrypt.update(p);
                return c;
            });

        currentOffset = localHeaderOffset + 30 + filenameLength + extraSize + packedSize;

        logger.Progress(++progress, total);
    }

    std::copy(std::istreambuf_iterator<char>(is), std::istreambuf_iterator<char>(), std::ostreambuf_iterator<char>(os));
}

void decipher(std::istream& is, std::size_t size, 
    std::size_t discard, std::ostream& os, Keys keys, Logger& logger)
{
    std::istreambuf_iterator<char> cipher(is);
    std::size_t i;

    for (i = 0; i < discard && i < size && cipher != std::istreambuf_iterator<char>(); i++, ++cipher)
    {
        keys.update(*cipher ^ keys.getK());
    }

    for(std::ostreambuf_iterator<char> plain(os); i < size && cipher != std::istreambuf_iterator<char>(); i++, ++cipher, ++plain)
    {
        byte p = *cipher ^ keys.getK();
        keys.update(p);
        *plain = p;
    }
}

