# Changelog

* [1.5.0](#150-2022-09-20)
* [1.4.0](#140-2022-07-03)
* [1.3.0](#130-2022-05-15)
* [1.2.0](#120-2022-04-10)
* [1.1.0](#110-2022-03-20)
* [1.0.0](#100-2022-03-08)

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
