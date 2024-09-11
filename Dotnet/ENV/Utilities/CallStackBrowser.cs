using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ENV.Data.DataProvider;
using ENV.Utilities;
using Firefly.Box;
using ENV.Data;
using Firefly.Box.Advanced;
using Firefly.Box.Data.Advanced;

namespace ENV.UI
{
}

namespace ENV.Utilities
{
    public class CallStackBrowser
    {
        static T Cast<T>(ITask t, Func<BusinessProcess, T> bp, Func<UIController, T> uic, Func<ModuleController, T> mc)
        {
            {
                var b = t as BusinessProcess;
                if (b != null)
                    return bp(b);
            }
            {
                var b = t as UIController;
                if (b != null)
                    return uic(b);
            }
            {
                var b = t as ModuleController;
                if (b != null)
                    return mc(b);
            }
            return default(T);
        }

        public static void Run()
        {
            bool done = false;
            Common.RunOnContextTopMostForm(y =>
                                   {
                                       if (y.ActiveControl != null)
                                       {
                                           done = true;
                                           Common.RunOnLogicContext(y.ActiveControl, ShowTheStack);
                                       }

                                   });
            if (!done)
                ShowTheStack();
        }

        static void ShowTheStack()
        {
            var activeTasks = Firefly.Box.Context.Current.ActiveTasks;
            var e = new Entity("Active Tasks", new MemoryDatabase());
            var nc = new NumberColumn("depth", "5");
            var prgNum = new NumberColumn("prgNum", "5Z");
            var taskName = new TextColumn("name", "100");
            var taskStatus = new TextColumn("Activity", "10");
            var inTransaction = new BoolColumn("Trans","5");
            var transactionStrategy = new TextColumn("Transaction", "10");
            var locking = new TextColumn("Locking", "10");
            var TypeName = new TextColumn("TypeName", "40","ClassName");
            var LongTypeName = new TextColumn("LongTypeName", "200", "LongClassName");
            e.Columns.Add(nc, prgNum, taskName, taskStatus, inTransaction, transactionStrategy, locking, prgNum, TypeName, LongTypeName);
            e.SetPrimaryKey(nc);
            int i = 0;
            foreach (var activeTask in activeTasks)
            {
                new BusinessProcess { From = e, Activity = Activities.Insert }.
                    ForFirstRow(() =>
                                {
                                    nc.Value = i++;
                                    taskName.Value = ENV.UserMethods.GetControllerName(activeTask);
                                    taskStatus.Value = activeTask.Activity.ToString();
                                    inTransaction.Value = activeTask.InTransaction;
                                    transactionStrategy.Value = Cast(activeTask, x => x.TransactionScope.ToString(),
                                        x => x.TransactionScope.ToString(),
                                        x => "");
                                    locking.Value = Cast(activeTask, x => x.RowLocking.ToString(),
                                        x => x.RowLocking.ToString(), x => "");
                                    ControllerBase.SendInstanceBasedOnTaskAndCallStack(activeTask,
                                        @base =>
                                        {
                                            var t = @base.GetType();
                                            if (@base._application != null)
                                                prgNum.Value = @base._application.AllPrograms.IndexOf(t);
                                            TypeName.Value = t.Name;
                                            LongTypeName.Value = t.FullName;
                                            

                                        });
                                });
            }
            var eb = new EntityBrowser(e);
            eb.OrderBy.Add(nc, SortDirection.Descending);
            eb.AddAction("Dataview", () => ShowDataview(activeTasks[nc]), true); 
            eb.AddAction("Columns", () => { ShowColumns(activeTasks[nc]); });
            eb.AddAction("Show Parameters", () =>
                                            {
                                                ControllerBase.SendInstanceBasedOnTaskAndCallStack(activeTasks[nc],
                                                    @base =>
                                                    {
                                                        using (var sw = new StringWriter())
                                                        {
                                                            sw.WriteLine("{0}({1})", LongTypeName.Trim(), prgNum.ToString().Trim());
                                                            sw.WriteLine();
                                                            @base.SendParametersTo((name, value) =>
                                                                                   {
                                                                                       sw.WriteLine(name + ":" + value);
                                                                                   });
                                                            EntityBrowser.ShowString(@base.GetType().FullName, sw.ToString());
                                                        }

                                                    });
                                            });
            eb.AddAction("Save Parameters", () =>
            {
                ControllerBase.SendInstanceBasedOnTaskAndCallStack(activeTasks[nc],
                    @base =>
                    {
                        var l = new List<string>();
                        @base.SendParametersTo((name, value) =>
                                               {
                                                   l.Add(value);
                                               });

                      
                        ProgramRunner.SaveParameters(l.ToArray(), @base.GetType(),true);

                    });
            });




            eb.Run();
        }

