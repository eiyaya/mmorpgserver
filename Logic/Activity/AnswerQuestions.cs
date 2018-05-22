#region using

using System;
using DataContract;
using DataTable;
using EventSystem;
using Shared;

#endregion

namespace Logic
{
    public interface IAnswerQuestions
    {
        ErrorCodes AnswerQuestion(CharacterController character, bool result, QuestionMessage msg);
        ErrorCodes AnswerQuestionUseItem(CharacterController character, QuestionMessage msg);
        void GetNowQuestion(CharacterController character, QuestionMessage msg);
        int RandomQuestion(CharacterController character, int count);
        ErrorCodes RemoveErrorAnswer(CharacterController character);
    }

    public class AnswerQuestionsDefaultImpl : IAnswerQuestions
    {
        //获得题目信息
        private void GetQuestionData(QuestionMessage msg, int qId)
        {
            var tb = Table.GetSubject(qId);
            if (tb == null)
            {
                msg.QuestionId = -1;
                return;
            }
            msg.QuestionId = qId;
            msg.Title = tb.Title;
            msg.Answer.Add(tb.RightKey);
            var passCount = MyRandom.Random(0, 3);
            for (var i = 0; i < tb.Wrong.Length; i++)
            {
                if (i == passCount)
                {
                    continue;
                }
                msg.Answer.Add(tb.Wrong[i]);
            }
        }

        //回答题目 使用道具
        public ErrorCodes AnswerQuestionUseItem(CharacterController character, QuestionMessage msg)
        {
            var hour = DateTime.Now.Hour;
            if (hour < AnswerQuestions.BeginTime || hour >= AnswerQuestions.EndTime)
            {
                return ErrorCodes.Error_AnswerNotTime;
            }
            var qIndex = character.GetExData(436); //当前题目索引
            if (qIndex >= AnswerQuestions.MaxQuestionCount)
            {
                return ErrorCodes.Error_Answer_Over;
            }
            if (character.mBag.GetItemCount(22051) < 1)
            {
                return ErrorCodes.ItemNotEnough;
            }
            character.mBag.DeleteItem(22051, 1, eDeleteItemType.AnswerQuestion);
            AnswerQuestion(character, true, msg);
            return ErrorCodes.OK;
        }

        //使用道具  移除错误答案
        public ErrorCodes RemoveErrorAnswer(CharacterController character)
        {
            var hour = DateTime.Now.Hour;
            if (hour < AnswerQuestions.BeginTime || hour >= AnswerQuestions.EndTime)
            {
                return ErrorCodes.Error_AnswerNotTime;
            }

            var qIndex = character.GetExData(436); //当前题目索引
            if (qIndex >= AnswerQuestions.MaxQuestionCount)
            {
                return ErrorCodes.Error_Answer_Over;
            }
            if (character.mBag.GetItemCount(22052) < 1)
            {
                return ErrorCodes.ItemNotEnough;
            }
            character.mBag.DeleteItem(22052, 1, eDeleteItemType.AnswerQuestion);
            return ErrorCodes.OK;
        }

        //回答题目 result=0:错误 1:正确
        public ErrorCodes AnswerQuestion(CharacterController character, bool result, QuestionMessage msg)
        {
            var hour = DateTime.Now.Hour;
            if (hour < AnswerQuestions.BeginTime || hour >= AnswerQuestions.EndTime)
            {
                return ErrorCodes.Error_AnswerNotTime;
            }

            var qIndex = character.GetExData(436); //当前题目索引
            if (qIndex >= AnswerQuestions.MaxQuestionCount)
            {
                msg.QuestionId = -1;
                return ErrorCodes.OK;
            }

            if (result)
            {
                character.AddExData(437, 1);
                //正确奖励
                var addExp =
                    (int)
                        (AnswerQuestions.ModifyReward[qIndex]*AnswerQuestions.ExpParam*
                         Table.GetLevelData(character.GetLevel()).DynamicExp);
                character.AddExData(66, addExp);
                character.mBag.AddItem(1, addExp, eCreateItemType.AnswerQuestion);
                var addMoney = (int) (AnswerQuestions.ModifyReward[qIndex]*AnswerQuestions.MoneyCount);
                character.AddExData(67, addMoney);
                character.mBag.AddItem(2, addMoney, eCreateItemType.AnswerQuestion);
            }
            else
            {
                //错误奖励
                var addExp =
                    (int)
                        (AnswerQuestions.ModifyReward[qIndex]*AnswerQuestions.ExpParam*
                         Table.GetLevelData(character.GetLevel()).DynamicExp*AnswerQuestions.ErrorResCount);
                character.AddExData(66, addExp);
                character.mBag.AddItem(1, addExp, eCreateItemType.AnswerQuestion);
                var addMoney =
                    (int)
                        (AnswerQuestions.ModifyReward[qIndex]*AnswerQuestions.MoneyCount*AnswerQuestions.ErrorResCount);
                character.AddExData(67, addMoney);
                character.mBag.AddItem(2, addMoney, eCreateItemType.AnswerQuestion);
            }
            qIndex = qIndex + 1;
            character.SetExData(436, qIndex); //当前题目索引+1
            var nextExdataId = qIndex - 1 + 459;
            var nextQ = RandomQuestion(character, qIndex);
            //int nextQ = character.RandomQuestion(qIndex);
            character.SetExData(nextExdataId, nextQ);
            GetQuestionData(msg, nextQ);
            return ErrorCodes.OK;
        }

