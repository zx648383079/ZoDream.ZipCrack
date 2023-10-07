#include "pch.h"
//#include "CodeConverter.hpp"
//
//CodeConverter::CodeConverter(const char* from_charset, const char* to_charset)
//{
//	codec = iconv_open(to_charset, from_charset);
//}
//
//CodeConverter::~CodeConverter() 
//{
//	iconv_close(codec);
//}
//
//int CodeConverter::convert(char* inbuf, int inlen, char* outbuf, int outlen)
//{
//	char** pin = &inbuf;
//	char** pout = &outbuf;
//
//	memset(outbuf, 0, outlen);
//	return iconv(codec, pin, (size_t*)&inlen, pout, (size_t*)&outlen);
//}