        static void ShowColumns(ITask task)
        {

            var e = new Entity("Column", new MemoryDatabase());
            var nc = new NumberColumn("Index", "5");

            var name = new TextColumn("name", "100");
            var value = new TextColumn("Value", "100");
            var type = new TextColumn("Type", "20");
            var entity = new TextColumn("entity caption", "100");
            var dbName = new TextColumn("dbName", "100");



            e.Columns.Add(name, value, type, entity, dbName);
            e.SetPrimaryKey(nc);
            int i = 0;

            foreach (var column in task.Columns)
            {
                new BusinessProcess { From = e, Activity = Activities.Insert }.ForFirstRow(
                    () =>
                    {
                        nc.Value = i++;
                        name.Value = column.Caption;
                        value.Value = column.ToString();
                        type.Value = column.GetType().Name;
                        if (column.Entity != null)
                        {
                            entity.Value = column.Entity.Caption;
                            dbName.Value = column.Entity.EntityName + "." + column.Name;
                        }
                    });
            }
            var eb = new EntityBrowser(e);

            eb.Run();



        }
        static void ShowDataview(ITask task)
        {
            var e = new Entity("Dataview of " + ENV.UserMethods.GetControllerName(task), new MemoryDatabase());
            var nc = new NumberColumn("Index", "5");
            var caption = new TextColumn("caption", "100");
            var type = new TextColumn("Type", "15");
            var found = new TextColumn("Found", "5");
            var where = new TextColumn("Where", "2000");
            var name = new TextColumn("name", "100");

            var rowLocking = new TextColumn("RowLocking", "10");
            e.Columns.Add(nc, caption, type, found, where, name);
            e.SetPrimaryKey(nc);
            int i = 0;
            var from = Cast<Firefly.Box.Data.Entity>(task, t => t.From, t => t.From, t => null);

            if (from != null)
            {
                var whereFrom = Cast<FilterCollection>(task, t => t.Where, t => t.Where, t => null);
                e.Insert(() =>
                {
                    nc.Value = i++;
                    caption.Value = from.Caption;
                    type.Value = "From";
                    name.Value = from.EntityName;
                    rowLocking.Value = from.AllowRowLocking ? "Lock" : "";
                    where.Value = FilterHelper.ToSQLWhere(whereFrom, true, from);
                });
            }
            foreach (var relation in Cast(task, x => x.Relations, x => x.Relations, x => x.Relations))
            {
                e.Insert(
                    () =>
                    {
                        nc.Value = i++;
                        caption.Value = relation.From.Caption;
                        type.Value = relation.Type.ToString();
                        found.Value = relation.RowFound ? "Found" : "";
                        name.Value = relation.From.EntityName;
                        rowLocking.Value = relation.From.AllowRowLocking ? "Lock" : "";
                        where.Value = FilterHelper.ToSQLWhere(relation.Where, false, relation.From);
                    });
            }
            var eb = new EntityBrowser(e);
            eb.AddAction("Show Entity Data", () =>
            {
                try
                {
                    var x =
                        System.Activator.CreateInstance(task.Entities[nc].GetType()) as
                        Firefly.Box.Data.Entity;
                    if (x != null)
                        new EntityBrowser(x).Run();
                }
                catch
                {
                }
            }, true);
            eb.Run();



        }
        static void ShowEntities(ITask task)
        {
            var e = new Entity("Entities", new MemoryDatabase());
            var nc = new NumberColumn("Index", "5");
            var caption = new TextColumn("caption", "100");
            var name = new TextColumn("name", "100");
            var rowLocking = new TextColumn("RowLocking", "10");
            e.Columns.Add(nc, caption, name);
            e.SetPrimaryKey(nc);
            int i = 0;
            foreach (var entity in task.Entities)
            {
                new BusinessProcess { From = e, Activity = Activities.Insert }.ForFirstRow(
                    () =>
                    {
                        nc.Value = i++;
                        caption.Value = entity.Caption;
                        name.Value = entity.EntityName;
                        rowLocking.Value = entity.AllowRowLocking ? "Lock" : "";
                    });
            }
            var eb = new EntityBrowser(e);
            eb.AddAction("Show Data", () =>
            {
                try
                {
                    var x =
                        System.Activator.CreateInstance(task.Entities[nc].GetType()) as
                        Firefly.Box.Data.Entity;
                    if (x != null)
                        new EntityBrowser(x).Run();
                }
                catch
                {
                }
            }, true);
            eb.Run();



        }



    }
}
