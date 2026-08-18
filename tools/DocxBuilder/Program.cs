using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using HtmlAgilityPack;

if (args.Length != 2) throw new ArgumentException("Usage: DocxBuilder input.html output.docx");
var html = new HtmlDocument(); html.Load(args[0]);
if (File.Exists(args[1])) File.Delete(args[1]);
using var doc = WordprocessingDocument.Create(args[1], WordprocessingDocumentType.Document);
var main = doc.AddMainDocumentPart(); main.Document = new W.Document(new W.Body());
var stylePart = main.AddNewPart<StyleDefinitionsPart>(); stylePart.Styles = BuildStyles(); stylePart.Styles.Save();
var body = main.Document.Body!; uint imageId = 1;
foreach (var node in html.DocumentNode.SelectSingleNode("//body")?.ChildNodes ?? Enumerable.Empty<HtmlNode>()) AddNode(node, body);
body.Append(new W.SectionProperties(new W.PageSize { Width = 12240, Height = 15840 }, new W.PageMargin { Top=1037,Right=1123,Bottom=1037,Left=1123,Header=500,Footer=500,Gutter=0 }));
main.Document.Save();

void AddNode(HtmlNode n, OpenXmlCompositeElement parent) {
  if (n.NodeType != HtmlNodeType.Element) return; var name=n.Name.ToLowerInvariant();
  if (name=="div") { foreach(var child in n.ChildNodes) AddNode(child,parent); return; }
  if (name is "h1" or "h2" or "h3") { var level=name[1]-'0'; parent.Append(Para(n.InnerText,$"Heading{level}",n.GetAttributeValue("class","").Contains("break"))); return; }
  if (name=="p") { parent.Append(Para(n.InnerText,"Normal",false,n.GetAttributeValue("class","").Contains("muted"))); return; }
  if (name is "ul" or "ol") { foreach(var li in n.SelectNodes("./li")??Enumerable.Empty<HtmlNode>()) parent.Append(Para("• "+li.InnerText,"List")); return; }
  if (name=="table") { parent.Append(MakeTable(n)); return; }
  if (name=="img") { var src=HtmlEntity.DeEntitize(n.GetAttributeValue("src","")); if(!File.Exists(src))src=Path.Combine(Path.GetDirectoryName(args[1])!,"gta-table-relationships.png");if(File.Exists(src)) parent.Append(ImageParagraph(src)); }
}
W.Paragraph Para(string text,string style,bool pageBreak=false,bool muted=false) {
  text=HtmlEntity.DeEntitize(text).Trim(); var props=new W.ParagraphProperties(new W.ParagraphStyleId{Val=style}); if(pageBreak)props.Append(new W.PageBreakBefore()); var rp=new W.RunProperties();if(muted)rp.Append(new W.Color{Val="687782"});return new W.Paragraph(props,new W.Run(rp,new W.Text(text){Space=SpaceProcessingModeValues.Preserve}));
}
W.Table MakeTable(HtmlNode n) {
  var table=new W.Table();table.Append(new W.TableProperties(new W.TableWidth{Width="9360",Type=W.TableWidthUnitValues.Dxa},new W.TableLayout{Type=W.TableLayoutValues.Fixed},new W.TableBorders(new W.TopBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"},new W.LeftBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"},new W.BottomBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"},new W.RightBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"},new W.InsideHorizontalBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"},new W.InsideVerticalBorder{Val=W.BorderValues.Single,Size=4,Color="BCC9D4"})));
  var rows=n.SelectNodes(".//tr")?.ToList()??[];var cols=rows.FirstOrDefault()?.SelectNodes("./th|./td")?.Count??1;var width=(9360/cols).ToString();
  for(int ri=0;ri<rows.Count;ri++){var tr=new W.TableRow();if(ri==0)tr.Append(new W.TableRowProperties(new W.TableHeader()));foreach(var cell in rows[ri].SelectNodes("./th|./td")??Enumerable.Empty<HtmlNode>()){var cp=new W.TableCellProperties(new W.TableCellWidth{Width=width,Type=W.TableWidthUnitValues.Dxa});if(ri==0)cp.Append(new W.Shading{Fill="E8EEF5"});var rp=new W.RunProperties(new W.RunFonts{Ascii="Aptos",HighAnsi="Aptos"},new W.FontSize{Val="17"});if(ri==0)rp.Append(new W.Bold());tr.Append(new W.TableCell(cp,new W.Paragraph(new W.ParagraphProperties(new W.SpacingBetweenLines{After="40"}),new W.Run(rp,new W.Text(HtmlEntity.DeEntitize(cell.InnerText).Trim()){Space=SpaceProcessingModeValues.Preserve}))));}table.Append(tr);}return table;
}
W.Paragraph ImageParagraph(string path) {
  var part=main.AddImagePart(ImagePartType.Png);using(var fs=File.OpenRead(path))part.FeedData(fs);var rel=main.GetIdOfPart(part);const long cx=5943600,cy=5528160;
  var picture=new PIC.Picture(
    new PIC.NonVisualPictureProperties(new PIC.NonVisualDrawingProperties{Id=0,Name=Path.GetFileName(path)},new PIC.NonVisualPictureDrawingProperties()),
    new PIC.BlipFill(new A.Blip{Embed=rel},new A.Stretch(new A.FillRectangle())),
    new PIC.ShapeProperties(new A.Transform2D(new A.Offset{X=0,Y=0},new A.Extents{Cx=cx,Cy=cy}),new A.PresetGeometry(new A.AdjustValueList()){Preset=A.ShapeTypeValues.Rectangle}));
  var pictureData=new A.GraphicData(picture){Uri="http://schemas.openxmlformats.org/drawingml/2006/picture"};
  var inline=new DW.Inline(new DW.Extent{Cx=cx,Cy=cy},new DW.EffectExtent{LeftEdge=0,TopEdge=0,RightEdge=0,BottomEdge=0},new DW.DocProperties{Id=imageId++,Name="SQL relationship diagram"},new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks{NoChangeAspect=true}),new A.Graphic(pictureData));
  var drawing=new W.Drawing(inline);
  return new W.Paragraph(new W.ParagraphProperties(new W.Justification{Val=W.JustificationValues.Center}),new W.Run(drawing));
}
W.Styles BuildStyles(){var s=new W.Styles();s.Append(Style("Normal","Aptos",20,"243447",0,100),Style("Heading1","Aptos Display",34,"1F4E79",280,140,true),Style("Heading2","Aptos Display",26,"2F5597",220,100,true),Style("Heading3","Aptos Display",23,"44546A",180,80,true),Style("List","Aptos",20,"243447",0,60));return s;}
W.Style Style(string id,string font,int size,string color,int before,int after,bool bold=false){var rp=new W.StyleRunProperties(new W.RunFonts{Ascii=font,HighAnsi=font},new W.Color{Val=color},new W.FontSize{Val=size.ToString()});if(bold)rp.Append(new W.Bold());return new W.Style(new W.StyleName{Val=id},id=="Normal"?null:new W.BasedOn{Val="Normal"},new W.NextParagraphStyle{Val="Normal"},new W.StyleParagraphProperties(new W.SpacingBetweenLines{Before=before.ToString(),After=after.ToString(),Line="276",LineRule=W.LineSpacingRuleValues.Auto}),rp){Type=W.StyleValues.Paragraph,StyleId=id,Default=id=="Normal"};}
