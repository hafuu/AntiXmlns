[<NUnit.Framework.TestFixture>]
module AntiXmlnsTest

open NUnit.Framework
open FsUnit

open System.Xml.Linq
open System.Xml.XPath

open AntiXmlns


[<Test>]
let ``xmlからxmlnsを削除できる``() =
  let xml = XElement.Parse("""<root xmlns="namespace">
  <elem1 attr1="val" attr2="val">textnode</elem1>
  <elem2>
    <elem3 />
  </elem2>
</root>""")
  let expected = """<root>
  <elem1 attr1="val" attr2="val">textnode</elem1>
  <elem2>
    <elem3 />
  </elem2>
</root>"""
  let actual = (removeNamespace xml).ToString()
  actual |> should equal expected

[<Test>]
let ``rootからのindexのpathを計算できる``() =
  let xml = XElement.Parse("""<root>
  <elem1></elem1>
  <elem2><elem3 /></elem2>
</root>""")
  let node = xml.XPathSelectElement("//elem3")
  let actual = indexPath node
  actual |> should equal [ 1; 0 ]

[<Test>]
let ``pathをたどる``() =
  let xml = XElement.Parse("""<root>
  <elem1></elem1>
  <elem2><elem3 /></elem2>
</root>""")
  let node = xml.XPathSelectElement("//elem3")
  let path = indexPath node
  let actual = follow xml path
  actual.Name.LocalName |> should equal "elem3"

[<TestCase("/elem1", "elem1")>]
[<TestCase("/elem2", "elem2")>]
[<TestCase("/elem2/elem3", "elem3")>]
let ``XElementに対してxmlnsを無視してxpathで検索できる`` x expected =
  let xml = XElement.Parse("""<root xmlns="namespace">
  <elem1>aaaaaaa</elem1>
  <elem2><elem3 /></elem2>
</root>""")
  let actual = xpathElement x xml |> Seq.map (fun x -> x.Name.LocalName) |> Seq.toList
  actual |> should equal [ expected ]

[<TestCase("/root/elem1", "elem1")>]
[<TestCase("/root/elem2", "elem2")>]
[<TestCase("/root/elem2/elem3", "elem3")>]
let ``XDocumentに対してxmlnsを無視してxpathで検索できる`` x expected =
  let doc = XDocument.Parse("""<root xmlns="namespace">
  <elem1>aaaaaaa</elem1>
  <elem2><elem3 /></elem2>
</root>""")
  let actual = xpathElement x doc |> Seq.map (fun x -> x.Name.LocalName) |> Seq.toList
  actual |> should equal [ expected ]