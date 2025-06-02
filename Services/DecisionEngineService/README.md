# DecisionEngineService

## Purpose and Overview

The **DecisionEngineService** is a modular rule-processing engine integrated into the BloogBot application. It provides a way to define custom business rules (called **Rule Engines**) and execute them dynamically against incoming data (as **Rule Executions**). Its primary responsibility is to allow developers and administrators to create, manage, and run decision logic without changing core code. In the broader application, this service acts as a **rule engine module** – developers can integrate it to automate decisions (e.g. form validations, automated workflows) by configuring rules, and users (with proper access) can define or modify these rules at runtime. The service is built as a self-contained Laravel package, complete with database models, controllers, routes, views, and a service provider, making it easy to plug into a Laravel application.

In summary, DecisionEngineService enables:

* **Dynamic Rule Definition:** Create and configure rules that contain validation criteria and executable logic (PHP code or Artisan commands).
* **Rule Execution:** Run a specified rule with given input data, automatically validate the input, execute the business logic, and obtain an output decision.
* **Audit and Logging:** Keep records of each execution and log changes, facilitating debugging and traceability of rule runs.
* **Integration:** Expose web UI and API endpoints so that rules can be managed through a browser interface and executed programmatically via HTTP requests, fitting into both admin dashboards and external systems.

## Architectural Structure and Integration

The DecisionEngineService is structured as a typical Laravel service module. It includes a **service provider** for bootstrapping, configuration files for customization, database migrations for persisting rules and their executions, Eloquent models for data access, controllers and routes for HTTP interfaces (UI and API), and Blade views for a basic management UI. This design allows it to integrate seamlessly into a Laravel application:

* **Service Provider:** Registers the module’s routes, merges configuration, and offers publishing of assets (config and migrations) into the host app. This means once the service is included (via Composer or as part of the codebase), it can auto-configure itself.
* **Routes:** The service defines dedicated routes (with a configurable URL prefix) for both web interface and API calls. By default, all routes are prefixed under `/decision-engine` (this can be changed via config). Web routes are protected by the Laravel `web` middleware guard by default, and API routes can be secured by specifying guards (empty by default, meaning no auth required unless configured).
* **Database Integration:** The service is database-backed – it introduces new tables for storing rule definitions, execution records, and logs. Migrations are provided and can run automatically (unless explicitly disabled) to create these tables. The module can use the default database connection or a specific connection as configured, and supports either integer IDs or UUIDs as primary keys based on configuration.
* **Use in Application:** In a larger application, DecisionEngineService can be used wherever dynamic decision-making is needed. Other parts of the app can invoke a rule execution via the provided API or by using the service classes directly. For example, if an external event requires evaluation of a business rule, a POST request can be made to the DecisionEngine API endpoint, or a developer can programmatically create a `RuleExecution` record and process it through the service. The module is designed to be mostly self-contained, interacting with the rest of the app mainly through the Auth system (to track which user creates rules or triggers executions) and through these well-defined interfaces.

## Components Breakdown

The DecisionEngineService directory contains several subcomponents, each fulfilling a specific role. Below is a breakdown of each major file and subcomponent, organized by their functionality:

### Service Provider

* **`Providers/DecisionEngineServiceProvider.php`:** This is the entry point that bootstraps the service into Laravel. It merges the configuration file into the application config (under key `decision-engine`) and registers routes, views, and migrations. In the `boot()` method, it loads web routes and API routes with the configured prefix and namespaces. It also sets up publishing of the config and migration files so developers can copy them into their application if needed. Additionally, it checks the `db_primary_key_type` setting and, if set to "uuid", switches the model classes to use UUID variants for RuleEngine, RuleExecution, and RuleExecutionLog. This provider is auto-discovered via Composer (as specified in the package’s composer.json) so it registers itself when the app starts.

### Configuration and Environment

