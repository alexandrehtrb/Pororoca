# Changelog

* [3.5.0](#350-2024-08-07)
* [3.4.2](#342-2024-07-02)
* [3.4.1](#341-2024-06-12)
* [3.4.0](#340-2024-05-29)
* [3.3.0](#330-2024-04-29)
* [3.2.0](#320-2024-04-09)
* [3.1.1](#311-2024-03-13)
* [3.1.0.1](#3101-2024-02-19)
* [3.1.0](#310-2024-02-14)
* [3.0.1.1](#3011-2024-01-17)
* [3.0.1](#301-2024-01-09)
* [3.0.0](#300-2024-01-01)
* [2.5.0](#240-2023-11-13)
* [2.4.0](#240-2023-10-15)
* [2.3.1](#231-2023-09-28)
* [2.3.0](#230-2023-09-26)
* [2.2.0](#220-2023-08-07)
* [2.1.0](#210-2023-04-24)
* [2.0.1](#201-2023-02-26)
* [2.0.0](#200-2022-12-20)
* [1.6.0](#160-2022-11-20)
* [1.5.0](#150-2022-09-20)
* [1.4.0](#140-2022-07-03)
* [1.3.0](#130-2022-05-15)
* [1.2.0](#120-2022-04-10)
* [1.1.0](#110-2022-03-20)
* [1.0.0](#100-2022-03-08)

## [3.5.0](https://github.com/alexandrehtrb/Pororoca/tree/3.5.0) (2024-08-07)

### Features

* Mouse drag-and-drop in collections' tree 🙌
* Collection and environments export redesigned. For collections, now you can select which environments you wish to include and whether to include their secret variables. (issue #82)
* Insomnia collections can now be imported.
* A dialog now appears when trying to import an invalid collection or environment file. (issue #35)
* A folder icon now appears to indicate a collection folder.

## Bug Fixes

* Postman collections and environments are now exported to files with UTF-8 without BOM encoding. Now you can export files from Pororoca to Insomnia, using Postman format.

### Others

* Raised .NET SDK to 8.0.303.
* Raised Microsoft.OpenAPI and test libs versions.

## [3.4.2](https://github.com/alexandrehtrb/Pororoca/tree/3.4.2) (2024-07-02)

### Features

* Count successes in repetition results (issue #113).
* Adds more names for predefined variables.
* Predefined variables can be applied on tables, like typing `$today` in a variable value will apply the current date formatted as yyyy-MM-dd. Read more about in our [docs](https://pororoca.io/docs/variables).

### Bug Fixes

* Fixed drag-and-drop in variables and headers tables.

## [3.4.1](https://github.com/alexandrehtrb/Pororoca/tree/3.4.1) (2024-06-12)

### Features

* Adds chinese language by [@LiarOnce](https://github.com/LiarOnce).

### Refactors

* Translated strings are now inside `.resx` files.

### CI/CD

* Removed unnecessary step in CI/CD.

### Others

* Raised .NET SDK to 8.0.302.

### New Contributors

* [@LiarOnce](https://github.com/LiarOnce) made his first contribution in PR [#111](https://github.com/alexandrehtrb/Pororoca/pull/111)

## [3.4.0](https://github.com/alexandrehtrb/Pororoca/tree/3.4.0) (2024-05-29)

### Features

* Allow prepending and appending whitespaces on templated variables. For example: `{{ MyVariableHere }}` is accepted now.
* Predefined variables (issue #81). Read more about in our [docs](https://pororoca.io/docs/variables).
* Paste headers from text into headers table (issue #68). You can copy a text like this from your notepad:

```
Pragma: no-cache
Accept: */*
Host: somehost.com
Connection: Keep-Alive
User-Agent: okhttp/3.12.1
```

And paste into a headers table, by right-click on it. (pasting with the keyboard doesn't work yet, will be fixed in the future).

### Bug Fixes

* Center dialogs on main window (issue #107).

### Refactors

* All of Pororoca entities are now records.
* PororocaRequestCommonTranslator as internal class.
* `TreatWarningsAsErrors` in most of the .csprojs.
* Run `dotnet format`.
* Centralized logic for dialogs.

### CI/CD

* Release checklist comment in PR to master.
* CI workflow when opening PR to develop.
* Unified the main CI/CD workflow.
* Added code coverage thresholds on CI/CD.

### Others

* Raised .NET SDK to 8.0.300.
* Added information about machine requirements for development, on [CONTRIBUTING.md]([CONTRIBUTING.md]).

## [3.3.0](https://github.com/alexandrehtrb/Pororoca/tree/3.3.0) (2024-04-29)

### Features

* Adds `.first()` and `.last()` functions to JSON response captures by [@cameronpyne-smith](https://github.com/cameronpyne-smith) (PR #90).

### Bug Fixes

* Number of repetitions to execute in sequential mode is now correctly displayed.

### Refactors

* App trimming to remove unused code and DLLs, reducing disk and memory usage:

| operating system | version | disk usage | start memory usage | end memory usage |
|---|---|---|---|---|
| Windows 7 x64 | Pororoca v3.3.0 | 49.7MB | 96MB | 250MB |
| Windows 7 x64 | Pororoca v3.2.0 | 70.2MB | 138MB | 259MB |
| Windows 7 x64 | Postman v10.24 | 526MB | 360MB | 787MB |
| Debian 12 KDE x64 | Pororoca v3.3.0 | 47.4MB | 171MB | 494MB |
| Debian 12 KDE x64 | Pororoca v3.2.0 | 67.5MB | 231MB | 497MB |
| Debian 12 KDE x64 | Postman v10.24 | 405.6MB | 430MB | 816MB |

The end memory usage was measured after importing several collections and sending many requests.

* Centralized classes and layouts for many tables.
* Removed useless grids and views, navigation is sleeker.

### CI/CD

* Releases are drafted when opening a PR to master.
* Code coverage report in PR to master.
* Pororoca.Test published to NuGet via GitHub Actions.


## [3.2.0](https://github.com/alexandrehtrb/Pororoca/tree/3.2.0) (2024-04-09)

### Features

* `.count()` function for JSON response capture.
* Scrollbars are now always visible on text editors (issue #91).
* Collection-scoped request headers. To set them, click on your collection in the left panel and then on the button *Set collection headers* (issue #92).

### Bug Fixes

* Improved collection-scoped auth when importing an OpenAPI / Swagger.

### Refactors

* Minor code refactors (switch cases, sealed modifiers).
* Optimized XML icons.

### Others

* Raised .NET SDK to version 8.0.204.

### New Contributors

* [@cameronpyne-smith](https://github.com/cameronpyne-smith) made his first contribution in PR [#87](https://github.com/alexandrehtrb/Pororoca/pull/87)

## [3.1.1](https://github.com/alexandrehtrb/Pororoca/tree/3.1.1) (2024-03-13)

### Features

* Prettify WebSocket JSON messages. (issue [#83](https://github.com/alexandrehtrb/Pororoca/issues/83))
* Improves Debian packaging to specify dependencies.

### Refactors

* JSON source generators for Postman collection, HTTP repeaters input data and GraphQL request body.

### Others

* Raised .NET SDK to 8.0.202.
* Raised Microsoft.OpenApi, xunit and coverlet dependencies versions.
* Added simplified chinese README.

### New Contributors

* [@francis-zhao](https://github.com/francis-zhao) made his first contribution in PR [#84](https://github.com/alexandrehtrb/Pororoca/pull/84)

## [3.1.0.1](https://github.com/alexandrehtrb/Pororoca/tree/3.1.0.1) (2024-02-19)

### Hot Fix

* Forgot to update version in Debian control file...
* This release is only for fixing Debian / Ubuntu packaging, the main version 3.1.0 remains. 3.1.0.1 is for tagging purposes.

## [3.1.0](https://github.com/alexandrehtrb/Pororoca/tree/3.1.0) (2024-02-14)

### Features

* Repeaters. You can now send many HTTP requests in a row from a single template. Read more about in our [docs](https://pororoca.io/docs/repeaters). (issue [#40](https://github.com/alexandrehtrb/Pororoca/issues/40))
* Response captures can now be saved into collection variables. (issue [#76](https://github.com/alexandrehtrb/Pororoca/issues/76))
* Environments can be disabled, leaving no active environment.

### Bug Fixes

* Better detection of SSL certificate problems for HTTP/3.

### Refactors

* CollectionViewModel and CollectionFolderViewModel now inherit from RequestsAndFoldersParentViewModel.

### CI/CD

* Upgraded `actions/setup-dotnet` and `actions/checkout` to v4.
* `actions/checkout` now uses fetch-depth of 1 (only most recent commit), faster cloning.
* Workflow for drafting releases.

### Others

* Editorconfig formatting for XML files.
* Raised .NET SDK to 8.0.200.
* Changed some italian and russian texts to be more succinct.

## [3.0.1.1](https://github.com/alexandrehtrb/Pororoca/tree/3.0.1) (2024-01-17)

### Bug Fix

* Fixes desktop shortcut for GNOME and start-up via Terminal (issues #73 and #74)
* This release is only for fixing Debian / Ubuntu packaging, the main version 3.0.1 remains. 3.0.1.1 is for tagging purposes.

## [3.0.1](https://github.com/alexandrehtrb/Pororoca/tree/3.0.1) (2024-01-09)

### Features

* Windows installers now support using a different user account for elevation (fetched from [Drizin/MultiUser](https://github.com/Drizin/NsisMultiUser)).
* Italian and russian translations for Windows installers.
* Rescaled icon for Mac OSX, looks better now.
* Improved image quality of icon and logos.
* Added installation size of Debian package.
* Reduced brightness and standardized style for tips question marks and texts.

### Others

* Raised .NET SDK to 8.0.101.
* Updated year to 2024 in licence and installer.
* Added italian README.

## [3.0.0](https://github.com/alexandrehtrb/Pororoca/tree/3.0.0) (2024-01-01)

### Breaking changes

* Shortcut for saving HTTP response body changed to <kbd>F9</kbd> key.
* User data folder for MacOSX moved to inside Application Support, no longer inside Applications folder.
* Test projects that use Pororoca.Test now require .NET 8 and enabled preview features in their .csproj files. Read more about it in the docs.
* `osx` releases work on Mac OSX computers with both x64 and ARM64 architectures (Apple Silicon).

### Features

* Improved visual interface and themes, with better distinction between primary and secondary actions and better colour contrast.
* Drag-and-drop on tables has been greatly improved with highlighting borders and row selection.
* Autocomplete for HTTP headers names and values.
* HTTP log export. You can now export a file that details exactly what was sent in a request and what was received in a response, when it began and how much time elapsed. Hotkey is <kbd>F10</kbd>.
* Multipart responses when all parts are text can now be seen in response body.
* Multipart response parts can be retrieved in `Pororoca.Test` tests.
* A welcome page shows up for new users.
* A *Go to docs* item has been added inside Help menu.
* Added request body MIME types for DNS+JSON, FHIR, SOAP, AVIF, CBOR, JSON-PATCH, JXL and SQL.
* Compatibility with Postman environment secret variables, for import and export.
* Packaging available for Debian / Ubuntu in `.deb` files.
* Security audit in CI/CD (`dotnet list package --vulnerable --include-transitive`).
* SBOMs are now included with the releases. A SBOM (software bill of materials) is a document that describes which components are used to make a software, in order to keep track and audit for vulnerabilities and licenses compliances.

### Bug Fixes

* HTTP/3 requests can now run on Windows Server 2022 machines (fixed detection of QUIC support on Windows).
* Fixed bug when pressing <kbd>Ctrl</kbd>+<kbd>PageUp</kbd> or <kbd>Ctrl</kbd>+<kbd>PageDown</kbd>.

### Refactors

* Collections and environments are now exported and imported using source generated JSON serializers, with better performance.
* Skips variable resolution if there are no effective variables, also for performance.
* Unified `FormatHttpVersion()` methods.
* Unified `GetTestFilePath()` methods in unit tests.
* Linux releases now are single-file published, meaning they have less files inside the folder.
* `FrozenSet` and `FrozenDictionary` used in many parts of the code, for faster speed.

### CI/CD

* Removed unnecessary setup of NSIS, as it comes pre-installed on GitHub Actions Windows runners.
* Windows portable releases are now generated in Linux runners.
* Upgraded `actions/upload-artifact` to v4.

### Others

* Pororoca.Test NuGet package now comes with a README.
* Fixed `rununittests.ps1` report to ignore source generated files.
* Added unit tests for collections and environments JSON serialization and deserialization.
* Raised .NET SDK to 8.0.100.
* Raised AvaloniaEdit to 11.0.5.
* Raised Microsoft.OpenApi to 1.6.11.

## [2.5.0](https://github.com/alexandrehtrb/Pororoca/tree/2.5.0) (2023-11-13)

### Features

* Adds italian language by [@alessiotm](https://github.com/alessiotm).
* Show WebSocket connection response HTTP status code and headers by [@tetropolix](https://github.com/tetropolix).
* Import OpenAPI / Swagger files.
* Re-run response captures (no need to send a request again).
* Drag-and-drop in tables - click, hold and drag the "`::`" at the left of the row to move it on the table.

### Bug Fixes

* Fixed crash of response capture when Content-Type was JSON, but the body wasn't JSON.
* Collection-scoped auth is now validated before requests.
* No more crash when copying text from help dialog (bug fixed in MessageBox).

### Refactors

* Variable resolution now uses regex and cached variables, which should improve performance and lower memory consumption, especially if you have a lot of collection and environment variables.
* XML body response captures now caches XmlDocument and XmlNamespaceManager, saving memory.
* Replaced Moq for NSubstitute in unit tests.

### Others

* Raised .NET SDK version to 7.0.403.
* Raised Avalonia version to 11.0.5.
* Raised MessageBox.Avalonia version to 3.1.5.1.
* Removed `global.json`.
* UI tests keybinding changed to Ctrl+F12 and menu item moved to inside Help.

### New Contributors

* [@alessiotm](https://github.com/alessiotm) made his first contribution in PR [#54](https://github.com/alexandrehtrb/Pororoca/pull/54)
* [@tetropolix](https://github.com/tetropolix) made his first contribution in PR [#55](https://github.com/alexandrehtrb/Pororoca/pull/55)


## [2.4.0](https://github.com/alexandrehtrb/Pororoca/tree/2.4.0) (2023-10-15)

### Important!

* If you are using Pororoca Desktop on Linux, please update msquic to version 2.2.2+ to make HTTP/3 requests.
* Our license has been [updated](https://github.com/alexandrehtrb/Pororoca/commit/b9e08ec356e929bcd978a29e2d1aaa0311ba7fc6).
* We also added a data protection policy in our README.

### Features

* You can now capture response values into variables!
* Collection-scoped authentication, including when importing from Postman.
* Windows authentication (NTLM / Kerberos).
* XML response bodies are now indented and prettified.
* Client certificate auth fields now become red when there is validation problem with them.
* UI now has better focus and distinction between primary and secondary actions.
* Tooltips now appear in some buttons, such as add request headers, add variables and add WebSocket client message.

### Refactors

* Variable resolution logic centralised into IPororocaVariableResolver.
* Logic for switching panels with ComboBoxes has been refactored to use Avalonia custom converters.

### Others

* Raised .NET SDK version to 7.0.402.

## [2.3.1](https://github.com/alexandrehtrb/Pororoca/tree/2.3.1) (2023-09-28)

### HotFix

* Disables cut, copy, paste and delete keybindings in tables (headers, variables, URL encoded and Form Data params) due to conflict with cells' texts actions. Those operations are still available by using the mouse right-click.

## [2.3.0](https://github.com/alexandrehtrb/Pororoca/tree/2.3.0) (2023-09-26)

### Important!

If you are using Pororoca Desktop on Linux, please use msquic version 2.1.8 to make HTTP/3 requests.

### Features

* Cut, copy, paste, delete actions in all tables: variables, headers, URL encoded and Form Data params.
* Hyperlinks are now coloured in text editors and ctrl + clicking on them opens the browser or email.
* The suggested file name when saving a response body now includes the names of the request and the environment used.
* Version name visible in Help menu item.
* Keyboard shortcuts added! They are:
  * Ctrl+Shift+S - Save all
  * Ctrl+PageUp - Switch to item above in tree
  * Ctrl+PageDown - Switch to item below in tree

### Bug Fixes

* Fixed bug of not being able to copy WebSocket client messages in tree.
* Improved detection of textual Content-Types, so they can be viewed in response body text editor. MIME types like `application/vnd.oracle.adf.batch+json`, `application/vnd.google-earth.kml+xml` and `image/svg+xml` are now considered text.

### Refactors

* Transformed PororocaHttpRequestFormDataParam into record.

### Tests

* Added UI tests! To enable them, the compilation must be on DEBUG or have the preprocessor flag UI_TESTS_VERSION. To run them, copy the TestFiles folder into your PororocaUserData and have the TestServer running along.

### Others

* Added global.json to fixate .NET SDK version (7.0.401 caused problems with HTTP/3 on Linux).

## [2.2.0](https://github.com/alexandrehtrb/Pororoca/tree/2.2.0) (2023-08-07)

### Features

* Visual themes are now available: light, dark, pampa and amazonian night.
* Default font now is Cabin.
* Adds russian language by [@RVShershnev](https://github.com/RVShershnev).
* Red border and background on inputs when there is an error related to them.
* A dialog now appears to confirm if you really want to delete an item.
* Keyboard shortcuts added! They are:
  * F1 - Show help
  * F2 - Rename
  * F4 - Focus on URL
  * F5 - Send request / connect WebSocket
  * F6 - Cancel request / disconnect WebSocket
  * F7 - Set previous environment as active
  * F8 - Set next environment as active
  * Ctrl+S - Save response body to file
  * Alt+Up - Move item up in tree
  * Alt+Down - Move item down in tree
  * Ctrl+X - Cut items
  * Ctrl+C - Copy items
  * Ctrl+V - Paste items
  * Ctrl+D - Duplicate collection
  * Delete - Delete items

### Bug Fixes

* Previously, response headers were not being updated if the response body tab was the one selected. Now, they are always updated.
* Removed duplicated `application/problem+json` MIME type.

### Refactors

* Organized visual styles in separate files, allowing for easier theming.
* Icons are now vectorized.
* Internationalization texts are now provided by a source generator.
* Compiled bindings are now the default in all visual controls.
* JSON strings are now detected using Utf8JsonReader from System.Text.Json, in a faster way.
* Safer mechanism of saving user collections on app shutdown.
* Some Pororoca domain classes were refactored into records.
* Code for copying, pasting and adding items is simpler now.

### Others

* Raised Avalonia version to 11.0.2.
* Raised .NET SDK version to 7.0.306.
* Created GitHub Actions pipeline for generation of Pororoca Desktop and Pororoca.Test releases.
* Drag and drop on tables was removed due to conflict with text inputs. This feature hopefully will be back soon.

### New Contributors

* [@RVShershnev](https://github.com/RVShershnev) made his first contributions in PRs [#24](https://github.com/alexandrehtrb/Pororoca/pull/24) and [#25](https://github.com/alexandrehtrb/Pororoca/pull/25)

## [2.1.0](https://github.com/alexandrehtrb/Pororoca/tree/2.1.0) (2023-04-24)

### Features

* Great improvement on UI/UX for headers, URL encoded and form data params, and websocket subprotocols. Their grids now support drag-and-drop and the item removal actions now have a button on each row.
* Adds MIME types `application/dns-json`, `application/dns-message` and `application/problem+xml` (issue [#19](https://github.com/alexandrehtrb/Pororoca/issues/19)).
* Protects against rare scenario that response body is text, but not in UTF-8 encoding.

### Bug Fixes

* On Linux, requests with client certificate authentication will have independent SSL sessions of others to the same destination host.

### Refactoring

* Now using `[Reactive]` from ReactiveUI.Fody attributes on ViewModel properties.
* Reformatted XML views.
* Reformatted C# code.

### Others

* Raised .NET SDK to 7.0.203.

## [2.0.1](https://github.com/alexandrehtrb/Pororoca/tree/2.0.1) (2023-02-26)

### Minor features

* Now shows label with the name of selected environment, next to environments group.
* SendMessageAsync method in Pororoca.Test no longer requires external waiting with Task.Delay, easier to use now.

### Bug Fixes

* Content-Type `application/problem+json` now shows as text in response body
* Better detection of Pororoca json files when importing collections and environments

### Refactoring

* Removed ids from requests, folders and WebSocket client messages, since they were unused.
* Removed documentation from this repo. Now available on its own site.

## [2.0.0](https://github.com/alexandrehtrb/Pororoca/tree/2.0.0) (2022-12-20)

### Breaking Changes

* If you are using Linux and want to make HTTP/3 requests, the [msquic](https://github.com/microsoft/msquic) version 2.x.y needs to be installed. This is because .NET 7 uses msquic v2.

### Features

* Adds support for WebSockets over HTTP/2
* `osx-arm64` release is back
* Adds CONNECT to list of HTTP methods
* Any file with the .json extension can now be accepted in import collection / environment dialogs
* WebSocket JSON messages are now exported to files with .json extension by default

### Bug Fixes

* Fixed problem that HTTP/3 requests could not be completed (issue [#13](https://github.com/alexandrehtrb/Pororoca/issues/13))
* Many improvements for better compatibility with Postman collections and environments.
* When using a variable in a request body file path, the file path is now verified and correctly resolved. Before, it was always rejected, for both Form Data and File bodies.

### Others

* Raised .NET SDK to 7.0.101
* Raised Avalonia version to 11.0.0-preview4

## [1.6.0](https://github.com/alexandrehtrb/Pororoca/tree/1.6.0) (2022-11-20)

### Breaking Changes

* Currently dropping support for `arm` and `arm64` releases of Pororoca Desktop. The version 1.5.0 will remain available for download and it supports `arm` architectures. The `Pororoca.Test` package does not have such restraint and can run on these machines.

### Features

* Adds support for WebSockets (only HTTP/1.1 for now!)
* Adds syntax highlighting for text editors!
  * Thanks for the [Avalonia](https://github.com/AvaloniaUI) team for the [Avalonia.Edit](https://github.com/AvaloniaUI/AvaloniaEdit) project!
  * This adds support for `Ctrl+F` search in text editors! Issue [#10](https://github.com/alexandrehtrb/Pororoca/issues/10#issue-1369086969)
  * This allows for zooming the text editor with the mouse scroll wheel!
* Generates Windows installer releases for Pororoca!
  * Thanks for [@Drizin](https://github.com/Drizin) for the [NsisMultiUser](https://github.com/Drizin/NsisMultiUser) project!
  * Pororoca installed on Windows will save collections and user preferences on `AppData\Roaming\Pororoca` folder.
* Files for HTTP request bodies are now loaded using async operating system APIs
* Shows check for updates reminder dialog, once every two months.

### Bug Fixes

* When a request, folder, environment is created, the main screen now is switched to it.
* The HTTP request screen splitter resizing is no longer bugged (thanks to the new text editor!)
* When clicking the `Send` button in HTTP request, the screen does not freeze anymore.

### Others

* Adds VS Code tasks for debugging Pororoca Desktop and TestServer.
* Sets PororocaUserData folder at project root when debugging, avoiding crash.
* Raised .NET SDK version to 6.0.403.
  * `global.json` file is no longer necessary for running tests on Windows 7.
* Raised Avalonia version to 0.10.18.
* Simplified docs and replaced .jpg images for .png.

## [1.5.0](https://github.com/alexandrehtrb/Pororoca/tree/1.5.0) (2022-09-20)

### Features

* Adds support for HTTP/2 on MacOSX

## [1.4.0](https://github.com/alexandrehtrb/Pororoca/tree/1.4.0) (2022-07-03)

### Features

* Supports HTTP response trailers (only available for HTTP/2 and HTTP/3)
* Shows button in response area to disable TLS verification
* Switches selected response tab to "Body" when exception occurs

### Others

* Improved .editorconfig and reformatted the entire code
* Moved TestServer to tests folder
* Renamed docs home pages to README.md
* Detailed on documentation how to select exportation file format
* Dynamically adds global.json for unit tests on Windows 7
* Raised .NET SDK used for compilation to version 6.0.301
* Unified Pororoca Desktop releases for Windows 7 and newer Windows versions

## [1.3.0](https://github.com/alexandrehtrb/Pororoca/tree/1.3.0) (2022-05-15)

### Features

* Expands collections tree after adding an environment, folder or request
* Now allows for copy, paste and deletion of multiple items in collections tree
* Applies rename when pressing Enter in text input

### Bug Fixes

* When pasting a copy of a current environment, the copy will no longer also be a current environment.

### Others

* Refactored makereleases.ps1 into smaller functions
* Generates Pororoca.Test package along in makereleases.ps1

## [1.2.0](https://github.com/alexandrehtrb/Pororoca/tree/1.2.0) (2022-04-10)

### Features

* Adds support for client certificate authentication
* Adds check mark next to selected language, changed check mark margins

### Bug Fixes

* Fixes possible bug when cloning some request bodies

### Others

* Organizes request validation i18n strings
* Adds .gitattributes to avoid line-break problems in Linux

## [1.1.0](https://github.com/alexandrehtrb/Pororoca/tree/1.1.0) (2022-03-20)

### Features

* Adds support for GraphQL requests
* Creates Pororoca.Test package for running Pororoca requests in tests
* Layout improvements:
  * Removes response content-type textbox
  * Decreases font size in request and response raw contents
  * Blocks horizontal scrollbar in collections' tree view
  * Decreases other font sizes and some margins

### Bug Fixes

* Now always exporting environments as non-current environments
* Now detects correctly Windows 11 to check for HTTP/3 support

### Improvements

* Restricting Postman model classes visibilities
* When exporting in Postman formats, now using their date-time format and other details
* Changed exporting when saving user collections and preferences

### Others

* Added information on how to donate to this project

## [1.0.0](https://github.com/alexandrehtrb/Pororoca/tree/1.0.0) (2022-03-08)

First release!
