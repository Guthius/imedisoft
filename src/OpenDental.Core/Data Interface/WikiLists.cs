using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;

namespace OpenDentBusiness;


public class WikiLists
{
    public static bool CheckExists(string listName)
    {
        var command = "SHOW TABLES LIKE 'wikilist\\_" + SOut.String(listName) + "'";
        if (DataCore.GetTable(command).Rows.Count == 1)
            //found exacty one table with that name
            return true;

        //no table found with that name
        return false;
    }

    public static DataTable GetByName(string listName)
    {
        return GetByName(listName, "");
    }

    public static DataTable GetByName(string listName, string orderBy)
    {
        var command = "SELECT * FROM wikilist_" + SOut.String(listName);
        if (!string.IsNullOrEmpty(orderBy))
            command += " ORDER BY " + SOut.String(orderBy); //Manual ovverride of Order By
        else
            using (var tableDescript = DataCore.GetTable("DESCRIBE wikilist_" + SOut.String(listName)))
            {
                if (tableDescript.Rows.Count == 1)
                    command += " ORDER BY " + tableDescript.Rows[0]["Field"]; //order by PK
                else if (tableDescript.Rows.Count > 1) command += " ORDER BY " + tableDescript.Rows[1]["Field"]; //order by the second column, even though we show the primary key
            }

        return DataCore.GetTable(command);
    }