        //随机一道新题目
        public int RandomQuestion(CharacterController character, int count)
        {
            var index = 0;
            var next = MyRandom.Random(0, AnswerQuestions.TotleQuestionCount);
            while (true)
            {
                index++;
                if (index > 20)
                {
                    break;
                }
                var isSame = false;
                for (var i = 459; i < 459 + count; i++)
                {
                    if (character.GetExData(i) == next)
                    {
                        isSame = true;
                        break;
                    }
                }
                if (isSame)
                {
                    next = MyRandom.Random(0, AnswerQuestions.TotleQuestionCount);
                }
                else
                {
                    break;
                }
            }
            //character.SetExData(459+count,next);
            return next;
        }

        //获取当前题目的信息
        public void GetNowQuestion(CharacterController character, QuestionMessage msg)
        {
            var qIndex = character.GetExData(436); //当前题目索引
            if (qIndex >= AnswerQuestions.MaxQuestionCount)
            {
                msg.QuestionId = -1;
                return;
            }
            var qId = character.GetExData(Math.Max(qIndex - 1, 0) + 459);
            if (qId == -1)
            {
                var next = MyRandom.Random(0, AnswerQuestions.TotleQuestionCount);
                character.SetExData(qIndex + 459, next);
                GetQuestionData(msg, next);
            }
            else
            {
                GetQuestionData(msg, qId);
            }
        }
    }


    public static class AnswerQuestions
    {
        public static int BeginTime = Table.GetServerConfig(206).ToInt();
        public static int EndTime = Table.GetServerConfig(207).ToInt();
        public static float ErrorResCount = 1.0f - Table.GetServerConfig(1150).ToInt()/10000.0f;
        public static int ExpParam = Table.GetServerConfig(1151).ToInt();
        //静态参数

        public static int MaxQuestionCount = Table.GetServerConfig(581).ToInt();

        public static float[] ModifyReward =
        {
            Table.GetServerConfig(1101).ToInt()/10000.0f,
            Table.GetServerConfig(1102).ToInt()/10000.0f,
            Table.GetServerConfig(1103).ToInt()/10000.0f,
            Table.GetServerConfig(1104).ToInt()/10000.0f,
            Table.GetServerConfig(1105).ToInt()/10000.0f,
            Table.GetServerConfig(1106).ToInt()/10000.0f,
            Table.GetServerConfig(1107).ToInt()/10000.0f,
            Table.GetServerConfig(1108).ToInt()/10000.0f,
            Table.GetServerConfig(1109).ToInt()/10000.0f,
            Table.GetServerConfig(1110).ToInt()/10000.0f,
            Table.GetServerConfig(1111).ToInt()/10000.0f,
            Table.GetServerConfig(1112).ToInt()/10000.0f,
            Table.GetServerConfig(1113).ToInt()/10000.0f,
            Table.GetServerConfig(1114).ToInt()/10000.0f,
            Table.GetServerConfig(1115).ToInt()/10000.0f,
            Table.GetServerConfig(1116).ToInt()/10000.0f,
            Table.GetServerConfig(1117).ToInt()/10000.0f,
            Table.GetServerConfig(1118).ToInt()/10000.0f,
            Table.GetServerConfig(1119).ToInt()/10000.0f,
            Table.GetServerConfig(1120).ToInt()/10000.0f
        };

        public static int MoneyCount = Table.GetServerConfig(1152).ToInt();
        private static IAnswerQuestions mStaticImpl;
        public static int TotleQuestionCount = 238;

        static AnswerQuestions()
        {
            LogicServer.Instance.UpdateManager.InitStaticImpl(typeof (AnswerQuestions),
                typeof (AnswerQuestionsDefaultImpl),
                o => { mStaticImpl = (IAnswerQuestions) o; });

            EventDispatcher.Instance.AddEventListener(ReloadTableEvent.EVENT_TYPE, ReloadTable);
        }

        //回答题目 result=0:错误 1:正确
        public static ErrorCodes AnswerQuestion(this CharacterController character, bool result, QuestionMessage msg)
        {
            return mStaticImpl.AnswerQuestion(character, result, msg);
        }

        //回答题目 使用道具
        public static ErrorCodes AnswerQuestionUseItem(this CharacterController character, QuestionMessage msg)
        {
            return mStaticImpl.AnswerQuestionUseItem(character, msg);
        }

        //随机一道新题目
        //public static int RandomQuestion(this CharacterController character, int count)
        //{
        //    return mStaticImpl.RandomQuestion(character, count);
        //}
        //获取当前题目的信息
        public static void GetNowQuestion(this CharacterController character, QuestionMessage msg)
        {
            mStaticImpl.GetNowQuestion(character, msg);
        }

        private static void ReloadTable(IEvent ievent)
        {
            var v = ievent as ReloadTableEvent;

            if (v.tableName == "ServerConfig")
            {
                MaxQuestionCount = Table.GetServerConfig(581).ToInt();
                ErrorResCount = 1.0f - Table.GetServerConfig(1150).ToInt()/10000.0f;
                ExpParam = Table.GetServerConfig(1151).ToInt();
                MoneyCount = Table.GetServerConfig(1152).ToInt();
                for (var i = 0; i < 20; i++)
                {
                    ModifyReward[i] = Table.GetServerConfig(1101 + i).ToInt()/10000.0f;
                }
            }
        }

        //使用道具  移除错误答案
        public static ErrorCodes RemoveErrorAnswer(this CharacterController character)
        {
            return mStaticImpl.RemoveErrorAnswer(character);
        }
    }
}