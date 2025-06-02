#ifndef MANGOS_H_VMAPDEFINITIONS
#define MANGOS_H_VMAPDEFINITIONS

#define LIQUID_TILE_SIZE (533.333f / 128.f)

namespace VMAP
{
    const char VMAP_MAGIC[] = "VMAP_4.0";                       /**< used in final vmap files */
    const char GAMEOBJECT_MODELS[] = "temp_gameobject_models";  /**< TODO */
}

#include <assert.h>
#define MANGOS_ASSERT(x) assert(x)
#define DEBUG_LOG(...) 0
#define DETAIL_LOG(...) 0
#define LOG_FILTER_MAP_LOADING true
#define DEBUG_FILTER_LOG(F,...) do{ if (F) DEBUG_LOG(__VA_ARGS__); } while(0)
#define ERROR_LOG(...) do{ printf("ERROR:"); printf(__VA_ARGS__); printf("\n"); } while(0)

#endif // _VMAPDEFINITIONS_H