* **`config/decision-engine.php`:** This configuration file defines various settings for the DecisionEngineService. Key configuration options include:

  * **`path`** – The base URI path for the service’s routes (defaults to `"decision-engine"`). For example, if left default, the rule engine UI will be accessible under `/decision-engine/rule-engines` in the app.
  * **`db_connection`** – The database connection name to use for the rule engine tables. By default, it uses the application’s primary DB (e.g. MySQL). This allows isolating the data store if necessary.
  * **`db_primary_key_type`** – The type of primary keys for the tables. Options are `"id"` (auto-increment integers) or `"uuid"` (GUID keys). The default is `"id"`; if set to `"uuid"`, the service will use UUID-based models and migrations to create UUID primary keys.
  * **`types`** – An associative array of allowed rule execution types. By default, two types are enabled: `"code"` and `"command"` (mapped to human labels "Code" and "Command"). A third type `"api"` is mentioned in comments as a possibility, but it is not included by default – developers can add it or other types here to extend functionality (see **Extensibility** below).
  * **`web_guards`** – An array of authentication guards/middleware for the web routes. Default is `['web']`, meaning the rule management UI requires the Laravel “web” session (usually implying the user must be logged in).
  * **`api_guards`** – Guards/middleware for the API route. Default is an empty array, which means no authentication is enforced on the API endpoint unless you specify one (e.g. you could set this to `['api']` to require a token via Laravel’s API guard).
  * **`pagination_size`** – The number of items per page for listing rule engines or executions in the UI (default `20`).
  * **`logger`** – The logging channel to use for DecisionEngine-specific log messages. This can tie into Laravel’s logging config (for example, to use a custom log file or slack channel for rule execution events). *(Note: The current implementation does not explicitly log to this channel in the provided code, but the option exists for future use or custom extension.)*

  These settings can be overridden via environment variables (`DECISION_ENGINE_PATH`, `DECISION_ENGINE_CONNECTION`, `DECISION_ENGINE_PRIMARY_KEY_TYPE`, `DECISION_ENGINE_LOGGER`, etc.) or by publishing and editing the config file in the host application. For instance, to use UUIDs for keys, one would set `DECISION_ENGINE_PRIMARY_KEY_TYPE=uuid` in the `.env` file.

### Data Models

The service defines several Eloquent models under `Models/` to represent the rule engine data and handle database interactions. All models use Laravel’s conventions (timestamps, soft deletes, etc.) and include built-in validation logic and relationships:

* **`RuleEngine` (`Models/RuleEngine.php`):** Represents a rule definition (the "engine"). It maps to the `rule_engines` database table. Important fields include:

  * `name` – A unique name for the rule (e.g. "LoanApprovalRules"). This is required and must be 3-150 characters (letters, numbers, dashes).
  * `type` – The execution type (e.g. `"code"` or `"command"`) indicating how the rule’s logic will be run. Must be one of the keys defined in the config’s `types` array.
  * `validation` – A PHP array (stored as text) defining Laravel validation rules for the expected input. This is optional while the rule is in draft, but required once the rule is active (status=1).
  * `business_rules` – The executable logic for the rule, stored as text. For `code` type, this is a snippet of PHP code; for `command` type, this is an Artisan command string. (Also required when the rule is active).
  * `status` – Indicates if the rule is active (likely `1` for active/enabled and `0` for draft/inactive). Only active rules can be executed.
  * Other fields: `created_by` (tracks the user who created the rule), `ipaddress` (client IP of creation), and timestamps. These are set automatically in model events.

  The `RuleEngine` model includes an internal validator on save: before saving, it runs `getValidationRules()` to ensure the rule’s own attributes are valid. Notably, it requires that if a rule is marked active (`status == 1`), the `validation` and `business_rules` fields must not be empty. If any validation fails, the save is aborted and errors are stored. The model also automatically populates `created_by`, `status`, and `ipaddress` on creation. Relationship: a RuleEngine has many `RuleExecution` records (one-to-many) representing each run of that rule.

