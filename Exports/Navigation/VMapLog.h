// VMapLog.h - Logging utilities for VMAP system
#pragma once

#include <iostream>
#include <sstream>

#define WARN_LOG
#define ERROR_LOG
#define INFO_LOG
#define DEBUG_LOG
#define TRACE_LOG
#define VECTOR3_LOG
#define RAY_LOG

#ifdef WARN_LOG
#define LOG_WARN(msg) do { \
        std::stringstream ss; \
        ss << "[WARN] " << msg; \
        std::cout << ss.str() << std::endl; \
    } while(0)
#else
#define LOG_WARN(msg)
#endif // WARN_LOG

#ifdef DEBUG_LOG
#define LOG_DEBUG(msg) do { \
        std::stringstream ss; \
        ss << "[DEBUG] " << msg; \
        std::cout << ss.str() << std::endl; \
    } while(0)
#else
#define LOG_DEBUG(msg)
#endif // DEBUG_LOG

#ifdef ERROR_LOG
#define LOG_ERROR(msg) do { \
        std::stringstream ss; \
        ss << "[ERROR] " << msg; \
        std::cout << ss.str() << std::endl; \
    } while(0)
#else
#define LOG_ERROR(msg)
#endif // ERROR_LOG

#ifdef INFO_LOG
#define LOG_INFO(msg) do { \
        std::stringstream ss; \
        ss << "[INFO] " << msg; \
        std::cout << ss.str() << std::endl; \
    } while(0)
#else
#define LOG_INFO(msg)
#endif // INFO_LOG

#ifdef TRACE_LOG
#define LOG_TRACE(msg) do { \
        std::stringstream ss; \
        ss << "[TRACE] " << msg; \
        std::cout << ss.str() << std::endl; \
    } while(0)
#else
#define LOG_TRACE(msg)
#endif // TRACE_LOG

#ifdef VECTOR3_LOG
#define LOG_VECTOR3(label, v) do { \
        std::cout << "[VECTOR3] " << label << ": (" \
                  << v.x << ", " << v.y << ", " << v.z << ")" << std::endl; \
    } while(0)
#else
#define LOG_VECTOR3(label, v)
#endif // VECTOR3_LOG

#ifdef RAY_LOG
#define LOG_RAY(label, r) do { \
        std::cout << "[RAY] " << label << ": origin(" \
                  << r.origin().x << ", " << r.origin().y << ", " << r.origin().z \
                  << ") dir(" << r.direction().x << ", " << r.direction().y << ", " \
                  << r.direction().z << ")" << std::endl; \
    } while(0)
#else
#define LOG_RAY(label, r)
#endif // RAY_LOG