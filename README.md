AntiXmlns
=========

AntiXmlnsは、XMLの名前空間を無視してXMLを操作できるライブラリです。

名前空間をもつXMLを操作する場合、目的の要素へのアクセス手順が煩雑になります。
例えば、XPthで要素にアクセスする場合は、次のようなコードになります。
```fsharp
let xml = XElement.Parse("""<root xmlns="namespace"><foo>text</foo></root>""")
let manager = XmlNamespaceManager(NameTable())
manager.AddNamespace("ns", "namespace")
let foo: XElement = xml.XPathSelectElement("/ns:foo", manager)
```

しかし、コード中で名前空間を厳密に扱いたい場合はあまり多くありません。
特にテストにおいては、XPathを、なるべく名前空間を使わず、シンプルに書きたくなります。
このようにxmlnsを削除するコードを書いたことがありませんか？
```fsharp
let src = """<root xmlns="namespace"><foo>text</foo></root>"""
let xml = XElement.Parse(Regex.Replace(src, "xmlns=\"[^\"]+\"", ""))
```
置換する方法は確かに問題を解決できますが、置換を忘れたり、名前空間つきのXMLと相互に扱えない、
という問題が新たに生まれます。
また、入力となるXML文字列を編集するのは、負けた気分でもあります。

AntiXmlnsでは、次のように、名前空間を指定せずに要素にアクセスが可能となります。
```fsharp
let xml = XElement.Parse("""<root xmlns="namespace"><foo>text</foo></root>""")
let foo: XElement = AntiXmlns.xpathElement "/foo" xml |> Seq.head
```

XMLの操作には名前空間を使用しませんが、入出力となるXMLは名前空間の有無を問いません。
入力XMLに名前空間が含まれていた場合、出力XMLにも名前空間が含まれます。

注意
====

このライブラリはテストのためのライブラリです。プロダクトコードには使用しないでください。
