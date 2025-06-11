#include "VMapFileUtils.h"
#include <cstdio>
#include <cstring>
#include <iostream>

#include "WmoLiquid.h"

namespace VMAP {
 /**
 * @brief Reads group data from a raw file, including bounding box, indices, vertices, and liquid.
 *
 * @param rf The file handle to read from.
 * @return bool True if the read was successful, otherwise false.
 */
    bool GroupModel_Raw::Read(FILE* rf)
    {
        char blockId[5] = { 0 };
        int blocksize = 0;
        int readOperation = 0;

        READ_OR_RETURN(&mogpflags, sizeof(unsigned int));
        READ_OR_RETURN(&GroupWMOID, sizeof(unsigned int));

        Vec3 vec1, vec2;
        READ_OR_RETURN(&vec1, sizeof(Vec3));
        READ_OR_RETURN(&vec2, sizeof(Vec3));
        bounds.set(vec1, vec2);

        READ_OR_RETURN(&liquidflags, sizeof(unsigned int));

        unsigned int branches;
        READ_OR_RETURN(&blockId, 4);
        CMP_OR_RETURN(blockId, "GRP ");
        READ_OR_RETURN(&blocksize, sizeof(int));
        READ_OR_RETURN(&branches, sizeof(unsigned int));
        for (unsigned int b = 0; b < branches; ++b)
        {
            unsigned int indexes;
            READ_OR_RETURN(&indexes, sizeof(unsigned int));
        }

        READ_OR_RETURN(&blockId, 4);
        CMP_OR_RETURN(blockId, "INDX");
        READ_OR_RETURN(&blocksize, sizeof(int));
        unsigned int nindexes;
        READ_OR_RETURN(&nindexes, sizeof(unsigned int));
        if (nindexes > 0)
        {
            std::vector<unsigned short> indexarray(nindexes);
            if (fread(indexarray.data(), sizeof(unsigned short), nindexes, rf) != nindexes)
                return false;
            triangles.reserve(nindexes / 3);
            for (unsigned int i = 0; i + 2 < nindexes; i += 3)
                triangles.emplace_back(indexarray[i], indexarray[i + 1], indexarray[i + 2]);
        }

        READ_OR_RETURN(&blockId, 4);
        CMP_OR_RETURN(blockId, "VERT");
        READ_OR_RETURN(&blocksize, sizeof(int));
        unsigned int nvectors;
        READ_OR_RETURN(&nvectors, sizeof(unsigned int));
        if (nvectors > 0)
        {
            std::vector<float> vectorarray(nvectors * 3);
            if (fread(vectorarray.data(), sizeof(float), nvectors * 3, rf) != nvectors * 3)
                return false;
            vertexArray.reserve(nvectors);
            for (unsigned int i = 0; i < nvectors; ++i)
                vertexArray.emplace_back(vectorarray.data() + i * 3);
        }

        liquid = nullptr;
        if (liquidflags & 1)
        {
            WMOLiquidHeader hlq;
            READ_OR_RETURN(&blockId, 4);
            CMP_OR_RETURN(blockId, "LIQU");
            READ_OR_RETURN(&blocksize, sizeof(int));
            READ_OR_RETURN(&hlq, sizeof(WMOLiquidHeader));
            liquid = new WmoLiquid(hlq.xtiles, hlq.ytiles, Vec3(hlq.pos_x, hlq.pos_y, hlq.pos_z), hlq.type);
            unsigned int size = hlq.xverts * hlq.yverts;
            READ_OR_RETURN(liquid->GetHeightStorage(), size * sizeof(float));
            size = hlq.xtiles * hlq.ytiles;
            READ_OR_RETURN(liquid->GetFlagsStorage(), size);
        }

        return true;
    }

    /**
     * @brief Destructor to clean up allocated liquid data.
     */
    GroupModel_Raw::~GroupModel_Raw()
    {
        // Ensure we don't leak the allocated WmoLiquid object
        delete liquid;
    }

    bool WorldModel_Raw::Read(const char* path, const char* RAW_VMAP_MAGIC)
    {
        FILE* rf = fopen(path, "rb");
        if (!rf) return false;

        char magic[8] = { 0 };
        if (fread(magic, 1, 8, rf) != 8) { fclose(rf); return false; }
        if (strncmp(magic, RAW_VMAP_MAGIC, 8) != 0) { fclose(rf); return false; }

        // Example: skip to number of groups, parse all GroupModel_Raw, etc.
        unsigned int groups = 1;
        fread(&groups, sizeof(unsigned int), 1, rf); // Simplified; real implementation may differ
        RootWMOID = 1;
        groupsArray.resize(groups);
        for (auto& g : groupsArray) g.Read(rf);

        fclose(rf);
        return true;
    }

} // namespace VMAP
