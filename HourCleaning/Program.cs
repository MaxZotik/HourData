using HourData.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HourCleaning
{
    class Program
    {

        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            string pathLog = path + "\\Log";

            if (!Directory.Exists(pathLog))
                Directory.CreateDirectory(pathLog);

            try
            {
                string[] read_result_array = File.ReadAllLines(path + @"\Resources\db_name.txt", Encoding.Default);
                string connectionString = $@"Data Source={read_result_array[0]};Initial Catalog={read_result_array[1]};User ID={read_result_array[2]};Password={read_result_array[3]}";
                SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();

                int index = Constant.CHANNELS_NUMBERS.Length;

                for (int i = 0; i < index; i++)
                {
                    string channelNumber = Constant.CHANNELS_NUMBERS[i];
                    string selectQuery = "SELECT COUNT (*) FROM MinuteData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(MINUTE, " + Constant.HOUR_DATA_INTERVAL + ", GETDATE())";
                    SqlCommand sqlCommand = new SqlCommand(selectQuery, conn);
                    int rowCount = Convert.ToInt32(sqlCommand.ExecuteScalar());

                    if(rowCount > 0)
                    {
                        string query =
                            "INSERT INTO HourData ([DateTime],[CnlNum],[Min],[Max],[Avg]) VALUES (GETDATE(), " + channelNumber +
                            ", (SELECT MIN(Min) FROM MinuteData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(MINUTE, " + Constant.HOUR_DATA_INTERVAL + ", GETDATE()))" +
                            ", (SELECT MAX(Max) FROM MinuteData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(MINUTE, " + Constant.HOUR_DATA_INTERVAL + ", GETDATE()))" +
                            ", (SELECT AVG(Avg) FROM MinuteData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(MINUTE, " + Constant.HOUR_DATA_INTERVAL + ", GETDATE()))" +
                            ")"
                            ;
                        sqlCommand = new SqlCommand(query, conn);
                        sqlCommand.ExecuteNonQuery();
                    }
                }

                if(DateTime.Now.Hour == Constant.HOUR_FOR_THINNING) //если условие выполнено, то запускает суточное прореживание
                {
                    for (int i = 0; i < index; i++)
                    {
                        string channelNumber = Constant.CHANNELS_NUMBERS[i];
                        string selectQuery = "SELECT COUNT (*) FROM HourData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.DAILY_DATA_INTERVAL + ", GETDATE())";
                        SqlCommand sqlCommand = new SqlCommand(selectQuery, conn);
                        int rowCount = Convert.ToInt32(sqlCommand.ExecuteScalar());
                        if (rowCount > 0)
                        {
                            string query =
                                "INSERT INTO DailyData ([DateTime],[CnlNum],[Min],[Max],[Avg]) VALUES (GETDATE(), " + channelNumber +
                                ", (SELECT MIN([Min]) FROM HourData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.DAILY_DATA_INTERVAL + ", GETDATE()))" +
                                ", (SELECT MAX([Max]) FROM HourData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.DAILY_DATA_INTERVAL + ", GETDATE()))" +
                                ", (SELECT AVG([Avg]) FROM HourData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.DAILY_DATA_INTERVAL + ", GETDATE()))" +
                                ")"
                                ;
                            sqlCommand = new SqlCommand(query, conn);
                            sqlCommand.ExecuteNonQuery();
                        }
                    }

                    string queryDelete = $"DELETE FROM MinuteData WHERE DateTime < DATEADD(DAY, {Constant.WEEKLY_DATA_INTERVAL}, GETDATE())";
                    SqlCommand newSqlCommand = new SqlCommand(queryDelete, conn);
                    newSqlCommand.ExecuteNonQuery();

                    string nameLog = $"{read_result_array[1]}_log";
                    string SqlSelect = $"USE [{read_result_array[1]}] DBCC SHRINKFILE (N'{nameLog}', 0, TRUNCATEONLY)";

                    try
                    {
                        SqlCommand command = new SqlCommand(SqlSelect, conn);
                        command.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        if (File.Exists(pathLog + "\\HourDataLog.txt") == false)
                        {
                            File.Create(pathLog + "\\HourDataLog.txt").Close();
                        }
                        File.AppendAllText(pathLog + "\\HourDataLog.txt", DateTime.Now.ToString() + " - " + "Ошибка сжатия файла логов в БД!" + " - " + ex.ToString() + Environment.NewLine);
                    }

                }

                if((int)DateTime.Now.DayOfWeek == 1 && DateTime.Now.Hour == Constant.HOUR_FOR_THINNING) //если условие выполнено, то запускает недельное прореживание
                {
                    for (int i = 0; i < index; i++)
                    {
                        string channelNumber = Constant.CHANNELS_NUMBERS[i];
                        string selectQuery = "SELECT COUNT (*) FROM DailyData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.WEEKLY_DATA_INTERVAL + ", GETDATE())";
                        SqlCommand sqlCommand = new SqlCommand(selectQuery, conn);
                        int rowCount = Convert.ToInt32(sqlCommand.ExecuteScalar());
                        if (rowCount > 0)
                        {
                            string query =
                                "INSERT INTO WeeklyData ([DateTime],[CnlNum],[Min],[Max],[Avg]) VALUES (GETDATE(), " + channelNumber +
                                ", (SELECT MIN([Min]) FROM DailyData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.WEEKLY_DATA_INTERVAL + ", GETDATE()))" +
                                ", (SELECT MAX([Max]) FROM DailyData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.WEEKLY_DATA_INTERVAL + ", GETDATE()))" +
                                ", (SELECT AVG([Avg]) FROM DailyData WHERE CnlNum = " + channelNumber + " AND DateTime > DATEADD(DAY, " + Constant.WEEKLY_DATA_INTERVAL + ", GETDATE()))" +
                                ")"
                                ;
                             sqlCommand = new SqlCommand(query, conn);
                             sqlCommand.ExecuteNonQuery();
                        }
                    }
                }
                conn.Close();
                
                if (File.Exists(pathLog + "\\HourDataLog.txt") == false)
                {
                    File.Create(pathLog + "\\HourDataLog.txt").Close();
                }
                File.AppendAllText(pathLog + "\\HourDataLog.txt", DateTime.Now.ToString() + " - Программа завершена успешно!" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                if (File.Exists(pathLog + "\\HourDataLog.txt") == false)
                {
                    File.Create(pathLog + "\\HourDataLog.txt").Close();
                }
                File.AppendAllText(pathLog + "\\HourDataLog.txt", DateTime.Now.ToString() + " - " + ex.ToString() + Environment.NewLine);
            }
        }
    }
}