* **`RuleExecution` (`Models/RuleExecution.php`):** Represents a single execution run of a rule, mapping to `rule_executions` table. Key fields:

  * `rule_engine_id` – Foreign key linking to the `RuleEngine` (the rule being executed).
  * `input` – The input data provided for execution, stored as a JSON string in the DB (text column). The model accessor/mutator converts this to/from an associative array in PHP for convenience.
  * `output` – The result produced by the rule execution, stored as text (often JSON or string). A mutator ensures if an array is set as output, it is JSON-encoded before saving.
  * `status` – A status message or code representing the outcome of execution (e.g. "Begin Execution", "Validation Failed", "Code Success", etc.). This starts as "Begin Execution" when the execution is first created and is later updated to reflect the result.
  * Other fields: `created_by` (user who initiated the execution, set on creation via Auth), `ipaddress` (client IP), plus timestamps.

  The `RuleExecution` model also has validation rules on save:

  * It requires a `rule_engine_id` that exists in the `rule_engines` table and refers to an active rule (status = 1).
  * It requires `input` to be provided (not null).
  * It requires `status` to be set once the record exists (on update) – though an initial status is auto-set, this ensures the status gets updated.

  On creating a RuleExecution, the model automatically sets the initial status and creator info. On each update of a RuleExecution (for example, after the rule logic has been executed and the output/status are updated), the model triggers an **audit log**: it creates a new `RuleExecutionLog` record capturing the “before” and “after” differences. This logging is handled in the `updated` model event, using the `getOriginal()` and `getDirty()` attributes to record changes. Relationships: a RuleExecution *belongs to* a RuleEngine (one execution for one rule), and *has many* `RuleExecutionLog` entries.

* **`RuleExecutionLog` (`Models/RuleExecutionLog.php`):** Represents a log entry for changes to a RuleExecution, stored in `rule_execution_logs` table. Fields include:

  * `rule_execution_id` – Reference back to the execution record.
  * `previous_attributes` – A JSON snapshot of the execution’s attributes before an update.
  * `new_attributes` – A JSON snapshot of the execution’s attributes after an update.
  * `created_at` timestamp (each log entry is created at the moment of an execution update).

  The model primarily just stores these blobs of data. It uses mutators to JSON-encode the `previous_attributes` and `new_attributes` on set (on retrieval, developers can decode these as needed to inspect what changed). A `RuleExecutionLog` belongs to a RuleExecution (inverse of the one-to-many relationship). Like other models, it also captures the `ipaddress` on creation for auditing. This log allows developers to trace how an execution’s status/output changed (e.g., from "Begin Execution" with no output to "Code Success" with a result payload).

* **UUID Variants:** For each of the above models, there is an alternative class to support UUID primary keys:

  * `RuleEngineUuid`, `RuleExecutionUuid`, and `RuleExecutionLogUuid` (in the same Models namespace). Each of these simply extends its base counterpart and uses Laravel’s `HasUuids` trait. When the config `db_primary_key_type` is set to `"uuid"`, the service provider will instruct the DecisionEngine to use these classes instead. Functionally they behave the same, but the database columns will be UUIDs instead of big integers. This design gives flexibility in choosing ID strategies without duplicating logic.

### Core Service Classes

These classes implement the core logic of the decision engine – validating inputs and executing the rule’s business logic:

* **`DecisionEngineService` (`Services/DecisionEngineService.php`):** This is the high-level service class that orchestrates a rule execution process. It is typically instantiated with a `RuleExecution` model instance (representing a pending execution) and provides methods to validate the input and process the rule:

  * **Constructor:** Accepts a `RuleExecution` object and initializes internal references to the input data and the associated `RuleEngine` (rule definition). It essentially prepares the context for execution.
  * **`validateInput()`** – Runs the input validation against the rule’s validation rules. It uses Laravel’s Validator facade, evaluating the `RuleEngine->validation` field (which is stored as a PHP code string for an array of rules) to get an array, then applying those rules to the provided input. If validation fails, it stores the errors and returns `false`; if it passes, returns `true`.
  * **`processInput()`** – This is the main method that executes the rule logic. It first creates a `RuleProcessService` instance to handle the low-level execution. Then:

    * If the `RuleEngine` configuration is missing (for some reason the rule definition isn’t found), it prepares a result with status "Invalid Rule Engine" and an error message.
    * Else if the input data fails validation (`validateInput()` returns false), it prepares a result with status "Validation Failed" and the validation errors.
    * Otherwise (valid case), it calls the `executeProcess()` method of `RuleProcessService` to actually run the business logic and get the result.
    * In each case of error (invalid rule or validation failure), it also calls `updateRuleExecution` on the `RuleProcessService` to immediately update the `RuleExecution` record in the database with the failure status and output. If execution is successful, `RuleProcessService` itself handles updating the record with the final status and output. Finally, `processInput()` returns an array containing the result status and output.

  In practice, developers do not call `DecisionEngineService` methods directly via the UI or API; instead, the API controller uses this service internally to encapsulate the execution flow. However, one could instantiate this service in application code to run a rule if needed (for example, in a unit test or a custom scenario) by providing a `RuleExecution` model.

