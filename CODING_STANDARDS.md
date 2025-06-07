# BloogBot C# Coding Standards and Architectural Guidelines

These guidelines outline the coding standards and architectural practices for the **BloogBot** project. They are derived from the current structure and patterns in the repository, with improvements added based on widely accepted C# and .NET best practices. The goal is to ensure a clean, maintainable, and scalable codebase.

## Project Architecture and Structure

**Solution Layout:** The BloogBot codebase is organized as a Visual Studio solution (`BloogBot.sln`) comprising multiple projects. This multi-project structure promotes separation of concerns and modularity. The main components include:

* **Core Bot Application (BloogBot):** The primary project (a WPF application) contains the bot engine and UI. It implements the main logic for interacting with the World of Warcraft client (e.g. memory reading/writing, event hooks) and presents a GUI (using MVVM with `MainViewModel`, etc.). UI elements (Views) and their ViewModels are separated to maintain a clear MVVM structure, keeping UI code-behind minimal and logic in ViewModel classes for clarity and testability.

* **AI/State Machine Plugins:** Bot “behavior” (class-specific or scenario-specific logic) is decoupled into separate class library projects (e.g. **ShadowPriestBot**, **WarriorBot**, or other bot profiles). These plug-in assemblies contain the state machine and decision-making code for different character classes or behaviors. They implement a common interface (e.g. `IBot`) defined in the core, allowing the core app to treat all bots uniformly. This plugin architecture is implemented via the Managed Extensibility Framework (MEF) for runtime discovery and dynamic reloading of bot logic. *Guideline:* Each distinct bot or AI module should reside in its own project or plugin class, implementing the core interface – this keeps the core engine generic and open to extension (Open/Closed Principle).

* **Bootstrap/Injection Loader:** (If applicable) A small injector/loader utility ensures the .NET runtime is loaded into the WoW process and launches the BloogBot core. This may be an unmanaged component or a minimal console app that uses Windows API calls to inject the core bot DLL into the game process (as described in the project documentation). *Guideline:* Keep this loader lightweight and isolated from core logic – its responsibility is just process injection and bootstrapping, adhering to single-responsibility.

* **Supporting Files and Data:** The repository contains supporting assets like SQL schema scripts (e.g. `SqlSchema.SQL`) indicating the use of a local database for storing data (perhaps for logging or caching game object info). Ensure that any data access (database or file I/O) is abstracted away from business logic (for example, through a repository pattern or service in the core project). Organize such functionality into a distinct folder or service class (e.g. a `Data` or `Persistence` service) to separate it from UI and bot behavior code.

**Folder Organization:** Within each project, files are grouped by feature or layer for clarity. For example, in the core project, UI-related code (Windows, Views, ViewModels) should live under a UI or Views folder, game interaction logic under a **Game** or **Engine** folder, and utility or helper classes in a **Utilities** folder. In the plugin projects, state machine classes and related logic can be grouped similarly (e.g. all combat logic in one file or namespace). This folder structure makes it easy to locate code and reinforces the separation of concerns. In the original BloogBot, for instance, there was an **AI** folder which was later extracted into its own project – continuing this practice, keep each folder/project focused on one aspect of the system (core engine vs. AI logic vs. data, etc.).

**Layered Design:** While BloogBot is a client-side desktop application (not a typical n-tier web app), it can still benefit from layering principles. Separate the concerns of **Presentation**, **Domain Logic**, and **Infrastructure**:

* Presentation (WPF UI) should handle user interaction and display.
* Domain logic (the bot’s decision-making and game interaction) should be in core classes or services, decoupled from the UI (e.g. the UI calls into an interface of the bot engine).
* Infrastructure concerns like file access, database operations, or inter-process communication should be abstracted behind interfaces or service classes so that the bot logic isn’t tightly coupled to implementation details.

By following a layered approach, the codebase becomes easier to maintain and test. Even in a single-project scenario, use clear **folder separation** to mimic layers. As the project grows, consider splitting layers into separate projects (e.g. a class library for data access) if needed for clarity.

**Guidelines:**

* **One Class per File:** Continue the convention of having each class in its own `.cs` file, named after the class. This matches C# best practices and improves findability.

* **Solution Structure:** New features should fit into the existing project structure or prompt creation of new projects if they don’t logically belong to any existing one. For example, if adding a logging subsystem that’s used by multiple bots, consider a separate utility project or at least a distinct namespace/folder for it.

