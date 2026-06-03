# Yaseri

Yet another serializer interface

## Why?

This aims to create an interface that is very much like .NET's `Utf8JsonReader`/`Utf8JsonWriter`,
while fixing some of the issues faced when with using it in a game engine.

- Efficient Utf8JsonReader like interface.
- Fully AOT compliant, no reflection.
- Interface allows switching between formats.
- Implement serializer for external types with duck typing, or implement `IPrimitive<T>` for first class support.
- Source generator to automatically implement `IPrimitive<T>` for your types.
	- Decorate a setter only property with `[Obsolete]` to provide backwards compatibility.

## What is Binson?

Binson is a JSON inspired format that has a 1 to 1 mapping of json constructs in binary form, with no additional complexities such as backreferences.


