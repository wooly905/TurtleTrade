using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TurtleTrade.Abstraction;
using TurtleTrade.Abstraction.Database;
using TurtleTrade.Infrastructure;

namespace TurtleTrade.Database
{
    public class TurtleDatabaseOperations : IDatabaseOperations
    {
        private readonly string _connectionString;
        private const string memberBuyStockTableName = "MemberBuyStock2";
        private const string memberStocksTableName = "MemberStock2";
        private const string stockPriceHistoryTableName = "StockPriceHistory";
        private const string stocksTableName = "Stocks";
        private const string waitingListTableName = "HistoricalDataWaitingList";

        public TurtleDatabaseOperations(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync()
        {
            List<IMemberStock> memberStocks = new List<IMemberStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select memberEmail, country, stockid, strategy, notify from {memberStocksTableName}";
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                
                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string memberEmail = reader[0].ToString();
                        CountryKind country = reader[1].ToString().GetCountryKindFromShortName();
                        string stockId = reader[2].ToString();
                        BuySellStrategyType strategy = reader[3].ToString().GetStockStrategy();
                        reader.TryGetValue("Notify", out bool isNotify);
                        memberStocks.Add(new MemberStock(memberEmail, country, stockId, isNotify, strategy));
                    }

                    reader.Close();
                }
            }

