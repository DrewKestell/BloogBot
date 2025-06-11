#pragma once
#include <cstdio>
#include <cstring>
#include <vector>
#include <iostream>

#include "Vec3Ray.h" // for Vec3, MeshTriangle, AABox
#include "WmoLiquid.h"

//-----------------------------------------------------------------------------

// Macros used to simplify chunk reading and comparison for GroupModel_Raw
#define READ_OR_RETURN(V,S) \
    if(fread((V), (S), 1, rf) != 1) \
    { \
        fclose(rf); \
        std::cout << "readfail, op = " << readOperation << std::endl; \
        return(false); \
    }
#define CMP_OR_RETURN(V,S) \
    if(strcmp((V),(S)) != 0) \
    { \
        fclose(rf); \
        std::cout << "cmpfail, " << (V) << "!=" << (S) << std::endl; \
        return(false); \
    }

#define CHUNK_FAIL_CHECK(cond, msg) if (!(cond)) { std::cerr << "Failed to read chunk: " << msg << std::endl; return false; }

namespace VMAP {

    // Disk-level representation of a WMO group chunk
    struct GroupModel_Raw
    {
        unsigned int mogpflags = 0;
        unsigned int GroupWMOID = 0;
        unsigned int liquidflags = 0;
        AABox bounds;
        std::vector<MeshTriangle> triangles;
        std::vector<Vec3> vertexArray;
        class WmoLiquid* liquid;

        GroupModel_Raw() = default;
        ~GroupModel_Raw();

        // Reads a group from an already-open FILE*
        bool Read(FILE* f);
    };

    // Disk-level representation of a WMO file
    struct WorldModel_Raw
    {
        unsigned int RootWMOID = 0;
        std::vector<GroupModel_Raw> groupsArray;

        // Reads a WMO from file
        bool Read(const char* path, const char* RAW_VMAP_MAGIC);
    };

} // namespace VMAP

struct FileHandle {
    explicit FileHandle(FILE* f) : file(f) {}
    ~FileHandle() { if (file) fclose(file); }
    FILE* get() const { return file; }
    operator bool() const { return file != nullptr; }
private: FILE* file;
};

inline bool readChunk(FILE* rf, char* dest, const char* compare, unsigned int len)
{
    if (fread(dest, sizeof(char), len, rf) != len)
        return false;
    return memcmp(dest, compare, len) == 0;
}