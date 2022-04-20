using MySql.Data.MySqlClient;

namespace LogReader
{
    class LogReader
    {
        static void Main()
        {
            string? fileLocation = string.Empty;

            List<string>? fileLines = new();

            Console.WriteLine("Please enter the log file location (include .log at end of file name):");
            Console.WriteLine(@"e.g. C:\Users\xande\OneDrive\Desktop\stdout_20220221171119_13772.log");
            fileLocation = Console.ReadLine();

            try
            {
                if (fileLocation != null)
                {
                    var logs = ProcessFile(fileLocation);

                    //DatabaseFirst(logs);
                    CodeFirst(logs);
                    
                    Console.WriteLine("Logs Succesfully Captured");
                }
                else
                {
                    Console.WriteLine("File Not Found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: { ex.Message}");
            }
        }

        static void CodeFirst(List<LogsModel> logs)
        {
            Console.WriteLine("Starting Database upload");
            using (var context = new Context())
            {
                context.Database.EnsureCreated();

                foreach(var log in logs)
                {
                    context.Add(log);
                    Console.WriteLine("Entry added");
                }

                context.SaveChanges();
            }

            Console.WriteLine("Database upload completed");
        }

        /*
         * This was used for testing and prototyping purposes
         * Felt I might leave it in to be as transparrent as possible
         */
        static void DatabaseFirst(List<LogsModel> logs)
        {
            string connString = "server=localhost;user=root;database=mydatabase;port=3306;password=your_password";

            MySqlConnection sqlConnection = new MySqlConnection(connString);

            Console.WriteLine("Opening Database Connection");

            sqlConnection.Open();

            Console.WriteLine("Database Connection Open");

            int amount = logs.Count();
            int start = 1;
            string sql = "";

            /*
             * I chose to upload each entry seperately instead of as
             * a collective in one big SQL Query to protect against
             * loss of connection or system error stopping any entries
             * from being uploaded
             */
            foreach (var log in logs)
            {
                MySqlCommand cmd = new MySqlCommand(sql, sqlConnection);

                cmd.CommandText = "INSERT IGNORE INTO Logs (LogType, Middelware, Message, Class, StackTrace) VALUES(?logType,?middelWare,?message,?class,?stackTrace);";
                cmd.Parameters.Add("?logType", MySqlDbType.VarChar).Value = log.LogType;
                cmd.Parameters.Add("?middelWare", MySqlDbType.VarChar).Value = log.Middelware;
                cmd.Parameters.Add("?message", MySqlDbType.VarChar).Value = log.Message;
                cmd.Parameters.Add("?class", MySqlDbType.VarChar).Value = log.Class;
                cmd.Parameters.Add("?stackTrace", MySqlDbType.VarChar).Value = log.StackTrace;

                cmd.ExecuteNonQuery();
                Console.WriteLine($"{start}/{amount} Entries Completed");
                start++;

                /*
                 * Visual In Console Test
                 * 
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine("LogType: " + log.LogType);
                Console.WriteLine("Middleware: " + log.Middelware);
                Console.WriteLine("Message: " + log.Message);
                Console.WriteLine("Class: " + log.Class);
                Console.WriteLine("StackTrace: " + log.StackTrace);
                */
            };

            sqlConnection.Close();
        }

        static List<LogsModel> ProcessFile(string location)
        {
            Console.WriteLine("Processing File");

            var linesArray = System.IO.File.ReadAllLines(@location);

            var lines = new List<string>(linesArray);
            int traceCounter = 0; // This keeps count of lines in StackTrace
            int lineCounter = 0;

            List<LogsModel> logs = new List<LogsModel>();
            LogsModel logsModel = new();

            /*
             * This algorithm looks very weird but it is suprisingly fast
             * Each line is inspected but procesing logic only runs if
             * the line is a new error log making it skip majority of the lines
             * 
             * Logs tend to follow a set style and structure which allows some
             * logic that depends on that structure (such as the final else if)
             * 
             * I'm sure there are even more efficient algos and I would
             * be very interested to see them so please do share.
             */
            foreach (var line in lines)
            {                
                traceCounter++;

                if (line.StartsWith("warn:"))
                {
                    if(traceCounter > 3)
                    {
                        logs.Add(logsModel);
                    }

                    traceCounter = 0;

                    logsModel = new LogsModel
                    {
                        LogType = "warn",
                        Middelware = line.Substring(5),
                        Message = lines[lineCounter + 1]
                    }; 
                }
                else if (line.StartsWith("info:"))
                {
                    if (traceCounter > 3)
                    {
                        logs.Add(logsModel);
                    }

                    traceCounter = 0;

                    logsModel = new LogsModel
                    {
                        LogType = "info",
                        Middelware = line.Substring(5),
                        Message = lines[lineCounter + 1]
                    };
                }
                else if (line.StartsWith("fail:"))
                {
                    logsModel = new LogsModel
                    {
                        LogType = "fail",
                        Middelware = line.Substring(5),
                        Message = lines[lineCounter + 1],
                        Class = lines[lineCounter + 2],
                        StackTrace = lines[lineCounter + 3]
                    };                    
                }
                else if(line.StartsWith("         at "))
                {
                    logsModel.StackTrace = logsModel.StackTrace + "::" + line;
                }

                lineCounter++;

                if(traceCounter == 0)
                {
                    logs.Add(logsModel);
                }
            }

            Console.WriteLine("File Processing Complete");
            return logs;
        }
    }    
}