// Fixed RegularGrid2D.h
// Professor's note: see comments for all changes.

#ifndef MANGOS_H_REGULAR_GRID
#define MANGOS_H_REGULAR_GRID

#include <vector>
#include <unordered_map>
#include "Vec3Ray.h"

/**
 * @brief Default HashTrait for use in grid or spatial structures.
 *        By default, uses std::hash<T> if possible.
 */
template <typename T>
struct HashTrait {
    static size_t hashCode(const T& value) {
        return std::hash<T>()(value);
    }
};

// ---- PositionTrait ----
template<typename T>
struct PositionTrait {
    static void getPosition(const T& obj, Vec3& pos) {
        pos = obj.GetPosition();
    }
};

// ---- NodeCreator ----
template<class Node>
struct NodeCreator {
    static Node* makeNode(int /*x*/, int /*y*/) { return new Node(); }
};

// ---- RegularGrid2D ----
template < class T,
    class Node,
    class NodeCreatorFunc = NodeCreator<Node>,
    class PositionFunc = PositionTrait<T>
>
class RegularGrid2D {
public:
    enum { CELL_NUMBER = 64 };

#define HGRID_MAP_SIZE  (533.33333f * 64.f)     // shouldn't be changed
#define CELL_SIZE       float(HGRID_MAP_SIZE/(float)CELL_NUMBER)

    // ---
    typedef std::unordered_map<const T*, Node*> MemberTable;

    MemberTable memberTable; // Now an unordered_map for pointer-to-node lookup
    Node* nodes[CELL_NUMBER][CELL_NUMBER];

    RegularGrid2D() { memset(nodes, 0, sizeof(nodes)); }
    ~RegularGrid2D() {
        for (int x = 0; x < CELL_NUMBER; ++x)
            for (int y = 0; y < CELL_NUMBER; ++y)
                delete nodes[x][y];
    }

    void insert(const T& value) {
        Vec3 pos;
        PositionFunc::getPosition(value, pos);
        Node& node = getGridFor(pos.x, pos.y);
        node.insert(value);
        memberTable[&value] = &node; // use map, not set()
    }
    void remove(const T& value) {
        auto it = memberTable.find(&value);
        if (it != memberTable.end()) {
            it->second->remove(value);
            memberTable.erase(it);
        }
    }

    void balance() {
        for (int x = 0; x < CELL_NUMBER; ++x)
            for (int y = 0; y < CELL_NUMBER; ++y)
                if (Node* n = nodes[x][y]) n->balance();
    }
    bool contains(const T& value) const {
        return memberTable.count(&value) > 0;
    }
    int size() const { return int(memberTable.size()); }

    struct Cell {
        int x, y;
        bool operator == (const Cell& c2) const { return x == c2.x && y == c2.y; }
        static Cell ComputeCell(float fx, float fy) {
            Cell c = { static_cast<int>(fx * (1.f / CELL_SIZE) + (CELL_NUMBER / 2)),
                       static_cast<int>(fy * (1.f / CELL_SIZE) + (CELL_NUMBER / 2)) };
            return c;
        }
        bool isValid() const { return x >= 0 && x < CELL_NUMBER && y >= 0 && y < CELL_NUMBER; }
    };

    Node& getGridFor(float fx, float fy) {
        Cell c = Cell::ComputeCell(fx, fy);
        return getGrid(c.x, c.y);
    }
    Node& getGrid(int x, int y) {
        assert(x >= 0 && x < CELL_NUMBER && y >= 0 && y < CELL_NUMBER);
        if (!nodes[x][y]) nodes[x][y] = NodeCreatorFunc::makeNode(x, y);
        return *nodes[x][y];
    }

    template<typename RayCallback>
    void intersectRay(Ray const& ray, RayCallback& intersectCallback, float max_dist, bool ignoreM2Model)
    {
        intersectRay(ray, intersectCallback, max_dist, ray.origin() + ray.direction() * max_dist, ignoreM2Model);
    }

    template<typename RayCallback>
    void intersectRay(Ray const& ray, RayCallback& intersectCallback, float& max_dist, Vec3 const& end, bool ignoreM2Model)
    {
        Cell cell = Cell::ComputeCell(ray.origin().x, ray.origin().y);
        if (!cell.isValid())
            return;

        Cell last_cell = Cell::ComputeCell(end.x, end.y);

        if (cell == last_cell)
        {
            if (Node* node = nodes[cell.x][cell.y])
                node->intersectRay(ray, intersectCallback, max_dist, ignoreM2Model);
            return;
        }

        float voxel = (float)CELL_SIZE;
        float kx_inv = ray.invDirection().x, bx = ray.origin().x;
        float ky_inv = ray.invDirection().y, by = ray.origin().y;

        int stepX, stepY;
        float tMaxX, tMaxY;
        if (kx_inv >= 0)
        {
            stepX = 1;
            float x_border = (cell.x + 1) * voxel;
            tMaxX = (x_border - bx) * kx_inv;
        }
        else
        {
            stepX = -1;
            float x_border = (cell.x - 1) * voxel;
            tMaxX = (x_border - bx) * kx_inv;
        }

        if (ky_inv >= 0)
        {
            stepY = 1;
            float y_border = (cell.y + 1) * voxel;
            tMaxY = (y_border - by) * ky_inv;
        }
        else
        {
            stepY = -1;
            float y_border = (cell.y - 1) * voxel;
            tMaxY = (y_border - by) * ky_inv;
        }

        //int Cycles = std::max((int)ceilf(max_dist/tMaxX),(int)ceilf(max_dist/tMaxY));
        //int i = 0;

        float tDeltaX = voxel * fabs(kx_inv);
        float tDeltaY = voxel * fabs(ky_inv);
        do
        {
            if (Node* node = nodes[cell.x][cell.y])
            {
                //float enterdist = max_dist;
                node->intersectRay(ray, intersectCallback, max_dist, ignoreM2Model);
            }
            if (cell == last_cell)
                break;
            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                cell.x += stepX;
            }
            else
            {
                tMaxY += tDeltaY;
                cell.y += stepY;
            }
            //++i;
        } while (cell.isValid());
    }

    template<typename IsectCallback>
    void intersectPoint(const Vec3& point, IsectCallback& intersectCallback) {
        Cell cell = Cell::ComputeCell(point.x, point.y);
        if (!cell.isValid()) return;
        if (Node* node = nodes[cell.x][cell.y])
            node->intersectPoint(point, intersectCallback);
    }
    template<typename RayCallback>
    void intersectZAllignedRay(Ray const& ray, RayCallback& intersectCallback, float& max_dist)
    {
        Cell cell = Cell::ComputeCell(ray.origin().x, ray.origin().y);
        if (!cell.isValid())
            return;
        if (Node* node = nodes[cell.x][cell.y])
            node->intersectRay(ray, intersectCallback, max_dist, false);
    }
};

#undef CELL_SIZE
#undef HGRID_MAP_SIZE

#endif
