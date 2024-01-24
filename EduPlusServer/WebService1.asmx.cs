using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Linq;
using static EduPlusServer.WebService1;


namespace EduPlusServer
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        SqlConnection myCon = new SqlConnection();

        [WebMethod]
        public void AddSubject(string subject)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter addSubjectAdapter = new SqlDataAdapter("SELECT * FROM Subjects ORDER BY subject_id", myCon);

            SqlCommandBuilder addSubjectBuilder = new SqlCommandBuilder(addSubjectAdapter);

            DataSet newQueryAddSubject = new DataSet();

            addSubjectAdapter.Fill(newQueryAddSubject, "Subjects");

            DataRow newRow = newQueryAddSubject.Tables["Subjects"].NewRow();

            newRow["subject_name"] = subject;

            newQueryAddSubject.Tables["Subjects"].Rows.Add(newRow);

            addSubjectAdapter.Update(newQueryAddSubject, "Subjects");
            myCon.Close();
        }

        public class SubjectInfo
        {
            public int SubjectId { get; set; }
            public string SubjectName { get; set; }
        }

        [WebMethod]
        public List<SubjectInfo> GetSubjects()
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";

            myCon.Open();

            SqlCommand command = new SqlCommand("SELECT subject_id, subject_name FROM Subjects", myCon);

            SqlDataReader reader = command.ExecuteReader();
            List<SubjectInfo> subjects = new List<SubjectInfo>();

            while (reader.Read())
            {
                subjects.Add(new SubjectInfo
                {
                    SubjectId = (int)reader["subject_id"],
                    SubjectName = reader["subject_name"].ToString()
                });
            }

            return subjects;
        }

        [WebMethod]
        public int AddQuestion(string question, int subject)
        {
            int questionId = -1;
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter addQuestionAdapter = new SqlDataAdapter("SELECT * FROM Questions ORDER BY question_id", myCon);

            SqlCommandBuilder addQuestionBuilder = new SqlCommandBuilder(addQuestionAdapter);

            DataSet newQueryAddQuestion = new DataSet();

            addQuestionAdapter.Fill(newQueryAddQuestion, "Questions");

            DataRow newRow = newQueryAddQuestion.Tables["Questions"].NewRow();

            newRow["questionText"] = question;
            newRow["subject_id"] = subject;
            newRow["countAsked"] = 0;
            newRow["countRight"] = 0;
            newRow["levelKnown"] = 50;

            newQueryAddQuestion.Tables["Questions"].Rows.Add(newRow);

            addQuestionAdapter.Update(newQueryAddQuestion, "Questions");

            SqlDataAdapter latestIdAdapter = new SqlDataAdapter("SELECT IDENT_CURRENT('Questions')", myCon);
            DataSet latestIdDataSet = new DataSet();
            latestIdAdapter.Fill(latestIdDataSet);

            // Check if there is a valid identity value
            if (latestIdDataSet.Tables[0].Rows.Count > 0)
            {
                questionId = Convert.ToInt32(latestIdDataSet.Tables[0].Rows[0][0]);
            }

            myCon.Close();

            return questionId;
        }

        [WebMethod]
        public void AddAnswer(string answer, int questionId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter existingAnswersAdapter = new SqlDataAdapter($"SELECT countShown FROM Answers", myCon);
            DataSet existingAnswersDataSet = new DataSet();
            existingAnswersAdapter.Fill(existingAnswersDataSet, "Answers");

            // Calculate the rounded average countShown
            int totalCountShown = 0;
            int totalAnswers = 0;

            foreach (DataRow row in existingAnswersDataSet.Tables["Answers"].Rows)
            {
                totalCountShown += Convert.ToInt32(row["countShown"]);
                totalAnswers++;
            }

            int roundedAverage = totalAnswers > 0 ? (int)Math.Round((double)totalCountShown / totalAnswers) : 0;

            SqlDataAdapter addAnswerAdapter = new SqlDataAdapter("SELECT * FROM Answers ORDER BY answer_id", myCon);

            SqlCommandBuilder addAnswerBuilder = new SqlCommandBuilder(addAnswerAdapter);

            DataSet newQueryAddAnswer = new DataSet();

            addAnswerAdapter.Fill(newQueryAddAnswer, "Answers");

            DataRow newRow = newQueryAddAnswer.Tables["Answers"].NewRow();

            newRow["answerText"] = answer;
            newRow["question_id"] = questionId;
            newRow["countShown"] = roundedAverage;

            newQueryAddAnswer.Tables["Answers"].Rows.Add(newRow);

            addAnswerAdapter.Update(newQueryAddAnswer, "Answers");
            myCon.Close();
        }

        [WebMethod]
        public void RightAnswerSubmited(int questionId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter rightAnswerAdapter = new SqlDataAdapter($"SELECT * FROM Questions WHERE question_id = {questionId}", myCon);

            SqlCommandBuilder rightAnswerBuilder = new SqlCommandBuilder(rightAnswerAdapter);

            DataSet newQueryRightAnswer = new DataSet();

            rightAnswerAdapter.Fill(newQueryRightAnswer, "Questions");

            DataRow row = newQueryRightAnswer.Tables["Questions"].Rows[0];

            // Increment countAsked and countRight
            row["countAsked"] = Convert.ToInt32(row["countAsked"]) + 1;
            row["countRight"] = Convert.ToInt32(row["countRight"]) + 1;

            // Update levelKnown to be the average of current levelKnown and 100
            row["levelKnown"] = (Convert.ToDouble(row["levelKnown"]) + 100) / 2;

            rightAnswerAdapter.Update(newQueryRightAnswer, "Questions");
            myCon.Close();
        }

        [WebMethod]
        public void WrongAnswerSubmited(int questionId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter wrongAnswerAdapter = new SqlDataAdapter($"SELECT * FROM Questions WHERE question_id = {questionId}", myCon);

            SqlCommandBuilder wrongAnswerBuilder = new SqlCommandBuilder(wrongAnswerAdapter);

            DataSet newQueryWrongAnswer = new DataSet();

            wrongAnswerAdapter.Fill(newQueryWrongAnswer, "Questions");

            DataRow row = newQueryWrongAnswer.Tables["Questions"].Rows[0];

            // Increment countAsked
            row["countAsked"] = Convert.ToInt32(row["countAsked"]) + 1;

            // Update levelKnown to be the current levelKnown halved
            row["levelKnown"] = Convert.ToDouble(row["levelKnown"]) / 2;

            wrongAnswerAdapter.Update(newQueryWrongAnswer, "Questions");
            myCon.Close();
        }

        [WebMethod]
        public void AnswerShown(int answerId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter answerShownAdapter = new SqlDataAdapter($"SELECT * FROM Answers WHERE answer_id = {answerId}", myCon);

            SqlCommandBuilder answerShownBuilder = new SqlCommandBuilder(answerShownAdapter);

            DataSet newQueryAnswerShown = new DataSet();

            answerShownAdapter.Fill(newQueryAnswerShown, "Answers");

            DataRow row = newQueryAnswerShown.Tables["Answers"].Rows[0];

            // Increment countShown
            row["countShown"] = Convert.ToInt32(row["countShown"]) + 1;

            answerShownAdapter.Update(newQueryAnswerShown, "Answers");
            myCon.Close();
        }

        public class Question
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public int CountAsked { get; set; }
            public int CountRight { get; set; }
            public int LevelKnown { get; set; }
        }

        [WebMethod]
        public Question GetRandomQuestion()
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            // Step 1: Determine the number of rows in the Questions table
            SqlCommand countCommand = new SqlCommand("SELECT COUNT(*) FROM Questions", myCon);
            int rowCount = (int)countCommand.ExecuteScalar();

            // Step 2: Generate a random index less than or equal to the number of rows
            Random random = new Random();
            int randomIndex = random.Next(1, rowCount);

            // Step 3: Get all rows in Questions, sorted by the levelKnown column
            SqlDataAdapter questionsAdapter = new SqlDataAdapter("SELECT * FROM Questions ORDER BY levelKnown", myCon);
            DataSet questionsDataSet = new DataSet();
            questionsAdapter.Fill(questionsDataSet, "Questions");

            // Step 4: Generate a random index less than or equal to randomIndex
            Random questionRandom = new Random();
            int questionRandomIndex = questionRandom.Next(0, randomIndex);

            // Step 5: Generate an object of a random Question from the sorted table
            DataRow randomQuestionRow = questionsDataSet.Tables["Questions"].Rows[questionRandomIndex];
            Question randomQuestion = new Question
            {
                QuestionId = Convert.ToInt32(randomQuestionRow["question_id"]),
                QuestionText = Convert.ToString(randomQuestionRow["questionText"]),
                CountAsked = Convert.ToInt32(randomQuestionRow["countAsked"]),
                CountRight = Convert.ToInt32(randomQuestionRow["countRight"]),
                LevelKnown = Convert.ToInt32(randomQuestionRow["levelKnown"]),
            };

            myCon.Close();

            return randomQuestion;
        }

        public class RightAnswerInfo
        {
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
        }

        [WebMethod]
        public RightAnswerInfo RightAnswer(int questionId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter rightAnswerAdapter = new SqlDataAdapter($"SELECT answer_id, answerText FROM Answers WHERE question_id = {questionId}", myCon);
            DataSet rightAnswerDataSet = new DataSet();
            rightAnswerAdapter.Fill(rightAnswerDataSet, "RightAnswer");

            myCon.Close();

            if (rightAnswerDataSet.Tables["RightAnswer"].Rows.Count > 0)
            {
                DataRow answerRow = rightAnswerDataSet.Tables["RightAnswer"].Rows[0];
                RightAnswerInfo answerInfo = new RightAnswerInfo
                {
                    AnswerId = Convert.ToInt32(answerRow["answer_id"]),
                    AnswerText = Convert.ToString(answerRow["answerText"])
                };
                return answerInfo;
            }
            else
            {
                // Handle the case where no answer is found for the given questionId
                return new RightAnswerInfo { AnswerId = -1, AnswerText = "No Answer Found" };
            }
        }

        public class WrongAnswersInfo
        {
            public int WrongAnswerId1 { get; set; }
            public string WrongAnswerText1 { get; set; }
            public int WrongAnswerId2 { get; set; }
            public string WrongAnswerText2 { get; set; }
            public int WrongAnswerId3 { get; set; }
            public string WrongAnswerText3 { get; set; }
            public int WrongAnswerId4 { get; set; }
            public string WrongAnswerText4 { get; set; }
        }

        [WebMethod]
        public WrongAnswersInfo GetRandomWrongAnswers(int questionId)
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            // Step 1: Get the subject_id corresponding to the provided question_id
            SqlCommand subjectIdCommand = new SqlCommand($"SELECT subject_id FROM Questions WHERE question_id = {questionId}", myCon);
            int subjectId = (int)subjectIdCommand.ExecuteScalar();

            // Step 2: Get four random wrong answers for the same subject
            SqlDataAdapter wrongAnswersAdapter = new SqlDataAdapter($@"
                SELECT TOP 4 answer_id, answerText
                FROM Answers
                WHERE question_id IS NOT NULL
                AND question_id <> {questionId}
                AND question_id IN (SELECT question_id FROM Questions WHERE subject_id = {subjectId})
                ORDER BY countShown", myCon);

            DataSet wrongAnswersDataSet = new DataSet();
            wrongAnswersAdapter.Fill(wrongAnswersDataSet, "WrongAnswers");

            myCon.Close();

            WrongAnswersInfo wrongAnswersInfo = new WrongAnswersInfo();

            if (wrongAnswersDataSet.Tables["WrongAnswers"].Rows.Count >= 4)
            {
                DataRow row1 = wrongAnswersDataSet.Tables["WrongAnswers"].Rows[0];
                DataRow row2 = wrongAnswersDataSet.Tables["WrongAnswers"].Rows[1];
                DataRow row3 = wrongAnswersDataSet.Tables["WrongAnswers"].Rows[2];
                DataRow row4 = wrongAnswersDataSet.Tables["WrongAnswers"].Rows[3];

                wrongAnswersInfo.WrongAnswerId1 = Convert.ToInt32(row1["answer_id"]);
                wrongAnswersInfo.WrongAnswerText1 = Convert.ToString(row1["answerText"]);

                wrongAnswersInfo.WrongAnswerId2 = Convert.ToInt32(row2["answer_id"]);
                wrongAnswersInfo.WrongAnswerText2 = Convert.ToString(row2["answerText"]);

                wrongAnswersInfo.WrongAnswerId3 = Convert.ToInt32(row3["answer_id"]);
                wrongAnswersInfo.WrongAnswerText3 = Convert.ToString(row3["answerText"]);

                wrongAnswersInfo.WrongAnswerId4 = Convert.ToInt32(row4["answer_id"]);
                wrongAnswersInfo.WrongAnswerText4 = Convert.ToString(row4["answerText"]);
            }

            return wrongAnswersInfo;
        }

        [WebMethod]
        public int[] GetQuestionStatistics()
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            // Query to get the required statistics
            SqlCommand statisticsCommand = new SqlCommand(@"
        SELECT 
            COUNT(*) AS TotalQuestions,
            COUNT(DISTINCT subject_id) AS DistinctSubjects,
            SUM(countAsked) AS TotalCountAsked,
            SUM(countRight) AS TotalCountRight,
            AVG(levelKnown) AS AverageLevelKnown
        FROM Questions", myCon);

            SqlDataReader reader = statisticsCommand.ExecuteReader();

            int[] statisticsArray = new int[5];

            // Read the result and populate the array
            if (reader.Read())
            {
                statisticsArray[0] = Convert.ToInt32(reader["TotalQuestions"]);
                statisticsArray[1] = Convert.ToInt32(reader["DistinctSubjects"]);
                statisticsArray[2] = Convert.ToInt32(reader["TotalCountAsked"]);
                statisticsArray[3] = Convert.ToInt32(reader["TotalCountRight"]);
                statisticsArray[4] = Convert.ToInt32(reader["AverageLevelKnown"]);
            }

            myCon.Close();

            return statisticsArray;
        }

        public class QuestionDataInfo
        {
            public string SubjectName { get; set; }
            public string QuestionText { get; set; }
            public string AnswerText { get; set; }
            public int CountAsked { get; set; }
            public int CountRight { get; set; }
            public double LevelKnown { get; set; }
            public int QuestionId { get; set; }
            public int AnswerId {  get; set; }
            public int SubjectId {  get; set; }
        }

        [WebMethod]
        public List<QuestionDataInfo> GetQuestionData()
        {
            myCon.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True";
            myCon.Open();

            SqlDataAdapter adapter = new SqlDataAdapter(
                @"SELECT Subjects.subject_name,
                 Subjects.subject_id,
                 Questions.questionText, 
                 Answers.answerText,
                 Answers.answer_id,
                 Questions.countAsked, 
                 Questions.countRight, 
                 Questions.levelKnown,
                 Questions.question_id
          FROM Questions
          INNER JOIN Subjects ON Questions.subject_id = Subjects.subject_id
          LEFT JOIN Answers ON Questions.question_id = Answers.question_id ORDER BY Subjects.subject_name, Questions.questionText",
                myCon);

            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet, "QuestionData");

            myCon.Close();

            List<QuestionDataInfo> questionDataList = new List<QuestionDataInfo>();
            foreach (DataRow row in dataSet.Tables["QuestionData"].Rows)
            {
                QuestionDataInfo questionData = new QuestionDataInfo
                {
                    SubjectName = Convert.ToString(row["subject_name"]),
                    QuestionText = Convert.ToString(row["questionText"]),
                    AnswerText = Convert.ToString(row["answerText"]),
                    CountAsked = Convert.ToInt32(row["countAsked"]),
                    CountRight = Convert.ToInt32(row["countRight"]),
                    LevelKnown = Convert.ToDouble(row["levelKnown"]),
                    QuestionId = Convert.ToInt32(row["question_id"]),
                    AnswerId = Convert.ToInt32(row["answer_id"]),
                    SubjectId = Convert.ToInt32(row["subject_id"]),
                };

                questionDataList.Add(questionData);
            }

            return questionDataList;
        }

        [WebMethod]
        public void UpdateData(int questionId, int answerId, int subjectId, string questionText, string answerText, int countAsked, int countRight, double levelKnown)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\IAP\\Programare avansata\\EduPlusServer\\EduPlusServer\\App_Data\\Database1.mdf; Integrated Security=True"))
            {
                connection.Open();

                // Update Subjects table
                //using (SqlCommand updateSubjectCommand = new SqlCommand($"UPDATE Subjects SET subject_name = '{subjectText}' WHERE subject_id = {subjectId}", connection))
                //{
                //    updateSubjectCommand.ExecuteNonQuery();
                //}

                // Update Questions table
                using (SqlCommand updateQuestionCommand = new SqlCommand($"UPDATE Questions SET questionText = '{questionText}', countAsked = {countAsked}, countRight = {countRight}, levelKnown = {levelKnown}, subject_id = {subjectId} WHERE question_id = {questionId}", connection))
                {
                    updateQuestionCommand.ExecuteNonQuery();
                }

                // Update Answers table
                using (SqlCommand updateAnswerCommand = new SqlCommand($"UPDATE Answers SET answerText = '{answerText}' WHERE answer_id = {answerId}", connection))
                {
                    updateAnswerCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
