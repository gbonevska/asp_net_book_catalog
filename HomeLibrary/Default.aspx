<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Списък с книги и автори</title>
</head>
<body>
    <form id="form1" runat="server">
    <a href="Authors.aspx"> Автори </a> | 
    <a href="Collections.aspx"> Колекции </a> |
    <p></p>
    <p>Въвеждане на нова книга:</p>    
	  <table border = "1">
            <tr>
                <td colspan="4" align="center">&nbsp
                    <font face="verdana" size="2"><strong>
                    <asp:Label runat="server" ID="Label1" Text="" ForeColor="Red" EnableViewState="false" Width="600" Visible="false"></asp:Label>    
                    </strong></font>
                    <asp:TextBox runat="server" Id="txtOldBookName" Visible="false" />
                    <asp:TextBox runat="server" Id="txtOldBookNotes" Visible="false"/>
                    <asp:ListBox runat="server" ID="lbOldBookAuthor" Rows="5" SelectionMode="Multiple" Visible="false"  />
                    <asp:ListBox runat="server" ID="lbOldBookCollection" Rows="5" SelectionMode="Multiple" Visible="false" />
                </td>
            </tr>  
            <tr>
                <th>Книга: </th>
                <th>Автор: </th>
                <th>Колекция: </th>
                <th>Забележка: </th>
            </tr>
             <tr>   
                 <td><asp:TextBox runat="server" Id="txtBookName" Height="100%"/></td>
                 <td><asp:ListBox runat="server" ID="lbBookAuthor" Rows="5" SelectionMode="Multiple" Width="300" OnSelectedIndexChanged="lbBookAuthor_SelectedIndexChanged" /></td>
                 <td><asp:ListBox runat="server" ID="lbBookCollection" Rows="5" SelectionMode="Multiple" Width="300" OnSelectedIndexChanged="lbBookCollection_SelectedIndexChanged" /></td>
                 <td><asp:TextBox runat="server" ID="txtBookNotes" Height="100%" Rows= "5" /></td>
            </tr>
	    </table>
         <asp:Button runat="server" text="Въведи" ID="btnInputBook" OnClick="InputBook_Click" Visible="true" />
         <asp:Button runat="server" text="Промени" ID="btnUpdateBook" OnClick="UpdateBook_Click" Visible="false" />
         <asp:Button runat="server" text="Изчисти" ID="btnClearBook" OnClick="ClearBook_Click" Visible="true" />
         <asp:Button runat="server" text="Отказ" ID="btnCancelEditBook" OnClick="CancelEditBook_Click" Visible="false" />
        <p></p>
        <table border = "1">
            <tr>
                <td colspan="4" align="center">&nbsp<font face="verdana" size="2"><strong><asp:Label runat="server" ID="lblError" Text="" ForeColor="Red" EnableViewState="false" Width="800" Visible="false"></asp:Label></strong></font></td>
            </tr>  
            <tr>
                <td colspan="4" align="center">
                    <asp:GridView ID="gridView" runat="server" BorderStyle="None" DataKeyNames="BookID" Width="1020" 
                        AutoGenerateColumns="False" 
                        OnRowDeleting="gridView_RowDeleting"
                        OnRowEditing="gridView_RowEditing" >
                        <Columns>
                            <asp:BoundField HeaderText="Книга:" SortExpression="BookTitle" DataField="BookTitle" HtmlEncode="false" ReadOnly="true"><ItemStyle HorizontalAlign="Left" Width="250" /></asp:BoundField>
                            <asp:BoundField HeaderText="Автор:" SortExpression="AuthorName" DataField="AuthorName" HtmlEncode="false" ReadOnly="true"><ItemStyle HorizontalAlign="Left" Width="250" /></asp:BoundField>
                            <asp:BoundField HeaderText="Колекция:" SortExpression="CollectionName" DataField="CollectionName" HtmlEncode="false" ReadOnly="true"><ItemStyle HorizontalAlign="Left" Width="250" /></asp:BoundField>
                            <asp:BoundField HeaderText="Забележки:" SortExpression="BookNotes" DataField="BookNotes" HtmlEncode="false" ReadOnly="true"><ItemStyle HorizontalAlign="Left" Width="250" /></asp:BoundField>
                            <asp:ButtonField ButtonType="Link" text="Редакция" CommandName="Edit" ><ItemStyle HorizontalAlign="Center"  Width="100" /></asp:ButtonField>
                            <asp:ButtonField ButtonType="Link" text="Изтрий" CommandName="Delete" ><ItemStyle HorizontalAlign="Center"  Width="100" /></asp:ButtonField>
			             </Columns>
                    </asp:GridView>
                </td>
            </tr>
	    </table>
   </form>
</body>
</html>
