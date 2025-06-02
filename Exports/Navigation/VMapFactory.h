#ifndef VMAP_FACTORY_H
#define VMAP_FACTORY_H

#include "IVMapManager.h"

namespace VMAP
{
    // Create or return the current VMap manager
    IVMapManager* createOrGetVMapManager();

    // Reset/clear the current VMap manager
    void clearVMapManager();
}

#endif // VMAP_FACTORY_H
