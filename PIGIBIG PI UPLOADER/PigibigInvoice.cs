using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIGIBIG_PI_UPLOADER
{
    public class PigibigInvoice
    {
        public string Reference { get; set; }
        public string SowNo { get; set; }
        public string ParityNo { get; set; }
        public string TypeTransaction { get; set; }
        public string BatchNo { get; set; }
        public string IdStock { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Qty { get; set; }
        public string UOM { get; set; }
        public string Price { get; set; }
        public string TotalAmount { get; set; }
        public string InvoiceDate { get; set; }
        public string BranchCode { get; set; }
        public string AccountNo { get; set; }
        public string CodeNo { get; set; }

        public void SaveTransaction(List<PigibigInvoice> data)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                List<string> prm = new List<string>();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                

                for (int count = 0; count < data.Count; count++)
                {
                    sb.Append(@"INSERT INTO pig_tran02 (TRANS_ID, REFERENCE_NO, SOW, PARITY, TYPE, BATCH, ID_STOCK, DESCRIPTION, CATEGORY, QUANTITY, UOM, PRICE, TOTAL, INV_DATE, CODE_NO, ACCOUNT_NO, BRANCH_CODE) VALUES ");
                    sb.Append($"(0, @reference{count}, @sow{count}, @parity{count}, @type{count}, @batch{count}, @idstock{count}, @desc{count}, @category{count}, @quantity{count}, @uom{count}, @price{count}, @total{count}, @invdate{count}, @codeno{count}, @account{count}, @branchcode{count}); {Environment.NewLine}");

                    dic.Add("@reference" + count, data[count].Reference);
                    dic.Add("@sow" + count, data[count].SowNo);
                    dic.Add("@parity" + count, data[count].ParityNo);
                    dic.Add("@type" + count, data[count].TypeTransaction);
                    dic.Add("@batch" + count, data[count].BatchNo);
                    dic.Add("@idstock" + count, data[count].IdStock);
                    dic.Add("@desc" + count, data[count].Reference);
                    dic.Add("@category" + count, data[count].Category);
                    dic.Add("@quantity" + count, data[count].Qty);
                    dic.Add("@uom" + count, data[count].UOM);
                    dic.Add("@price" + count, data[count].Price);
                    dic.Add("@total" + count, data[count].TotalAmount);
                    dic.Add("@invdate" + count, data[count].InvoiceDate);
                    dic.Add("@codeno" + count, data[count].CodeNo);
                    dic.Add("@account" + count, data[count].AccountNo);
                    dic.Add("@branchcode" + count, data[count].BranchCode);
                }

                using (var mysql = new MySQLHelper(Properties.Settings.Default.DB))
                {
                    mysql.BeginTransaction();

                    mysql.ArgSQLCommand = sb;
                    mysql.ArgSQLParam = dic;
                    mysql.ExecuteMySQL();

                    //update TRANS_ID in pigibig invoice 
                    mysql.ArgSQLCommand = new StringBuilder(@"UPDATE pig_tran02 pi
                            INNER JOIN pig_cust00 cust ON pi.CODE_NO = cust.CODE_NO
                            SET pi.TRANS_ID = cust.ID
                            WHERE pi.TRANS_ID = 0;");
                    mysql.ExecuteMySQL();


                    mysql.CommitTransaction();

                }

            }
            catch
            {

                throw;
            }
        }
    }
}
