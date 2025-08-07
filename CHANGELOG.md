# Changelog

## [1.0.5] - 2025-08-07
### Complexity optimization of IncreaseCapacity() and Swap() and Compact() Methods
* Improved space/time complexity of IncreaseCapacity, Swap, and Compact
* Added additional tests

## [1.0.4] - 2025-08-05
### Runtime Resizable (IncreaseCapacity) and Swap and Compact Methods
* Added IncreaseCapacity method to increase IInventory capacity at runtime
* Added Swap method to swap to inventory slots in inventory by index
* Added Compact method to sort the inventory in a compact manner
* Added docstrings for new methods
* Added tests for new methods
* Rebuild docs (docfx)

## [1.0.3] - 2025-08-03
### Factories
* Added factories for Inventory and EquipmentInventory
* License fix

## [1.0.2] - 2025-08-03
### Armor/Weapon Removal
* Removed armor and weapons from the equipment system
* Style refactors
* Directory structure refactors
* Changes in IInventory events
* Made Inventory and EquipmentInventory internal (should only expose the interface)
* Reordered and renamed ItemData fields
* General refactor
* Added additional docstrings
* Added changelog, license, and documentation url

## [1.0.1] - 2025-08-01
### GitHub Pages Release
- Added Documentation
- Deployed Documentation on GitHub Pages

## [1.0.0] - 2025-07-31
### First Release
- Includes IInventory, IEquipmentInventory and IItem
- Includes runtime test suite