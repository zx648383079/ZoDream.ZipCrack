#include "pch.h"
#include "Cracker.hpp"
#include "zip.hpp"
#include "Data.hpp"
#include "Zreduction.hpp"
#include "Attack.hpp"
#include "password.hpp"
#include <charconv>
#include <algorithm>

Cracker::Cracker(Logger& logger): logger{logger}
{}

std::vector<Keys> Cracker::FindKey(const std::string& cipherFile, const std::string& cipherFileName, const std::string& plainFile, const std::string& plainFileName)
{
	bytevec plaintext = loadZipEntry(plainFile, plainFileName, ZipEntry::Encryption::None, Plainsize);
	std::size_t needed = Data::ENCRYPTION_HEADER_SIZE;
	needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + Offset + plaintext.size());
	if (!ExtraPlaintext.empty())
	{
		needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + ExtraPlaintext.rbegin()->first + 1);
	}
	return FindKey(loadZipEntry(cipherFile, cipherFileName, ZipEntry::Encryption::Traditional, needed), plaintext);
}

std::vector<Keys> Cracker::FindKey(const std::string& cipherFile, const std::string& cipherFileName, const std::string& plainTextFile)
{
	bytevec plaintext = loadFile(plainTextFile, Plainsize);
	std::size_t needed = Data::ENCRYPTION_HEADER_SIZE;
	needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + Offset + plaintext.size());
	if (!ExtraPlaintext.empty())
	{
		needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + ExtraPlaintext.rbegin()->first + 1);
	}
	return FindKey(loadZipEntry(cipherFile, cipherFileName, ZipEntry::Encryption::Traditional, needed), plaintext);
}
std::vector<Keys> Cracker::FindKey(const std::string& cipherFile, long cipherBegin, long cipherEnd, const std::string& plainFile, long plainBegin, long plainEnd)
{
	std::ifstream is = openInput(plainFile);
	is.seekg(plainBegin, std::ios::beg);
	return FindKey(cipherFile, cipherBegin, cipherEnd, loadStream(is, plainEnd - plainBegin));
}
std::vector<Keys> Cracker::FindKey(const std::string& cipherFile, const std::string& cipherFileName, bytevec plainData)
{
	std::size_t needed = Data::ENCRYPTION_HEADER_SIZE;
	needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + Offset + plainData.size());
	if (!ExtraPlaintext.empty())
	{
		needed = (std::max)(needed, Data::ENCRYPTION_HEADER_SIZE + ExtraPlaintext.rbegin()->first + 1);
	}
	return FindKey(loadZipEntry(cipherFile, cipherFileName, ZipEntry::Encryption::Traditional, needed), plainData);
}
std::vector<Keys> Cracker::FindKey(const std::string& cipherFile, long cipherBegin, long cipherEnd, bytevec plainData)
{
	std::ifstream is = openInput(cipherFile);
	is.seekg(cipherBegin, std::ios::beg);
	return FindKey(loadStream(is, cipherEnd - cipherBegin), plainData);
}

std::vector<Keys> Cracker::FindKey(bytevec cipherData, bytevec plainData)
{
	Data data = Data(std::move(cipherData), std::move(plainData), Offset, ExtraPlaintext);
	Zreduction zr(data.keystream);

	if (data.keystream.size() > Attack::CONTIGUOUS_SIZE)
	{
		char buffer[100];
		sprintf_s(buffer, "Z reduction using %d bytes of known plaintext", data.keystream.size() - Attack::CONTIGUOUS_SIZE);
		logger.Info(buffer);
		zr.reduce(logger);
	}
    // generate Zi[2,32) values
    zr.generate();

	char buffer[100];
	sprintf_s(buffer, "Attack on %d  Z values at index %d", zr.getCandidates().size(), 
		data.offset + zr.getIndex() - Data::ENCRYPTION_HEADER_SIZE);
	logger.Info(buffer);
    return attack(data, zr.getCandidates(), zr.getIndex(), Exhaustive, logger);

}
bool Cracker::Unpack(const Keys& keys, const std::string& cipherFile, const std::string& distFolder)
{
	std::ifstream cipherStream = openInput(cipherFile);
	for (ZipIterator iter = locateZipEntries(cipherStream); iter != ZipIterator(); iter++)
	{
		if (logger.IsCancellationRequested) {
			continue;
		}
		ZipEntry e = *iter;
		logger.Info(e.name.c_str());
		std::ofstream distStream = openOutput(distFolder + e.name);
		decipher(cipherStream, e.size, e.offset, distStream, keys);
	}
	return true;
}
bool Cracker::Unpack(const Keys& keys, const std::string& cipherFile, long cipherBegin, long cipherEnd, const std::string& distFile)
{
	std::ofstream distStream = openOutput(distFile);
	std::ifstream cipherStream = openInput(cipherFile);
	cipherStream.seekg(cipherBegin, std::ios::beg);
	decipher(cipherStream, cipherEnd  - cipherBegin, Data::ENCRYPTION_HEADER_SIZE, distStream, keys);
	return true;
}
bool Cracker::Unpack(const Keys& keys, const std::string& cipherFile, const std::string& cipherFileName, const std::string& distFile)
{
	std::size_t ciphersize = (std::numeric_limits<std::size_t>::max)();
	std::ofstream distStream = openOutput(distFile);
	std::ifstream cipherStream = openZipEntry(cipherFile, cipherFileName, ZipEntry::Encryption::Traditional, ciphersize);
	decipher(cipherStream, ciphersize, Data::ENCRYPTION_HEADER_SIZE, distStream, keys);
	return true;
}
bool Cracker::Pack(const Keys& keys, const std::string& cipherFile, const std::string& distFile)
{
	return Pack(keys, cipherFile, distFile, "");
}
bool Cracker::Pack(const Keys& keys, const std::string& cipherFile, const std::string& distFile, const std::string& password)
{
	std::ifstream encrypted = openInput(cipherFile);
	std::ofstream unlocked = openOutput(distFile);
	changeKeys(encrypted, unlocked, keys, Keys(password), logger);
	return true;
}
bool Cracker::RecoverPassword(const Keys& keys, std::size_t maxLength, const bytevec& charset, std::string& password)
{
	return recoverPassword(keys, maxLength, charset, password, logger);
}

bool Cracker::RecoverPassword(const Keys& keys, std::size_t maxLength, const std::string& charset, std::string& password)
{
	const bytevec& charsetData = passwordCharset(charset);
	return RecoverPassword(keys, maxLength, charsetData, password);
}