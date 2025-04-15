using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialBot
{
    public class AnketaService
    {
        private readonly Dictionary<long, UserSession> _sessions = new();
        private readonly Dictionary<long, Queue<string>> userHistory = new Dictionary<long, Queue<string>>();

        private readonly string[] questions = new[]
        {
        "📋 Вопрос 2: Как вы относитесь к риску? (Негативно, Нейтрально, Положительно)",
        "📋 Вопрос 3: На какой срок планируете инвестировать? (Краткосрочно, Среднесрочно, Долгосрочно)"
    };

        public void StartAnketa(long userId)
        {
            _sessions[userId] = new UserSession();
            if (!userHistory.ContainsKey(userId))
            {
                userHistory[userId] = new Queue<string>();
            }
        }

        public (string Question, InvestmentRecommendation Recommendation) ProcessAnswer(long userId, string answer)
        {
            if (!_sessions.ContainsKey(userId)) return (null, null);

            var session = _sessions[userId];
            session.Answers.Add(answer);

            if (session.Answers.Count < questions.Length + 1)
            {
                return (questions[session.Answers.Count - 1], null);
            }

            var rec = Analyze(session.Answers);
            _sessions.Remove(userId);
            return (null, rec);
        }

        // Вернуться к предыдущему вопросу
        public string GoBack(long userId)
        {
            if (userHistory.ContainsKey(userId) && userHistory[userId].Count > 1)
            {
                // Убираем текущий вопрос
                userHistory[userId].Dequeue();
                return userHistory[userId].Peek();
            }
            return null;
        }

        public InvestmentRecommendation Analyze(List<string> answers)
        {
            int age = int.TryParse(answers[0], out int a) ? a : 30;
            string risk = answers[1].ToLower();
            string horizon = answers[2].ToLower();

            var recommendation = new InvestmentRecommendation();

            if (age > 50 || risk.Contains("негатив") || horizon.Contains("кратко"))
            {
                recommendation.Profile = "🐢 Консервативный";
                recommendation.Strategy = "Сохранение капитала, минимальный риск.";
                recommendation.Products = new List<string>
        {
            "📌 Государственные облигации",
            "📌 Банковские депозиты",
            "📌 ETF на золото"
        };
            }
            else if (risk.Contains("нейтрал") || horizon.Contains("средне"))
            {
                recommendation.Profile = "⚖️ Сбалансированный";
                recommendation.Strategy = "Комбинация роста и стабильности.";
                recommendation.Products = new List<string>
        {
            "📌 ETF на индекс S&P 500",
            "📌 Облигации + акции 50/50",
            "📌 Дивидендные акции"
        };
            }
            else
            {
                recommendation.Profile = "🐂 Агрессивный";
                recommendation.Strategy = "Максимизация дохода при высоком риске.";
                recommendation.Products = new List<string>
        {
            "📌 Акции роста (Tesla, Nvidia)",
            "📌 Криптовалюта (BTC, ETH)",
            "📌 IPO и венчурные инвестиции"
        };
            }

            return recommendation;
        }


        private class UserSession
        {
            public List<string> Answers { get; } = new();
        }
    }

}
