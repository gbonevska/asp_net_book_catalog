using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.Common;
using System.Collections;

public partial class Default : System.Web.UI.Page
{
    protected string[] AuthorNamesSelected = null;
    protected string[] CollectionNamesSelected = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            ArrayList authors = GetAuthorsNames();
            foreach (string author in authors)
            {
                lbBookAuthor.Items.Add(author);
            }

            ArrayList collections = GetCollectionsNames();
            foreach (string collection in collections)
            {
                lbBookCollection.Items.Add(collection);
            }

            this.GrdLoad();
        }
    }

    private ArrayList GetCollectionsNames()
    {
        string errMsg = "";
        DataRowCollection collections = dbClass.GetTableForGrid(ref errMsg, "collections").Rows;

        // have database error
        if (errMsg.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = errMsg;
            return new ArrayList();
        }

        ArrayList collectionsNames = new ArrayList();
        foreach (DataRow collection in collections)
        {
            collectionsNames.Add(collection.ItemArray[1].ToString());
        }

        return collectionsNames;
    }

    private ArrayList GetAuthorsNames()
    {
        string errMsg = "";
        DataRowCollection authors = dbClass.GetTableForGrid(ref errMsg, "authors").Rows;

        // have database error
        if (errMsg.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = errMsg;
            return null;
        }

        int num = authors.Count;
        ArrayList authorNames = new ArrayList();
        foreach (DataRow author in authors)
        {
            authorNames.Add(author.ItemArray[1].ToString());
        }

        return authorNames;
    }

    protected void lbBookAuthor_SelectedIndexChanged(object sender, EventArgs e)
    {
        int[] selectedIndices = lbBookAuthor.GetSelectedIndices();
        AuthorNamesSelected = new string[selectedIndices.Length];
        for (int i = 0; i < selectedIndices.Length; i++)
        {
            AuthorNamesSelected[i] = lbBookAuthor.Items[selectedIndices[i]].ToString();
        }
    }


    protected void lbBookCollection_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lbBookCollection.GetSelectedIndices().Length > 0)
        {
            int[] selectedIndices = lbBookCollection.GetSelectedIndices();
            CollectionNamesSelected = new string[selectedIndices.Length];
            for (int i = 0; i < selectedIndices.Length; i++)
            {
                CollectionNamesSelected[i] = lbBookCollection.Items[selectedIndices[i]].ToString();
            }
        }
        else
        {
            CollectionNamesSelected = new string[0] { };
        }
    }

    protected void InputBook_Click(object sender, EventArgs e)
    {
        string BookNotes = txtBookNotes.Text;
        string BookName = txtBookName.Text;
        if (BookName == string.Empty)
        {
            Label1.Visible = true;
            Label1.Text = "Името на книгата не може да е празно!";
            return;
        }

        lbBookAuthor_SelectedIndexChanged(sender, e);
        lbBookCollection_SelectedIndexChanged(sender, e);

        if (AuthorNamesSelected.Length <= 0)
        {
            Label1.Visible = true;
            Label1.Text = "Трябва да бъде избран поне един автор!";
            return;
        }

        // Въвеждане на нова книга
        if (dbClass.InsertBook(BookName, AuthorNamesSelected, CollectionNamesSelected, BookNotes))
        {
            this.GrdLoad();
            this.ClearInputForm();
        }
        else
        {
            Response.Write("Database error.");
        }
    }

    private void ClearInputForm()
    {
        txtBookName.Text = "";
        txtBookNotes.Text = "";
        lbBookAuthor.ClearSelection();
        lbBookCollection.ClearSelection();

        btnInputBook.Visible = true;
        btnUpdateBook.Visible = false;
        btnClearBook.Visible = true;
        btnCancelEditBook.Visible = false;

        gridView.EditIndex = -1;

        txtOldBookName.Text = "";
        txtOldBookNotes.Text = "";
        lbOldBookAuthor.ClearSelection();
        lbOldBookCollection.ClearSelection();
    }

    private bool GrdLoad()
    {
        // load grid data
        string errMsg = "";
        int[] authorIds = new int[] { };
      
        gridView.DataSource = dbClass.GetAllBooksByAuthors(ref errMsg, authorIds);
        gridView.DataBind();

        // have database error
        if (errMsg.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = errMsg;
            return false;
        }

        return true;
    }

    protected void ClearBook_Click(object sender, EventArgs e)
    {
        this.ClearInputForm();
    }

    protected void gridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int id = Convert.ToInt32(gridView.DataKeys[e.RowIndex].Values[0]);

        if (id > 0)
        {
            if (!dbClass.DeleteBookData(id))
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
            this.ClearInputForm();
            this.GrdLoad();
        }
    }

    protected void gridView_RowEditing(object sender, GridViewEditEventArgs e)
    {
        this.ClearInputForm();

        gridView.EditIndex = e.NewEditIndex;
        btnInputBook.Visible = false;
        btnUpdateBook.Visible = true;
        btnClearBook.Visible = false;
        btnCancelEditBook.Visible = true;

        this.Page_Load(sender, e);

        // get book_id
        int bookId = Convert.ToInt32(gridView.DataKeys[e.NewEditIndex].Values[0]);

        string Error = "";
        DataTable bookDetails = dbClass.GetBookDetails(ref Error, bookId);
        if (Error.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = "Грешка при редакция на книга!";
            this.GrdLoad();
        }

        foreach (DataRow row in bookDetails.Rows)
        {
            txtBookName.Text = row["BookTitle"].ToString();
            txtBookNotes.Text = row["BookNotes"].ToString();
            lbBookAuthor.Items.FindByValue(row["AuthorName"].ToString()).Selected = true;
            if (row["CollectionName"].ToString() != "")
            {
                lbBookCollection.Items.FindByValue(row["CollectionName"].ToString()).Selected = true;
            }
        }

        // save old state before user changes
        txtOldBookName.Text = txtBookName.Text;
        txtOldBookNotes.Text = txtBookNotes.Text;
        lbOldBookAuthor = lbBookAuthor;
        lbOldBookCollection = lbBookCollection;
    }

    protected void UpdateBook_Click(object sender, EventArgs e)
    {
        int bookId = Convert.ToInt32(gridView.DataKeys[gridView.EditIndex].Values[0]);

        string bookTitle = "";
        string bookNotes = "";
        int[] listBookAuthors = null;
        int[] listBookCollections = null;

        if (txtBookName.Text == "")
        {
            Label1.Visible = true;
            Label1.Text = "Името на книгата не може да е празно!";
            return;
        }

        lbBookAuthor_SelectedIndexChanged(sender, e);
        lbBookCollection_SelectedIndexChanged(sender, e);

        if (AuthorNamesSelected.Length <= 0)
        {
            Label1.Visible = true;
            Label1.Text = "Трябва да бъде избран поне един автор!";
            return;
        }

        if( txtBookName.Text != txtOldBookName.Text
            || txtBookNotes.Text != txtOldBookNotes.Text
            || lbOldBookCollection.GetSelectedIndices() != lbBookCollection.GetSelectedIndices()
            || lbOldBookAuthor.GetSelectedIndices() != lbBookAuthor.GetSelectedIndices()
            )
        {
            bookTitle = txtBookName.Text;
            bookNotes = txtBookNotes.Text;

            if (lbOldBookAuthor.GetSelectedIndices() != lbBookAuthor.GetSelectedIndices())
            {
                listBookAuthors = new int[AuthorNamesSelected.Length];
                for (int i = 0; i < CollectionNamesSelected.Length; i++)
                {
                    listBookAuthors[i] = dbClass.GetIdFromName(AuthorNamesSelected[i], "authors");
                }
            }

            if (lbOldBookCollection.GetSelectedIndices() != lbBookCollection.GetSelectedIndices())
            {
                listBookCollections = new int[CollectionNamesSelected.Length];
                for (int i = 0; i < CollectionNamesSelected.Length; i++)
                {
                    listBookCollections[i] = dbClass.GetIdFromName(CollectionNamesSelected[i], "collections");
                }
            }

            dbClass.UpdateBookDetails(bookId, bookTitle, listBookAuthors, listBookCollections, bookNotes);
        }       

        btnInputBook.Visible = true;
        btnUpdateBook.Visible = false;
        btnClearBook.Visible = true;
        btnCancelEditBook.Visible = false;

        // make in ClearInputForm()  gridView.EditIndex = -1;
        this.ClearInputForm();
        this.GrdLoad();
    }

    protected void CancelEditBook_Click(object sender, EventArgs e)
    {
        this.ClearInputForm();
    }
}