* **`RuleProcessService` (`Services/RuleProcessService.php`):** This class contains the implementation of executing the rule’s business logic based on its type. It’s only used internally by `DecisionEngineService`. Key aspects:

  * It is constructed with a `RuleExecution` instance, from which it derives the associated `RuleEngine` and the input data.
  * It defines private methods for each supported execution type:

    * **`executeCode()`** – Executes the rule when `type == "code"`. It uses PHP’s `eval()` to run the PHP code stored in `RuleEngine->business_rules`. Before evaluating, it extracts the `$data` array (the input) into individual variables. The evaluated code is expected to return or produce some result, which is captured in `$result`. The service then wraps this in a standardized output array with status "Code Success" (or catches exceptions to produce "Code Execution Failure" along with the error trace).
    * **`executeCommand()`** – Executes the rule when `type == "command"`. This treats the business rule as an Artisan console command. The code builds the command string (it also `eval()` the string in case it contains PHP variables that need to be substituted) and runs it via `Artisan::call()`. The result (exit code) and any console output are captured and returned in an array with status "Command Success" (or "Command Execution Failure" if an exception is thrown).
    * **`executeApi()`** – *(Not implemented in the current code, but conceptually would handle an `"api"` type if added. Currently, if a rule’s type were set to `"api"`, `RuleProcessService` would attempt to call a method `executeApi` that does not exist, resulting in an exception. Developers can extend this class to add support for new types as needed; see **Extensibility**.)*
  * **`executeProcess()`** – This method determines which of the above execution methods to call based on the rule’s type at runtime. It uses Laravel’s string helper to capitalize the type and dynamically call the corresponding `executeX` method (e.g., for type "code", it calls `$this->executeCode()`). It wraps this call in a try-catch to handle any unforeseen exceptions, labeling such errors as "Exception Rule Process Failure". After obtaining a result (success or failure), it calls `updateRuleExecution($result)` to save the outcome to the database, then returns the result array. The `updateRuleExecution()` method simply updates the `RuleExecution` model with the new status and output fields (triggering the model’s `updated` event which logs the change).

  `RuleProcessService` is the component that actually runs user-provided code/commands. Because it uses `eval()` and can run Artisan commands, **it assumes that only trusted users define the rule content**. In a production setting, one should restrict who can create/modify rules to avoid security risks, as malicious code could be executed. The design choice to allow direct code execution provides flexibility (you can write any PHP logic for a rule) at the cost of requiring careful access control.

* **`DecisionEngine` (`DecisionEngine.php` in the root of the service src):** This is a utility class providing a static interface to some configurable aspects of the engine. It’s akin to a facade or manager for the module’s models. It is not an actual Laravel Facade, but a plain class with static properties and methods:

  * It holds static properties for the class names of the models in use: `$ruleEngineModel`, `$ruleExecutionModel`, `$ruleExecutionLogModel`. By default, these are set to the base model classes (e.g. `"Samsin33\DecisionEngine\Models\RuleEngine"` as a string). The service provider may swap these to the Uuid variants if needed.
  * Methods like `useRuleEngineModel($class)`, `useRuleExecutionModel($class)`, etc., allow overriding which model classes to use. This is useful if a developer wants to extend the models (adding relationships or methods) and have the DecisionEngine use those extended classes instead of the built-ins.
  * Helper methods `ruleEngine(array $data)`, `ruleExecution(array $data)`, `ruleExecutionLog(array $data)` are factories that create new model instances of the respective class. The controllers use these to remain decoupled from concrete classes – e.g., `DecisionEngine::ruleEngine($data)` will instantiate either a `RuleEngine` or `RuleEngineUuid` depending on config.
  * It also has toggles `ignoreMigrations()` and `ignoreRoutes()` which set static flags `$runsMigrations` and `$registersRoutes` to false. These can be called in the application’s `AppServiceProvider` (or another early stage) if the developer wants to manually control migrations or route registration. For example, an application might call `DecisionEngine::ignoreRoutes()` and then define its own routes to the controllers (perhaps to wrap them with additional middleware or to mount under a different path), or call `ignoreMigrations()` if it prefers to copy the migration files and run them as part of the main app’s migrations.

