using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TaskListCrud
{
    public static TaskList SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TaskList> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TaskList> TableToList(DataTable table)
    {
        var retVal = new List<TaskList>();
        foreach (DataRow row in table.Rows)
        {
            var taskList = new TaskList
            {
                TaskListNum = SIn.Long(row["TaskListNum"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                Parent = SIn.Long(row["Parent"].ToString()),
                DateTL = SIn.Date(row["DateTL"].ToString()),
                IsRepeating = SIn.Bool(row["IsRepeating"].ToString()),
                DateType = (TaskDateType) SIn.Int(row["DateType"].ToString()),
                FromNum = SIn.Long(row["FromNum"].ToString()),
                ObjectType = (TaskObjectType) SIn.Int(row["ObjectType"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                GlobalTaskFilterType = (EnumTaskFilterType) SIn.Int(row["GlobalTaskFilterType"].ToString()),
                TaskListStatus = (TaskListStatusEnum) SIn.Int(row["TaskListStatus"].ToString())
            };
            retVal.Add(taskList);
        }

        return retVal;
    }

    public static void Insert(TaskList taskList)
    {
        var command = "INSERT INTO tasklist (";

        command += "Descript,Parent,DateTL,IsRepeating,DateType,FromNum,ObjectType,DateTimeEntry,GlobalTaskFilterType,TaskListStatus) VALUES(";

        command +=
            "'" + SOut.String(taskList.Descript) + "',"
            + SOut.Long(taskList.Parent) + ","
            + SOut.Date(taskList.DateTL) + ","
            + SOut.Bool(taskList.IsRepeating) + ","
            + SOut.Int((int) taskList.DateType) + ","
            + SOut.Long(taskList.FromNum) + ","
            + SOut.Int((int) taskList.ObjectType) + ","
            + "NOW()" + ","
            + SOut.Int((int) taskList.GlobalTaskFilterType) + ","
            + SOut.Int((int) taskList.TaskListStatus) + ")";
        {
            taskList.TaskListNum = Db.NonQ(command, true, "TaskListNum", "taskList");
        }
    }

    public static void Update(TaskList taskList)
    {
        var command = "UPDATE tasklist SET "
                      + "Descript            = '" + SOut.String(taskList.Descript) + "', "
                      + "Parent              =  " + SOut.Long(taskList.Parent) + ", "
                      + "DateTL              =  " + SOut.Date(taskList.DateTL) + ", "
                      + "IsRepeating         =  " + SOut.Bool(taskList.IsRepeating) + ", "
                      + "DateType            =  " + SOut.Int((int) taskList.DateType) + ", "
                      + "FromNum             =  " + SOut.Long(taskList.FromNum) + ", "
                      + "ObjectType          =  " + SOut.Int((int) taskList.ObjectType) + ", "
                      + "DateTimeEntry       =  " + SOut.DateTime(taskList.DateTimeEntry) + ", "
                      + "GlobalTaskFilterType=  " + SOut.Int((int) taskList.GlobalTaskFilterType) + ", "
                      + "TaskListStatus      =  " + SOut.Int((int) taskList.TaskListStatus) + " "
                      + "WHERE TaskListNum = " + SOut.Long(taskList.TaskListNum);
        Db.NonQ(command);
    }

    public static void Update(TaskList taskList, TaskList oldTaskList)
    {
        var command = "";
        if (taskList.Descript != oldTaskList.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = '" + SOut.String(taskList.Descript) + "'";
        }

        if (taskList.Parent != oldTaskList.Parent)
        {
            if (command != "") command += ",";
            command += "Parent = " + SOut.Long(taskList.Parent) + "";
        }

        if (taskList.DateTL.Date != oldTaskList.DateTL.Date)
        {
            if (command != "") command += ",";
            command += "DateTL = " + SOut.Date(taskList.DateTL) + "";
        }

        if (taskList.IsRepeating != oldTaskList.IsRepeating)
        {
            if (command != "") command += ",";
            command += "IsRepeating = " + SOut.Bool(taskList.IsRepeating) + "";
        }

        if (taskList.DateType != oldTaskList.DateType)
        {
            if (command != "") command += ",";
            command += "DateType = " + SOut.Int((int) taskList.DateType) + "";
        }

        if (taskList.FromNum != oldTaskList.FromNum)
        {
            if (command != "") command += ",";
            command += "FromNum = " + SOut.Long(taskList.FromNum) + "";
        }

        if (taskList.ObjectType != oldTaskList.ObjectType)
        {
            if (command != "") command += ",";
            command += "ObjectType = " + SOut.Int((int) taskList.ObjectType) + "";
        }

        if (taskList.DateTimeEntry != oldTaskList.DateTimeEntry)
        {
            if (command != "") command += ",";
            command += "DateTimeEntry = " + SOut.DateTime(taskList.DateTimeEntry) + "";
        }

        if (taskList.GlobalTaskFilterType != oldTaskList.GlobalTaskFilterType)
        {
            if (command != "") command += ",";
            command += "GlobalTaskFilterType = " + SOut.Int((int) taskList.GlobalTaskFilterType) + "";
        }

        if (taskList.TaskListStatus != oldTaskList.TaskListStatus)
        {
            if (command != "") command += ",";
            command += "TaskListStatus = " + SOut.Int((int) taskList.TaskListStatus) + "";
        }

        if (command == "") return;
        command = "UPDATE tasklist SET " + command
                                         + " WHERE TaskListNum = " + SOut.Long(taskList.TaskListNum);
        Db.NonQ(command);
    }
}