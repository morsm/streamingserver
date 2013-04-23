<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="StreamingApp.Error" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
    Oh dear</h2>
<p>
    An error has occured. Those responsible have been sacked. Please try again, or, 
    as an alternative, despair.</p>
<p>
    <asp:Label ID="lbDetailedErrorMessage" runat="server"></asp:Label>
</p>
</asp:Content>