* **Dependencies Between Projects:** Minimize tight coupling between projects. The core should define contracts (interfaces) for bot behaviors; plugin projects implement those interfaces. The core can load plugins via MEF or an IoC container without needing compile-time references to each plugin (plugins export themselves via MEF \[Export] attributes, and the core imports them). This inversion of control allows adding or updating bots without modifying core code, adhering to open/closed principle.

* **Consistency:** Follow a consistent architecture across the solution. If using MVVM in one part of the UI, use MVVM for all new UI components (with proper use of INotifyPropertyChanged, commands, etc.). If plugins are loaded via MEF, use MEF conventions for any new extension points (appropriate \[Export]/\[Import] attributes, etc.) to maintain uniformity.

## Naming Conventions

Consistent naming is critical for readability and maintainability. BloogBot’s code should adhere to standard .NET naming conventions:

* **PascalCase for Types and Methods:** Use PascalCasing for class names, structs, enum types, properties, method names, and constants. For example, `CombatRoutine`, `GameManager`, and `CalculateDamage()` are appropriately PascalCased. Names should be **descriptive** and avoid abbreviations (e.g. use `InitializeBot()` instead of `InitBt()`). This makes the purpose clear at a glance.

* **CamelCase for Variables and Parameters:** Local variables and method parameter names should use camelCasing (start with lower-case). For instance, `playerHealth` or `targetId` as a variable. This rule also applies to private fields *if* you choose not to use underscore prefix (more on that below). Temporary loop indices can be short (i, j), but in general avoid single-letter names except in very short scopes.

* **Interface Names:** Prefix interface names with **I**. For example, `IBot`, `ICombatStrategy`, `ILogger`. This clearly distinguishes interfaces from classes. Correspondingly, **implementing classes** should have descriptive names without the `I`. Prefer naming the class after its role rather than just prefixing with a concrete word – e.g., class `FileLogger` implements `ILogger`, `WarriorBot` implements `IBot`. (The Ed-Fi convention suggests using the interface name minus the “I” as part of the class name, which is a good practice for clarity.)

* **Private Fields:** Use an underscore `_` prefix for private fields. For example, `_currentTarget` or `_isRunning`. This is a common convention in C# to visually distinguish class fields from local variables. (Public or protected fields should be avoided in favor of properties, but if they exist, they should follow PascalCase with no underscore.) Note that in some legacy code you might encounter different styles – if a file consistently uses a different convention, prefer to maintain consistency within that file when making minor changes, but new code should follow the standard.

* **Naming Clarity:** Names should be self-explanatory. Avoid cryptic abbreviations. For instance, use `positionX` instead of `posX`. Follow English reading order and naming guidelines from the .NET Framework Design Guidelines (e.g. **no Hungarian notation or prefixes denoting types** like `szName` or `tblData` – these are disallowed).

* **Namespaces:** Use a clear namespace hierarchy that mirrors the folder structure. The root namespace should likely be `BloogBot` (or `QiMata.BloogBot` if following a company naming scheme). Sub-namespaces can reflect sub-modules, e.g. `BloogBot.Plugins.ShadowPriest` or `BloogBot.Core.GameInterop`. All classes should belong to an appropriately named namespace; avoid the default global namespace. Namespace names use PascalCase and typically correspond to project names or functionality areas.

* **File Names:** Match file name to the primary class or interface inside. For example, `MainViewModel.cs` contains the `MainViewModel` class. This is already practiced in the repository and should be continued for consistency.

* **Member Naming:** Methods should be named with **verbs or verb phrases** (since they perform actions), e.g. `StartBot()`, `LoadProfile()`. Properties represent data, so name them as nouns or adjectives (e.g. `IsBotRunning`, `PlayerHealth`). Event handlers can use the `OnSomething` pattern if appropriate (e.g. `OnCombatStarted`). Constants and readonly static fields should follow PascalCase as they are treated like variables/fields (e.g. `MaxRetryCount`). Enum members also use PascalCase (e.g. `LogLevel.Error`, `LogLevel.Warning`).

* **Consistency:** Ensure naming is consistent across the project. For example, if one part of the code uses “NPC” to mean non-player character, avoid using “Monster” elsewhere for the same concept. Consistent terminology reduces confusion. Use comments to clarify any domain-specific terms if needed.

