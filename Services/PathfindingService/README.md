# PathfindingService

## Overview

The **PathfindingService** is a dedicated module responsible for computing navigation paths in the game world for the bot. It plans routes from a starting position (e.g. the player’s current location) to a target destination, navigating around terrain and obstacles. This service ensures the bot can move autonomously to desired points by providing a sequence of waypoints or coordinates that form a safe and efficient path. By encapsulating pathfinding logic into its own service, the bot’s movement and navigation features are easier to maintain, test, and extend without impacting other systems.

This service operates as a .NET worker component running alongside other bot services. It targets **.NET 8.0** and is built as a library integrated into the bot’s host process. In practice, PathfindingService works behind the scenes whenever the bot needs to travel, supplying the next steps the bot should take to reach its goal.

## Architecture

### Service Structure

PathfindingService is implemented as a background service using the .NET **Generic Host** framework. It utilizes the `Microsoft.Extensions.Hosting` worker SDK, meaning it can run continuously in a hosted environment and perform work on a background thread. The service does not have a standalone executable; instead, it is included in the overall bot application and started as part of the application’s startup. Being a hosted service allows path computations to occur asynchronously, without blocking the bot’s main logic.

Key architectural points:

* **Dependencies:** The PathfindingService project references the core game data and low-level OS interaction libraries of the bot. Notably, it depends on `GameData.Core` and `WinProcessImports`.

  * **GameData.Core** provides game world information and data structures (e.g. maps, coordinates, terrain or navigation data) that the pathfinding algorithm uses. This likely includes the map graph or navigation mesh representing walkable areas of the game world.
  * **WinProcessImports** offers low-level Windows process integrations – for example, reading the game client’s memory or invoking movement functions. PathfindingService uses this to get real-time game state (such as the player’s current position) and possibly to interface with movement controls.
* **Unsafe Operations:** The service allows unsafe code blocks. This indicates it performs some low-level memory or pointer manipulation, likely for performance-critical sections like parsing binary map data or interfacing with the game process at high speed. Using unsafe code suggests the service might be doing custom parsing of navigation mesh files or manipulating structures for the pathfinding algorithm efficiently.
* **Project Composition:** The service is organized under the `Services/PathfindingService` directory as its own project (with a `.csproj` file) within the BloogBot solution. It has its own **Properties** (including resources, if any) and implementation classes. The output is a class library (`OutputType` is *Library*), which is loaded by the main Bot application. The compiled binaries are placed into the bot’s output folder (configured as `..\..\Bot` base output path), so the Bot can use the service at runtime.

### Integration with the Bot

At runtime, PathfindingService likely registers itself with the bot’s dependency injection container or service manager. It may implement an interface (for example, `IHostedService` or a custom `IPathfindingService`) and is added to the host so that it starts on application launch. Once running, it waits for pathfinding tasks or monitors the bot’s movement needs.

Other components – for example, combat or quest logic – interact with PathfindingService by requesting paths. This could be done via direct method calls if an instance is injected, or via a messaging system. Given the presence of a **BotCommLayer** in the project (and use of reactive and protobuf libraries there), the architecture might use messages or events: e.g. a component publishes a “need path from A to B” request, which PathfindingService subscribes to and processes in the background, then returns the result. In simpler terms, a bot behavior could call a method like `PathfindingService.FindPath(startPosition, targetPosition)` and get back a list of points making up the route.

## Key Components

The PathfindingService module contains several important classes and data structures that work together to produce paths:

* **PathfindingService (Class):** The core class (likely named after the service) coordinating the pathfinding operations. This class probably inherits from `.NET`’s `BackgroundService` or implements `IHostedService`, continuously running a loop to handle path requests. It is responsible for initializing necessary data (loading maps or graphs), receiving pathfinding jobs, invoking the search algorithm, and returning the results. It may also manage threading (ensuring the heavy computation runs off the main thread).