    /// <summary>
    ///     Creates empty table with a column for PK and optionally the columns in listHeaders. List name must be formatted
    ///     correctly before
    ///     being passed here, i.e. no spaces, all lowercase.  If listHeaders is not null or empty, they will be inserted into
    ///     the db.
    ///     If dropTableIfExists==true, the table with name wikilist_listName will be dropped if it exists and any
    ///     wikilistheaderwidth rows for the table
    ///     will be deleted before creating the new table and inserting any new wikilistheaderwidth rows.
    /// </summary>
    public static void CreateNewWikiList(string listName, List<WikiListHeaderWidth> listWikiListHeaderWidths = null, bool dropTable = false)
    {
        var listStringColDefs = new List<string>();
        if (listWikiListHeaderWidths.IsNullOrEmpty())
        {
            var wikiListHeaderWidth = new WikiListHeaderWidth();
            wikiListHeaderWidth.ListName = listName;
            wikiListHeaderWidth.ColName = listName + "Num";
            wikiListHeaderWidth.ColWidth = 100;
            listWikiListHeaderWidths = new List<WikiListHeaderWidth> {wikiListHeaderWidth};
        }

        listStringColDefs.Add($"{SOut.String(listWikiListHeaderWidths[0].ColName)} bigint NOT NULL auto_increment PRIMARY KEY"); //listHeaders guaranteed to not be null or empty
        listStringColDefs.AddRange(listWikiListHeaderWidths.Skip(1).Select(x => $"{SOut.String(x.ColName)} TEXT NOT NULL")); //first in listHeaders added as PK already
        var command = "";
        if (dropTable)
        {
            command += $"DROP TABLE IF EXISTS wikilist_{SOut.String(listName)}; ";
            WikiListHeaderWidths.DeleteForList(listName);
        }

        command += $@"CREATE TABLE wikilist_{SOut.String(listName)} (
					{string.Join(@",
					", listStringColDefs)}
					) DEFAULT CHARSET=utf8";
        Db.NonQ(command);
        WikiListHeaderWidths.InsertMany(listWikiListHeaderWidths);
    }

    ///<summary>Column is automatically named "Column#" where # is the number of columns+1.</summary>
    public static void AddColumn(string listName)
    {
        //Find Valid column name-----------------------------------------------------------------------------------------
        var tableColumnNames = DataCore.GetTable("DESCRIBE wikilist_" + SOut.String(listName));
        var newColumnName = "Column1"; //default in case table has no columns. Should never happen.
        for (var i = 0; i < tableColumnNames.Rows.Count + 1; i++)
        {
            //+1 to guarantee we can find a valid name.
            newColumnName = "Column" + (1 + i); //ie. Column1, Column2, Column3...
            for (var j = 0; j < tableColumnNames.Rows.Count; j++)
                if (newColumnName == tableColumnNames.Rows[j]["Field"].ToString())
                {
                    newColumnName = "";
                    break;
                }

            if (newColumnName != "") break; //found a valid name.
        }

        if (newColumnName == "")
            //should never happen.
            throw new ApplicationException("Could not create valid column name.");

        //Add new column name--------------------------------------------------------------------------------------------
        var command = "ALTER TABLE wikilist_" + SOut.String(listName) + " ADD COLUMN " + SOut.String(newColumnName) + " TEXT NOT NULL";
        Db.NonQ(command);
        //Add column widths to wikiListHeaderWidth Table-----------------------------------------------------------------
        var wikiListHeaderWidth = new WikiListHeaderWidth();
        wikiListHeaderWidth.ColName = newColumnName;
        wikiListHeaderWidth.ListName = listName;
        wikiListHeaderWidth.ColWidth = 100;
        WikiListHeaderWidths.InsertNew(wikiListHeaderWidth);
    }

    ///<summary>Check to see if column can be deleted, returns true is the column contains only nulls.</summary>
    public static bool CheckColumnEmpty(string listName, string colName)
    {
        var command = "SELECT COUNT(*) FROM wikilist_" + SOut.String(listName) + " WHERE " + SOut.String(colName) + "!=''";
        return Db.GetCount(command).Equals("0");
    }

    ///<summary>Check to see if column can be deleted, returns true is the column contains only nulls.</summary>
    public static void DeleteColumn(string listName, string colName)
    {
        var command = "ALTER TABLE wikilist_" + SOut.String(listName) + " DROP " + SOut.String(colName);
        Db.NonQ(command);
        WikiListHeaderWidths.Delete(listName, colName);
    }

    /// <summary>Shifts the column to the left, does nothing if trying to shift leftmost two columns.</summary>
    public static void ShiftColumnLeft(string listName, string colName)
    {
        var tableColumnNames = DataCore.GetTable("DESCRIBE wikilist_" + SOut.String(listName));
        if (tableColumnNames.Rows.Count < 3) return; //not enough columns to reorder.

        var index = tableColumnNames.Select().ToList().FindIndex(x => x[0].ToString() == colName);
        if (index > 1 && index < tableColumnNames.Rows.Count)
        {
            var command = $@"ALTER TABLE wikilist_{SOut.String(listName)}
					MODIFY {SOut.String(colName)} TEXT NOT NULL AFTER {SOut.String(tableColumnNames.Rows[index - 2][0].ToString())}";
            Db.NonQ(command);
        }
    }

    /// <summary>Shifts the column to the right, does nothing if trying to shift the rightmost column.</summary>
    public static void ShiftColumnRight(string listName, string colName)
    {
        var tableColumnNames = DataCore.GetTable("DESCRIBE wikilist_" + SOut.String(listName));
        if (tableColumnNames.Rows.Count < 3) return; //not enough columns to reorder.

        var index = tableColumnNames.Select().ToList().FindIndex(x => x[0].ToString() == colName);
        if (index > 0 && index < tableColumnNames.Rows.Count - 1)
        {
            var command = $@"ALTER TABLE wikilist_{SOut.String(listName)}
					MODIFY {SOut.String(colName)} TEXT NOT NULL AFTER {SOut.String(tableColumnNames.Rows[index + 1][0].ToString())}";
            Db.NonQ(command);
        }
    }

    ///<summary>Adds one item to wiki list and returns the new PK.</summary>
    public static long AddItem(string listName)
    {
        var command = "INSERT INTO wikilist_" + SOut.String(listName) + " VALUES ()"; //inserts empty row with auto generated PK.
        return Db.NonQ(command, true);
    }

    /// <summary></summary>
    /// <param name="tableItems">Should be a DataTable object with a single DataRow containing the item.</param>
    public static void UpdateItem(string listName, DataTable tableItems)
    {
        if (tableItems.Columns.Count < 2)
            //if the table contains only a PK column.
            return;

        var listStringRowSets = tableItems.Columns.OfType<DataColumn>().Skip(1) //skip 1 because we do not need to update the PK
            .Select(x => SOut.String(x.ColumnName) + $"='{SOut.String(tableItems.Rows[0][x].ToString())}'").ToList();
        var command = $@"UPDATE wikilist_{SOut.String(listName)} SET {string.Join(@",
				", listStringRowSets)}
				WHERE {SOut.String(tableItems.Columns[0].ColumnName)}={SOut.Long(SIn.Long(tableItems.Rows[0][0].ToString()))}";
        Db.NonQ(command);
    }

    public static DataTable GetItem(string listName, long itemNum, string colName = null)
    {
        colName = SOut.String(string.IsNullOrEmpty(colName) ? listName + "Num" : colName);
        var command = $"SELECT * FROM wikilist_{SOut.String(listName)} WHERE {colName}={SOut.Long(itemNum)}";
        return DataCore.GetTable(command);
    }

    public static void DeleteItem(string listName, long itemNum, string colName = null)
    {
        colName = SOut.String(string.IsNullOrEmpty(colName) ? listName + "Num" : colName);
        var command = $@"DELETE FROM wikilist_{SOut.String(listName)} WHERE {colName}={SOut.Long(itemNum)}";
        Db.NonQ(command);
    }

    public static void DeleteList(string listName)
    {
        var command = "DROP TABLE wikilist_" + SOut.String(listName);
        Db.NonQ(command);
        WikiListHeaderWidths.DeleteForList(listName);
    }

    public static List<string> GetAllLists()
    {
        var listStringWikiList = new List<string>();
        var command = "SHOW TABLES LIKE 'wikilist\\_%'"; //must escape _ (underscore) otherwise it is interpreted as a wildcard character.
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++) listStringWikiList.Add(table.Rows[i][0].ToString());

        return listStringWikiList;
    }