By following these naming conventions, the code becomes more uniform and easier to navigate. Automated tooling can help enforce many of these rules (see **Linting** below) – for instance, an `.editorconfig` can flag if a private field doesn’t start with `_` or if a public member isn’t capitalized.

## Dependency Management and Inversion of Control (IoC)

BloogBot’s architecture should embrace Inversion of Control to reduce tight coupling between components. The project already demonstrates IoC principles through its plugin system, and we can expand on these practices:

* **Program to Interfaces:** Key components should be referenced via interfaces or abstractions rather than concrete classes. For example, the core uses an `IBot` interface to refer to the loaded bot implementation. This decouples the core logic from any specific bot behavior. Continue this pattern: define interfaces for services like `ILoginManager`, `IWowClient`, etc., so that implementations can vary (which aids testing and future modifications). Depending on abstractions ensures the Dependency Inversion Principle (the “D” in SOLID) is followed.

* **Managed Extensibility Framework (MEF):** BloogBot uses MEF to import/export implementations at runtime, which is a form of IoC (the framework injects the `IBot` implementation dynamically). **Guideline:** Continue to use MEF for plugging in new bot modules or features. Mark new plugins with `[Export(interfaceType)]` and import them in the core via `[ImportMany]` or `[Import]` as needed. MEF helps to **decouple** the creation of objects from their usage – the core doesn’t `new` up a `Bot` directly, it asks MEF to provide one, aligning with IoC best practices.

* **Avoid Static/Global State:** *Anti-pattern to fix:* Avoid using static classes or singletons as a crutch for sharing state or accessing dependencies. Static global state makes testing and maintenance harder. Instead, use explicit dependency injection. For example, if multiple components need to log messages, pass an `ILogger` interface to those components (via constructor injection or a service locator) rather than using a global static logger instance. The .NET dependency injection guidelines advise designing services to avoid global state and to inject dependencies instead. If a certain object truly needs to be a single instance (e.g. a configuration or game memory reader), treat it as a singleton in the IoC container or MEF, but still access it via an interface.

* **Dependency Injection Container:** In addition to MEF (which is mainly for extensibility), consider using a DI container for general dependency management if the project grows. In a .NET Core application, one might use the built-in DI (Microsoft.Extensions.DependencyInjection) to register and resolve services. In .NET Framework WPF, you can integrate libraries like Unity, Autofac, or SimpleInjector. A DI container can manage object lifetimes (transient vs singleton services) and simplify the wiring of complex object graphs. *Example:* You could register the WoW memory-access service, data repository, etc., in a container and have them auto-injected into viewmodels or controllers. This reduces manual instantiation in code (which currently might be done directly in factories or the ViewModel). Reducing **new** calls in code (especially inside business logic methods) is a sign of good IoC practice – it means your code is not tightly coupling itself to concrete classes.

* **Constructors and Injection:** If using DI or even without a formal container, follow the practice of **constructor injection** for required dependencies. For example, if `MainViewModel` needs an `IBot` and an `ILogger`, provide a constructor that accepts those interfaces. This makes dependencies explicit and makes unit testing easier (you can pass in a mock implementation). Keep constructors relatively simple – just assigning injected fields – and do not do heavy work in constructors.

* **Service Locator (Use Cautiously):** If not using a full DI container, another pattern is to have a central service registry or locator that can provide instances of needed services. This can be acceptable (especially in WPF apps via a simple static `ServiceProvider`) but use it sparingly – overuse can become a hidden dependency mechanism which is harder to follow. Prefer explicit injection over pulling from a locator in the middle of business code, as the latter can obscure what dependencies a class really needs (breaking SRP and making testing harder).

* **Loose Coupling:** Each class or module should have limited knowledge of other parts of the system. For instance, the bot plugins should only rely on the contracts provided by the core (like `IBot` and perhaps a few services or event hooks). The core should not need to know details of each plugin. Using IoC (MEF or DI) to invert control means the core calls an abstraction, and the concrete implementation is injected at runtime. This **loose coupling** enables adding new behaviors without changing core code (extensibility) and makes it possible to replace components (for example, swap out one pathfinding service for another) with minimal changes.