* **Navigation Data / World Graph:** Under the hood, the service relies on a representation of the game world’s walkable topology. This is often a **graph** of nodes and connections or a **navigation mesh**. In BloogBot’s context, the navigation data comes from **precomputed “move maps”** of the game world. These move maps are essentially navigation meshes generated from the game client data. The PathfindingService loads these map files (placed in the `Bot/mmaps` directory) at startup or on demand, using them to know where the bot can walk. Each map corresponds to an area (e.g., a continent or dungeon) and contains a network of polygons or waypoints that represent traversable terrain.

  *Implementation detail:* The service likely parses the binary move map files into an internal structure (this is where unsafe code may be used for efficiency). It may create in-memory objects representing **nodes** (points in the world) and **edges** (connections between points or polygons). For a mesh, each polygon could be considered a node in the graph, with edges to adjacent polygons. For a waypoint network, each waypoint is a node with edges to nearby waypoints. The data structures might include classes or structs like `NavNode`, `NavMeshPolygon`, or a generic `Node` with properties like coordinates and a list of neighbors. These classes would be defined either in PathfindingService or in the GameData.Core library that PathfindingService uses.

* **Pathfinding Algorithm (A* Search):*\* The heart of the service is an implementation of the A\* pathfinding algorithm (or a variant of it). A\* is well-suited for finding shortest paths on graphs and is almost certainly used here to navigate the game world graph efficiently. Key components of the algorithm in the code would include:

  * An **open set** (typically a priority queue or sorted list) of nodes to evaluate, prioritized by estimated total cost (distance traveled so far plus heuristic).
  * A **closed set** (set of nodes already evaluated).
  * Cost tracking for each node: **g-cost** (distance from start to that node) and **h-cost** (heuristic estimate from that node to goal, likely the straight-line distance). The sum (f-cost) determines priority.
  * The algorithm iteratively picks the node with lowest f-cost from the open set, explores its neighbors, and continues until the target is reached or open set is exhausted.
  * A heuristic function: given the 3D nature of the game world, the heuristic is probably the Euclidean distance (or 2D distance on the ground plane) between a node and the destination. This guides the search toward the goal.
  * Backpointer or parent tracking to reconstruct the path once the goal is found.

  The service likely has a class or method implementing this search, possibly something like `Pathfinder` or `FindPathInternal`. It might be written to be static or as an instance helper within PathfindingService. Given that no external pathfinding library is referenced, this algorithm is custom-implemented in the code. The absence of heavy external dependencies suggests the authors wrote their own A\* rather than using an off-the-shelf library. This gives developers freedom to modify or optimize the algorithm if needed.

* **Position and Coordinates:** The service uses the bot’s `Position` structure (from `BloogBot.Game` or similar) to represent points in the world. Positions include X, Y, Z coordinates (floating-point) and have utility methods like distance calculation. The pathfinding uses these to measure distances (for cost calculations) and to output waypoints. A path returned by the service would typically be a list of `Position` objects, which the movement controller can then follow one by one.

* **Path Result / Waypoints:** After computing a route, the service packages the result – likely as an ordered list of coordinates from start to finish. There may be a small class or just a data structure (e.g. `List<Position>`) to hold this path. The service might also do some post-processing on the raw path: for example, smoothing the path (removing unnecessary zig-zags) or annotating it (e.g., indicating where to jump or other actions if needed). The current implementation likely focuses on just straight path points, as any extra behaviors would be handled by the movement controller elsewhere.

* **Integration Hooks:** If the service runs continuously, it may expose methods or use events for other systems to get paths. For instance, the PathfindingService might subscribe to an event like “OnDestinationChanged” or have a method `RequestPath(TargetPosition)` that other components call. In a more decoupled design, it might pull requests from a thread-safe queue. The **BotCommLayer** being referenced in GameData suggests there could be a messaging system; however, PathfindingService itself does not directly reference the comm layer in its project, meaning the communication might be indirect or not yet implemented. In any case, developers interact with the service through its public interface (method calls or message triggers) rather than by directly manipulating its internal data.

