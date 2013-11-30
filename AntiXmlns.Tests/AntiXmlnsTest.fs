[<NUnit.Framework.TestFixture>]
module AntiXmlnsTest

open NUnit.Framework
open FsUnit

open System.Xml.Linq
open System.Xml.XPath

open AntiXmlns

let doc() = XDocument.Parse("""<root xmlns="namespace">
  <elem1 attr1="hoge">aaaaa</elem1>
  <elem2 attr2="piyo">
    <elem3>bbbbb</elem3>
  </elem2>
  <elem4 />
</root>""")

[<Test>]
let ``xmlからxmlnsを削除できる``() =
  let expected = """<root>
  <elem1 attr1="hoge">aaaaa</elem1>
  <elem2 attr2="piyo">
    <elem3>bbbbb</elem3>
  </elem2>
  <elem4 />
</root>"""
  let actual = (removeNamespace (doc().Root)).ToString()
  actual |> should equal expected

[<Test>]
let ``XDocumetに対してXPathを使用できる``() =
  let actual = xpathElement "/root/elem1" (doc()) |> Seq.map (fun x -> x.Name.LocalName) |> Seq.toList
  actual |> should equal [ "elem1" ]

[<Test>]
let ``XElementに対してXPathを使用できる``() =
  let actual = xpathElement "/elem1" (doc().Root) |> Seq.map (fun x -> x.Name.LocalName) |> Seq.toList
  actual |> should equal [ "elem1" ]

[<TestCase("/root/elem1", [| "elem1" |])>]
[<TestCase("/root/elem2", [| "elem2" |])>]
[<TestCase("/root/elem2/elem3", [| "elem3" |])>]
let ``要素を取得できる`` xpath (expected: string[]) =
  let actual = xpathElement xpath (doc()) |> Seq.map (fun x -> x.Name.LocalName) |> Seq.toList
  actual |> should equal (List.ofSeq expected)

[<TestCase("/root/elem1/@attr1", [| "hoge" |])>]
[<TestCase("/root/elem2/@attr2", [| "piyo" |])>]
[<TestCase("/root/elem2/@null", ([||]: string[]))>]
[<TestCase("/root/null/@attr1", ([||]: string[]))>]
let ``属性を取得できる`` xpath (expected: string[]) =
  let actual = xpathAttribute xpath (doc()) |> Seq.map (fun x -> x.Value) |> Seq.toList
  actual |> should equal (List.ofSeq expected)

[<TestCase("/root/elem1/text()", [| "aaaaa" |])>]
[<TestCase("/root/elem2/elem3/text()", [| "bbbbb" |])>]
[<TestCase("/root/elem4/text()", ([||]: string[]))>]
[<TestCase("/root/null/text()", ([||]: string[]))>]
let ``Text要素を取得できる`` xpath (expected: string[]) =
  let actual = xpathText xpath (doc()) |> Seq.map (fun x -> x.Value) |> Seq.toList
  actual |> should equal (List.ofSeq expected)