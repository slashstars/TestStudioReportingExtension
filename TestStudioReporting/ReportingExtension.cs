using System.Data;
using ArtOfTest.WebAii.Design.Execution;
using ArtOfTest.WebAii.Design;
using ArtOfTest.WebAii.Design.ProjectModel;
using System;
using ArtOfTest.Common.Design;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TestStudioReporting
{
    public class ReportingExtension : IExecutionExtension
    {
        private static readonly string ConnectionString = @"Data Source=SERVER;Initial Catalog=DATABASE;Integrated Security=False;User Id=User; Password=*****;";

        private Guid CurrentRunResultId = Guid.Empty;
        private Stack<Guid> testResultStack = new Stack<Guid>();

        public void OnAfterTestListCompleted(RunResult result)
        {
            if (CurrentRunResultId == Guid.Empty) return;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand updateRunResult = GetUpdateRunResult(conn))
                {
                    Console.WriteLine("Commit run result to database...");

                    try
                    {
                        updateRunResult.AssignParameterValue("@Id", CurrentRunResultId);
                        updateRunResult.AssignParameterValue("@TestListId", result.TestListId);
                        updateRunResult.AssignParameterValue("@Name", result.Name);
                        updateRunResult.AssignParameterValue("@FileName", result.FileName);
                        updateRunResult.AssignParameterValue("@Passed", result.PassedResult);
                        updateRunResult.AssignParameterValue("@Summary", result.Summary);
                        updateRunResult.AssignParameterValue("@Comment", result.Comment);
                        updateRunResult.AssignParameterValue("@StartTime", result.StartTime);
                        updateRunResult.AssignParameterValue("@EndTime", result.EndTime);
                        updateRunResult.AssignParameterValue("@IsManual", result.IsManual);
                        updateRunResult.AssignParameterValue("@AllCount", result.AllCount);
                        updateRunResult.AssignParameterValue("@NotRunCount", result.NotRunCount);
                        updateRunResult.AssignParameterValue("@PassedCount", result.PassedCount);
                        updateRunResult.AssignParameterValue("@FailedCount", result.FailedCount);

                        updateRunResult.ExecuteNonQuery();

                        CurrentRunResultId = Guid.Empty;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        public void OnStepFailure(ExecutionContext executionContext, AutomationStepResult stepResult)
        {
            if (testResultStack.Count <= 0) return;
            var currentTestResultId = testResultStack.Peek();
            
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand insertData = GetInsertStepResultData(conn),
                    insertStepResult = GetInsertStepResult(conn))
                {
                    Console.WriteLine("Test step failed capturing data...");

                    try
                    {
                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                                                                  
                            insertStepResult.Transaction = tran;
                            insertStepResult.AssignParameterValue("@Description", stepResult.StepDescription);
                            insertStepResult.AssignParameterValue("@Comment", stepResult.UserComment);
                            insertStepResult.AssignParameterValue("@Order", stepResult.Order);
                            insertStepResult.AssignParameterValue("@WasEnabled", stepResult.WasStepEnabled);
                            insertStepResult.AssignParameterValue("@IsManual", stepResult.IsManual);
                            insertStepResult.AssignParameterValue("@FailureException", stepResult.Exception == null ? string.Empty :
                                stepResult.Exception.Message + " : " + stepResult.Exception.StackTrace);
                            insertStepResult.AssignParameterValue("@ResultTypeId", (int)stepResult.ResultType);
                            insertStepResult.AssignParameterValue("@TestResultId", currentTestResultId);

                            insertStepResult.ExecuteNonQuery();

                            insertData.Transaction = tran;
                            insertData.AssignParameterValue("@Screenshot", executionContext.Manager.TakeScreenshot());
                            insertData.AssignParameterValue("@DOM", executionContext.Manager.CaptureDOM());
                            insertData.AssignParameterValue("@TestResultId", currentTestResultId);
                            insertData.AssignParameterValue(@"StepOrder", stepResult.Order);

                            insertData.ExecuteNonQuery();

                            tran.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }

        }

        public void OnBeforeTestListStarted(TestList list)
        {
            var runResultId = Guid.NewGuid();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand insertRunResult = GetInsertRunResult(conn))
                {
                    Console.WriteLine("Init run result to database...");

                    try
                    {
                        insertRunResult.AssignParameterValue("@Id", runResultId);
                        insertRunResult.AssignParameterValue("@TestListId", list.Id);
                        insertRunResult.AssignParameterValue("@Name", string.Empty);
                        insertRunResult.AssignParameterValue("@FileName", string.Empty);
                        insertRunResult.AssignParameterValue("@Passed", false);
                        insertRunResult.AssignParameterValue("@Summary", string.Empty);
                        insertRunResult.AssignParameterValue("@Comment", string.Empty);
                        insertRunResult.AssignParameterValue("@StartTime", null);
                        insertRunResult.AssignParameterValue("@EndTime", null);
                        insertRunResult.AssignParameterValue("@IsManual", false);
                        insertRunResult.AssignParameterValue("@AllCount", 0);
                        insertRunResult.AssignParameterValue("@NotRunCount", 0);
                        insertRunResult.AssignParameterValue("@PassedCount", 0);
                        insertRunResult.AssignParameterValue("@FailedCount", 0);

                        insertRunResult.ExecuteNonQuery();
                        CurrentRunResultId = runResultId;                    
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        public void OnBeforeTestStarted(ExecutionContext executionContext, Test test)
        {
            if (CurrentRunResultId == Guid.Empty) return;
            var testResultId = Guid.NewGuid();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var insertTestResult = GetInsertTestResult(conn))
                {
                    Console.WriteLine("Init test result to database...");

                    try
                    {
                        insertTestResult.AssignParameterValue("@Id", testResultId);
                        insertTestResult.AssignParameterValue("@Description", test.Description);
                        insertTestResult.AssignParameterValue("@Name", test.Name);
                        insertTestResult.AssignParameterValue("@Path", test.Path);
                        insertTestResult.AssignParameterValue("@TestId", string.Empty);
                        insertTestResult.AssignParameterValue("@Message", string.Empty);
                        insertTestResult.AssignParameterValue("@IsDataDriven", false);
                        insertTestResult.AssignParameterValue("@IsManual", false);
                        insertTestResult.AssignParameterValue("@StartTime", null);
                        insertTestResult.AssignParameterValue("@EndTime", null);
                        insertTestResult.AssignParameterValue("@FailureException", string.Empty);
                        insertTestResult.AssignParameterValue("@FailedStepComment", string.Empty);
                        insertTestResult.AssignParameterValue("@AllCount", 0);
                        insertTestResult.AssignParameterValue("@NotRunCount", 0);
                        insertTestResult.AssignParameterValue("@PassedCount", 0);
                        insertTestResult.AssignParameterValue("@BrowserTypeId", null);
                        insertTestResult.AssignParameterValue("@ResultTypeId", null);
                        insertTestResult.AssignParameterValue("@RunResultId", CurrentRunResultId);

                        insertTestResult.ExecuteNonQuery();
                        testResultStack.Push(testResultId);                          
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        public void OnAfterTestCompleted(ExecutionContext executionContext, TestResult result)
        {
            if (CurrentRunResultId == Guid.Empty || testResultStack.Count <= 0) return;
            var currentTestResultId = testResultStack.Pop();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (SqlCommand updateTestResult = GetUpdateTestResult(conn), 
                    insertStepResult = GetInsertStepResult(conn))
                {
                    Console.WriteLine("Commit test result to database...");

                    try
                    {
                        using (SqlTransaction tran = conn.BeginTransaction())
                        {
                            updateTestResult.Transaction = tran;
                            updateTestResult.AssignParameterValue("@Id", currentTestResultId);
                            updateTestResult.AssignParameterValue("@Description", result.TestDescription);
                            updateTestResult.AssignParameterValue("@Name", result.TestName);
                            updateTestResult.AssignParameterValue("@Path", result.TestPath);
                            updateTestResult.AssignParameterValue("@TestId", result.TestId);
                            updateTestResult.AssignParameterValue("@Message", result.Message);
                            updateTestResult.AssignParameterValue("@IsDataDriven", result.IsDataDrivenResult);
                            updateTestResult.AssignParameterValue("@IsManual", result.IsManualResult);
                            updateTestResult.AssignParameterValue("@StartTime", result.StartTime);
                            updateTestResult.AssignParameterValue("@EndTime", result.EndTime);
                            updateTestResult.AssignParameterValue("@FailureException", result.FailureException == null ? string.Empty :
                                result.FailureException.Message + " : " + result.FailureException.StackTrace);
                            updateTestResult.AssignParameterValue("@FailedStepComment", result.FirstFailedStepComment);
                            updateTestResult.AssignParameterValue("@AllCount", result.AllTestStepCount);
                            updateTestResult.AssignParameterValue("@NotRunCount", result.TotalNumberOfNotRunSteps);
                            updateTestResult.AssignParameterValue("@PassedCount", result.TotalPassedSteps);
                            updateTestResult.AssignParameterValue("@BrowserTypeId", (int)result.Browser);
                            updateTestResult.AssignParameterValue("@ResultTypeId", (int)result.Result);
                            updateTestResult.AssignParameterValue("@RunResultId", CurrentRunResultId);

                            updateTestResult.ExecuteNonQuery();

                            foreach (var stepResult in result.StepResults)
                            {
                                if (stepResult.ResultType == ResultType.Fail) continue;

                                insertStepResult.Transaction = tran;
                                insertStepResult.AssignParameterValue("@Description", stepResult.StepDescription);
                                insertStepResult.AssignParameterValue("@Comment", stepResult.UserComment);
                                insertStepResult.AssignParameterValue("@Order", stepResult.Order);
                                insertStepResult.AssignParameterValue("@WasEnabled", stepResult.WasStepEnabled);
                                insertStepResult.AssignParameterValue("@IsManual", stepResult.IsManual);
                                insertStepResult.AssignParameterValue("@FailureException", stepResult.Exception == null ? string.Empty :
                                    stepResult.Exception.Message + " : " + stepResult.Exception.StackTrace);
                                insertStepResult.AssignParameterValue("@ResultTypeId", (int)stepResult.ResultType);
                                insertStepResult.AssignParameterValue("@TestResultId", currentTestResultId);

                                insertStepResult.ExecuteNonQuery();
                            }

                            tran.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        #region Queries

        private SqlCommand GetInsertRunResult(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"INSERT INTO [dbo].[RunResult] ([Id],[TestListId],[Name],[FileName],[Passed],[Summary],[Comment],
		                                                [StartTime],[EndTime],[IsManual],[AllCount],[NotRunCount],[PassedCount],[FailedCount]) 
                                                    VALUES (@Id,@TestListId,@Name,@FileName,@Passed,@Summary,@Comment,@StartTime,@EndTime,@IsManual,
		                                                @AllCount,@NotRunCount,@PassedCount,@FailedCount)";
            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@TestListId", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@Name", SqlDbType.NVarChar);
            command.Parameters.Add("@FileName", SqlDbType.NVarChar);
            command.Parameters.Add("@Passed", SqlDbType.Bit);
            command.Parameters.Add("@Summary", SqlDbType.NVarChar);
            command.Parameters.Add("@Comment", SqlDbType.NVarChar);
            command.Parameters.Add("@StartTime", SqlDbType.DateTime);
            command.Parameters.Add("@EndTime", SqlDbType.DateTime);
            command.Parameters.Add("@IsManual", SqlDbType.Bit);
            command.Parameters.Add("@AllCount", SqlDbType.Int);
            command.Parameters.Add("@NotRunCount", SqlDbType.Int);
            command.Parameters.Add("@PassedCount", SqlDbType.Int);
            command.Parameters.Add("@FailedCount", SqlDbType.Int);

            return command;
        }
        private SqlCommand GetInsertTestResult(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"INSERT INTO [dbo].[TestResult] ([Id],[Description],[Name],[Path],[TestId],[Message],
                                                        [IsDataDriven],[IsManual],[StartTime],[EndTime],[FailureException],[FailedStepComment],
	                                                    [AllCount],[NotRunCount],[PassedCount],[BrowserTypeId],[ResultTypeId],[RunResultId]) 
                                                     VALUES (@Id,@Description,@Name,@Path,@TestId,@Message,@IsDataDriven,@IsManual,@StartTime,@EndTime,
                                                        @FailureException,@FailedStepComment,@AllCount,@NotRunCount,@PassedCount,@BrowserTypeId,
                                                        @ResultTypeId,@RunResultId)";

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@Description", SqlDbType.NVarChar);
            command.Parameters.Add("@Name", SqlDbType.NVarChar);
            command.Parameters.Add("@Path", SqlDbType.NVarChar);
            command.Parameters.Add("@TestId", SqlDbType.NVarChar);
            command.Parameters.Add("@Message", SqlDbType.NVarChar);
            command.Parameters.Add("@IsDataDriven", SqlDbType.Bit);
            command.Parameters.Add("@IsManual", SqlDbType.Bit);
            command.Parameters.Add("@StartTime", SqlDbType.DateTime);
            command.Parameters.Add("@EndTime", SqlDbType.DateTime);
            command.Parameters.Add("@FailureException", SqlDbType.NVarChar);
            command.Parameters.Add("@FailedStepComment", SqlDbType.NVarChar);
            command.Parameters.Add("@AllCount", SqlDbType.Int);
            command.Parameters.Add("@NotRunCount", SqlDbType.Int);
            command.Parameters.Add("@PassedCount", SqlDbType.Int);
            command.Parameters.Add("@BrowserTypeId", SqlDbType.Int);
            command.Parameters.Add("@ResultTypeId", SqlDbType.Int);
            command.Parameters.Add("@RunResultId", SqlDbType.UniqueIdentifier);

            return command;
        }
        private SqlCommand GetInsertStepResult(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"INSERT INTO [dbo].[StepResult] ([Description],[Comment],[Order],[WasEnabled],
                                                        [IsManual],[FailureException],[ResultTypeId],[TestResultId]) 
                                                     VALUES (@Description,@Comment,@Order,@WasEnabled,@IsManual,
	                                                    @FailureException,@ResultTypeId,@TestResultId)";

            command.Parameters.Add("@Description", SqlDbType.NVarChar);
            command.Parameters.Add("@Comment", SqlDbType.NVarChar);
            command.Parameters.Add("@Order", SqlDbType.Int);
            command.Parameters.Add("@WasEnabled", SqlDbType.Bit);
            command.Parameters.Add("@IsManual", SqlDbType.Bit);
            command.Parameters.Add("@FailureException", SqlDbType.NVarChar);
            command.Parameters.Add("@ResultTypeId", SqlDbType.Int);
            command.Parameters.Add("@TestResultId", SqlDbType.UniqueIdentifier);

            return command;
        }
        private SqlCommand GetInsertStepResultData(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"INSERT INTO [dbo].[StepResultData] ([Screenshot],[DOM],[TestResultId],[StepOrder])
                                    VALUES (@Screenshot,@DOM,@TestResultId,@StepOrder)";

            command.Parameters.Add("@Screenshot", SqlDbType.VarBinary);
            command.Parameters.Add("@DOM", SqlDbType.NVarChar);
            command.Parameters.Add("@TestResultId", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@StepOrder", SqlDbType.Int);

            return command;
        }
        private SqlCommand GetUpdateTestResult(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"UPDATE [dbo].[TestResult] SET [Description]=@Description,[Name]=@Name,[Path]=@Path,[TestId]=@TestId,
                                                [Message]=@Message,[IsDataDriven]=@IsDataDriven,[IsManual]=@IsManual,[StartTime]=@StartTime,
                                                [EndTime]=@EndTime,[FailureException]=@FailureException,[FailedStepComment]=@FailedStepComment,
                                                [AllCount]=@AllCount,[NotRunCount]=@NotRunCount,[PassedCount]=@PassedCount,[BrowserTypeId]=@BrowserTypeId,
                                                [ResultTypeId]=@ResultTypeId,[RunResultId]=@RunResultId WHERE [Id] = @Id";

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@Description", SqlDbType.NVarChar);
            command.Parameters.Add("@Name", SqlDbType.NVarChar);
            command.Parameters.Add("@Path", SqlDbType.NVarChar);
            command.Parameters.Add("@TestId", SqlDbType.NVarChar);
            command.Parameters.Add("@Message", SqlDbType.NVarChar);
            command.Parameters.Add("@IsDataDriven", SqlDbType.Bit);
            command.Parameters.Add("@IsManual", SqlDbType.Bit);
            command.Parameters.Add("@StartTime", SqlDbType.DateTime);
            command.Parameters.Add("@EndTime", SqlDbType.DateTime);
            command.Parameters.Add("@FailureException", SqlDbType.NVarChar);
            command.Parameters.Add("@FailedStepComment", SqlDbType.NVarChar);
            command.Parameters.Add("@AllCount", SqlDbType.Int);
            command.Parameters.Add("@NotRunCount", SqlDbType.Int);
            command.Parameters.Add("@PassedCount", SqlDbType.Int);
            command.Parameters.Add("@BrowserTypeId", SqlDbType.Int);
            command.Parameters.Add("@ResultTypeId", SqlDbType.Int);
            command.Parameters.Add("@RunResultId", SqlDbType.UniqueIdentifier);

            return command;
        }
        private SqlCommand GetUpdateRunResult(SqlConnection conn)
        {
            var command = conn.CreateCommand();

            command.CommandText = @"UPDATE [dbo].[RunResult] SET [TestListId]=@TestListId,[Name]=@Name,[FileName]=@FileName,[Passed]=@Passed,[Summary]=@Summary,
                                                        [Comment]=@Comment,[StartTime]=@StartTime,[EndTime]=@EndTime,[IsManual]=@IsManual,[AllCount]=@AllCount,
                                                        [NotRunCount]=@NotRunCount,[PassedCount]=@PassedCount,[FailedCount]=@FailedCount WHERE [Id] = @Id";
            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@TestListId", SqlDbType.UniqueIdentifier);
            command.Parameters.Add("@Name", SqlDbType.NVarChar);
            command.Parameters.Add("@FileName", SqlDbType.NVarChar);
            command.Parameters.Add("@Passed", SqlDbType.Bit);
            command.Parameters.Add("@Summary", SqlDbType.NVarChar);
            command.Parameters.Add("@Comment", SqlDbType.NVarChar);
            command.Parameters.Add("@StartTime", SqlDbType.DateTime);
            command.Parameters.Add("@EndTime", SqlDbType.DateTime);
            command.Parameters.Add("@IsManual", SqlDbType.Bit);
            command.Parameters.Add("@AllCount", SqlDbType.Int);
            command.Parameters.Add("@NotRunCount", SqlDbType.Int);
            command.Parameters.Add("@PassedCount", SqlDbType.Int);
            command.Parameters.Add("@FailedCount", SqlDbType.Int);

            return command;
        }

        #endregion

        #region Not implemented

        public DataTable OnInitializeDataSource(ExecutionContext executionContext)
        {
            return null;
        }

        #endregion

    }
}
