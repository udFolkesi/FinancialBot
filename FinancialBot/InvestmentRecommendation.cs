using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialBot
{
    public class InvestmentRecommendation
    {
        public string Profile { get; set; }
        public string Strategy { get; set; }
        public List<string> Products { get; set; }
        public string? Other { get; set; }
    }

}
