# iCircuit Language Translations

[iCircuit](http://icircuitapp.com) is used all around the world in a variety
of languages.

I would like to ask for your help translating it to
as many of those languages as possible.

If you spot an error in a translation, please open a pull request with the correction - you will be helping many people and I'll be sure to thank you in the app!

Please also feel free to start a new language translation.

Thank you very much for your help!

<a rel="license" href="http://creativecommons.org/licenses/by/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by/4.0/88x31.png" /></a>



## Supported Languages

| Language | Prefix | Link |
|--|--|--|
|English|Base|[Strings](Base.lproj/Localizable.strings)|
|Japanese|js|[Strings](ja.lproj/Localizable.strings)|
|German|de|[Strings](de.lproj/Localizable.strings)|
|Chinese (Simplified)|zh-Hans|[Strings](zh-Hans.lproj/Localizable.strings)|
|Chinese (Traditional)|zh-Hant|[Strings](zh-Hant.lproj/Localizable.strings)|
|Spanish|es|[Strings](es.lproj/Localizable.strings)|
|Italian|it|[Strings](it.lproj/Localizable.strings)|
|Russian|ru|[Strings](ru.lproj/Localizable.strings)|
|French|fr|[Strings](fr.lproj/Localizable.strings)|

(Sorted by app usage.)



## Style Guide

### Keep things short

Most of the text is presented in dialog boxes on phones so keeping
translations short is preferable to being verbose.

Acronymns, initialisms, and common abbreviations are all welcome so long
as they would be recognized by beginners in electronics.

### Prefer spaces over hyphenations

If compound words can either be hyphenated or combined with space,
prefer space. This is to help the rendering engine layout text properly.

### Use Title Casing

Most of the words are presented at title of things that can be modified.
In English, this means most words get capitalized. Please use the right
form of titling for the language.



### Weird Words

* `AM` is short for "Amplitude Modulation"
* `Autowire` is short for "keep components connected during movement by automatically re-routing wires"
* `Current` is electrical current



## Machine Auto-translation

The script `mtranslate.fsx` is used to automatically translate missing
words and was used to build the initial translations.

It can be run with the following command:

```bash
fsharpi --lib:$HOME/.nuget/packages/newtonsoft.json/11.0.2/lib/net45 --exec mtranslate.fsx
```