### Controllers and Routes

The service provides two HTTP controllers (in `Http/Controllers/`) that allow interaction with rule engines and executions. The routes for these controllers are defined in the `routes/web.php` and `routes/api.php` files, which are loaded by the service provider.

* **`RuleEnginesController` (`Http/Controllers/RuleEnginesController.php`):** Handles **management of rule definitions** (the RuleEngine model). Its routes are prefixed with `/rule-engines` under the main service path. It uses resourceful routes covering CRUD operations:

  * `index()` – List all rule engines with pagination (uses the `pagination_size` config). Returns a Blade view `decision-engine::RuleEngines.index` with the data.
  * `create()` – Show a form to create a new rule engine. It initializes a new RuleEngine model (unsaved) via `DecisionEngine::ruleEngine()` and passes it to the view `decision-engine::RuleEngines.create`.
  * `store()` – Process form submission for creating a rule engine. It instantiates a RuleEngine with the provided data (`$request->rule_engine` array) and attempts to save it. If save succeeds, it redirects the user to the edit page of the newly created rule (with a success message). If validation fails (model returns false on save), it re-displays the create form view with the model (which contains validation error messages in `$rule_engine->getErrors()`).
  * `show($id)` – Display details of a specific rule engine (and possibly its executions). It finds the RuleEngine by ID and returns the `decision-engine::RuleEngines.show` view.
  * `edit($id)` – Show the edit form for an existing rule engine. It loads the rule and reuses the same form view as create (the view likely handles showing it as edit if the model exists).
  * `update($id)` – Process form submission for updating a rule engine. It finds the rule, attempts to update it with new data. On success, redirects back to the edit page with a success notice; on failure, returns the form view again with the model (which now holds errors).
  * `destroy($id)` – Delete a rule engine. On success, redirects to the index with a confirmation message. On failure, it also redirects to index but with an error message.

  This controller uses the `DecisionEngine` class to retrieve the appropriate model classes, which ensures transparency regarding UUID vs ID models. The Blade views for RuleEngines (not detailed here) would include forms and tables to interact with these methods. Typically, only authorized users (admins) should have access to these routes, which is why the `web` guard is applied – you might further restrict via middleware or policies in a real app.