* **Small, Focused Services:** Following IoC best practices often leads to creating many small services or classes, each with a single responsibility (for example, a `TargetingService` just handles target selection logic). This is good – classes should remain *small, well-factored, and easily tested*. If you find a class has too many injected dependencies or too many responsibilities, consider refactoring into multiple classes. (A class requiring too many different services could be violating the Single Responsibility Principle, indicating it should be split.) Each service can then be independently tested and maintained.

* **Package Management:** Use NuGet for external dependencies and keep them updated. The repository’s `.csproj` files track NuGet references. Do not add binary DLLs directly to source control. Instead, reference via NuGet so that CI/build can restore them. If BloogBot uses packages (e.g. for JSON, or a memory reading library), ensure the package versions are compatible and security-patched. Use `PackageReference` in the project files (as is standard in SDK-style projects) for clean dependency management.

In summary, **Inversion of Control** (through interfaces, DI, and MEF) should be a guiding principle: *high-level modules (like the core bot logic) should not depend on low-level implementations; both should depend on abstractions*. This leads to a more flexible design that can evolve over time.

## Clean, Maintainable Code Practices

Writing clean code is about readability, simplicity, and reducing complexity. The BloogBot team should follow these practices to ensure the codebase remains healthy:

* **SOLID Principles:** Strive to follow the SOLID principles in design –

  * *Single Responsibility:* Each class or method should have one responsibility or reason to change.
  * *Open/Closed:* Code should be open for extension but closed for modification (achieved by abstracting and extending rather than editing core logic).
  * *Liskov Substitution:* Derived classes or implementations should be usable wherever the base is expected (ensure your interfaces and inheritance hierarchies are correct and don’t violate expectations).
  * *Interface Segregation:* Define focused, small interfaces that clients need, rather than one large “god” interface. In BloogBot, for instance, instead of one huge `IBot` interface with dozens of methods for every action, you might have smaller interfaces (if needed) or at least ensure `IBot` stays concise.
  * *Dependency Inversion:* As discussed, depend on abstractions, not concretions – a summary of IoC above.

  These principles help create a robust, scalable, and maintainable software architecture. Following them yields benefits such as reduced coupling, improved readability and extensibility, lower complexity, and better testability.

* **Small Functions & Clear Logic:** Keep methods and functions short – ideally 5-20 lines. Each method should do one thing and do it well. If you find yourself writing a 100-line method that “does everything,” consider refactoring it into smaller private methods or collaborating classes. This improves readability and makes unit testing specific behaviors easier. Long methods can often be broken down by extracting logic (e.g., a method `NavigateToTarget()` might internally call smaller methods `CalculatePath()` and `MoveAlongPath()` instead of doing both tasks inline).

* **Meaningful Naming and Self-Documentation:** We covered naming conventions above – leveraging those makes the code self-explanatory. If the code’s intent is not obvious from the implementation, consider adding a **comment** or renaming variables for clarity. For example, a complex algorithm might warrant a brief comment, or a particularly tricky workaround should be commented (including reasoning). However, avoid redundant comments – prefer self-documenting code. For instance, instead of a comment *“// decrease health by damage”* on a line `health -= damage;`, you can make the code clearer by naming things clearly (like calling the variable `currentHealth`). Use **XML documentation comments** (`/// <summary>...`) for public classes/members that are part of an API, so that intellisense can show usage hints.

* **Avoid Magic Numbers/Strings:** Do not hardcode literal values scattered in code. Use constants or readonly static fields for any significant “magic” values with a meaningful name (e.g. `const int MaxScanDistance = 1000;`). This makes code more adaptable and readable. Similarly, if certain strings are used (e.g. keys for settings), define them as constants or put them in a resource file.

* **Error Handling and Exceptions:** Use exceptions judiciously for error conditions. For expected scenarios (like a not-found result), prefer returning a result or null rather than using exceptions for flow control. Always clean up resources in error cases – utilize `try/finally` or `using` blocks for disposables. For instance, ensure any file or memory handles are closed even if an error occurs (e.g., if BloogBot opens process handles, make sure to close them). Catch exceptions at boundaries (like top-level loops or event handlers) to log errors and prevent the entire application from crashing ungracefully. Provide meaningful messages or logs when exceptions are caught, to aid in debugging.

