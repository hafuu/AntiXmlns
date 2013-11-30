module AntiXmlns

open System
open System.Xml.XPath
open System.Xml.Linq

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

let rec internal indexPath' indexies (node: XElement) =
  match node.Parent with
  | null -> indexies
  | x ->
    let index = node.ElementsBeforeSelf() |> Seq.length
    indexPath' (index :: indexies) x

let internal indexPath (node: XElement) = indexPath' [] node

let rec internal follow (xml: XElement) = function
  | [] -> xml
  | index :: xs ->
    let next = xml.Elements() |> Seq.nth index
    follow next xs

/// XPath式を使用して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xdocument_xpath xpath (dirtyDoc: XDocument) =
  let xml = removeNamespace dirtyDoc.Root
  let doc = XDocument(dirtyDoc.Declaration, box xml)

  doc.XPathSelectElements(xpath)
  |> Seq.map indexPath
  |> Seq.map (follow dirtyDoc.Root)

/// XPath式を使用して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xelement_xpath xpath dirtyXml =
  (removeNamespace dirtyXml).XPathSelectElements(xpath)
  |> Seq.map indexPath
  |> Seq.map (follow dirtyXml)

/// XPath式を使用して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
let xpathElement xpath (x: XNode) =
  let target, dirtyRoot =
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
  target.XPathSelectElements(xpath)
  |> Seq.map indexPath
  |> Seq.map (follow dirtyRoot)

/// XPath式を使用して要素のコレクションを選択します。
/// XMLに名前空間がある場合は無視するため、XPathに名前空間の指定は不要です。
[<Obsolete("AntiXmlns.xpathElementを使用して下さい。")>]
let xpath xpath (x: XNode) = xpathElement xpath x