* **`RuleExecutionsController` (`Http/Controllers/RuleExecutionsController.php`):** Handles **execution of rules and viewing execution logs**. Its routes exist in both web and API context:

  * **Web routes (`routes/web.php`):** Only `index` and `show` are exposed on the web interface.

    * `index()` – List all rule execution records, paginated. It fetches all executions (likely recent ones) and returns `decision-engine::RuleExecutions.index` view with the list.
    * `show($id)` – Display details of a specific execution, including its log entries. It loads the execution (with related logs via Eloquent `with`) and returns `decision-engine::RuleExecutions.show` view.
    * (There is no direct `create` or `edit` for executions via UI – because executions are not manually created by users through a form. They are typically triggered via the API or some action.)
  * **API route (`routes/api.php`):** Only the `store` action is included for API calls:

    * `store()` – Execute a rule (create a new RuleExecution and run it). This is the core endpoint that clients or other parts of the system will call to process a rule. It expects a request (e.g. JSON payload) that contains a `rule_execution` object with at least a `rule_engine_id` (to specify which rule to run) and the `input` data for that rule.

    When `store()` is called, it performs the following:

    1. Construct a new RuleExecution model with the provided data (`DecisionEngine::ruleExecution($request->rule_execution)`), which will populate the model’s fields (but not save yet).
    2. Save the RuleExecution to the database by calling `$rule_execution->save()`. This triggers the model’s validation – ensuring the referenced RuleEngine exists and is active, and that input is provided. If this save fails, it means the input or setup was invalid; the controller will then return a JSON response with error details and HTTP 422 status.
    3. If the save succeeds, the RuleExecution record now exists with status "Begin Execution". The controller creates a `DecisionEngineService` with this execution and calls `$decision_engine_service->processInput()`. This runs the full validation + business logic as described earlier. The result is an array containing the final status and output of the rule.
    4. The controller then returns a JSON response containing the `output` (result array) and the `rule_execution` model data. By this time, the `rule_execution` will include updated fields (status changed from "Begin Execution" to e.g. "Code Success", and output filled with the result) because `DecisionEngineService` and `RuleProcessService` update it in the database. The response enables the caller to see the outcome and also the record of execution (which includes its ID, timestamps, etc.).

    Example: A client could POST to `/decision-engine/rule-executions` with a JSON body:

    ```json
    {
      "rule_execution": {
        "rule_engine_id": 5,
        "input": {
          "amount": 10000,
          "creditScore": 720
        }
      }
    }
    ```

    This would trigger rule engine with ID 5 using the given input data. The response might look like:

    ```json
    {
      "output": {
        "status": "Code Success",
        "output": { "approved": true, "rate": 3.5 }
      },
      "rule_execution": {
        "id": 42,
        "rule_engine_id": 5,
        "created_by": 1,
        "input": { "amount": 10000, "creditScore": 720 },
        "output": { "approved": true, "rate": 3.5 },
        "status": "Code Success",
        "created_at": "2025-06-02T15:00:00Z",
        "updated_at": "2025-06-02T15:00:01Z"
      }
    }
    ```

    The exact structure of the output content depends on what the rule’s code returns. The `status` field will reflect whether the execution was successful or if there were errors (e.g., "Validation Failed" with details if input didn’t pass validation, or "Code Execution Failure" if an exception was thrown during code execution). On success, it’s up to the rule’s code/command to define what the output contains (could be a boolean decision, a calculated value, etc.).

The routes grouping in the service provider attaches these controllers under the main prefix and applies middleware guards as configured. By default:

* Web UI for rules is at: **`/decision-engine/rule-engines`** (list, create, edit, etc.) and **`/decision-engine/rule-executions`** (list executions, view log). These require a logged-in session (or whatever guard is in `web_guards`).
* API for executing a rule: **`POST /decision-engine/rule-executions`** (with JSON body). By default, no auth (if `api_guards` is empty), but you can protect it by setting an API guard. Typically, you’d restrict this to internal use or authorized service calls, since execution of rules might be a sensitive operation.

### Views (User Interface)

Under `resources/views/decision-engine/` (and subfolders), the service includes Blade templates that provide a basic user interface for managing rule engines and viewing executions. These views are referenced by the controllers and give developers a starting point:

* **Rule Engines Views:** likely include `RuleEngines/index.blade.php` (table of rules), `RuleEngines/create.blade.php` (form for create/edit), and `RuleEngines/show.blade.php` (details of a rule, possibly listing associated executions).
* **Rule Executions Views:** likely include `RuleExecutions/index.blade.php` (list of execution records) and `RuleExecutions/show.blade.php` (details of a single execution, with possibly the log history of status changes).

The views use the Blade namespace `decision-engine::` as set in the service provider. They are published with the package, meaning you can override them by copying to your own `resources/views/vendor/decision-engine` if needed. The provided UI is functional but basic – developers might want to customize the look or integrate it with their layout. For example, the create/edit form for a RuleEngine would include fields for name, type (probably rendered as a dropdown of allowed types), validation rules (perhaps as a code input or JSON), and business logic (maybe a textarea for code/command). The index pages likely show listings with pagination. While using these views is optional, they are convenient for quickly managing rules without building a UI from scratch.

### Database Migrations

In the `database/migrations/` directory, the service provides migration files to create the necessary tables:

* **CreateRuleEnginesTable** – Creates `rule_engines` table with columns: id/UUID, created\_by, name, type, validation (text), business\_rules (text), status (tiny int), ipaddress, timestamps, soft deletes.
* **CreateRuleExecutionsTable** – Creates `rule_executions` table with columns: id/UUID, rule\_engine\_id (foreign key to engines, bigint or UUID accordingly), created\_by, input (text), output (text), status (string up to 30), ipaddress, timestamps, soft deletes.
* **CreateRuleExecutionLogsTable** – Creates `rule_execution_logs` table with: id/UUID, rule\_execution\_id (bigint/UUID), previous\_attributes (text), new\_attributes (text), ipaddress, timestamps, soft deletes.