* **Logging:** Maintain a consistent logging strategy. The bot likely has logging for debug or user info (whether to console, file, or on-screen). Use a logging interface (as mentioned, e.g. `ILogger`) and log important events, errors, and warnings. This not only helps developers troubleshoot but can serve as a form of documentation of what the program is doing at runtime. Follow a consistent format for log messages. Avoid overly verbose logging in tight loops (to prevent performance issues), but ensure errors and key state changes are logged.

* **Eliminate Dead Code:** Remove or refactor out any unused code. If there are sections of code that are commented out or obsolete classes (perhaps remnants from older versions), clean them up. Dead code confuses readers and increases maintenance burden. Use source control history as an archive rather than keeping commented blocks.

* **Code Formatting:** Adhere to a consistent formatting style throughout (see Linting section for automation). Key points include using 4 spaces for indentation (no tabs) and aligning code blocks for readability. Always put braces `{}` even for single-line `if/else` or loops – this prevents errors when adding new lines later and conforms to the team’s style (the **Allman** style: braces on a new line, aligned vertically). Keep line lengths reasonable (around 100-120 characters is a common internal standard; the Microsoft docs suggest 65 for samples on websites, but in code, aim to avoid horizontal scrolling). Use blank lines to separate logical sections of code, but not excessively – for example, a blank line between methods and between local variable declarations and the first statement can enhance readability.

* **Review and Refactor Regularly:** Encourage code reviews for all significant changes. A second pair of eyes can catch potential bugs and enforce standards. Also, allocate time for refactoring when you spot growing technical debt – e.g. if a function has grown too complex or if two classes have duplicate code, plan a refactor to address it (don’t let “temporary” hacks live forever). Refactoring should be done in small, safe steps, ideally with tests to ensure nothing breaks.

By following these clean code practices, the codebase will not only be easier to work with but will also minimize bugs and ease onboarding of new contributors. Remember Robert C. Martin’s Clean Code advice: “always leave the code **cleaner** than you found it” – meaning any change or review is an opportunity to tidy up and improve clarity.

## Testing Conventions

A robust test suite is essential for long-term code health, especially as BloogBot’s complexity grows. Here are guidelines for testing:

* **Testing Framework:** Use a modern unit testing framework like **xUnit** or **NUnit** (or MSTest if it’s the team preference) for automated tests. Create a separate test project (e.g. `BloogBot.Tests`) in the solution. This project should mirror the structure of the main projects (e.g. a test class for each key class or feature). Having a dedicated test project keeps test code separate from production code.

* **Test Naming:** Follow a clear naming convention for test methods. A common standard is **MethodName\_Scenario\_ExpectedOutcome**. For example: `CalculateDamage_TargetHasArmor_ReturnsReducedDamage()` or `BotStart_NoTarget_ThrowsException()`. This naming scheme makes it immediately obvious what behavior is being tested and what result is expected. Good test names serve as documentation for the code – someone reading the test list should understand the intended behavior without reading the implementation. **Test class** names typically correspond to the class under test (e.g. `BotEngineTests` for `BotEngine` class, or scenario-based like `NavigationServiceTests`). If using BDD-style tests, you could also use descriptive names or even the `[Trait]` attribute or categories to group tests (like "Integration" vs "Unit").

* **Arrange-Act-Assert Pattern:** Structure every test clearly into three sections: **Arrange** (setup your objects and state), **Act** (perform the operation under test), and **Assert** (verify the outcome). Separate these sections with blank lines to visually distinguish them. For example:

  ```csharp
  // Arrange
  var bot = new Bot();
  bot.LoadProfile(testProfile);

  // Act
  bool result = bot.Start();

  // Assert
  Assert.IsTrue(result);
  Assert.Equal(BotState.Running, bot.State);
  ```

  This pattern ensures tests are easy to read and understand. Only one logical assertion scenario should be tested per test method (though you can have multiple Assert statements verifying aspects of that one scenario’s outcome).

* **Test Coverage:** Aim to write unit tests for all core logic: state machine decisions, utility functions (e.g. math or pathfinding algorithms), and any bug fixes (to prevent regressions). For parts of the code that interact with external systems (like the WoW process memory or file system), consider using abstraction so that you can inject a fake or mock in tests. For example, have an interface `IWoWMemoryReader` and implement a fake version for tests to simulate game states.

