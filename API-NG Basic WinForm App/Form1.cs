using API_NG_App.TO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;


namespace API_NG_App
{
    public partial class Form1 : Form
    {
        private JsonRpcClient jsonRpcClient;
        private String m_marketId;

        public Form1()
        {
            InitializeComponent();

            // get the Client connection that we have established in the login form
            jsonRpcClient = Connection.m_jsonRpcClient;
        }


        private void btnGetRace_Click(object sender, EventArgs e)
        {
            try
            {
                var marketFilter = new MarketFilter();

                var eventTypes = jsonRpcClient.listEventTypes(marketFilter);

                ISet<string> eventypeIds = new HashSet<string>();
                foreach (EventTypeResult eventType in eventTypes)
                {
                    if (eventType.EventType.Name.Equals("Horse Racing"))
                    {
                        rtbTextAddText(string.Format("Found event type for Horse Racing: {0}",
                                Json.JsonConvert.Serialize<EventTypeResult>(eventType)));

                        eventypeIds.Add(eventType.EventType.Id);
                    }
                }
                rtbTextAddText(string.Format("Found {0} different event types\n\n", eventTypes.Count));

                var time = new TimeRange();
                time.From = DateTime.Now;
                time.To = DateTime.Now.AddDays(1);

                marketFilter = new MarketFilter();
                marketFilter.EventTypeIds = eventypeIds;
                marketFilter.MarketStartTime = time;
                marketFilter.MarketCountries = new HashSet<string>() { "GB", "IE" };
                marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

                var marketSort = MarketSort.FIRST_TO_START;
                var maxResults = "1";

                //as an example we requested runner metadata 
                ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
                marketProjections.Add(MarketProjection.RUNNER_METADATA);
                marketProjections.Add(MarketProjection.MARKET_DESCRIPTION);
                marketProjections.Add(MarketProjection.EVENT);
                marketProjections.Add(MarketProjection.MARKET_START_TIME);

                var marketCatalogues = jsonRpcClient.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
                //extract the marketId of the next horse race
                m_marketId = marketCatalogues[0].MarketId;
                rtbTextAddText(String.Format("Got the next available horse racing market. marketId = {0}\n", m_marketId));

                rtbTextAddText(string.Format("{0}  Race {1:HH:mm} {2}\n\n",
                        marketCatalogues[0].MarketName,
                        marketCatalogues[0].MarketStartTime.ToLocalTime(),
                        marketCatalogues[0].Event.Venue));

                StringBuilder sb = new StringBuilder();
                sb.Append("                        Horse    SelectionId               Jockey\n");
                sb.Append("==================================================================\n");

                for (int i = 0; i < marketCatalogues[0].Runners.Count; i++)
                {
                    sb.AppendFormat("{0,2}: {1,25}   {2,12}   {3,18}\n", i + 1,
                        marketCatalogues[0].Runners[i].RunnerName,
                        marketCatalogues[0].Runners[i].SelectionId,
                        marketCatalogues[0].Runners[i].Metadata["JOCKEY_NAME"]);
                }
                sb.Append("\n");
                rtbTextAddText(sb.ToString());

                btnGetPrices.Enabled = true;      // need mktId before we can get prices
                btnSendOrder.Enabled = true;
            }
            catch (SystemException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        private void btnGetPrices_Click(object sender, EventArgs e)
        {
            IList<MarketBook> marketBook = getMarketBook();

            StringBuilder sb = new StringBuilder();
            sb.Append("      SelectionId     Status         Amount       Price\n");
            sb.Append("                                    Matched      To Back\n");
            sb.Append("==========================================================\n");

            for (int i = 0; i < marketBook[0].Runners.Count; i++)
            {
                string priceToUse = "-";
                if (marketBook[0].Runners[i].ExchangePrices.AvailableToBack.Count > 1)
                    priceToUse = string.Format("{0,10:n2}", marketBook[0].Runners[i].ExchangePrices.AvailableToBack[0].Price);

                sb.AppendFormat("{0,2}:  {1,12}   {2,8}   {3,12:n2}  {4,10}\n", i + 1,
                    marketBook[0].Runners[i].SelectionId,
                    marketBook[0].Runners[i].Status,
                    marketBook[0].Runners[i].TotalMatched,
                    priceToUse
                    );
            }
            sb.Append("\n");
            rtbTextAddText(sb.ToString());

        }

        private IList<MarketBook> getMarketBook()
        {
            IList<string> marketIds = new List<string>();
            marketIds.Add(m_marketId);

            ISet<PriceData> priceData = new HashSet<PriceData>();
            //get all prices from the exchange
            priceData.Add(PriceData.EX_BEST_OFFERS);

            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;

            rtbTextAddText(String.Format("Getting prices for market: {0}\n\n", m_marketId));
            var marketBook = jsonRpcClient.listMarketBook(marketIds, priceProjection);
            return marketBook;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbText.Clear();
        }

        private void rtbTextAddText(string txt)
        {
            rtbText.Text += txt;
            rtbText.SelectionStart = rtbText.Text.Length;
            rtbText.ScrollToCaret();
        }

        private void btnSendOrder_Click(object sender, EventArgs e)
        {
            IList<MarketBook> marketBook = getMarketBook();

            if (marketBook.Count != 0)
            {
                //get the first runner from the market
                var runner = marketBook[0].Runners[0];
               
                //Console.WriteLine("\nPreparing to place bet on runner with Selection Id: " + selectionId
                //    + "\nBelonging to marketId: " + m_marketId
                //    + "\nBelow minimum betsize and expecting a INVALID_BET_SIZE response");

                IList<PlaceInstruction> placeInstructions = new List<PlaceInstruction>();
                var placeInstruction = new PlaceInstruction();

                placeInstruction.Handicap = 0;
                placeInstruction.Side = Side.BACK;
                placeInstruction.OrderType = OrderType.LIMIT;

                var limitOrder = new LimitOrder();
                limitOrder.PersistenceType = PersistenceType.LAPSE;
                // place a back bet at rediculous odds so it doesn't get matched 
                limitOrder.Price = 19;
                limitOrder.Size = 0.05; // placing a bet below minimum stake, expecting a error in report

                placeInstruction.LimitOrder = limitOrder;
                placeInstruction.SelectionId = runner.SelectionId;
                placeInstructions.Add(placeInstruction);

                var customerRef = "123456";

                rtbTextAddText(string.Format("Placing An Order Using Runner: {0}  £{1:n2} @ {2:n2}\n",
                        runner.SelectionId, limitOrder.Size, limitOrder.Price));
                rtbTextAddText("This should be rejected as size is below minimum of 1.00 & (odds * stake) < 1.00 also\n\n");

                var placeExecutionReport = jsonRpcClient.placeOrders(m_marketId, customerRef, placeInstructions);

                ExecutionReportErrorCode executionErrorcode = placeExecutionReport.ErrorCode;
                InstructionReportErrorCode instructionErroCode = placeExecutionReport.InstructionReports[0].ErrorCode;
                rtbTextAddText(string.Format("PlaceExecutionReport error code is: {0}\nInstructionReport error code is:    {1}\n\n",
                    executionErrorcode, instructionErroCode));
            }
            else
            {
                rtbTextAddText("\nSorry the market has no runner to place a bet on, try again later\n\n");
            }

        }


        private void btnLogout_Click(object sender, EventArgs e)
        {
            Connection.LogOut();
            Close();
        }
    }
}