    /// <summary>
    ///     <para>
    ///         Surround with try catch.  Safely renames list by creating new list, selecting existing list into new list,
    ///         then deleting existing list.
    ///     </para>
    ///     <para>This code could be used to either copy or backup lists in the future. (With minor modifications).</para>
    /// </summary>
    public static void Rename(string nameOriginal, string nameNew)
    {
        //Name should already have been validated and available.
        var command = "CREATE TABLE wikilist_" + SOut.String(nameNew) + " AS SELECT * FROM wikilist_" + SOut.String(nameOriginal);
        Db.NonQ(command);
        //Validate content before altering and deleting things
        var tableNewNames = GetByName(nameNew);
        var tableOldNames = GetByName(nameOriginal);
        if (tableNewNames.Rows.Count != tableOldNames.Rows.Count)
        {
            command = "DROP TABLE wikilist_" + SOut.String(nameNew);
            Db.NonQ(command);
            throw new Exception("Error occurred renaming list.  Mismatch found in row count. No changes made.");
        }

        if (tableNewNames.Columns.Count != tableOldNames.Columns.Count)
        {
            command = "DROP TABLE wikilist_" + SOut.String(nameNew);
            Db.NonQ(command);
            throw new Exception("Error occurred renaming list.  Mismatch found in column count. No changes made.");
        }

        for (var r1 = 0; r1 < tableNewNames.Rows.Count; r1++)
        for (var r2 = 0; r2 < tableOldNames.Rows.Count; r2++)
        {
            if (tableNewNames.Rows[r1][0] != tableOldNames.Rows[r2][0]) continue; //pk does not match

            for (var c = 0; c < tableNewNames.Columns.Count; c++)
            {
                //both lists have same number of columns
                if (tableNewNames.Rows[r1][c] == tableOldNames.Rows[r2][c]) continue; //contents match

                throw new Exception("Error occurred renaming list.  Mismatch Error found in row data. No changes made.");
            } //end columns
        } //end tableOld

        //end tableNew
        //Alter table names----------------------------------------------------------------------------
        var priKeyColNameOrig = SOut.String(nameOriginal) + "Num";
        if (!tableNewNames.Columns.Contains(priKeyColNameOrig))
            //if new table doesn't contain a PK based on the old table name, make the first column the nameNew+"Num" PK column
            priKeyColNameOrig = SOut.String(tableNewNames.Columns[0].ColumnName);
        command = "ALTER TABLE wikilist_" + SOut.String(nameNew) + " CHANGE " + priKeyColNameOrig + " " + SOut.String(nameNew) + "Num bigint NOT NULL auto_increment PRIMARY KEY";
        Db.NonQ(command);
        command = "UPDATE wikilistheaderwidth SET ListName='" + SOut.String(nameNew) + "' WHERE ListName='" + SOut.String(nameOriginal) + "'";
        Db.NonQ(command);
        command = $@"UPDATE wikilistheaderwidth SET ColName='{SOut.String(nameNew)}Num'
				WHERE ListName='{SOut.String(nameNew)}' AND ColName='{priKeyColNameOrig}'";
        Db.NonQ(command);
        //drop old table---------------------
        command = "DROP TABLE wikilist_" + SOut.String(nameOriginal);
        Db.NonQ(command);
        WikiListHeaderWidths.RefreshCache();
    }
}