using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;
using System.Collections;

public class dbClass
{
    /// <summary>
    /// InsertData - insert authors or collections
    /// </summary>
    /// <param name="Name">name of author or collection</param>
    /// <param name="Table">table name - collections or authors</param>
    /// <returns>true if sucess, false if not</returns>
    public static bool InsertData(string Name, string Table)
    {
        StringBuilder name = new StringBuilder(Name.Trim());

        // Заместваме ' с '' против SQL Injection
        if (name.Length > 0)
        {
            name.Replace("'", "''");
        }

        string query = "";
        if (Table.Equals("collections"))
        {
            query = "INSERT INTO [collections] ([collection_name]) VALUES ('" + name.ToString() + "');";
        }

        if (Table.Equals("authors"))
        {
            query = "INSERT INTO [authors] ([author_name]) VALUES ('" + name.ToString() + "');";
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            name = null;
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return true;
    }

    /// <summary>
    /// InsertBook - main function for insert of new book with details
    /// </summary>
    /// <param name="BookName"></param>
    /// <param name="AuthorName"></param>
    /// <param name="CollectionName"></param>
    /// <param name="BookNotes"></param>
    /// <returns>true if sucessfull, false if not</returns>
    public static bool InsertBook(string BookName, string[] AuthorName, string[] CollectionName, string BookNotes)
    {
        if (BookName == null || BookName == "")
        {
            return false;
        }

        if (AuthorName == null)
        {
            return false;
        }

        int[] authorsIds = new int[AuthorName.Length];
        for (int i = 0; i < AuthorName.Length; i++)
        {
            authorsIds[i] = GetIdFromName(AuthorName[i], "authors");
        }

        int[] collectionsIds;
        if (CollectionName == null)
        {
            collectionsIds = new int[0] { };
        }
        else
        {
            collectionsIds = new int[CollectionName.Length];
            for (int i = 0; i < CollectionName.Length; i++)
            {
                collectionsIds[i] = GetIdFromName(CollectionName[i], "collections");
            }
        }

        int insertedBookId = 0;
        insertedBookId = InsertNewBook(BookName, BookNotes);
        if (insertedBookId != 0)
        {
            return InsertRelationBookAuthorCollection(insertedBookId, authorsIds, collectionsIds);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// InsertNewBook - insert record in table books
    /// </summary>
    /// <param name="bookName"></param>
    /// <param name="bookNotes"></param>
    /// <returns>inserted book's id</returns>
    private static int InsertNewBook(string BookName, string BookNotes)
    {
        int insertedBookId = 0;
        string query = "";

        StringBuilder bookName = new StringBuilder(BookName.Trim());
        StringBuilder bookNotes = new StringBuilder(BookNotes.Trim());

        // Заместваме ' с '' против SQL Injection
        bookName.Replace("'", "''");
        bookNotes.Replace("'", "''");

        if (bookNotes.Length > 0)
        {
            query = "INSERT INTO books(book_title, notes) output INSERTED.book_id VALUES ('" + bookName.ToString() + "','" + bookNotes.ToString() + "')";
        }
        else
        {
            query = "INSERT INTO books(book_title)  output INSERTED.book_id VALUES('" + bookName.ToString() + "')";
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        try
        {
            myConnection.Open();
            insertedBookId = (int)myCommand.ExecuteScalar();
        }
        catch
        {
            insertedBookId = 0;
            return insertedBookId;
        }
        finally
        {
            bookNotes = null;
            bookName = null;
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return insertedBookId;
    }

    public static BookTable GetBookDetails(ref string ErrorMessage, int bookId)
    {
        // Create table
        BookTable result = new BookTable();
        ErrorMessage = "";

        string query = "SELECT DISTINCT b.book_id, " +
                                      " b.book_title, " +
                                      " a.author_name, " +
                                      " ISNULL(c.collection_name,''), " +
                                      " ISNULL(b.notes,'') " +
                                      ", a.author_id " +
                                      ", ISNULL(ba.collection_id,0) " +
                             " FROM books_authors as ba " +
                             " INNER JOIN books as b " +
                             " ON ba.book_id = b.book_id " +
                             " INNER JOIN books_authors as bba " +
                             " ON bba.book_id = ba.book_id " +
                             " INNER JOIN authors as a " +
                             " ON bba.author_id = a.author_id " +
                             " LEFT OUTER JOIN collections c " +
                             " ON c.collection_id = ba.collection_id " +
                             " WHERE b.book_id = '" + bookId.ToString() + "' " +
                             " ORDER BY b.book_title, a.author_name;";

        // Get result from database
        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        SqlDataReader reader = null;
        try
        {
            myConnection.Open();
            reader = myCommand.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result.FillBookRow(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(5), reader.GetString(2), reader.GetInt32(6), reader.GetString(3), reader.GetString(4));
                }
            }
            else
            {
                result.EmtpyBookRow();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            if (result != null) result.Dispose();
            return null;
        }
        finally
        {
            if (reader != null)
            {
                if (reader.IsClosed == false) reader.Close();
                reader.Dispose();
                reader = null;
            }
            if (myConnection != null)
            {
                if (myConnection.State == ConnectionState.Open) myConnection.Close();
                myConnection.Dispose();
                myConnection = null;
            }
            if (myCommand != null)
            {
                myCommand.Dispose();
                myCommand = null;
            }
        }

        return result;
    }

    /// <summary>
    /// InsertRelationBookAuthorCollection - insert record in relation table books_authors
    /// </summary>
    /// <param name="NewBookId"></param>
    /// <param name="AuthorIds"></param>
    /// <param name="BookCollectionIds"></param>
    /// <returns>true if sucess, false if not</returns>
    public static bool InsertRelationBookAuthorCollection(int NewBookId, int[] AuthorIds, int[] BookCollectionIds)
    {
        if (NewBookId <= 0 || AuthorIds == null || AuthorIds.Length == 0)
        {
            return false;
        }

        string query = "";
        if (BookCollectionIds != null && BookCollectionIds.Length > 0)
        {
            for (int i = 0; i < AuthorIds.Length; i++)
            {
                for (int j = 0; j < BookCollectionIds.Length; j++)
                {
                    query += "INSERT INTO books_authors(author_id, book_id, collection_id) " +
                            " VALUES(" + AuthorIds[i] + ", " + NewBookId + ", " + BookCollectionIds[j] + ");";
                }
            }
        }
        else
        {
            for (int i = 0; i < AuthorIds.Length; i++)
            {
                query += "INSERT INTO books_authors(author_id, book_id) " +
                        " VALUES(" + AuthorIds[i] + ", " + NewBookId + ");";
            }
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return true;
    }

    /// <summary>
    /// UpdateBookDetails - on update of book, make changes in corespond tables books and books_authors
    /// </summary>
    /// <param name="BookId"></param>
    /// <param name="NewBookName"></param>
    /// <param name="NewAuthorIds"></param>
    /// <param name="NewBookCollectionIds"></param>
    /// <param name="NewBookNotes"></param>
    /// <returns></returns>
    public static bool UpdateBookDetails(int BookId, string NewBookName, int[] NewAuthorIds, int[] NewBookCollectionIds, string NewBookNotes)
    {
        if (BookId <= 0 || NewAuthorIds == null || NewAuthorIds.Length == 0)
        {
            return false;
        }

        //first step delete all old relations from table books_authors by given book_id
        if(!DeleteRelationBookAuthorCollection(BookId))
        {
            return false;
        }
        //second step insert all new relations into table books_authors by NewAuthors and NewBookCollectionsIds
        if(!InsertRelationBookAuthorCollection(BookId, NewAuthorIds, NewBookCollectionIds))
        {
            return false;
        }
        
        //third step update table books with NewBookName and NewBookNotes if there are not empty
        if(NewBookName != "")
        {
            StringBuilder newBookName = new StringBuilder(NewBookName);
            StringBuilder newBookNotes = new StringBuilder(NewBookNotes);
            newBookNotes.Replace("'", "''");
            newBookName.Replace("'", "''");

            string query = "";

            if (NewBookNotes == null)
            {
                query = "UPDATE [books] SET [book_title] = '" + newBookName.ToString() 
                     + "' WHERE [book_id] ='" + BookId.ToString()+"';";
            }
            else
            {
                query = "UPDATE [books] SET [book_title] = '" + newBookName.ToString() 
                          + "', [notes] ='" + newBookNotes.ToString() 
                          + "' WHERE [book_id] ='" + BookId.ToString() + "';";
            }

            SqlConnection myConnection = new SqlConnection(ConnectionString);
            SqlCommand myCommand = new SqlCommand(query, myConnection);
            try
            {
                myConnection.Open();
                myCommand.ExecuteNonQuery();
            }
            catch
            {
                return false;
            }
            finally
            {
                myConnection.Close();
                myConnection.Dispose();
                myCommand.Dispose();
            }
        }
        
        return true;
    }

    /// <summary>
    /// DeleteRelationBookAuthorCollection - delete books_authors relation records for given book_id
    /// </summary>
    /// <param name="BookId"></param>
    /// <returns></returns>
    private static bool DeleteRelationBookAuthorCollection(int BookId)
    {
        string query = "";
        if (BookId > 0)
        {
            query = "DELETE FROM [books_authors] WHERE [book_id] ='" + BookId.ToString() + "';";
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }
        return true;
    }


    /// <summary>
    /// DeleteCollectionData - set collection_id to null in books_authors table
    /// </summary>
    /// <param name="Id">collection id</param>
    /// <returns>true if sucess, false if not</returns>
    public static bool DeleteCollectionData(int Id)
    {
        //Delete relations, between books and deleted collection
        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand("UPDATE [books_authors] SET [collection_id] = null WHERE [collection_id]='" + Id.ToString() + "';", myConnection);
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }
        return DeleteData(Id, "collections");
    }

    /// <summary>
    /// DeleteData - delete row by id from corespond table
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="TableName">available values - collections, authors, books</param>
    /// <returns>true if success, false if error</returns>
    public static bool DeleteData(int Id, string TableName)
    {
        if(!(Id > 0))
        {
            return false;
        }

        string query = "";
        if (TableName.Equals("collections"))
        {
            query = "DELETE FROM [collections] WHERE [collection_id]='" + Id.ToString() + "';";
        }

        if (TableName.Equals("authors"))
        {
            query = "DELETE FROM [authors] WHERE [author_id]='" + Id.ToString() + "';";
        }

        if (TableName.Equals("books"))
        {
            query = "DELETE FROM [books] WHERE [book_id]='" + Id.ToString() + "';";
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection); ;
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return true;
    }

    /// <summary>
    /// DeleteBookData - delete records from table books and relation records from table books_authors by given book_id
    /// </summary>
    /// <param name="BookId"></param>
    /// <returns>true if success, false if not</returns>
    public static bool DeleteBookData(int BookId)
    {
        if (DeleteRelationBookAuthorCollection(BookId))
        {
            return DeleteData(BookId, "books");
        }
        return false;
    }

    /// <summary>
    /// UpdateData - update names from tables collections, authors or books by id
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="TableName">collections, authors, books</param>
    /// <returns>true if sucess, false if not</returns>
    public static bool UpdateData(int Id, string Name, string TableName)
    {
        StringBuilder name = new StringBuilder(Name.Trim());
        name.Replace("'", "''");

        string query = "";
        if (TableName == "collections")
        {
            query = "UPDATE [collections] SET [collection_name] = '" + name.ToString() + "' WHERE [collection_id]='" + Id.ToString() + "';";
        }
        if (TableName == "authors")
        {
            query = "UPDATE [authors] SET [author_name] = '" + name.ToString() + "' WHERE [author_id]='" + Id.ToString() + "';";
        }
        if (TableName == "books")
        {
            query = "UPDATE [books] SET [book_title] = '" + name.ToString() + "' WHERE [book_id]='" + Id.ToString() + "';";
        }

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        try
        {
            myConnection.Open();
            myCommand.ExecuteNonQuery();
        }
        catch
        {
            return false;
        }
        finally
        {
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return true;
    }

    /// <summary>
    /// GetIdFromName - search record by name from tables collections or authors
    /// </summary>
    /// <param name="SearchName"></param>
    /// <param name="TableName">available values - collections or authors</param>
    /// <returns>returned id</returns>
    public static int GetIdFromName(string SearchName, string TableName)
    {
        int result = 0;
        SqlDataReader reader = null;
        SqlConnection myConnection = new SqlConnection(ConnectionString);
        // Заместваме ' с '' против SQL Injection
        StringBuilder searchName = new StringBuilder(SearchName.Trim());
        searchName.Replace("'", "''");

        SqlCommand myCommand = null;
        if (TableName.Equals("collections"))
        {
            myCommand = new SqlCommand("SELECT [collection_id] FROM [collections] WHERE [collection_name]='" + searchName.ToString() + "';", myConnection);
        }

        if (TableName.Equals("authors"))
        {
            myCommand = new SqlCommand("SELECT [author_id] FROM [authors] WHERE [author_name]='" + searchName.ToString() + "';", myConnection);
        }

        try
        {
            myConnection.Open();
            reader = myCommand.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                result = reader.GetInt32(0);
            }
        }
        catch
        {
            return 0;
        }
        finally
        {
            if (reader != null)
            {
                if (reader.IsClosed != false) reader.Close();
                reader.Dispose();
            }
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return result;
    }

    /// <summary>
    /// GetTableForGrid - select all tables collections or authors
    /// </summary>
    /// <param name="ErrorMessage"></param>
    /// <param name="TableName">available vaules - collections or authors</param>
    /// <returns>DataTable</returns>
    public static DataTable GetTableForGrid(ref string ErrorMessage, string TableName)
    {
        // Create table
        DataTable result = new DataTable(TableName);
        DataColumn col = null;
        ErrorMessage = "";
        string query = "";

        col = new DataColumn("ID");
        col.DataType = System.Type.GetType("System.Int32");
        result.Columns.Add(col);
        col.Dispose();

        col = new DataColumn("Name");
        col.DataType = System.Type.GetType("System.String");
        result.Columns.Add(col);
        col.Dispose();

        if (TableName.Equals("collections"))
        {
            query = "SELECT [collection_id], [collection_name] FROM [collections] ORDER BY [collection_name];";
        }

        if (TableName == "authors")
        {
            query = "SELECT [author_id], [author_name] FROM [authors] ORDER BY [author_name];";
        }
        DataRow row = null;

        // Get result from database
        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        SqlDataReader reader = null;

        try
        {
            myConnection.Open();
            reader = myCommand.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    row = result.NewRow();
                    row["ID"] = reader.GetInt32(0);
                    row["Name"] = reader.GetString(1);

                    result.Rows.Add(row);
                }
            }
            else
            {
                // empty grid
                row = result.NewRow();
                row["id"] = 0;
                row["Name"] = "";

                result.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            if (result != null) result.Dispose();
            return null;
        }
        finally
        {
            if (reader != null)
            {
                if (reader.IsClosed == false) reader.Close();
                reader.Dispose();
                reader = null;
            }
            if (myConnection != null)
            {
                if (myConnection.State == ConnectionState.Open) myConnection.Close();
                myConnection.Dispose();
                myConnection = null;
            }
            if (myCommand != null)
            {
                myCommand.Dispose();
                myCommand = null;
            }
            row = null;
        }

        return result;
    }

    /// <summary>
    /// HaveBooksByAuthor - select number of books exists in books_authors
    /// </summary>
    /// <param name="authorId"></param>
    /// <returns>true if exists book, false if no exists book in books_authors</returns>
    public static bool HaveBooksByAuthor(int authorId)
    {
        string query = "SELECT [ba.book_id] " +
                      " FROM [books_authors] as ba " +
                      " WHERE [ba.author_id] ='" + authorId.ToString() + "';";

        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        SqlDataReader reader = null;

        try
        {
            myConnection.Open();
            reader = myCommand.ExecuteReader();

            if (reader.HasRows)
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            if (reader != null)
            {
                if (reader.IsClosed != false) reader.Close();
                reader.Dispose();
            }
            myConnection.Close();
            myConnection.Dispose();
            myCommand.Dispose();
        }

        return false;
    }

    /// <summary>
    /// GetAllBooksByAuthors
    /// if array of authorIds is empty, select all books, by all authors.
    /// </summary>
    /// <param name="ErrorMessage"></param>
    /// <param name="authorIds"></param>
    /// <returns>DataTable of all books, by authorIds</returns>
    public static DataTable GetAllBooksByAuthors(ref string ErrorMessage, int[] authorIds)
    {
        // Create table
        BookTable books = new BookTable();
        ErrorMessage = "";
        string where = "";

        if (authorIds.Length > 0)
        {
            where = " WHERE [ba.author_id] IN(" + string.Join(",", authorIds) + ")";
        }

        string query = "SELECT DISTINCT b.book_id, " +
                                      " b.book_title, " +
                                      " a.author_name, " +
                                      " ISNULL(c.collection_name,'') as collection_name, " +
                                      " ISNULL(b.notes,'') as notes, " +
                                      " a.author_id, " +
                                      " ISNULL(ba.collection_id,0) as collection_id" +
                              " FROM books_authors as ba " +
                             "  INNER JOIN books as b " +
                             "  ON ba.book_id = b.book_id " +
                             "  INNER JOIN books_authors as bba " +
                             "  ON bba.book_id = ba.book_id " +
                             "  INNER JOIN authors as a " +
                             " ON bba.author_id = a.author_id " +
                             " LEFT OUTER JOIN collections c " +
                             " ON c.collection_id = ba.collection_id " +
                              where + " ORDER BY b.book_title, a.author_name;";

        // Get result from database
        SqlConnection myConnection = new SqlConnection(ConnectionString);
        SqlCommand myCommand = new SqlCommand(query, myConnection);
        SqlDataReader reader = null;

        try
        {
            myConnection.Open();
            reader = myCommand.ExecuteReader();

            if (reader.HasRows)
            {

                while (reader.Read())
                {
                    books.FillBookRow(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(5), reader.GetString(2), reader.GetInt32(6), reader.GetString(3), reader.GetString(4));
                }
            }
            else
            {
                books.EmtpyBookRow();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            if (books != null) books.Dispose();
            return null;
        }
        finally
        {
            if (reader != null)
            {
                if (reader.IsClosed == false) reader.Close();
                reader.Dispose();
                reader = null;
            }
            if (myConnection != null)
            {
                if (myConnection.State == ConnectionState.Open) myConnection.Close();
                myConnection.Dispose();
                myConnection = null;
            }
            if (myCommand != null)
            {
                myCommand.Dispose();
                myCommand = null;
            }
        }

        return books.GetFormattedBook();
    }

    private static string ConnectionString
    {
        get { return ConfigurationManager.ConnectionStrings["connStr"].ToString(); }
    }


    public class BookTable : DataTable
    {
        public BookTable() : base("books")
        {
            DataColumn col = null;

            col = new DataColumn("BookID");
            col.DataType = System.Type.GetType("System.Int32");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("BookTitle");
            col.DataType = System.Type.GetType("System.String");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("AuthorID");
            col.DataType = System.Type.GetType("System.Int32");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("AuthorName");
            col.DataType = System.Type.GetType("System.String");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("CollectionID");
            col.DataType = System.Type.GetType("System.Int32");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("CollectionName");
            col.DataType = System.Type.GetType("System.String");
            this.Columns.Add(col);
            col.Dispose();

            col = new DataColumn("BookNotes");
            col.DataType = System.Type.GetType("System.String");
            this.Columns.Add(col);
            col.Dispose();
        }

        public void EmtpyBookRow()
        {
            DataRow row = null;
            row = this.NewRow();
            row["BookID"] = 0;
            row["BookTitle"] = "";
            row["AuthorID"] = 0;
            row["AuthorName"] = "";
            row["CollectionID"] = 0;
            row["CollectionName"] = "";
            row["BookNotes"] = "";

            this.Rows.Add(row);
        }

        public void FillBookRow(int bookId, string bookName, int authorId, string authorName, 
            int collectionId = 0, string collectionName = "", string bookNotes = "")
        {
            DataRow row = null;
            row = this.NewRow();
            row["BookID"] = bookId;
            row["BookTitle"] = bookName;
            row["AuthorID"] = authorId;
            row["AuthorName"] = authorName;
            row["CollectionID"] = collectionId;
            row["CollectionName"] = collectionName;
            row["BookNotes"] = bookNotes;
            
            this.Rows.Add(row);
        }

        public DataTable GetFormattedBook()
        {
            DataTable FormattedBooks = new BookTable();
            int previousBookId = -1;
            int currentBookId = 0;
            string currentBookTitle = "";
            string currentBookNotes = "";
            string currentAuthor = "";
            string currenctCollection = "";
            ArrayList formattedAuthors = new ArrayList();
            ArrayList formattedCollections = new ArrayList();
            DataRow formattedRow = null;
           
            foreach (DataRow row in this.Rows)
            {
                // save current data
                currentBookId = (int)row["BookId"];
                currentBookTitle = row["BookTitle"].ToString();
                currentBookNotes = row["BookNotes"].ToString();
                currentAuthor = row["AuthorName"].ToString();
                currenctCollection = row["CollectionName"].ToString();

                // save previous rows data into DataTable + formate authors and collections
                if (currentBookId != previousBookId && previousBookId != -1)
                {                   
                    // format authors and collections data, and save them into formattedRow
                    formattedRow["AuthorName"] = String.Join(", ", formattedAuthors.ToArray());
                    formattedRow["CollectionName"] = String.Join(", ", formattedCollections.ToArray());

                    // add formattedRow with data from previous datarow into formatted DataTable
                    FormattedBooks.Rows.Add(formattedRow);

                    // clear formatted arrays for next row
                    formattedAuthors = new ArrayList();
                    formattedCollections = new ArrayList();
                }

                if (currentBookId == previousBookId)
                {
                    if (!formattedAuthors.Contains(currentAuthor))
                    {
                        formattedAuthors.Add(currentAuthor);
                    }

                    if ((currenctCollection != "") 
                        && (!formattedCollections.Contains(currenctCollection)))
                    {
                        formattedCollections.Add(currenctCollection);
                    }
                }
                else
                {
                    // if currentBookId != previousBookId - save current data to formattedRow collection
                    formattedRow = FormattedBooks.NewRow();
                    formattedRow["BookID"] = currentBookId;
                    formattedRow["BookTitle"] = currentBookTitle;
                    //formattedRow["AuthorName"] = currentAuthor;
                    //formattedRow["CollectionName"] = currenctCollection;
                    formattedRow["BookNotes"] = currentBookNotes;

                    //save data into arrays if there are more that one rows with the same bookId
                    formattedAuthors.Add(currentAuthor);
                    formattedCollections.Add(currenctCollection);
                }
                previousBookId = currentBookId;
            }

            //format and add last row
            {
                // format authors and collections data, and save them into formattedRow
                formattedRow["AuthorName"] = String.Join(", ", formattedAuthors.ToArray());
                formattedRow["CollectionName"] = String.Join(", ", formattedCollections.ToArray());

                // add formattedRow with data from previous datarow into formatted DataTable
                FormattedBooks.Rows.Add(formattedRow);
            }

            return FormattedBooks;
        }
    }
}