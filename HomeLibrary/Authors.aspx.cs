using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Authors : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            this.GrdLoad();            
        }
    }

    protected void Input_Click(object sender, EventArgs e)
    {
        if(txtName.Text == string.Empty)
        {
            return;
        }

        // Въвеждане на запис
        if (dbClass.InsertData(txtName.Text, "authors"))
        {
            this.GrdLoad();
        }
        else
        {
            Response.Write("Database error.");
        }
    }

    private bool GrdLoad()
    {
        // load grid data
        string errMsg = "";

        gridView.DataSource = dbClass.GetTableForGrid(ref errMsg, "authors");
        gridView.DataBind();

        // have database error
        if (errMsg.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = errMsg;
            return false;
        }

        txtName.Text = "";

        return true;
    }

    protected void OnRowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        gridView.EditIndex = -1;
        this.GrdLoad();
    }

    protected void gridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        // get author_id
        int id = Convert.ToInt32(gridView.DataKeys[e.RowIndex].Values[0]);

        if (id > 0)
        {
            if (dbClass.HaveBooksByAuthor(id))
            {
                lblError.Visible = true;
                lblError.Text = "Този автор не може да бъде изтрит! Има въведени книги с този автор!";
                return;
            }

            if (!dbClass.DeleteData(id, "authors"))
            {
                lblError.Visible = true;
                lblError.Text = "Неуспешно изтриване!";
                return;
            }
            else
            {
                lblError.Visible = true;
                lblError.Text = "Успешно изтриване!";
            }
            this.GrdLoad();
        }
    }

    protected void gridView_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gridView.EditIndex = e.NewEditIndex;
        this.GrdLoad();
    }

    protected void gridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        string newName = e.NewValues["Name"].ToString();
        // get author_id
        int id = Convert.ToInt32(gridView.DataKeys[gridView.EditIndex].Values[0]);

        if (newName.Length > 0)
        {
            if (!dbClass.UpdateData(id, newName, "authors"))
            {
                lblError.Visible = true;
                lblError.Text = "Неуспешна промяна!";
                return;
            }
            else
            {
                lblError.Visible = true;
                lblError.Text = "Успешна промяна!";
            }
            gridView.EditIndex = -1;
        }
        else
        {
            lblError.Visible = true;
            lblError.Text = "Името на автора е невалидно!";
        }

        this.GrdLoad();
    }
}