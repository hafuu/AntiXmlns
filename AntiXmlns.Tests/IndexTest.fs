[<NUnit.Framework.TestFixture>]
module IndexTest

open NUnit.Framework
open FsUnit

open System.Xml.Linq
open System.Xml.XPath

open AntiXmlns.Index

let xml() = XElement.Parse("""<root>
  <elem1></elem1>
  <elem2><elem3 /><elem3 attr1="hoge" attr2="piyo" /></elem2>
</root>""")

[<Test>]
let ``要素の位置を計算できる``() =
  let node = xml().XPathSelectElement("//elem3[2]")
  let actual = path [] node
  actual |> should equal [ Element 1; Element 1 ]

[<Test>]
let ``属性の位置を計算できる``() =
  let name = XName.Get("attr1")
  let attr = xml().XPathSelectElement("//elem3[2]").Attribute(name)
  let actual = path [] attr
  actual |> should equal [ Element 1; Element 1; Attribute name ]

[<Test>]
let ``pathを辿って要素を取得できる``() =
  let pathes = [ Element 1; Element 1 ]
  let actual = follow (xml()) pathes |> unbox<XElement>
  actual.Name.LocalName |> should equal "elem3"

[<Test>]
let ``pathを辿って属性を取得できる``() =
  let pathes = [ Element 1; Element 1; Attribute (XName.Get("attr1")) ]
  let actual = follow (xml()) pathes |> unbox<XAttribute>
  actual.Name.LocalName |> should equal "attr1"