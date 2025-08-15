# Changelog

## [1.0.1] - 2025-08-15

### Fixed

- Removed catching and logging exceptions from the property drawer. It was never necessary, and Unity throws exceptions for GUI control flow.
- Fixed nested arrays and lists accumulating too much indentation.
- Fixed label width to always truncate correctly. No more magic number.
- Truncate a copy of the the original label object to not affect the original.
- Made the GetScriptableObjectReference method protected so it can be used from derived drawers for deciding whether to render the base drawer when only a derived type should show properties.

## [1.0.0] - 2025-08-08

### Added

- Initial release.
- Enables direct access to the properties of ScriptableObject references in the editor.