* **Testing Difficult Areas:** Some parts of BloogBot (like actual injection into a game process or timing-specific loops) are hard to unit test. Isolate the non-deterministic or external-dependent code behind interfaces or check for conditions so that logic can be tested in isolation. For instance, you might not unit-test the actual `InjectDll()` function (that requires a live process), but you can test that your `BotManager` calls `IInjector.Inject()` with correct parameters when starting, by substituting a mock injector.

* **Consistency in Tests:** Keep test code quality high – apply the same standards to test code as production code. Use meaningful variable names in tests, avoid copy-paste, and ensure tests are deterministic (no random sleeps or order dependencies). If a test is flaky or fails randomly, fix it immediately – unstable tests undermine trust in the test suite.

* **Test Organization:** Organize tests logically. One approach is to mirror the namespace structure of the code under test. Another is to group by behavior or feature. Choose a structure and stick to it. Within a test class, you can use region tags or comments to separate tests for different methods of the target class. Use the `[Setup]` (MSTest) or `[SetUp]` (NUnit) or constructor (xUnit) to initialize common objects needed by multiple tests to reduce repetition.

* **Use of Mocks/Fakes:** Use mocking frameworks (like Moq or NSubstitute) to simulate dependencies for unit tests. For example, if testing the bot logic that relies on an `IGameApi`, create a mock `IGameApi` that returns controlled data. This ensures tests only validate *BloogBot’s* logic, not the external dependency. However, be careful not to overuse mocks – sometimes a simple fake implementation is enough and more maintainable than complex mocking setups. The guiding principle is isolation: each unit test should test one unit (class or method) in isolation from external effects.

* **Integration Tests:** In addition to unit tests, consider integration tests for critical scenarios (if feasible). For example, a test that runs the bot logic for a short cycle with a simulated environment to see that it transitions states correctly. These could be in a separate project (e.g. `BloogBot.IntegrationTests`) and perhaps marked with a category if they require more setup or slower execution. They can use more real components together (maybe a lightweight in-memory simulation of game objects).

* **Test Execution:** Integrate test execution into the build process and CI (see CI/CD section). All tests should run on each pull request or build, and failures should block the build. This ensures regressions are caught early. Keep tests fast so that they don’t slow down development or CI; long-running tests (if any) might be flagged as such or run less frequently.

