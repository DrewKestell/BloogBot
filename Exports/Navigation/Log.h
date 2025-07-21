// wow/vmap/Log.h
#pragma once
#include <cstdio>
#include <cstdarg>

namespace wow::vmap
{
    inline void LogInfo(const char* fmt, ...) {}
    inline void LogWarn(const char* fmt, ...) { std::va_list va; va_start(va, fmt); std::vfprintf(stderr, fmt, va); va_end(va); }
    inline void LogError(const char* fmt, ...) { std::va_list va; va_start(va, fmt); std::vfprintf(stderr, fmt, va); va_end(va); }
}
