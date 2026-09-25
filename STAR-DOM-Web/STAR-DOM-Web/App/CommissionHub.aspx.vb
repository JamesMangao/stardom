Imports System.Text
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports STAR_DOM.Helpers
Imports STAR_DOM.Models
Imports STAR_DOM.Services

Namespace STAR_DOM.Web

    Public Class CommissionHubPage
        Inherits Page

        Protected Out As Literal
        Private ReadOnly _catalog As New CatalogService()
        Private ReadOnly _commissions As New CommissionService()

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            Guard.RequireLogin()
            Try
                RenderHub()
            Catch ex As Exception
                Out.Text = WebUi.AlertBox("Could not load the commission hub: " & ex.Message)
            End Try
        End Sub

        Private Sub RenderHub()
            Dim sb As New StringBuilder()
            sb.Append(WebUi.Section("Open Commission Slots", "BESPOKE ART & ON-SITE SKETCHES",
                                    "Commission bespoke digital art, original characters, portraits, or live event sketches directly from the STAR:DOM artist."))

            Dim slots As List(Of CommissionSlotView) = _catalog.CommissionSlots()
            If slots.Count > 0 Then
                sb.Append("<div class=""grid cards"">")
                For Each s As CommissionSlotView In slots
                    sb.Append(SlotCard(s))
                Next
                sb.Append("</div>")
            Else
                sb.Append(WebUi.EmptyRow("No commission slots are open right now."))
            End If

            ' my requests
            Dim mine As List(Of Commission) = _commissions.ListMyCommissions().OrderByDescending(Function(c) c.Id).ToList()
            sb.Append("<div class=""sec-head"" style=""margin-top:30px""><div>")
            sb.Append("<div class=""eyebrow"">MY REQUEST QUEUE</div>")
            sb.Append("<h2>My Commission Requests</h2></div>")
            sb.Append("<div class=""sub"">" & mine.Count.ToString() & " request(s) in the atelier pipeline</div></div>")

            If mine.Count = 0 Then
                sb.Append(WebUi.EmptyRow("You haven't submitted a commission request yet."))
            Else
                sb.Append("<div class=""tblwrap""><table class=""tbl""><thead><tr>")
                For Each h As String In {"REQUEST", "CATEGORY", "QTY", "STATUS", "UPDATED", ""}
                    sb.Append("<th>" & h & "</th>")
                Next
                sb.Append("</tr></thead><tbody>")
                For Each c As Commission In mine
                    sb.Append("<tr>")
                    sb.Append("<td><b>" & WebUi.Esc(c.CommissionNumber) & "</b><br><span class=""sub"" style=""font-size:12px"">" &
                              WebUi.Esc(c.Title) & "</span></td>")
                    sb.Append("<td>" & WebUi.Esc(c.CategoryName) & "</td>")
                    sb.Append("<td>" & c.Quantity.ToString() & "</td>")
                    sb.Append("<td>" & WebUi.Badge(c.Status) & "</td>")
                    sb.Append("<td>" & WebUi.Esc(c.UpdatedAt.ToString("MMM d")) & "</td>")
                    sb.Append("<td class=""rowact""><a href=""/App/CommissionDetail.aspx?id=" & c.Id.ToString() & """>View / Reply</a></td>")
                    sb.Append("</tr>")
                Next
                sb.Append("</tbody></table></div>")
            End If

            Out.Text = sb.ToString()
        End Sub

        Private Function SlotCard(s As CommissionSlotView) As String
            Dim sb As New StringBuilder()
            sb.Append("<div class=""pcard"">")
            sb.Append("<div style=""position:relative"">")
            sb.Append(WebUi.Art(s.Seed, s.MerchantName, "height:170px"))
            sb.Append("<span class=""badge warn"" style=""position:absolute;top:8px;right:8px"">" & WebUi.Esc(s.SlotsText) & "</span></div>")
            sb.Append("<div class=""pbody"">")
            sb.Append("<span class=""brand"">" & WebUi.Esc(s.MerchantTagline) & "</span>")
            sb.Append("<b style=""font-size:16px"">" & WebUi.Esc(s.MerchantName) & "</b>")
            sb.Append("<div class=""kv"" style=""grid-template-columns:110px 1fr"">")
            sb.Append("<dt>Starting</dt><dd>" & WebUi.Money(s.StartingPrice) & "</dd>")
            sb.Append("<dt>Turnaround</dt><dd>" & WebUi.Esc(s.Turnaround) & "</dd>")
            sb.Append("<dt>Formats</dt><dd>" & WebUi.Esc(s.Formats) & "</dd>")
            sb.Append("</div>")
            Dim cta As String = If(s.CtaText <> "", s.CtaText, "Request Slot")
            sb.Append(WebUi.BtnHref("/App/CommissionRequest.aspx?m=" & s.MerchantId.ToString(), cta, "primary", "draw"))
            sb.Append("</div></div>")
            Return sb.ToString()
        End Function

    End Class

End Namespace