            return memberStocks;
        }

        public async Task<IReadOnlyList<IMemberStock>> GetMemberStocksAsync(string memberEmail)
        {
            if (string.IsNullOrEmpty(memberEmail))
            {
                return null;
            }

            List<IMemberStock> memberStocks = new List<IMemberStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select country, stockid, Notify from {memberStocksTableName} where MemberEmail=@memberEmail";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        CountryKind country = reader["Country"].ToString().GetCountryKindFromShortName();
                        string stockId = reader["StockId"].ToString();
                        reader.TryGetValue("Notify", out bool isNotify);
                        memberStocks.Add(new MemberStock(memberEmail, country, stockId, isNotify));
                    }

                    reader.Close();
                }
            }

            return memberStocks;
        }

        public async Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(string memberEmail)
        {
            if (string.IsNullOrEmpty(memberEmail))
            {
                return null;
            }

            List<IMemberBuyStock> memberStocks = new List<IMemberBuyStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select country, stockid, BuyPrice, NValue, StopPrice, State, Strategy, BuyDate from {memberBuyStockTableName} where MemberEmail=@memberEmail";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        CountryKind country = reader["Country"].ToString().GetCountryKindFromShortName();
                        string stockId = reader["StockId"].ToString();
                        decimal.TryParse(reader["BuyPrice"].ToString(), out decimal buyPrice);
                        decimal.TryParse(reader["NValue"].ToString(), out decimal nValue);
                        decimal.TryParse(reader["StopPrice"].ToString(), out decimal stopPrice);
                        StockBuyState state = default(StockBuyState);

                        if (int.TryParse(reader["State"].ToString(), out int stateNumber))
                        {
                            state = stateNumber.GetStockBuyState();
                        }

                        BuySellStrategyType strategy = reader["Strategy"].ToString().GetStockStrategy();

                        DateTime buyDate = DateTime.Parse(reader["BuyDate"].ToString());
                        memberStocks.Add(new MemberBuyStock(memberEmail, country, stockId, buyPrice, nValue, stopPrice, state, strategy, buyDate));
                    }

                    reader.Close();
                }
            }

            return memberStocks;
        }

        public async Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync(CountryKind country, StockBuyState state)
        {
            List<IMemberBuyStock> memberStocks = new List<IMemberBuyStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select memberEmail, StockId, BuyPrice, NValue, StopPrice, Strategy, BuyDate from {memberBuyStockTableName} where                              Country=@country and State=@state";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("state", state.GetStockBuyStateValue());
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string memberEmail = reader["memberEmail"].ToString();
                        string stockId = reader["StockId"].ToString();
                        decimal.TryParse(reader["BuyPrice"].ToString(), out decimal buyPrice);
                        decimal.TryParse(reader["NValue"].ToString(), out decimal nValue);
                        decimal.TryParse(reader["StopPrice"].ToString(), out decimal stopPrice);
                        BuySellStrategyType strategy = reader["Strategy"].ToString().GetStockStrategy();
                        DateTime buyDate = DateTime.Parse(reader["BuyDate"].ToString());
                        memberStocks.Add(new MemberBuyStock(memberEmail, country, stockId, buyPrice, nValue, stopPrice, state, strategy, buyDate));
                    }

                    reader.Close();
                }
            }

            return memberStocks;
        }

        public async Task SetMemberBuyStockBuyStateAsync(string memberEmail, CountryKind country, string stockId, StockBuyState buyState)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"update {memberBuyStockTableName} set State = @State where MemberEmail=@memberEmail and Country=@country and StockId=@StockId";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockId);
                command.Parameters.AddWithValue("State", buyState.GetStockBuyStateValue());
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockId, DateTime start, DateTime end)
        {
            List<StockPriceHistory> stockHistoryList = new List<StockPriceHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                start = start.GetDateAtStartOfDay();
                end = end.GetDateAtEndOfDay();
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "select TradeDate, LowPrice, HighPrice, ClosePrice, OpenPrice, ATR, N20, PriceChange, PriceRange, " +
                                      " Volume, YearRange, N20, N40, N60, " +
                                      $"HighIn20, LowIn10, HighIn40, LowIn15, HighIn60, LowIn20, MA20, MA40, MA60, MA120, MA240 from {stockPriceHistoryTableName} where Country=@country and StockId=@StockId and TradeDate >= @start and TradeDate <= @end";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockId);
                command.Parameters.AddWithValue("start", start);
                command.Parameters.AddWithValue("end", end);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        StockPriceHistory history = new StockPriceHistory(country, stockId, DateTime.Parse(reader["TradeDate"].ToString()))
                        {
                            LowPrice = decimal.Parse(reader["LowPrice"].ToString()),
                            HighPrice = decimal.Parse(reader["HighPrice"].ToString()),
                            ClosePrice = decimal.Parse(reader["ClosePrice"].ToString()),
                            OpenPrice = decimal.Parse(reader["OpenPrice"].ToString()),
                            Volume = int.Parse(reader["Volume"].ToString()),
                            ATR = reader.GetValue<decimal>("ATR"),
                            N20 = reader.GetValue<decimal>("N20"),
                            N40 = reader.GetValue<decimal>("N40"),
                            N60 = reader.GetValue<decimal>("N60"),
                            PriceChange = reader.GetStringValue("PriceChange"),
                            PriceRange = reader.GetStringValue("PriceRange"),
                            YearRange = reader.GetStringValue("YearRange"),
                            HighIn20 = reader.GetValue<decimal>("HighIn20"),
                            LowIn10 = reader.GetValue<decimal>("LowIn10"),
                            HighIn40 = reader.GetValue<decimal>("HighIn40"),
                            LowIn15 = reader.GetValue<decimal>("LowIn15"),
                            HighIn60 = reader.GetValue<decimal>("HighIn60"),
                            LowIn20 = reader.GetValue<decimal>("LowIn20"),
                            MA20 = reader.GetValue<decimal>("MA20"),
                            MA60 = reader.GetValue<decimal>("MA60"),
                            MA120 = reader.GetValue<decimal>("MA120"),
                            MA240 = reader.GetValue<decimal>("MA240")
                        };

                        stockHistoryList.Add(history);
                    }

                    reader.Close();
                }
            }

            return stockHistoryList;
        }

        public async Task<IReadOnlyList<IStock>> GetStocksAsync(CountryKind country)
        {
            List<IStock> stocks = new List<IStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select StockId, StockName, StockExchange, Description, ManagementFee, StockExchangeID from {stocksTableName} where Country=@country";
                command.Parameters.AddWithValue("country", country.GetShortName());
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string stockId = reader["StockId"].ToString();
                        string stockName = reader["StockName"].ToString();
                        string stockExchangeId = reader["StockExchangeID"].ToString();
                        Stock stock = new Stock(country, stockId, stockName, stockExchangeId)
                        {
                            StockExchangeName = reader.GetStringValue("StockExchange"),
                            Description = reader.GetStringValue("Description")
                        };

                        stocks.Add(stock);
                    }
                }
            }

            return stocks;
        }

        public async Task<IStockPriceHistory> GetStockPriceHistoryAsync(CountryKind country, string stockId, DateTime date)
        {
            StockPriceHistory history = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = " select TradeDate, LowPrice, HighPrice, ClosePrice, OpenPrice, ATR, N20, N40, N60, PriceChange, PriceRange, Volume, YearRange, " +
                                      $" HighIn20, LowIn10, HighIn40, LowIn15, HighIn60, LowIn20, MA20, MA40, MA60, MA120, MA240 from {stockPriceHistoryTableName} where Country=@country and StockId=@StockId and " +
                                      " Year(TradeDate) = @year and Month(TradeDate) = @month and Day(TradeDate) = @day";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockId);
                command.Parameters.AddWithValue("year", date.Year);
                command.Parameters.AddWithValue("month", date.Month);
                command.Parameters.AddWithValue("day", date.Day);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        history = new StockPriceHistory(country, stockId, date)
                        {
                            LowPrice = decimal.Parse(reader["LowPrice"].ToString()),
                            HighPrice = decimal.Parse(reader["HighPrice"].ToString()),
                            ClosePrice = decimal.Parse(reader["ClosePrice"].ToString()),
                            OpenPrice = decimal.Parse(reader["OpenPrice"].ToString()),
                            ATR = reader.GetValue<decimal>("ATR"),
                            N20 = reader.GetValue<decimal>("N20"),
                            N40 = reader.GetValue<decimal>("N40"),
                            N60 = reader.GetValue<decimal>("N60"),
                            PriceChange = reader.GetStringValue("PriceChange"),
                            PriceRange = reader.GetStringValue("PriceRange"),
                            Volume = int.Parse(reader["Volume"].ToString()),
                            YearRange = reader.GetStringValue("YearRange"),
                            HighIn20 = reader.GetValue<decimal>("HighIn20"),
                            LowIn10 = reader.GetValue<decimal>("LowIn10"),
                            HighIn40 = reader.GetValue<decimal>("HighIn40"),
                            LowIn15 = reader.GetValue<decimal>("LowIn15"),
                            HighIn60 = reader.GetValue<decimal>("HighIn60"),
                            LowIn20 = reader.GetValue<decimal>("LowIn20"),
                            MA20 = reader.GetValue<decimal>("MA20"),
                            MA40 = reader.GetValue<decimal>("MA40"),
                            MA60 = reader.GetValue<decimal>("MA60"),
                            MA120 = reader.GetValue<decimal>("MA120"),
                            MA240 = reader.GetValue<decimal>("MA240")
                        };
                    }

                    reader.Close();
                }
            }

            return history;
        }

        public async Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockID, DateTime dateBefore, int recordNumber)
        {
            List<StockPriceHistory> stockHistoryList = new List<StockPriceHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                command.CommandText = $" select top {recordNumber} TradeDate, LowPrice, HighPrice, ClosePrice, OpenPrice, ATR, N20, N40, N60, PriceChange, PriceRange, Volume, YearRange, " +
                                      $" HighIn20, LowIn10, HighIn40, LowIn15, HighIn60, LowIn20, MA20, MA40, MA60, MA120, MA240 from {stockPriceHistoryTableName} where Country=@country and StockId=@StockId and " +
                                      " TradeDate < @dateBefore order by TradeDate desc";
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockID);
                command.Parameters.AddWithValue("dateBefore", dateBefore);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        StockPriceHistory history = new StockPriceHistory(country, stockID, DateTime.Parse(reader["TradeDate"].ToString()))
                        {
                            LowPrice = decimal.Parse(reader["LowPrice"].ToString()),
                            HighPrice = decimal.Parse(reader["HighPrice"].ToString()),
                            ClosePrice = decimal.Parse(reader["ClosePrice"].ToString()),
                            OpenPrice = decimal.Parse(reader["OpenPrice"].ToString()),
                            ATR = reader.GetValue<decimal>("ATR"),
                            N20 = reader.GetValue<decimal>("N20"),
                            N40 = reader.GetValue<decimal>("N40"),
                            N60 = reader.GetValue<decimal>("N60"),
                            PriceChange = reader.GetStringValue("PriceChange"),
                            PriceRange = reader.GetStringValue("PriceRange"),
                            Volume = int.Parse(reader["Volume"].ToString()),
                            YearRange = reader.GetStringValue("YearRange"),
                            HighIn20 = reader.GetValue<decimal>("HighIn20"),
                            LowIn10 = reader.GetValue<decimal>("LowIn10"),
                            HighIn40 = reader.GetValue<decimal>("HighIn40"),
                            LowIn15 = reader.GetValue<decimal>("LowIn15"),
                            HighIn60 = reader.GetValue<decimal>("HighIn60"),
                            LowIn20 = reader.GetValue<decimal>("LowIn20"),
                            MA20 = reader.GetValue<decimal>("MA20"),
                            MA60 = reader.GetValue<decimal>("MA60"),
                            MA120 = reader.GetValue<decimal>("MA120"),
                            MA240 = reader.GetValue<decimal>("MA240")
                        };

                        stockHistoryList.Add(history);
                    }

                    reader.Close();
                }
            }

            return stockHistoryList;
        }

        public async Task AddOrUpdateStockPriceHistoryAsync(IStockPriceHistory data)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                bool isExist = false;
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $" select * from {stockPriceHistoryTableName} where Country=@country and StockId=@StockId and " +
                                      $" Year(TradeDate) = @year and Month(TradeDate) = @month and Day(TradeDate) = @day";
                command.Parameters.AddWithValue("country", data.Country.GetShortName());
                command.Parameters.AddWithValue("StockId", data.StockId);
                command.Parameters.AddWithValue("year", data.TradeDateTime.Year);
                command.Parameters.AddWithValue("month", data.TradeDateTime.Month);
                command.Parameters.AddWithValue("day", data.TradeDateTime.Day);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    isExist = true;
                }

                reader?.Close();

                if (isExist)
                {
                    // update operation
                    SqlCommand command2 = connection.CreateCommand();
                    command2.CommandType = CommandType.Text;
                    command2.CommandText = $" update {stockPriceHistoryTableName} " +
                                           " set LowPrice = @lowPrice, HighPrice = @highPrice, ClosePrice = @closePrice, OpenPrice = @openPrice, " +
                                           " ATR = @atr, N20= @n20, N40=@N40, N60=@N60, PriceChange = @priceChange, PriceRange = @priceRange, Volume = @volume, YearRange = @YearRange, " +
                                           " HighIn20 = @highIn20, LowIn10 = @lowIn10, HighIn40=@highIn40, LowIn15=@lowIn15, HighIn60=@highIn60, LowIn20=@lowIn20, " +
                                           " MA20 = @ma20, MA40 =@MA40, MA60=@ma60, MA120=@ma120, MA240 = @ma240 " +
                                           " where Country=@country and StockId=@StockId and " +
                                           " Year(TradeDate) = @year and Month(TradeDate) = @month and Day(TradeDate) = @day";
                    command2 = AssignStockPriceHistoryValuesByYearMonthDay(command2, data);
                    await command2.ExecuteNonQueryAsync().ConfigureAwait(false);
                    connection.Close();

                    return;
                }

                // add operation
                SqlCommand command3 = connection.CreateCommand();
                command3.CommandType = CommandType.Text;
                command3.CommandText = $" insert into {stockPriceHistoryTableName} " +
                                       $" values ( @country, @stockid, @tradeDate, @lowPrice, @highPrice, @closePrice, @openPrice, " +
                                       $" @atr, @n20, @n40, @n60, @priceChange, @priceRange, @volume, @YearRange, " +
                                       $" @highIn20, @lowIn10, @highIn40, @lowIn15, @highIn60, @lowIn20, @ma20, @ma40, @ma60, @ma120, @ma240)";

                command3 = AssignStockPriceHistoryValuesByDate(command3, data);
                await command3.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        private static SqlCommand AssignStockPriceHistoryValuesByYearMonthDay(SqlCommand command, IStockPriceHistory data)
        {
            command.Parameters.AddWithValue("year", data.TradeDateTime.Year);
            command.Parameters.AddWithValue("month", data.TradeDateTime.Month);
            command.Parameters.AddWithValue("day", data.TradeDateTime.Day);

            return AssignStockPriceHistoryValuesInternal(command, data);
        }

        private static SqlCommand AssignStockPriceHistoryValuesByDate(SqlCommand command, IStockPriceHistory data)
        {
            command.Parameters.AddWithValue("tradeDate", data.TradeDateTime);

            return AssignStockPriceHistoryValuesInternal(command, data);
        }

        private static SqlCommand AssignStockPriceHistoryValuesInternal(SqlCommand command, IStockPriceHistory data)
        {
            command.Parameters.AddWithValue("country", data.Country.GetShortName());
            command.Parameters.AddWithValue("StockId", data.StockId);
            command.Parameters.AddWithValue("lowPrice", data.LowPrice);
            command.Parameters.AddWithValue("highPrice", data.HighPrice);
            command.Parameters.AddWithValue("closePrice", data.ClosePrice);
            command.Parameters.AddWithValue("openPrice", data.OpenPrice);
            command.Parameters.AddWithValue("atr", data.ATR.GetValueOrDBNull());
            command.Parameters.AddWithValue("n20", data.N20.GetValueOrDBNull());
            command.Parameters.AddWithValue("n40", data.N40.GetValueOrDBNull());
            command.Parameters.AddWithValue("n60", data.N60.GetValueOrDBNull());
            command.Parameters.AddWithValue("priceChange", data.PriceChange.GetValueOrDBNull());
            command.Parameters.AddWithValue("priceRange", data.PriceRange.GetValueOrDBNull());
            command.Parameters.AddWithValue("volume", data.Volume);
            command.Parameters.AddWithValue("YearRange", data.YearRange.GetValueOrDBNull());
            command.Parameters.AddWithValue("highIn20", data.HighIn20.GetValueOrDBNull());
            command.Parameters.AddWithValue("lowIn10", data.LowIn10.GetValueOrDBNull());
            command.Parameters.AddWithValue("highIn40", data.HighIn40.GetValueOrDBNull());
            command.Parameters.AddWithValue("lowIn15", data.LowIn15.GetValueOrDBNull());
            command.Parameters.AddWithValue("highIn60", data.HighIn60.GetValueOrDBNull());
            command.Parameters.AddWithValue("lowIn20", data.LowIn20.GetValueOrDBNull());
            command.Parameters.AddWithValue("ma20", data.MA20.GetValueOrDBNull());
            command.Parameters.AddWithValue("ma40", data.MA40.GetValueOrDBNull());
            command.Parameters.AddWithValue("ma60", data.MA60.GetValueOrDBNull());
            command.Parameters.AddWithValue("ma120", data.MA120.GetValueOrDBNull());
            command.Parameters.AddWithValue("ma240", data.MA240.GetValueOrDBNull());

            return command;
        }

        public async Task<IAllPricesEntry> GetTheLatestStockPriceAsync(CountryKind country, string stockID, DateTime date)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = " select top 1 LowPrice, HighPrice, ClosePrice, OpenPrice, TradeDate, YearRange, Volume, " +
                                      " ATR, N20, N40, N60, HighIn20, LowIn10, HighIn40, LowIn15, HighIn60, LowIn20, " + 
                                      $" MA20, MA40, MA60, MA120, MA240 from {stockPriceHistoryTableName} " +
                                      " where Country=@country and StockId=@StockId and TradeDate < @date order by TradeDate desc";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockID);
                command.Parameters.AddWithValue("date", date.GetDateAtStartOfDay());

                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                    decimal lowPrice = decimal.Parse(reader["LowPrice"].ToString());
                    decimal highPrice = decimal.Parse(reader["HighPrice"].ToString());
                    decimal closePrice = decimal.Parse(reader["ClosePrice"].ToString());
                    decimal openPrice = decimal.Parse(reader["OpenPrice"].ToString());
                    DateTime tradeDate = DateTime.Parse(reader["TradeDate"].ToString());
                    string yearRange = reader.GetStringValue("YearRange");
                    int volume = int.Parse(reader["Volume"].ToString());
                    decimal? atr = reader.GetValue<decimal>("ATR");
                    decimal? n20 = reader.GetValue<decimal>("N20");
                    decimal? high20 = reader.GetValue<decimal>("HighIn20");
                    decimal? low10 = reader.GetValue<decimal>("LowIn10");
                    decimal? n40 = reader.GetValue<decimal>("N40");
                    decimal? high40 = reader.GetValue<decimal>("HighIn40");
                    decimal? low15 = reader.GetValue<decimal>("LowIn15");
                    decimal? n60 = reader.GetValue<decimal>("N60");
                    decimal? high60 = reader.GetValue<decimal>("HighIn60");
                    decimal? low20 = reader.GetValue<decimal>("LowIn20");
                    decimal? ma20 = reader.GetValue<decimal>("MA20");
                    decimal? ma40 = reader.GetValue<decimal>("MA40");
                    decimal? ma60 = reader.GetValue<decimal>("MA60");
                    decimal? ma120 = reader.GetValue<decimal>("MA120");
                    decimal? ma240 = reader.GetValue<decimal>("MA240");
                    reader.Close();

                    return new AllPricesEntry(country, stockID, lowPrice, highPrice, closePrice, openPrice, 
                                              tradeDate, yearRange, volume, atr, n20, high20, low10, n40, high40, low15, n60, high60, low20,
                                              ma20, ma40, ma60, ma120, ma240);
                }
            }

            return null;
        }

        public async Task<IReadOnlyList<IAllPricesEntry>> GetTheLatestStockPriceAsync(CountryKind country, DateTime date)
        {
            List<IAllPricesEntry> entries = new List<IAllPricesEntry>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = " select StockId, LowPrice, HighPrice, ClosePrice, OpenPrice, TradeDate, YearRange, Volume, " +
                                      " ATR, N20, HighIn20, LowIn10, N40, HighIn40, LowIn15, N60, HighIn60, LowIn20," +
                                      $" MA20, MA40, MA60, MA120, MA240 from {stockPriceHistoryTableName} " +
                                      " where Country=@country and TradeDate < @date order by TradeDate desc";

                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("date", date.GetDateAtStartOfDay());
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string stockId = reader["StockId"].ToString();
                        decimal lowPrice = decimal.Parse(reader["LowPrice"].ToString());
                        decimal highPrice = decimal.Parse(reader["HighPrice"].ToString());
                        decimal closePrice = decimal.Parse(reader["ClosePrice"].ToString());
                        decimal openPrice = decimal.Parse(reader["OpenPrice"].ToString());
                        DateTime tradeDate = DateTime.Parse(reader["TradeDate"].ToString());
                        string yearRange = reader.GetStringValue("YearRange");
                        int volume = int.Parse(reader["Volume"].ToString());
                        decimal? atr = reader.GetValue<decimal>("ATR");
                        decimal? n20 = reader.GetValue<decimal>("N20");
                        decimal? high20 = reader.GetValue<decimal>("HighIn20");
                        decimal? low10 = reader.GetValue<decimal>("LowIn10");
                        decimal? n40 = reader.GetValue<decimal>("N40");
                        decimal? high40 = reader.GetValue<decimal>("HighIn40");
                        decimal? low15 = reader.GetValue<decimal>("LowIn15");
                        decimal? n60 = reader.GetValue<decimal>("N60");
                        decimal? high60 = reader.GetValue<decimal>("HighIn60");
                        decimal? low20 = reader.GetValue<decimal>("LowIn20");
                        decimal? ma20 = reader.GetValue<decimal>("MA20");
                        decimal? ma40 = reader.GetValue<decimal>("MA40");
                        decimal? ma60 = reader.GetValue<decimal>("MA60");
                        decimal? ma120 = reader.GetValue<decimal>("MA120");
                        decimal? ma240 = reader.GetValue<decimal>("MA240");
                        entries.Add(new AllPricesEntry(country, stockId, lowPrice, highPrice, closePrice, openPrice, 
                                                       tradeDate, yearRange, volume, atr, n20, high20, low10, n40, high40, low15, n60, high60, low20,
                                                       ma20, ma40, ma60, ma120, ma240));
                    }

                    reader.Close();
                }
            }

            return entries;
        }

        public Task AddStockAsync(CountryKind country, string stockId, string stockName, string stockExchangeName, string stockDescription, string stockExchangeId)
        {
            // TODO :
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<IStockPriceHistory>> GetStockPriceHistoryAsync(CountryKind country, string stockId)
        {
            List<StockPriceHistory> stockHistoryList = new List<StockPriceHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "select TradeDate, LowPrice, HighPrice, ClosePrice, OpenPrice, ATR, N20, PriceChange, PriceRange, Volume, YearRange, HighIn20, LowIn10, " +
                                      $"MA20, MA55, MA120, MA240, N55, HighIn55, LowIn20 from {stockPriceHistoryTableName} where Country=@country and StockId=@StockId ";

                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockId);
                DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        StockPriceHistory history = new StockPriceHistory(country, stockId, DateTime.Parse(reader["TradeDate"].ToString()))
                        {
                            LowPrice = decimal.Parse(reader["LowPrice"].ToString()),
                            HighPrice = decimal.Parse(reader["HighPrice"].ToString()),
                            ClosePrice = decimal.Parse(reader["ClosePrice"].ToString()),
                            OpenPrice = decimal.Parse(reader["OpenPrice"].ToString()),
                            Volume = int.Parse(reader["Volume"].ToString()),
                            ATR = reader.GetValue<decimal>("ATR"),
                            N20 = reader.GetValue<decimal>("N20"),
                            PriceChange = reader.GetStringValue("PriceChange"),
                            PriceRange = reader.GetStringValue("PriceRange"),
                            YearRange = reader.GetStringValue("YearRange"),
                            HighIn20 = reader.GetValue<decimal>("HighIn20"),
                            LowIn10 = reader.GetValue<decimal>("LowIn10"),
                            MA20 = reader.GetValue<decimal>("MA20"),
                            MA60 = reader.GetValue<decimal>("MA55"),
                            MA120 = reader.GetValue<decimal>("MA120"),
                            MA240 = reader.GetValue<decimal>("MA240"),
                            N60 = reader.GetValue<decimal>("N55"),
                            HighIn60 = reader.GetValue<decimal>("HighIn55"),
                            LowIn20 = reader.GetValue<decimal>("LowIn20")
                        };
                        stockHistoryList.Add(history);
                    }

                    reader.Close();
                }
            }

            return stockHistoryList;
        }


        public async Task UpdateMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId, decimal stopPrice, StockBuyState newState)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $" update {memberBuyStockTableName} set StopPrice = @stopPrice, State=@newState " +
                                       " where MemberEmail = @memberEmail and Country = @country and StockId = @stockID ";

                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockID", stockId);
                command.Parameters.AddWithValue("stopPrice", stopPrice);
                command.Parameters.AddWithValue("newState", newState.GetStockBuyStateValue());
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<IMemberBuyStock>> GetMemberBuyStocksAsync()
        {
            List<IMemberBuyStock> memberStocks = new List<IMemberBuyStock>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"select MemberEmail, country, stockid, BuyPrice, NValue, StopPrice, State, Strategy, BuyDate from {memberBuyStockTableName}";
                SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string memberEmail = reader["MemberEmail"].ToString();
                        CountryKind country = reader["Country"].ToString().GetCountryKindFromShortName();
                        string stockId = reader["StockId"].ToString();
                        decimal.TryParse(reader["BuyPrice"].ToString(), out decimal buyPrice);
                        decimal.TryParse(reader["NValue"].ToString(), out decimal nValue);
                        decimal.TryParse(reader["StopPrice"].ToString(), out decimal stopPrice);
                        StockBuyState state = StockBuyState.Unknown;
                        if (int.TryParse(reader["State"].ToString(), out int stateNumber))
                        {
                            state = stateNumber.GetStockBuyState();
                        }

                        BuySellStrategyType strategy = reader["SystemN"].ToString().GetStockStrategy();

                        DateTime buyDate = DateTime.Parse(reader["BuyDate"].ToString());
                        memberStocks.Add(new MemberBuyStock(memberEmail, country, stockId, buyPrice, nValue, stopPrice, state, strategy, buyDate));
                    }

                    reader.Close();
                }
            }

            return memberStocks;
        }

        public async Task<IReadOnlyList<IHistoricalDataWaitingEntry>> GetWaitingEntriesAsync(CountryKind country)
        {
            List<IHistoricalDataWaitingEntry> waitingEntries = new List<IHistoricalDataWaitingEntry>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"select Country, StockId, DataStartDate, DataEndDate from {waitingListTableName} where State = 0 and Country=@country";
                command.Parameters.AddWithValue("country", country.GetShortName());
                SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                if (reader?.HasRows == true)
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string stockId = reader["StockId"].ToString();
                        DateTime startDate = DateTime.Parse(reader["DataStartDate"].ToString());
                        DateTime endDate = DateTime.Parse(reader["DataEndDate"].ToString());
                        waitingEntries.Add(new HistoricalDataWaitingEntry(country, stockId, HistoricalDataWaitingState.Waiting, startDate, endDate));
                    }

                    reader.Close();
                }
            }

            return waitingEntries;
        }

        public Task SetWaitingEntryToWorkingAsync(CountryKind country, string stockId)
        {
            return SetWaitingEntryToStateInternalAsync(country, stockId, HistoricalDataWaitingState.Working);
        }

        private async Task SetWaitingEntryToStateInternalAsync(CountryKind country, string stockId, HistoricalDataWaitingState state)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"update {waitingListTableName} set State=@state where country=@country and stockid=@stockid";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockid", stockId);
                command.Parameters.AddWithValue("State", state.GetStateValue());
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public Task SetWaitingEntryToDoneAsync(CountryKind country, string stockId)
        {
            return SetWaitingEntryToStateInternalAsync(country, stockId, HistoricalDataWaitingState.Done);
        }

        public async Task DeleteDoneWaitingEntriesAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"delete from {waitingListTableName} where state = 99";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task AddWaitingEntryAsync(CountryKind country, string stockId, DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"insert {waitingListTableName} values (@country, @stockid, @state, @DataStartDate, @DataEndDate) ";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockid", stockId);
                command.Parameters.AddWithValue("state", HistoricalDataWaitingState.Waiting);
                command.Parameters.AddWithValue("DataStartDate", startDate);
                command.Parameters.AddWithValue("DataEndDate", endDate);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteStockPriceHistoryAsync(CountryKind country, string stockId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"delete from {stockPriceHistoryTableName} where country=@country and stockid=@stockid";
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockid", stockId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandText = $"delete from {memberBuyStockTableName} where memberEmail=@memberEmail and country=@country and stockid=@stockid";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockid", stockId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task AddMemberBuyStockAsync(string memberEmail, CountryKind country, string stockId, decimal buyPrice, decimal N, BuySellStrategyType strategy, DateTime buyDate)
        {
            decimal stopPrice = buyPrice - N * 2;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"insert {memberBuyStockTableName} values (@memberEmail, @country, @stockID, @buyPrice, @NValue, @stopPrice, @state, @Strategy, @buyDate)";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockID", stockId);
                command.Parameters.AddWithValue("buyPrice", buyPrice);
                command.Parameters.AddWithValue("NValue", N);
                command.Parameters.AddWithValue("stopPrice", stopPrice);
                command.Parameters.AddWithValue("state", StockBuyState.Buy);
                command.Parameters.AddWithValue("Strategy", strategy.GetString());
                command.Parameters.AddWithValue("buyDate", buyDate);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task AddMemberStockAsync(string memberEmail, CountryKind country, string stockId, BuySellStrategyType strategy, bool isNotify)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"insert {memberStocksTableName} values (@memberEmail, @country, @stockID, @strategy, @notify)";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("stockID", stockId);
                command.Parameters.AddWithValue("strategy", strategy.GetString());
                command.Parameters.AddWithValue("notify", isNotify);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task SetMemberStockAsync(string memberEmail, CountryKind country, string stockId, BuySellStrategyType strategy, bool isNotify)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                SqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = $"update {memberStocksTableName} set Strategy = @strategy, Notify = @notify where MemberEmail = @memberEmail and Country = @country and StockId = @StockId";
                command.Parameters.AddWithValue("memberEmail", memberEmail);
                command.Parameters.AddWithValue("country", country.GetShortName());
                command.Parameters.AddWithValue("StockId", stockId);
                command.Parameters.AddWithValue("strategy", strategy.GetString());
                command.Parameters.AddWithValue("notify", isNotify);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
