<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <% if (Request.IsAuthenticated)
        { 
    %>
        <h1>Logged in</h1>
    <% }else { %>
        <h1>Not Logged in</h1>
    <% } %>
</asp:Content>