* **Naming and Style in Tests:** Although tests are code, some coding standards can be slightly relaxed for brevity – for example, some teams allow longer method names (because test names are inherently longer for descriptiveness) and underscores in test method names (e.g. `MethodUnderTest_Scenario_Expected()` format uses underscores intentionally). This is acceptable as it improves readability of test reports. Other coding standards (like using PascalCase vs camelCase) still apply within test code (e.g. method names of tests are PascalCase by default in C#, even if they contain underscores). Maintain the same brace and indentation style in tests as elsewhere.

By adhering to these testing conventions, the team will ensure that BloogBot’s behavior is continuously verified. A solid test suite acts as a safety net for refactoring and adding new features, and it documents the intended behavior of the system.

## Linting and Formatting Standards

To maintain consistency in coding style, use automated linting and formatting tools. Consistent style reduces cognitive load in code reviews and helps catch potential issues early. Here are the standards and tools recommended:

* **EditorConfig:** Utilize an `.editorconfig` file at the root of the repository to enforce coding style rules. This file can specify naming conventions, whitespace, indent size, etc., which are automatically respected by modern IDEs (Visual Studio, VS Code, Rider) and analyzers. For example, you can enforce that private fields must begin with `_` and that public members must be PascalCase by adding naming rules – the compiler/IDE will produce a warning or error if the rule is violated. The EditorConfig can also set preferences like “use var for local variables” or spacing around braces. This ensures everyone’s environment formats code the same way.

* **Indentation and Braces:** Follow standard C# formatting: 4 spaces per indent level (no tabs), and use the Allman style for braces (opening brace on a new line aligned with the block start, closing brace on its own line). Automated formatters can enforce this. Ensure the EditorConfig reflects this (e.g., `indent_style = space`, `indent_size = 4`). Consistently formatted code is easier to read and diff.

* **Line Length and Wrapping:** Aim to keep lines at a reasonable length. A common practice is \~120 characters max per line. The EditorConfig can specify a hard or soft limit (e.g. `max_line_length = 120`). Break up long lines, especially LINQ queries or concatenations, into multiple lines to improve readability. The code editor or `dotnet format` can help wrap lines as needed. Consistent line length avoids horizontal scrolling and makes side-by-side code review easier.

* **Linting Tools:** Introduce **StyleCop Analyzers** or **Roslyn code analyzers** to the project. These tools check code style and practices against predefined rules. For example, StyleCop can ensure spacing, naming, ordering of elements, etc. If using StyleCop, include a `stylecop.json` or configure rules in EditorConfig. You might enforce rules like: all using statements at the top of file, file must end with a newline, no trailing whitespace, etc. These seem trivial but they keep the repository clean.

* **Custom Rules:** Define any project-specific rules. For example, you might decide that every ViewModel class name must end with “ViewModel” (which could be enforced by a naming rule). Or that async methods must end with “Async” in their name. These can be encoded in EditorConfig naming rules or verified via analyzers. Another example: if the code uses regions, maybe enforce a standard for region names (or decide to avoid regions altogether unless necessary). Document these decisions in this guide and enforce via linting if possible.

* **Automated Formatting:** Use the `dotnet format` tool or IDE formatting consistently. You can add a check in CI to run `dotnet format --verify-no-changes` which will fail if the code is not formatted per the EditorConfig. This encourages developers to format code before pushing (many IDEs can format on save). Automating code formatting saves time in code reviews (you don’t need to comment on spacing or indent issues) and improves code quality.

* **Trailing Spaces and EOF:** Ensure no trailing whitespace on lines and files end with a newline. Most linters/formatters handle this. It prevents unnecessary diffs.

* **Style Consistency:** Use consistent brace usage (even optional braces are included as per our standard), consistent use of `var` vs explicit types (the team should decide one – e.g. “use `var` when the right-hand side makes the type obvious, otherwise use explicit type” – and have analyzers enforce it to avoid debates). Also decide on other style preferences: e.g., expression-bodied members usage, lambda vs delegate preference, etc., and encode these in the EditorConfig or as guidelines here. The key is not which style is chosen, but that the team **uses the same style** everywhere.

* **Comments and Documentation Style:** The code should also be checked for documentation comments on public members if this is an API or large project. Tools like **XML Comments analyzer** can warn if a public method lacks a `<summary>` comment. If BloogBot is mostly an internal app, this might not be critical, but it’s good practice especially if parts of the code (like plugin interfaces) are intended for use by others. Also ensure TODO or hack comments are tracked (some teams configure analyzers to flag TODO comments so they aren’t forgotten).

* **Continuous Linting:** Make the linting part of the build. Treat style warnings as errors if possible (at least for key rules). The `.editorconfig` can set severity for each rule (none, suggestion, warning, error). For example, you could set the rule “private field must begin with \_” as an error, so code that violates it won’t compile without fixing. This immediately enforces standards. However, be cautious with adopting too many new rules at once in an existing codebase – you may need to gradually introduce them and fix the code incrementally or suppress certain rules if not immediately feasible to fix everywhere.

Using these linting and formatting measures, the code style becomes uniform across contributors. This reduces stylistic arguments and lets the team focus on logic during reviews. As the Microsoft documentation notes, having a consistent coding style and automating its enforcement can improve code quality and reduce time spent in reviews. The end goal is that any piece of code in BloogBot looks like it could have been written by the same person, even as multiple people contribute.

## Continuous Integration and Delivery (CI/CD) Workflows

Establishing a solid CI/CD pipeline will ensure code quality and ease the deployment of BloogBot. Here are recommended practices for automating building, testing, and deployment:

* **Version Control and Branching:** All development should occur on feature branches or pull requests. The main branch (e.g. `main` or `master`) should always be in a deployable state. Use PRs to integrate changes, with CI running on each PR to catch issues before merge.

* **Build Automation:** Use a CI service (GitHub Actions is a convenient choice for GitHub projects, or alternatives like Azure DevOps Pipelines) to automate builds. The build pipeline should:

  * Restore NuGet packages (`dotnet restore`).
  * Build the solution (`dotnet build`) for the intended configuration (Debug for CI testing, Release for production artifact).
  * Run unit tests (`dotnet test`) and report results. This catches any failing tests immediately.
  * Run linters/format checks. For instance, have a step that runs `dotnet format --verify-no-changes` to ensure code formatting adheres to standards, and run static analysis (like StyleCop or FXCop analyzers) to enforce coding rules. If any issues are found, the pipeline should fail, preventing merge of non-conforming code.
  * Optionally, calculate code coverage and fail if below an agreed threshold, to keep test coverage from dropping.

* **Quality Gates:** Integrate tools like **SonarQube** or **CodeQL** analysis in CI for deeper static analysis and security scanning. The CI can upload the code to SonarQube (if set up) to check for code smells, duplicate code, and vulnerabilities. While not mandatory, this is a good practice for enterprise-quality codebases.

* **Artifact Generation:** If BloogBot produces an executable or installer, configure the pipeline to produce artifacts. For example, use `dotnet publish` to get a self-contained build, or package into a ZIP or installer (MSI). The CI should then archive these artifacts so they can be downloaded or used in deployment.

* **Continuous Delivery/Deployment:** Since BloogBot is a desktop app, “deployment” might simply mean preparing a release package. You can automate creating a GitHub Release when a version is ready. For example, when a commit is tagged (e.g. `v1.2.3`), have the pipeline draft a release with the packaged artifacts. This ensures that deployment is consistent and not a manual, error-prone process. If the bot has an auto-update mechanism, CI can also update the update feed or repository.

* **Environmental Configuration:** Use CI to build for multiple configurations if needed (e.g., maybe a debug build for internal testing that connects to a test game server, and a release build for actual use). You might have separate config files – ensure the pipeline picks the right ones. Manage any secrets (like code signing certificates or server credentials) via the CI’s secure variable store (never hard-code secrets in the repo).

* **Testing in CI:** All tests should run in CI, but for something like BloogBot, you might also want an integration test stage. If possible, set up a headless mode or simulation to run the bot logic in CI (without a game) to do end-to-end testing of critical flows. This might be challenging, but even a simplified integration test (like running the bot for a tick with a dummy target) could be included. Mark such tests as integration so they can be separated from unit tests if needed.

* **Code Coverage & Badges:** It’s motivating to track code coverage and other metrics. CI can produce a coverage report (via Coverlet or similar) and you can display a badge in the README. Similarly, a build status badge from GitHub Actions can show the build is passing. This promotes accountability for keeping the build green.

* **Static Analysis and Linters in CI:** As mentioned in linting, integrate these fully. The pipeline will serve as the gatekeeper – if someone forgets to run format or violates a rule, the CI will fail the build. This automated enforcement ensures standards are upheld uniformly.

* **Continuous Delivery of NuGet (if applicable):** If any part of BloogBot (like a core library or plugin framework) is meant to be consumed via NuGet, set up the pipeline to pack and publish NuGet packages for it. For example, if `BloogBot.Core` were a library, CI on a new version could push a package to GitHub Packages or NuGet.org. (For the BloogBot application itself, NuGet is likely not applicable – instead focus on installer or release artifacts.)

* **Deployment/Release Workflow:** Use a workflow where after tests pass on the main branch, an automated action creates a release build. Optionally, use **GitHub Releases** with change logs. You could also implement an auto-updater in the app that checks a URL for latest version; if so, ensure CI updates that URL or file. Having a one-click deployment pipeline ensures that releasing a new version is not a stressful, manual process but rather an outcome of the regular CI run.

* **Zero-Downtime / Incremental Rollouts:** These concepts mostly apply to web services, but if relevant (for example, if deploying to multiple users), consider feature toggles or configurations that can be turned on gradually. In a bot context, this might not be needed, but if you had a user base, you might release a “beta” version to a subset before full release.

* **Documentation and CI:** Ensure that any user-facing documentation (like the README or a Wiki) is updated as part of releases. You can automate some of this. For instance, if you maintain an external changelog, you could have CI append to it when creating a release. Always include instructions with artifacts (like a README in the ZIP).

In summary, treat CI/CD as an integral part of development:

* Every commit triggers build + test + lint.
* Only quality-checked code gets merged (enforce via required CI pass on PRs).
* Releases are automated and consistent.

Adopting these CI/CD best practices yields faster, more reliable releases and maintains high code quality. It reduces the risk of “it works on my machine” by ensuring everyone’s changes are validated in a clean environment. Over time, this discipline will give the team confidence to refactor and improve the code since tests and CI guard against breaking things.