## Algorithms and Techniques

**A**\* (A-star) **Pathfinding:** The primary algorithm used by PathfindingService is A\* search on the navigation mesh/graph. This algorithm is well-known for pathfinding in games due to its efficiency in finding shortest paths with the help of heuristics. In the context of PathfindingService, A\* likely works as follows:

1. **Graph Preparation:** Using the move map data, the service knows all **reachable nodes** in the area. A node might represent a small region of terrain or a waypoint in the game world. Each node has connections to others (neighbors) that are directly reachable without collision (for example, adjacent polygons on a mesh or waypoints within line-of-sight and slope limits).

2. **Heuristic:** The heuristic is probably the straight-line distance between a given node’s position and the target position. This is admissible (never overestimates the true path cost) since one cannot do better than flying straight to the goal. Because movement is constrained to the ground, the actual distance traveled will be equal or longer than the straight-line distance, so this heuristic guides the search optimistically without compromising correctness.

3. **Search Process:** Starting from the node nearest the start position, the algorithm pushes it onto the open set. Then it iteratively:

   * Takes the best candidate node (lowest f-cost = g + h) from the open set.
   * If this node is the goal or is close enough to the destination, the search ends successfully.
   * Otherwise, it moves the node to the closed set and examines each of its neighbor nodes:

     * It calculates a tentative g-cost for the neighbor (current node’s g + distance from current node to neighbor).
     * If this new path to neighbor is shorter than any previous known path, it updates the neighbor’s g-cost (and computes its f = g + h) and records the current node as the neighbor’s parent (for path reconstruction).
     * It then adds the neighbor to the open set (or updates its priority if it’s already in the open set with a higher cost).
   * This continues until the goal is reached or no more nodes remain to explore (which would mean the target is unreachable).

4. **Path Reconstruction:** When the target node is reached, the service reconstructs the path by following parent links from the target node back to the start. This yields the sequence of nodes (which correspond to positions in the world) from start to finish, but in reverse order, which is then reversed to give the forward path.

5. **Result output:** The final path of coordinates is returned or made available. The bot’s movement controller will then move the character along this list of waypoints in order.

**Handling Dynamic Conditions:** The current implementation is primarily focused on static pathfinding – the world geometry is static (buildings, terrain). If the bot encounters dynamic obstacles (like creatures or players in the way), the PathfindingService itself doesn’t directly account for those (that would be handled by higher-level logic like steering or combat routines). However, the pathfinding might incorporate basic logic like *re-pathing* if the bot deviates or gets stuck. For example, if after following part of a path the bot is off course or the next waypoint is not reachable for some reason, the bot can request the PathfindingService to compute a new path from its new current position.

**Performance Considerations:** Pathfinding can be CPU-intensive, especially if the search space is large (many nodes). By running as a background service, PathfindingService can take advantage of multi-threading – computing paths on a separate thread so the main bot loop (which handles combat, AI, etc.) remains responsive. The use of unsafe code and direct data parsing is likely an optimization to speed up loading and querying the navmesh data. Additionally, the algorithm might incorporate optimizations like:

* **Hierarchical pathfinding:** Possibly using a high-level graph of regions first, then refining into detailed path – though there’s no explicit evidence of this, it’s a common extension if needed.
* **Caching:** If the bot frequently travels along the same routes, the service could cache computed paths or partial paths. There’s no explicit mention of caching in the code we reviewed, but developers could extend the service to add caching for performance.

## Extensibility and Usage

One of the advantages of isolating pathfinding in this service is that developers can extend or modify it independently of other systems. Here are ways to interact with or extend PathfindingService:

* **Using the Service (API):** Developers can call the pathfinding service to get paths. For example, if writing a new bot behavior that needs the character to move to a certain location, you would obtain a path by calling something like:

  ```csharp
  List<Position> path = pathfindingService.FindPath(startPos, destinationPos);
  foreach (Position waypoint in path) {
      MoveTo(waypoint);
  }
  ```

  In practice, the actual method name and usage might differ (it could return a custom Path object or use an async pattern), but the concept is the same. The service handles the heavy lifting and returns waypoints that your code can iterate through to command the character to move.