These migrations are designed to respect the config for connection and primary key type (notice the checks for `config('decision-engine.db_primary_key_type')` to choose between `$table->id()` and `$table->uuid()` in each migration). By default, the service will run these migrations as part of `artisan migrate` (because `$runsMigrations` is true by default). If you prefer to manage migrations manually, you can disable auto-loading via `DecisionEngine::ignoreMigrations()` and publish the migration files to your app (using the provider’s publish tag `decision-engine-migrations`).

**Note:** After adding the DecisionEngineService to your project, ensure the migrations are executed (run `php artisan migrate`). This will create the tables required for the service to function.

## Usage Guide

Using the DecisionEngineService involves a few steps: configuring the service (optional, for custom setups), defining rules, and then executing those rules.

**1. Setup and Configuration:** If the service is installed via Composer, it will register automatically. Verify that the `DecisionEngineServiceProvider` is loaded (in Laravel this is automatic via discovery, or you can manually add to `config/app.php` providers if needed). Optionally, publish the config file (`php artisan vendor:publish --tag=decision-engine-config`) to adjust settings like route path or guards. Set any desired environment variables (e.g., `DECISION_ENGINE_PATH`, `DECISION_ENGINE_PRIMARY_KEY_TYPE=uuid` if you want UUIDs). Run migrations to create tables. If you plan to use the default web UI, ensure your auth system is in place (so `Auth::user()` works for created\_by tracking, and your users have access to the `/decision-engine` routes as appropriate).

**2. Defining a Rule (RuleEngine):** You can create rules using the web interface or directly via code:

* **Via Web UI:** Navigate to `/<your-app>/decision-engine/rule-engines`. Use the "Create Rule" form. You will input a Name, select a Type (e.g. Code or Command), optionally write Validation rules, and provide the Business Rules logic. For example:

  * Name: **"LoanApprovalRule"**
  * Type: **Code**
  * Validation: `['amount' => 'required|numeric', 'creditScore' => 'required|integer|min:300']` (this is a PHP array in string form – the UI might expect you to enter something like the array syntax).
  * Business Rules: e.g.

    ```php
    return [
        'approved' => ($creditScore > 600 && $amount < 50000),
        'recommendedAmount' => min($amount, 50000)
    ];
    ```

    (This sample code would approve a loan if creditScore > 600 and amount < 50k, returning a recommendation.)
  * Mark the rule as active (status = 1) if it's ready to be executed. (If you leave it as draft/inactive, it won’t run.)
  * Submit the form to create the rule. The service will validate the fields (e.g. ensure name is unique, etc.) and store it. On success, you can then edit or test the rule.
* **Via Code:** You can also create a rule in a seeder or script:

  ```php
  use Samsin33\DecisionEngine\DecisionEngine;
  $rule = DecisionEngine::ruleEngine([
      'name' => 'LoanApprovalRule',
      'type' => 'code',
      'validation' => "['amount' => 'required|numeric', 'creditScore' => 'required|integer|min:300']",
      'business_rules' => "return [ 'approved' => (\$creditScore > 600 && \$amount < 50000) ];",
      'status' => 1
  ]);
  $rule->save();
  if ($rule->getErrors()->any()) {
      // Handle validation errors...
  }
  ```

  This uses the static factory to get the correct model instance and attempts to save it. Note that the `validation` and `business_rules` are provided as strings containing PHP code (because the package will eval them when running). After save, check `$rule->getErrors()` for any issues.

**3. Executing a Rule (RuleExecution):** Once a rule is defined and active, you can run it:

