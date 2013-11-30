module AntiXmlns

open System
open System.Xml.XPath
open System.Xml.Linq

module internal Index =
  open System.Xml.Linq

  type ObjectType =
    | Element of int
    | Text
    | Attribute of XName

  let rec path indexies (x: XObject) =
    match x with
    | :? XElement as elem ->
      match elem.Parent with
      | null -> indexies
      | x ->
        let index = elem.ElementsBeforeSelf() |> Seq.length
        path (Element index :: indexies) x
    | :? XAttribute as attr ->
      path (Attribute attr.Name :: indexies) attr.Parent
    | :? XText as text -> path (Text:: indexies) text.Parent
    | _ -> failwith "unknown XNode object"

  let rec follow (elem: XElement) = function
    | [] -> elem :> XObject
    | Element index :: path ->
      let next = elem.Elements() |> Seq.nth index
      follow next path
    | Attribute name :: _ ->
      elem.Attribute(name) :> XObject
    | Text :: _ -> elem.Nodes() |> Seq.find (function :? XText -> true | _ -> false) :> XObject

/// XMLから名前区間を削除します。
let rec removeNamespace (dirtyXml: XElement) =
  let xml = XElement(XName.Get(dirtyXml.Name.LocalName))
  let attrs =
    dirtyXml.Attributes()
    |> Seq.filter (fun a -> a.Name.LocalName <> "xmlns")
    |> Seq.map (fun a -> XAttribute(XName.Get(a.Name.LocalName), a.Value) |> box)
    |> Seq.toArray
  xml.Add(attrs)

  let nodes =
    dirtyXml.Nodes()
    |> Seq.map (function
      | :? XElement as x -> removeNamespace x |> box
      | :? XDocumentType as x -> XDocumentType(x) |> box
      | :? XProcessingInstruction as x -> XProcessingInstruction(x) |> box
      | :? XCData as x -> XCData(x) |> box
      | :? XText as x -> XText(x) |> box
      | :? XComment as x -> XComment(x) |> box
      | _ -> failwith "unknown XNode object"
    )
    |> Seq.toArray
  xml.Add(nodes)
  xml

/// XPath式を評価します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
let xpathEvaluate xpath (x: XNode) =
  let target, root =
    match x with
    | :? XDocument as dirtyDoc ->
      let root = dirtyDoc.Root
      let xml = removeNamespace root
      let doc = XDocument(dirtyDoc.Declaration, box xml) :> XNode
      doc, root
    | :? XElement as dirtyElem ->
      let xml = removeNamespace dirtyElem :> XNode
      xml, dirtyElem
    | _ -> failwith "unknown XNode object"
  target.XPathEvaluate(xpath)
  |> function
    | :? seq<obj> as xs ->
      xs
      |> Seq.cast<XObject>
      |> Seq.map (Index.path [])
      |> Seq.map (Index.follow root)
      |> box
    | other -> other

/// XPath式を評価して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
let xpathElement xpath x = xpathEvaluate xpath x |> unbox<seq<XObject>> |> Seq.cast<XElement>

/// XPath式を評価して属性のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
let xpathAttribute xpath x = xpathEvaluate xpath x |> unbox<seq<XObject>> |> Seq.cast<XAttribute>

/// XPath式を評価してテキスト要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
let xpathText xpath x = xpathEvaluate xpath x |> unbox<seq<XObject>> |> Seq.cast<XText>

/// XPath式を評価して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xdocument_xpath xpath (dirtyDoc: XDocument) = xpathElement xpath dirtyDoc

/// XPath式を評価して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xelement_xpath xpath dirtyXml = xpathElement xpath dirtyXml

/// XPath式を評価して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xpath xpath (x: XNode) = xpathElement xpath x