* **Configuration:** If the navigation data (move maps) is not present or you have custom maps (say for a custom game area or a different game client version), you can generate or plug in new data. The service is designed to load navigation data from the `Bot/mmaps` directory at runtime (as long as the files follow the expected format). To extend this, you can:

  * Add new move map files for additional game zones or expansions. The pathfinding logic will automatically take advantage of them when navigating those areas.
  * Update or replace move map data if you find issues (for instance, if the bot gets stuck due to missing obstacles in the data, you could regenerate that area’s mesh with corrections).

* **Custom Heuristics or Algorithms:** Advanced developers can tweak the pathfinding algorithm itself. Since the code is self-contained, you could implement alternative heuristics (for example, if you wanted the bot to favor safer paths over shortest paths, you might increase the cost for certain regions). You could also integrate **Jump Point Search** or other optimizations for grid-based maps – however, given the irregular nature of MMO terrain, A\* on a navmesh is already a suitable choice. The service’s code can be modified to adjust these behaviors without affecting other services.

* **Extending Pathfinding Features:** You might want to extend the service to handle multi-modal navigation. For example:

  * **Using Transportation:** If the bot should use game mechanisms like flight paths or boats, you could integrate that into PathfindingService. This might involve adding special nodes that represent a flight master or boat departure, and edges that correspond to those travel routes. The algorithm could then choose a path that includes a flight if it’s significantly faster.
  * **Dynamic Obstacles:** Although the default implementation treats the world as static, one could extend it to react to dynamic changes. For example, if certain areas become dangerous (temporary hazards or crowd of enemies), the service could be extended to mark those nodes as temporarily blocked or high-cost, forcing the pathfinder to route around them.
  * **Stuck Detection and Re-pathing:** The bot has a separate StuckHelper (as seen in the project) for detecting when it’s not making progress. In the future, PathfindingService could be extended to automatically re-compute a path if progress stalls, rather than waiting for the high-level stuck handler. Developers can add hooks such that if the bot deviates too much from the path or hasn’t reached the next waypoint in a reasonable time, a new pathfinding request is triggered.

* **Interfacing with Other Systems:** If building new features, such as a UI to visualize the path or a debug tool, one can tap into PathfindingService. For instance, you could log or draw the computed path for analysis. Since the service likely uses standard data structures (list of Position points), it’s straightforward to take that data and, say, print it to a console or plot it on a map overlay for debugging.

In summary, the PathfindingService is designed to be a clear separation of concerns for navigation. Developers can work on improving pathfinding accuracy or efficiency within this service without risking side effects in combat or other bot logic. Likewise, if an alternative pathfinding system is desired, one could replace or subclass this service. For example, if moving the bot to a different game or a different context, you could swap out the navigation data and perhaps the movement rules (like adding jump/climb capabilities) by extending this module.

## Conclusion

**PathfindingService** provides the BloogBot with the crucial ability to navigate complex environments autonomously. Its implementation centers on loading precomputed world navigation data and using a classic A\* search algorithm to find optimal paths. Key components like the navigation mesh, node graph, and search algorithm are encapsulated within the service, making it a self-contained unit of functionality.

From a developer’s perspective, the service’s structure (a .NET 8 worker with clear inputs/outputs) makes it both understandable and extensible. Whether you need to troubleshoot why the bot is taking a strange route, add support for new maps, or tweak the pathfinding behavior, the PathfindingService is the place to look. By focusing solely on pathfinding responsibilities, it allows the rest of the bot to query for paths without needing to know the complex details of how those paths are computed. This clean separation and the use of robust algorithms and data structures ensure the bot can move intelligently through its world, and that developers can continually improve this intelligence in an isolated, maintainable way.