* **Via API:** Make an HTTP POST request to the `/decision-engine/rule-executions` endpoint (or the custom path you configured) with the required JSON. As shown earlier, the payload should include `rule_engine_id` (the ID of the rule to run) and `input` (an object with input data). The service will respond with the result. This is suitable for triggering rule evaluations from external systems or via AJAX in your application. For instance, if you have a front-end form and want to get a decision from the rule engine without full page reload, you could call this API and display the decision to the user.
* **Via Laravel code:** You can also run a rule execution within the server-side code:

  ```php
  use Samsin33\DecisionEngine\DecisionEngine;
  use Samsin33\DecisionEngine\Services\DecisionEngineService;

  // Prepare a new execution instance
  $execution = DecisionEngine::ruleExecution([
      'rule_engine_id' => 5,
      'input' => ['amount' => 10000, 'creditScore' => 720]
  ]);
  if (! $execution->save()) {
      $errors = $execution->getErrors();
      // Handle validation errors (e.g., rule not found or input missing)
  } else {
      $service = new DecisionEngineService($execution);
      $result = $service->processInput();
      // $result is an array with 'status' and 'output'
      // Also, $execution->refresh() to get updated output/status from DB if needed
  }
  ```

  This mirrors what the API controller does: create and save a `RuleExecution`, then use `DecisionEngineService` to process it. After calling `processInput()`, the `$result` might be, for example, `['status' => 'Code Success', 'output' => ['approved' => true, ...]]`. The `$execution` record in the database will also have its `status` and `output` fields updated accordingly. You can retrieve or use those as needed.

The first time an execution is created, its status will start as "Begin Execution". If the input doesn’t satisfy the rule’s validation rules, the process will short-circuit and mark the execution with "Validation Failed" and provide error messages. If the rule logic runs but throws an exception, the status will be "Code Execution Failure" or "Command Execution Failure" with details. On success, you define the status by the type: the code sets "Code Success" or "Command Success" accordingly. These statuses and outputs are saved in the database and returned to the caller.

**4. Viewing Results and Logs:** Through the web interface, you can navigate to **Rule Executions** to see all runs. Each execution’s detail page will show the input provided, the output generated, the final status, and a log of changes (usually just one log entry capturing the transition from "Begin Execution" with no output to the final status with output). This is helpful for auditing and verifying that the rules are working as expected. If something went wrong (e.g., a "Failure" status), the output may contain an error trace or message for debugging.

**5. Extensibility and Customization:**

* *Adding New Rule Types:* If you want to support a new type of rule execution (for example, calling an external API or running a different kind of script), you can extend the system. You would add a new entry to the `types` config (e.g. `'api' => 'API'`), and then provide an implementation for that type. The simplest way is to extend the `RuleProcessService` class in your own code and add a method `executeApi()` (or any name matching the pattern `executeX` for your type). Because the core uses dynamic method calls based on type, it will invoke your method if the `RuleProcessService` instance has one. To use a custom `RuleProcessService`, you would currently have to modify the package or override how `DecisionEngineService` instantiates it (since in the given code it’s hard-coded). Alternatively, you could contribute to the service by adding the new logic directly. Keep in mind adding new types also means ensuring the `RuleEngine` model validation allows that type (it already will if you add it to config, due to the Rule::in check referencing config types).
* *Custom Models:* As mentioned, you can subclass the provided models to add fields or methods. Use `DecisionEngine::useRuleEngineModel(YourRuleEngine::class)` (and the analogous methods for execution and log) early in the application boot process to instruct the service to use your classes. Your custom model could override table names, relationships, or add business methods. For example, you might add a `description` field to rules for documentation, or override `booted()` to add additional business constraints.
* *Ignoring Package Routes/Migrations:* If you prefer to define the HTTP routes yourself (say, to integrate with your app’s existing API structure), call `DecisionEngine::ignoreRoutes()` in a service provider. Then you can set up routes to `RuleEnginesController` and `RuleExecutionsController` as you see fit (perhaps under a different URL or with additional middleware). Similarly, if you want to review or modify the migrations, use `ignoreMigrations()` and publish the migrations to your app to run them (maybe adjusting table names or columns to suit your conventions).
* *UI Customization:* The provided Blade views are a starting point. You can publish them (the provider doesn’t explicitly provide a tag for views in the snippet, but you can copy them from the package directory) into your `resources/views/vendor/decision-engine` and tweak the layout or styling. Since they are basic, you might want to integrate them into your app’s layout, add front-end validation, or improve the code editor for writing business rules (for example, using a syntax-highlighted editor for PHP code in the form).
