# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-14

**BREAKING**:
- Package renamed from `com.gamelovers.dataextensions` to `com.gamelovers.gamedata`

**New**:
- Merged `com.gamelovers.configsprovider` into this package
- Added `ConfigsProvider` for type-safe config storage and versioning
- Added `ConfigsSerializer` for JSON serialization with backend sync support
- Added `ConfigsScriptableObject` for ScriptableObject-based config containers
- Added `IConfigsProvider`, `IConfigsAdder`, `IConfigsContainer` interfaces
- Added Newtonsoft.Json dependency for serialization
- Added Unity 6 UI Toolkit support for `ReadOnlyPropertyDrawer` and `EnumSelectorPropertyDrawer`
- Added `Runtime/link.xml` to prevent code stripping of core serialization logic
- Added Newtonsoft JSON converters for `Color`, `Vector2`, `Vector3`, `Vector4`, and `Quaternion` to `ConfigsSerializer`

**Changed**:
- **BREAKING**: Package renamed from `com.gamelovers.dataextensions` to `com.gamelovers.gamedata`
- Improved IL2CPP/AOT safety for `SerializableType<T>` with better type resolution
- Optimized `EnumSelector` with static dictionary caching and O(1) lookups

**Fix**:
- Fixed `EnumSelector.SetSelection` to correctly handle enums with explicit/non-contiguous values
- Added graceful duplicate key handling and logging to `ConfigsScriptableObject`

---

## [0.7.0] - 2025-11-03

**New**:
- Added *Rebind* functionality to all Observable classes (*ObservableField*, *ObservableList*, *ObservableDictionary*) allowing rebinding to new data sources without losing existing observers
- Added *Rebind* methods to all Observable Resolver classes (*ObservableResolverField*, *ObservableResolverList*, *ObservableResolverDictionary*) to rebind to new origin collections and resolver functions
- Added new *IObservableResolverField* interface with *Rebind* method for resolver field implementations

## [0.6.7] - 2025-04-07

**New**:
- Added the *UnityObjectExtensions* to help add extra logic to Unity's *GameObject* type objects

## [0.6.6] - 2024-11-30

**Fix**:
- *ObservableDictionary.Remove(T)* no longer sends an update if id doesn't find the element to remove it

## [0.6.5] - 2024-11-20

**Fix**:
- Fixed the issues of *ObservableDictionary* when subscribing/unsubscribing to actions while removing/adding elements
- Fixed the issues of *ObservableList* when subscribing/unsubscribing to actions while removing/adding elements

## [0.6.4] - 2024-11-13

**Fix**:
- Fixed the unit tests for *ObservableDictionary* that was preventing some builds to run

## [0.6.3] - 2024-11-02

**Fix**:
- Fixed the compilation issues of *ObservableDictionary*

## [0.6.2] - 2024-11-02

**New**:
- Added the *ObservableUpdateFlag* to help performance when updating subscribers to the *ObservableDictionary*. By default is set *ObservableUpdateFlag.KeyUpdateOnly*

**Fix**:
- Fixed an issue that would no setup Remove update action to Subscribers when calling *Clear* on the *ObservableDictionary*

## [0.6.1] - 2024-11-01

**Fix**:
- Fixed an issue that would crash the execution when calling *Remove()* & *RemoveOrigin* from *ObservableResolverDictionary*

## [0.6.0] - 2023-08-05

**Changed**:
- Improved the *ObservableResolverList* and *ObservableResolverDictionary* data types to properly resolve lists and dictionaries with different data types from the original collection.

## [0.5.1] - 2023-09-04

**New**:
- Added StructPair data type to support both object and struct type containers, improving memory usage performance.

**Fix**:
- Fixed the dispose extension methods for GameObject and Object, removing pragma directives and adding null reference check in GetValid method to avoid unwanted exceptions

## [0.5.0] - 2023-08-05

**New**:
- Added **floatP**, a deterministic floating-point number type, enhancing precision and predictability in mathematical operations. Including arithmetic and comparison operators for floatP to support complex calculations and conversion methods between floatP and float types.

## [0.4.0] - 2023-07-30

**New**:
- Added utility methods and extensions for Unity's Object and GameObject types, enhancing the codebase's functionality.
- Introduced a SerializableType struct for viewing, modifying, and saving types from the inspector, with serialization support and compatibility with filter attributes.

## [0.3.0] - 2023-07-28

**New**:
- Added support for observing field updates with previous and current values in the ObservableField class.
- Introduced a UnitySerializedDictionary class that allows serialization of dictionaries in Unity.

## [0.2.0] - 2020-09-28

**New**:
- Added new *ObservableResolverList*, *ObservableResolverDictionary* & *ObservableResolverField* to allow to create observable types without referencing the collection directly
- Added Unit tests to all data types in the project

**Changed**:
- Removed *ObservableIdList* because it's behaviour was too confusing and the same result can be obtained with *ObservableList* or *ObservableDictionary*
- Removed all Pair Data and moved them to new *Pair<Key,Value>* serialized type that can now be serializable on Unity 2020.1
- Moved all Vector2, Vector3 & Vector4 extensions to the ValueData file

## [0.1.1] - 2020-08-31

**Changed**:
- Renamed Assembly Definitions to match this package
- Removed unnecessary files

## [0.1.0] - 2020-08-31

- Initial submission for package distribution

---

## ConfigsProvider History (merged from com.gamelovers.configsprovider)

## [0.2.1] - 2025-11-02

**New**:
- Added the *TryGetConfig* for requesting singleton configs from the *ConfigProvider*.

## [0.2.0] - 2025-09-24

**New**:
- Added the *ConfigsScriptableObject* to contain a Dictionary map collection in a ScriptableObject config.

## [0.1.2] - 2024-11-13

**Changed**:
- Removed the struct constraints from *IConfigsProvider*

## [0.1.1] - 2023-09-03

**Fix**:
- Replace bad struct objects to classes
- Add missing documentation

## [0.1.0] - 2023-08-03

- Initial submission for package distribution
