<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebApp.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Secure Page, logged in as <%= User.Identity.Name %></h1>
    <ul>
    <% foreach (var claim in ((System.Security.Claims.ClaimsPrincipal)User).Claims){ %>
        <li><%: claim.Type + ", " + claim.Value %></li>
    <%} %>
    </ul>
</asp:Content>
