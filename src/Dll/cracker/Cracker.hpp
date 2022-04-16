
#include "Logger.hpp"
#include "Keys.hpp"
#include <map>

class Cracker
{
public:
	Cracker(Logger& logger);
	int Offset = 0;
	std::size_t Plainsize = 1 << 20;
	std::map<int, byte> ExtraPlaintext;
	bool Exhaustive = false;
	/// <summary>
	/// 获取key
	/// </summary>
	/// <param name="cipherFile"></param>
	/// <param name="cipherFileName"></param>
	/// <param name="plainFile"></param>
	/// <param name="plainFileName"></param>
	/// <returns></returns>
	std::vector<Keys> FindKey(const std::string& cipherFile, const std::string& cipherFileName, const std::string& plainFile, const std::string& plainFileName);
	std::vector<Keys> FindKey(const std::string& cipherFile, const std::string& cipherFileName, const std::string& plainTextFile);
	std::vector<Keys> FindKey(const std::string& cipherFile, long cipherBegin, long cipherEnd, const std::string& plainFile, long plainBegin, long plainEnd);
	std::vector<Keys> FindKey(const std::string& cipherFile, long cipherBegin, long cipherEnd, bytevec plainData);
	std::vector<Keys> FindKey(const std::string& cipherFile, const std::string& cipherFileName, bytevec plainData);
	std::vector<Keys> FindKey(bytevec cipherData, bytevec plainData);
	/// <summary>
	/// 根据key获取压缩文件
	/// </summary>
	/// <param name="keys"></param>
	/// <param name="cipherFile"></param>
	/// <param name="distFolder"></param>
	/// <returns></returns>
	bool Unpack(const Keys& keys, const std::string& cipherFile, const std::string& distFolder);
	bool Unpack(const Keys& keys, const std::string& cipherFile, long cipherBegin, long cipherEnd, const std::string& distFile);
	bool Unpack(const Keys& keys, const std::string& cipherFile, const std::string& cipherFileName, const std::string& distFile);
	/// <summary>
	/// 根据keys把压缩包转成不设密码的压缩包
	/// </summary>
	/// <param name="keys"></param>
	/// <param name="cipherFile"></param>
	/// <param name="distFile"></param>
	/// <returns></returns>
	bool Pack(const Keys& keys, const std::string& cipherFile, const std::string& distFile);
	/// <summary>
	/// 根据keys把压缩包转成已知密码的压缩包
	/// </summary>
	/// <param name="keys"></param>
	/// <param name="cipherFile"></param>
	/// <param name="distFile"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	bool Pack(const Keys& keys, const std::string& cipherFile, const std::string& distFile, const std::string& password);
	/// <summary>
	/// 根据keys修复密码
	/// </summary>
	/// <param name="keys"></param>
	/// <param name="maxLength"></param>
	/// <param name="charset"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	bool RecoverPassword(const Keys& keys, std::size_t maxLength, const bytevec& charset, std::string& password);
	bool RecoverPassword(const Keys& keys, std::size_t maxLength, const std::string& charset, std::string& password);
private:
	Logger& logger;
};
