<div align="center">
<h1><img src="https://github.com/NedoProgrammer/ScratchScript/blob/master/Images/ScratchLogo.png?raw=true" width=50 height=50>ScratchScript<img src="https://github.com/NedoProgrammer/ScratchScript/blob/master/Images/CSharp.png?raw=true" width=50 height=50></h1><p>A programming language that compiles to Scratch!</p>
<img src="https://img.shields.io/github/languages/top/NedoProgrammer/ScratchScript"><img src="https://img.shields.io/github/languages/code-size/NedoProgrammer/ScratchScript"><img src="https://img.shields.io/github/license/NedoProgrammer/ScratchScript">
</div>

## ..why?

Purely for fun! :D

## ..how?

ANTLR is used for parsing the language. Several wrappers are created to make it easier to translate code to Scratch's
JSON format.

## ..what does it have?

- Diagnostics!

```csharp
Warning: Division by zero is not recommended.
 --> test.scrs:1:13
1 | var a = 1 / 0;
                ~

For more information, try `ScratchScript explain W1`.
Error: Cannot assign a value of type "Single" to a variable of type "Int32".
 --> test.scrs:3:3
3 | a = b;
      ~

For more information, try `ScratchScript explain E8`.
```

- Imports!
- Easy syntax!

```js
function double(x)
{
	return x * 2;
}
var four = double(2);
say(four);
```

## how do i install it?

I am planning to add precompiled executables with Github Actions, but for now you will have to compile it from source.

**You need .NET 7.0 to compile the project.**

The C# compiler for ScratchScript does not depend on your operating system, but you will need `git` and the `dotnet`
toolchain.

- `git clone https://github.com/NedoProgrammer/ScratchScript`
- `cd ScratchScript && dotnet build --configuration Release`

The toolchain will be located in `bin/net7.0/Release`.

## current progress

- [x] Variable declarations
- [x] Variable assignments
- [x] Operators
-
    - [x] +, -, *, /, %
-
    - [ ] **
-
    - [x] ==, >, <
-
    - [ ] > =, <=
-
    - [x] &&, ||, <del>^</del>
- [x] If statements
- [ ] While statements
- [ ] Function declarations
- [ ] Function calls
- [ ] Imports
