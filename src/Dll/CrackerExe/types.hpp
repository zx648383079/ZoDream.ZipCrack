#ifndef BKCRACK_TYPES_HPP
#define BKCRACK_TYPES_HPP

/// \file types.hpp
/// \brief Typedefs, useful constants and utility functions

#include <stdexcept>
#include <cstdint>
#include <vector>
#include <array>
#include <string>

/// Base exception type
class BaseError : public std::runtime_error
{
public:
    /// Constructor
    BaseError(const std::string& type, const std::string& description);
};

// scalar types

using byte = std::uint8_t;    ///< Unsigned integer type with width of exactly 8 bits
using uint16 = std::uint16_t; ///< Unsigned integer type with width of exactly 16 bits
using uint32 = std::uint32_t; ///< Unsigned integer type with width of exactly 32 bits
using uint64 = std::uint64_t; ///< Unsigned integer type with width of exactly 64 bits

// container types

template <std::size_t N>
using bytearr = std::array<byte, N>; ///< Array of bytes

template <std::size_t N>
using u32arr = std::array<uint32, N>; ///< Array of 32-bits integers

using bytevec = std::vector<byte>;  ///< Vector of bytes
using u32vec = std::vector<uint32>; ///< Vector of 32-bits integers

#endif
