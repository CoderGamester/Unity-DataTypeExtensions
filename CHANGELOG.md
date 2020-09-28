# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2020-09-28

- Removed *ObservableIdList* because it's behaviour was too confusing and the same result can be obtained with *ObservableList* or *ObservableDictionary*
- Added new *ObservableResolverList*, *ObservableResolverDictionary* & *ObservableResolverField* to allow to create observable types without referencing the collection directly
- Added Unit tests to all types

**Changed**:
- Removed all Pair Data and moved them to new *Pair<Key,Value>* serialized type that can now be serializable on Unity 2020.1
- Moved all Vector2, Vector3 & Vector4 extensions to the ValueData file

## [0.1.1] - 2020-08-31

- Renamed Assembly Definitions to match this package
- Removed unnecessary files

## [0.1.0] - 2020-08-31

- Initial submission for package distribution
