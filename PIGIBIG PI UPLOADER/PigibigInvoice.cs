using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
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
        public string BranchName { get; set; }

        public void SaveTransaction(List<PigibigInvoice> data)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                List<string> prm = new List<string>();
                Dictionary<string, object> dic = new Dictionary<string, object>();
                

                for (int count = 0; count < data.Count; count++)
                {
                    sb.Append(@"INSERT INTO pig_tran02 (TRANS_ID, REFERENCE_NO, SOW, PARITY, TYPE, BATCH, ID_STOCK, DESCRIPTION, CATEGORY, QUANTITY, UOM, PRICE, TOTAL, INV_DATE, CODE_NO, ACCOUNT_NO, BRANCH_CODE, TRANS_DATE, BRANCH) VALUES ");
                    sb.Append($"(0, @reference{count}, @sow{count}, @parity{count}, @type{count}, @batch{count}, @idstock{count}, @desc{count}, @category{count}, @quantity{count}, @uom{count}, @price{count}, @total{count}, @invdate{count}, @codeno{count}, @account{count}, @branchcode{count}, @transdate{count}, @branch{count}); {Environment.NewLine}");

                    dic.Add("@reference" + count, data[count].Reference);
                    dic.Add("@sow" + count, data[count].SowNo);
                    dic.Add("@parity" + count, data[count].ParityNo.PadLeft(5, '0'));
                    dic.Add("@type" + count, data[count].TypeTransaction);
                    dic.Add("@batch" + count, data[count].BatchNo);
                    dic.Add("@idstock" + count, data[count].IdStock);
                    dic.Add("@desc" + count, data[count].Description);
                    dic.Add("@category" + count, data[count].Category);
                    dic.Add("@quantity" + count, data[count].Qty);
                    dic.Add("@uom" + count, data[count].UOM);
                    dic.Add("@price" + count, data[count].Price);
                    dic.Add("@total" + count, data[count].TotalAmount);
                    dic.Add("@invdate" + count, data[count].InvoiceDate);
                    dic.Add("@codeno" + count, data[count].CodeNo);
                    dic.Add("@account" + count, data[count].AccountNo);
                    dic.Add("@branchcode" + count, data[count].BranchCode);
                    dic.Add("@transdate" + count, data[count].InvoiceDate);
                    dic.Add("@branch" + count, data[count].BranchName);
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

        public int UpdateExtractedPI()
        {
            try
            {
                int result = 0;

                using (var mysql = new MySQLHelper(Properties.Settings.Default.DB_POS))
                {
                    mysql.BeginTransaction();

                    //update TRANS_ID in pigibig invoice 
                    mysql.ArgSQLCommand = new StringBuilder(@"UPDATE invoice SET extractedPI = 'Y' WHERE LEFT(REFERENCE,2) = 'PI' AND extractedPI = 'N'");
                    result += mysql.ExecuteMySQL();

                    mysql.ArgSQLCommand = new StringBuilder(@"UPDATE ledger SET extractedPI = 'Y' WHERE LEFT(REFERENCE,2) = 'PI' AND extractedPI = 'N'");
                    result += mysql.ExecuteMySQL();


                    mysql.ArgSQLCommand = new StringBuilder(@"update ledger m set m.extractedPI = 'Y' WHERE (LEFT(m.reference, 2) = 'RC' AND LEFT(m.crossReference, 2 ) = 'PI') and extractedPI = 'N';");
                    result += mysql.ExecuteMySQL();

                    mysql.CommitTransaction();

                }

                return result;
            }
            catch
            {

                throw;
            }
        }

        public DataTable GetPIFromFile(string file)
        {
            try
            {
                string conn = string.Empty;
                string sheetName = string.Empty;
                string fileExt = Path.GetExtension(file);
                DataTable dt = new DataTable();

                if(fileExt.CompareTo(".xls") == 0)
                    conn = $@"provider=Microsoft.Jet.OLEDB.4.0;Data Source={file};Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007
                else
                    conn = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={file};Extended Properties='Excel 12.0;HDR=YES';";

                

                using (OleDbConnection db = new OleDbConnection(conn))
                {
                    db.Open();

                    DataTable sheet = db.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    foreach (DataRow row in sheet.Rows)
                    {
                        sheetName = row["TABLE_NAME"].ToString();
                    }


                    using (OleDbDataAdapter da = new OleDbDataAdapter($"SELECT * FROM [{sheetName}]" ,db))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch
            {

                throw;
            }
        }

        public List<PigibigInvoice> GetPI()
        {
            try
            {
                var data = new List<PigibigInvoice>();
                var sb = new StringBuilder();

                sb.Append(@"SELECT
				        inv.REFERENCE,
				        lgd.SOW,
				        LPAD(CAST(lgd.PARITY as unsigned), 5, '0') `PARITY`,
				        lgd.lrtype 'TYPE',
				        lgd.lrbatch 'BATCH',
				        inv.idstock 'ITEM_CODE',
				        stk.name 'DESCRIPTION',
				        stk.type'CATEGORY',
				        inv.quantity 'QUANTITY',
				        inv.UNIT 'UOM',
				        inv.SELLING 'PRICE',
				        inv.AMOUNT 'AMOUNT',
				        DATE_FORMAT(inv.date, '%Y-%m-%d') 'INV_DATE',
				        lgd.idfile 'CODE_NO',
				        lgd.idAccount 'ACCOUNT_NUMBER',
					        bns.BranchCode 'BRANCH_CODE',
					        lgd.idbranch 'BRANCH_NAME'
					        FROM business_segments bns
					        INNER JOIN invoice inv ON bns.idBranch = inv.idBranch
					        INNER JOIN ledger lgd ON bns.idBranch = lgd.idbranch AND inv.reference = lgd.reference AND inv.idfile = lgd.idfile
					        INNER JOIN stocks stk ON inv.idstock = stk.idstock
					        WHERE LEFT(inv.reference, 2) = 'PI'
					        -- AND lgd.extractedPI = 'N'
					        -- AND inv.extractedPI = 'N'
					        AND lgd.cancelled = 0
					        AND inv.cancelled = 0

			        UNION all

			        SELECT
				        rc.REFERENCE,
				        lg.SOW,
				        LPAD(CAST(lg.PARITY as unsigned), 5, '0') `PARITY`,
				        rc.TYPE,
				        rc.BATCH,
				        rc.ITEM_CODE,
				        rc.DESCRIPTION,
				        rc.CATEGORY,
				        rc.QUANTITY,
				        rc.UOM,
				        rc.PRICE,
				        rc.AMOUNT,
				        rc.INV_DATE,
				        rc.CODE_NO,
				        rc.ACCOUNT_NUMBER,
				        rc.BRANCH_CODE,
				        rc.BRANCH_NAME
			        FROM(SELECT
				        inv.REFERENCE,
				        lgd.SOW,
				        LPAD(CAST(lgd.PARITY as unsigned), 5, '0') `PARITY`,
				        lgd.lrtype 'TYPE',
				        lgd.lrbatch 'BATCH',
				        inv.idstock 'ITEM_CODE',
				        stk.name 'DESCRIPTION',
				        stk.type'CATEGORY',
				        inv.quantity * -1'QUANTITY',
				        inv.UNIT 'UOM',
				        inv.SELLING * -1 'PRICE',
				        inv.AMOUNT * -1 'AMOUNT',
				        DATE_FORMAT(inv.date, '%Y-%m-%d') 'INV_DATE',
				        lgd.idfile 'CODE_NO',
				        lgd.idAccount 'ACCOUNT_NUMBER',
					        bns.BranchCode 'BRANCH_CODE',
				        lgd.crossReference,
				        lgd.idbranch 'BRANCH_NAME'
					        FROM business_segments bns
					        INNER JOIN invoice inv ON bns.idBranch = inv.idBranch
					        INNER JOIN ledger lgd ON bns.idBranch = lgd.idbranch AND inv.reference = lgd.reference AND inv.idfile = lgd.idfile
					        INNER JOIN stocks stk ON inv.idstock = stk.idstock
					        WHERE LEFT(inv.reference, 2) = 'RC' AND LEFT(lgd.crossReference, 2) = 'PI'
					        -- AND lgd.extractedPI = 'N'
					        -- AND inv.extractedPI = 'N'
					        AND lgd.cancelled = 0
					        AND inv.cancelled = 0
			        ) as rc INNER JOIN ledger lg ON rc.crossReference = lg.reference");

                using (var conn = new MySQLHelper(Properties.Settings.Default.DB_POS, sb))
                {
                    using (var dr = conn.MySQLReader())
                    {
                        while (dr.Read())
                        {
                            data.Add(new PigibigInvoice
                            {
                                Reference = dr["REFERENCE"].ToString(),
                                SowNo = dr["SOW"].ToString(),
                                ParityNo = dr["PARITY"].ToString(),
                                TypeTransaction = dr["TYPE"].ToString(),
                                BatchNo = dr["BATCH"].ToString(),
                                IdStock = dr["ITEM_CODE"].ToString(),
                                Description = dr["DESCRIPTION"].ToString(),
                                Category = dr["CATEGORY"].ToString(),
                                Qty = dr["QUANTITY"].ToString(),
                                UOM = dr["UOM"].ToString(),
                                Price = dr["PRICE"].ToString(),
                                TotalAmount = dr["AMOUNT"].ToString(),
                                AccountNo = dr["ACCOUNT_NUMBER"].ToString(),
                                CodeNo = dr["CODE_NO"].ToString(),
                                InvoiceDate = dr["INV_DATE"].ToString(),
                                BranchCode = dr["BRANCH_CODE"].ToString(),
                                BranchName = dr["BRANCH_NAME"].ToString()
                            });
                        }
                    }
                }

                return data;
            }
            catch
            {

                throw;
            }
        }
    }
}
